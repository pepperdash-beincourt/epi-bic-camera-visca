// For Basic SIMPL# Classes
// For Basic SIMPL#Pro classes

using System;
using System.Collections.Generic;
using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Core;

namespace ViscaCameraPlugin
{
	public class ViscaCameraDevice : EssentialsBridgeableDevice
	{
		private readonly byte _address = 0x81;
		private const uint AddressMax = 7;

		private const int PresetSaveHoldTimeMs = 5000; // 5s
		private const int PresetMax = 16;

		private const uint PanSpeedDefault = 9; // 00...18 (hex)
		private const uint PanSpeedMax = 18;

		private const uint TiltSpeedDefault = 9; // 00...18 (hex)
		private const uint TiltSpeedMax = 18;

		private const uint ZoomSpeedDefault = 4; // 00...07 (hex)
		private const uint ZoomSpeedMax = 7;

		private const uint FocusSpeedDefault = 4; // 00...07 (hex)
		private const uint FocusSpeedMax = 7;

		private readonly long _pollTimeMs = 30000; // 30s
		private readonly long _warningTimeoutMs = 60000; // 60s
		private readonly long _errorTimeoutMs = 180000; // 180s

		private readonly IBasicCommunication _comms;
		private byte[] _commsByteBuffer = new byte[] { };
		private readonly GenericCommunicationMonitor _commsMonitor;
		private readonly bool _commsIsSerial;

		private bool _power;
		private bool _autoFocus;
		private int _presetCount;
		private uint _panSpeed = PanSpeedDefault;
		private uint _tiltSpeed = TiltSpeedDefault;
		private uint _zoomSpeed = ZoomSpeedDefault;
		private uint _focusSpeed = FocusSpeedDefault;
		private readonly uint _privacyOnPreset;
		private readonly uint _privacyOffPreset;

		/// <summary>
		/// Connect property
		/// </summary>
		public bool Connect
		{
			get { return _comms.IsConnected; }
			set
			{
				if (value)
				{
					_comms.Connect();
					_commsMonitor.Start();
				}
				else
				{
					_comms.Disconnect();
					_commsMonitor.Stop();
				}
			}
		}
		/// <summary>
		/// Power property
		/// </summary>
		public bool Power
		{
			get { return _power; }
			set
			{
				if (_power == value) return;
				_power = value;
				PowerFeedback.FireUpdate();
			}
		}
		/// <summary>
		/// Auto focus property
		/// </summary>
		public bool AutoFocus
		{
			get { return _autoFocus; }
			set
			{
				if (_autoFocus == value) return;
				_autoFocus = value;
				AutoFocusFeedback.FireUpdate();
			}
		}
		/// <summary>
		/// Preset count property
		/// </summary>
		public uint PresetCount
		{
			get { return (uint)_presetCount; }
			set
			{
				if (_presetCount == value) return;
				_presetCount = (int)value;
				PresetCountFeedback.FireUpdate();
			}
		}
		/// <summary>
		/// Pan speed
		/// </summary>
		public uint PanSpeed
		{
			get { return (uint)_panSpeed; }
			set
			{
				if (_panSpeed == value) return;
				_panSpeed = (value < 1 || value > PanSpeedMax) ? PanSpeedDefault : value;
				PanSpeedFeedback.FireUpdate();
			}
		}
		/// <summary>
		/// Tilt speed
		/// </summary>
		public uint TiltSpeed
		{
			get { return (uint)_tiltSpeed; }
			set
			{
				if (_tiltSpeed == value) return;
				_tiltSpeed = (value < 1 || value > TiltSpeedMax) ? TiltSpeedDefault : value;
				TiltSpeedFeedback.FireUpdate();
			}
		}
		/// <summary>
		/// Zoom speed
		/// </summary>
		public uint ZoomSpeed
		{
			get { return (uint)_zoomSpeed; }
			set
			{
				if (_zoomSpeed == value) return;
				_zoomSpeed = (value < 1 || value > ZoomSpeedMax) ? ZoomSpeedDefault : value;
				ZoomSpeedFeedback.FireUpdate();
			}
		}
		/// <summary>
		/// Focus speed
		/// </summary>
		public uint FocusSpeed
		{
			get { return (uint)_focusSpeed; }
			set
			{
				if (_focusSpeed == value) return;
				_focusSpeed = (value < 1 || value > FocusSpeedMax) ? FocusSpeedDefault : value;
				FocusSpeedFeedback.FireUpdate();
			}
		}		
		/// <summary>
		/// Move PTZ direction enumeration
		/// </summary>
		public enum EDirection
		{
			Stop = 0,
			Home = 1,
			PanLeft = 2,
			PanRight = 3,
			TiltUp = 4,
			TiltDown = 5,
			ZoomIn = 6,
			ZoomOut = 7,
			FocusAuto = 8,
			FocusNear = 9,
			FocusFar = 10,
			PrivacyOn = 11,
			PrivacyOff = 12
		}

		private Dictionary<uint, ViscaCameraPresetConfig> _presetNames; 

		/// <summary>
		/// Connect feedback
		/// </summary>
		public BoolFeedback ConnectFeedback { get; private set; }
		/// <summary>
		/// Online feedback
		/// </summary>
		public BoolFeedback OnlineFeedback { get; private set; }
		/// <summary>
		/// Status value feedback
		/// </summary>
		public IntFeedback StatusFeedback { get; private set; }
		/// <summary>
		/// Power feedback
		/// </summary>
		public BoolFeedback PowerFeedback { get; private set; }
		/// <summary>
		/// Auto focus feedback
		/// </summary>
		public BoolFeedback AutoFocusFeedback { get; private set; }
		/// <summary>
		/// Pan speed feedback
		/// </summary>
		public IntFeedback PanSpeedFeedback { get; private set; }
		/// <summary>
		/// Tilt speed feedback
		/// </summary>
		public IntFeedback TiltSpeedFeedback { get; private set; }
		/// <summary>
		/// Zoom speed feedback
		/// </summary>
		public IntFeedback ZoomSpeedFeedback { get; private set; }
		/// <summary>
		/// Focus speed feedback
		/// </summary>
		public IntFeedback FocusSpeedFeedback { get; private set; }
		/// <summary>
		/// Preset count feedback
		/// </summary>
		public IntFeedback PresetCountFeedback { get; private set; }
		/// <summary>
		/// Preset name feedbacks
		/// </summary>
		public Dictionary<uint, StringFeedback> PresetNameFeedbacks { get; private set; }
		/// <summary>
		/// Preset enable feedbacks
		/// </summary>
		public Dictionary<uint, BoolFeedback> PresetEnableFeedbacks { get; private set; }

		

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="key">device key</param>
		/// <param name="name">device name</param>
		/// <param name="config">device config</param>
		/// <param name="comms">IBasicCommunications</param>
		public ViscaCameraDevice(string key, string name, ViscaCameraConfig config, IBasicCommunication comms)
			: base(key, name)
		{
			Debug.Console(0, this, "Constructing new VISCA Camera instance");

			_presetNames = new Dictionary<uint, ViscaCameraPresetConfig>();
			_presetNames = config.Presets;

			ConnectFeedback = new BoolFeedback(() => Connect);
			OnlineFeedback = new BoolFeedback(() => _commsMonitor.IsOnline);
			StatusFeedback = new IntFeedback(() => (int)_commsMonitor.Status);

			PowerFeedback = new BoolFeedback(() => Power);
			AutoFocusFeedback = new BoolFeedback(() => AutoFocus);
			PresetNameFeedbacks = new Dictionary<uint, StringFeedback>();
			PresetCountFeedback = new IntFeedback(() => (int)PresetCount);
			PanSpeedFeedback = new IntFeedback(() => (int)PanSpeed);
			TiltSpeedFeedback = new IntFeedback(() => (int)TiltSpeed);
			ZoomSpeedFeedback = new IntFeedback(() => (int)ZoomSpeed);
			FocusSpeedFeedback = new IntFeedback(() => (int)FocusSpeed);

			if (config.PollTimeMs > 0 && config.PollTimeMs != _pollTimeMs)
				_pollTimeMs = config.PollTimeMs;

			if (config.WarningTimeoutMs > 0 && config.WarningTimeoutMs != _warningTimeoutMs)
				_warningTimeoutMs = config.WarningTimeoutMs;

			if (config.ErrorTimeoutMs > 0 && config.ErrorTimeoutMs != _errorTimeoutMs)
				_errorTimeoutMs = config.ErrorTimeoutMs;

			if (config.Address > 0 && config.Address <= AddressMax && config.Address != _address)
				_address = Convert.ToByte(0x80 + config.Address);

			if (config.PanSpeed > 0 && config.PanSpeed <= PanSpeedMax && config.PanSpeed != PanSpeed)
				PanSpeed = config.PanSpeed;

			if (config.TiltSpeed > 0 && config.TiltSpeed <= TiltSpeedMax && config.TiltSpeed != TiltSpeed)
				TiltSpeed = config.TiltSpeed;

			if (config.ZoomSpeed > 0 && config.ZoomSpeed <= ZoomSpeedMax && config.ZoomSpeed != ZoomSpeed)
				ZoomSpeed = config.ZoomSpeed;

			if (config.FocusSpeed > 0 && config.FocusSpeed <= FocusSpeedMax && config.FocusSpeed != FocusSpeed)
				FocusSpeed = config.FocusSpeed;

			if (config.PrivacyOnPreset > 0 && config.PrivacyOnPreset <= PresetMax && config.PrivacyOnPreset != _privacyOnPreset)
				_privacyOnPreset = config.PrivacyOnPreset;

			if (config.PrivacyOffPreset > 0 && config.PrivacyOffPreset <= PresetMax && config.PrivacyOffPreset != _privacyOffPreset)
				_privacyOffPreset = config.PrivacyOffPreset;

			_comms = comms;
			_comms.BytesReceived += Handle_BytesRecieved;
			_commsMonitor = new GenericCommunicationMonitor(this, _comms, _pollTimeMs, _warningTimeoutMs, _errorTimeoutMs, Poll);

			var socket = _comms as ISocketStatus;
			if (socket != null)
			{
				// device is configured for IP control
				_commsIsSerial = false;
				socket.ConnectionChange += socket_ConnectionChange;
			}
			else
			{
				// device is configured for RS232 control
				_commsIsSerial = true;
			}

			AddPostActivationAction(() => InitializePresets(_presetNames));
			AddPostActivationAction(() => Connect = true);
		}

		private void InitializePresets(Dictionary<uint, ViscaCameraPresetConfig> presets)
		{
			foreach (var preset in presets)
			{
				var item = preset;

				Debug.Console(2, this,"Preset-{0} Enabled: {1} Name: {2}", item.Key, item.Value.Enabled.ToString(), item.Value.Name);

				if(PresetNameFeedbacks == null)
					PresetNameFeedbacks = new Dictionary<uint, StringFeedback>();

				if (PresetNameFeedbacks.ContainsKey(item.Key))
					PresetNameFeedbacks[item.Key] = new StringFeedback(() => item.Value.Name);
				else
					PresetNameFeedbacks.Add(item.Key, new StringFeedback(() => item.Value.Name));
			}
		}

		#region Overrides of EssentialsBridgeableDevice

		/// <summary>
		/// Link to API method replaces bridge class, this method will be called by the bridge directly
		/// </summary>
		/// <param name="trilist"></param>
		/// <param name="joinStart"></param>
		/// <param name="joinMapKey"></param>
		/// <param name="bridge"></param>
		public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
		{
			var joinMap = new ViscaCameraBridgeJoinMap(joinStart);

			// This adds the join map to the collection on the bridge
			if (bridge != null)
			{
				bridge.AddJoinMap(Key, joinMap);
			}

			var customJoins = JoinMapHelper.TryGetJoinMapAdvancedForDevice(joinMapKey);
			if (customJoins != null)
			{
				joinMap.SetCustomJoinData(customJoins);
			}

			Debug.Console(1, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));
			Debug.Console(0, "Linking to Bridge Type {0}", GetType().Name);

			// link joins to bridge
			trilist.SetString(joinMap.DeviceName.JoinNumber, Name);

			//ConnectFeedback.LinkInputSig(trilist.BooleanInput[joinMap.Connect.JoinNumber]);
			StatusFeedback.LinkInputSig(trilist.UShortInput[joinMap.Status.JoinNumber]);
			OnlineFeedback.LinkInputSig(trilist.BooleanInput[joinMap.IsOnline.JoinNumber]);

			// power
			trilist.SetBoolSigAction(joinMap.PowerOn.JoinNumber, SetPower);
			PowerFeedback.LinkInputSig(trilist.BooleanInput[joinMap.PowerOn.JoinNumber]);

			trilist.SetBoolSigAction(joinMap.PowerOff.JoinNumber, SetPower);
			PowerFeedback.LinkInputSig(trilist.BooleanInput[joinMap.PowerOff.JoinNumber]);

			// preset
			PresetCountFeedback.LinkInputSig(trilist.UShortInput[joinMap.PresetCount.JoinNumber]);
			foreach (var item in PresetNameFeedbacks)
			{
				// preset number
				var preset = (ushort)item.Key;

				// preset names
				var nameJoin = preset + joinMap.PresetNames.JoinNumber - 1;
				var nameFeedback = item.Value;
				nameFeedback.LinkInputSig(trilist.StringInput[nameJoin]);

				// preset recall
				var recallJoin = preset + joinMap.PresetRecall.JoinNumber - 1;
				trilist.SetSigHeldAction(recallJoin, PresetSaveHoldTimeMs, () => SavePreset(preset), () => RecallPreset(preset));

				// preset save/store
				var saveJoin = preset + joinMap.PresetSave.JoinNumber - 1;
				trilist.SetSigTrueAction(saveJoin, () => SavePreset(preset));
			}

			// home
			trilist.SetBoolSigAction(joinMap.Home.JoinNumber, sig => Move(sig, EDirection.Home));

			// pan
			trilist.SetBoolSigAction(joinMap.PanLeft.JoinNumber, sig => Move(sig, EDirection.PanLeft));
			trilist.SetBoolSigAction(joinMap.PanRight.JoinNumber, sig => Move(sig, EDirection.PanRight));
			trilist.SetUShortSigAction(joinMap.PanSpeed.JoinNumber, value => SetPanSpeed(value));
			PanSpeedFeedback.LinkInputSig(trilist.UShortInput[joinMap.PanSpeed.JoinNumber]);

			// tilt
			trilist.SetBoolSigAction(joinMap.TiltUp.JoinNumber, sig => Move(sig, EDirection.TiltUp));
			trilist.SetBoolSigAction(joinMap.TiltDown.JoinNumber, sig => Move(sig, EDirection.TiltDown));
			trilist.SetUShortSigAction(joinMap.TiltSpeed.JoinNumber, value => SetTiltSpeed(value));
			PanSpeedFeedback.LinkInputSig(trilist.UShortInput[joinMap.TiltSpeed.JoinNumber]);

			// zoom
			trilist.SetBoolSigAction(joinMap.ZoomIn.JoinNumber, sig => Move(sig, EDirection.ZoomIn));
			trilist.SetBoolSigAction(joinMap.ZoomOut.JoinNumber, sig => Move(sig, EDirection.ZoomOut));
			trilist.SetUShortSigAction(joinMap.ZoomSpeed.JoinNumber, value => SetZoomSpeed(value));
			PanSpeedFeedback.LinkInputSig(trilist.UShortInput[joinMap.ZoomSpeed.JoinNumber]);

			// focus
			trilist.SetBoolSigAction(joinMap.AutoFocus.JoinNumber, sig => Move(sig, EDirection.FocusAuto));
			AutoFocusFeedback.LinkInputSig(trilist.BooleanInput[joinMap.AutoFocus.JoinNumber]);

			// privacy
			trilist.SetBoolSigAction(joinMap.PrivacyOn.JoinNumber, sig => Move(sig, EDirection.PrivacyOn));
			trilist.SetBoolSigAction(joinMap.PrivacyOff.JoinNumber, sig => Move(sig, EDirection.PrivacyOff));

			UpdateFeedbacks();

			trilist.OnlineStatusChange += (o, a) =>
			{
				if (a.DeviceOnLine)
				{
					trilist.SetString(joinMap.DeviceName.JoinNumber, Name);
					UpdateFeedbacks();
				}
			};
		}

		private void UpdateFeedbacks()
		{
			ConnectFeedback.FireUpdate();
			OnlineFeedback.FireUpdate();
			StatusFeedback.FireUpdate();

			PowerFeedback.FireUpdate();
			PresetCountFeedback.FireUpdate();
			PanSpeedFeedback.FireUpdate();
			TiltSpeedFeedback.FireUpdate();
			ZoomSpeedFeedback.FireUpdate();

			foreach (var item in PresetNameFeedbacks)
				item.Value.FireUpdate();
		}

		#endregion

		private void socket_ConnectionChange(object sender, GenericSocketStatusChageEventArgs args)
		{
			Debug.Console(2, this, args.Client.ClientStatus.ToString());

			if (ConnectFeedback != null)
				ConnectFeedback.FireUpdate();

			if (StatusFeedback != null)
				StatusFeedback.FireUpdate();
		}


		/// <summary>
		/// Send bytes to device
		/// </summary>
		/// <param name="bytes"></param>
		public void SendBytes(byte[] bytes)
		{
			if (bytes == null) return;

			if (!_comms.IsConnected)
				_comms.Connect();

			_comms.SendBytes(bytes);
		}

		private void Handle_BytesRecieved(object sender, GenericCommMethodReceiveBytesArgs args)
		{
			if (args == null || args.Bytes == null)
			{
				Debug.Console(2, this, "Handle_BytesRecieved args or args.Bytes is null");
				return;
			}

			Debug.Console(2, this, "Handle_BytesRecieved: {0}", args.Bytes);

			var byteBuffer = new byte[_commsByteBuffer.Length + args.Bytes.Length];
			_commsByteBuffer.CopyTo(byteBuffer, 0);
			args.Bytes.CopyTo(byteBuffer, _commsByteBuffer.Length);

			Debug.Console(2, this, "Handle_BytesRecieved byteBuffer: {0}", ComTextHelper.GetEscapedText(byteBuffer));

			// TODO [ ] complete method

			// power on:	0xy0, 0x50, 0x02, 0xFF
			// power off:	0xy0, 0x50, 0x03, 0xFF

			// focus auto:		0xy0, 0x50, 0x02, 0xFF	??? same as power on in document
			// focus manual:	0xy0, 0x50, 0x03, 0xFF	??? same as power off in document

			// from Sony SRG-300SE IP Visco Processor v1.0
			//CHANGE From_Device
			//{
			//    if(left(From_Device, 7) = "\x02\x00\x00\x02\x00\x00\x00" && right(From_Device, 2) = "\x0F\x01")	// if camera didn't like the sequence number in the command that was sent
			//    {
			//        iCounter = 0xFFFFFFFF; // reset the sequence number to it's highest value
			//        makestring(sCommand, "\x01\x00\x00%s%s%s%s%s%s", chr(len(sStringToSend)), chr(iCounter {{ 8), chr(iCounter {{ 16), chr(iCounter {{ 24), chr(iCounter {{ 32),  sStringToSend);
			//        // generate command w/ new sequence number
			//        To_Device = sCommand;  // resend command
			//    }
			//    else if(left(From_Device, 2) = "\x01\x11" && right(From_Device, 1) = "\xFF")	// if valid response
			//    {
			//        Response = mid(From_Device, 9, byte(From_Device, 4));
			//    }
			//}

			// save partial message here
			//_commsByteBuffer = byteBuffer;
		}

		/// <summary>
		/// Poll 
		/// </summary>
		public void Poll()
		{
			byte[] cmd;

			// power inquiry ? [serial cmd] : [ip cmd]			
			// TODO [ ] Replace serial VISCA commands with VISCA over IP commands

			// from Sony SRG-300SE IP v1.1.umc
			// S-2.3 : Serial I/O > String_To_Send
			// Power_On_B:		"\x8\[#Address (1-7)\]\x01\x04\x00\x02\xFF"
			// Power_Off_B:		"\x8\[#Address (1-7)\]\x01\x04\x00\x03\xFF"

			// from Sony SRG-300SE IP Visco Processor v1.0
			//CHANGE String_To_Send
			//{
			//    sStringToSend = String_To_Send;
			//    if(iCounter = 0xFFFFFFFF)
			//        iCounter = 0;
			//    else
			//        iCounter = iCounter + 1;
			//    makestring(sCommand, "\x01\x00\x00%s%s%s%s%s%s", chr(len(sStringToSend)), chr(iCounter {{ 8), chr(iCounter {{ 16), chr(iCounter {{ 24), chr(iCounter {{ 32),  sStringToSend);
			//    // generate command
			//    To_Device = sCommand;
			//}

			 
			cmd = _commsIsSerial 
				? new byte[] { _address, 0x09, 0x04, 0x00, 0xFF } 
				: new byte[] { _address };
			SendBytes(cmd);

			if (!Power) return;

			// focus mode inquiry ? [serial cmd] : [ip cmd]
			// TODO [ ] Replace serial VISCA commands with VISCA over IP commands
			cmd = _commsIsSerial 
				? new byte[] { _address, 0x09, 0x04, 0x38, 0xFF } 
				: new byte[] { _address };
			SendBytes(cmd);
		}

		/// <summary>
		/// Set power state
		/// </summary>
		/// <param name="state">power on/off</param>
		public void SetPower(bool state)
		{
			byte[] cmd;

			// VISCA serial command
			if (_commsIsSerial)
			{
				// Power ? [send off] : [send on]
				cmd = Power
					? new byte[] { _address, 0x01, 0x04, 0x00, 0x03, 0xFF }
					: new byte[] { _address, 0x01, 0x04, 0x00, 0x02, 0xFF };

				SendBytes(cmd);
			}
			// VISCA over IP command
			// TODO [ ] Replace serial VISCA commands with VISCA over IP commands
			else
			{
				// Power ? [send off] : [send on]
				cmd = Power
					? new byte[] { _address }
					: new byte[] { _address };

				SendBytes(cmd);
			}
		}

		/// <summary>
		/// Move camera
		/// </summary>
		/// <param name="state">sig action true/false</param>
		/// <param name="direction">EMoveDirection direction</param>
		public void Move(bool state, EDirection direction)
		{
			if (_commsIsSerial)
				MoveSerial(state, direction);
			else
				MoveIp(state, direction);
		}

		// VISCA serial commands
		private void MoveSerial(bool state, EDirection direction)
		{
			byte[] cmd;

			switch (direction)
			{
				case EDirection.Home:
					{
						cmd = state
							? new byte[] { _address, 0x01, 0x06, 0x04, 0xFF }
							: null;
						SendBytes(cmd);
						break;
					}
				case EDirection.PanLeft:
					{
						// state ? [moving] : [stop]
						cmd = state
							? new byte[] { _address, 0x01, 0x06, 0x01, Convert.ToByte(PanSpeed), Convert.ToByte(TiltSpeed), 0x01, 0x03, 0xFF }
							: new byte[] { _address, 0x01, 0x06, 0x01, Convert.ToByte(PanSpeed), Convert.ToByte(TiltSpeed), 0x03, 0x03, 0xFF };
						SendBytes(cmd);
						break;
					}
				case EDirection.PanRight:
					{
						// state ? [moving] : [stop]
						cmd = state
							? new byte[] { _address, 0x01, 0x06, 0x01, Convert.ToByte(PanSpeed), Convert.ToByte(TiltSpeed), 0x02, 0x03, 0xFF }
							: new byte[] { _address, 0x01, 0x06, 0x01, Convert.ToByte(PanSpeed), Convert.ToByte(TiltSpeed), 0x03, 0x03, 0xFF };
						SendBytes(cmd);
						break;
					}
				case EDirection.TiltUp:
					{
						// state ? [moving] : [stop]
						cmd = state
							? new byte[] { _address, 0x01, 0x06, 0x01, Convert.ToByte(PanSpeed), Convert.ToByte(TiltSpeed), 0x03, 0x01, 0xFF }
							: new byte[] { _address, 0x01, 0x06, 0x01, Convert.ToByte(PanSpeed), Convert.ToByte(TiltSpeed), 0x03, 0x03, 0xFF };
						SendBytes(cmd);
						break;
					}
				case EDirection.TiltDown:
					{
						// state ? [moving] : [stop]
						cmd = state
							? new byte[] { _address, 0x01, 0x06, 0x01, Convert.ToByte(PanSpeed), Convert.ToByte(TiltSpeed), 0x03, 0x02, 0xFF }
							: new byte[] { _address, 0x01, 0x06, 0x01, Convert.ToByte(PanSpeed), Convert.ToByte(TiltSpeed), 0x03, 0x03, 0xFF };
						SendBytes(cmd);
						break;
					}
				case EDirection.ZoomIn:
					{
						// state ? [moving] : [stop]
						cmd = state
							? new byte[] { _address, 0x01, 0x04, 0x07, Convert.ToByte(0x30 + ZoomSpeed), 0xFF }
							: new byte[] { _address, 0x01, 0x04, 0x07, 0x00, 0xFF };
						SendBytes(cmd);
						break;
					}
				case EDirection.ZoomOut:
					{
						// state ? [moving] : [stop]
						cmd = state
							? new byte[] { _address, 0x01, 0x04, 0x07, Convert.ToByte(0x20 + ZoomSpeed), 0xFF }
							: new byte[] { _address, 0x01, 0x04, 0x07, 0x00, 0xFF };
						SendBytes(cmd);
						break;
					}
				case EDirection.FocusAuto:
					{
						// state ? [moving] : [stop]
						cmd = state
							? new byte[] { _address, 0x01, 0x04, 0x38, 0x03, 0xFF }
							: new byte[] { _address, 0x01, 0x04, 0x38, 0x02, 0xFF };
						SendBytes(cmd);
						break;
					}
				case EDirection.FocusNear:
					{
						// state ? [moving] : [stop]
						cmd = state
							? new byte[] { _address, 0x01, 0x04, 0x08, Convert.ToByte(0x30 + FocusSpeed), 0xFF }
							: new byte[] { _address, 0x01, 0x04, 0x08, 0x02, 0xFF };
						SendBytes(cmd);
						break;
					}
				case EDirection.FocusFar:
					{
						// state ? [moving] : [stop]
						cmd = state
							? new byte[] { _address, 0x01, 0x04, 0x08, Convert.ToByte(0x20 + FocusSpeed), 0xFF }
							: new byte[] { _address, 0x01, 0x04, 0x08, 0x02, 0xFF };
						SendBytes(cmd);
						break;
					}
				case EDirection.PrivacyOn:
					{
						if (_privacyOnPreset == 0) return;
						RecallPreset(_privacyOnPreset);
						break;
					}
				case EDirection.PrivacyOff:
					{
						if (_privacyOffPreset == 0) return;
						RecallPreset(_privacyOffPreset);
						break;
					}
			}
		}

		// VISCA over IP commands
		private void MoveIp(bool state, EDirection direction)
		{
			byte[] cmd;

			// TODO [ ] Replace serial VISCA commands with VISCA over IP commands
			switch (direction)
			{
				case EDirection.Home:
					{
						cmd = state
							? new byte[] { _address, 0x01, 0x06, 0x04, 0xFF }
							: null;
						SendBytes(cmd);
						break;
					}
				case EDirection.PanLeft:
					{
						// state ? [moving] : [stop]
						cmd = state
							? new byte[] { _address, 0x01, 0x06, 0x01, Convert.ToByte(PanSpeed), Convert.ToByte(TiltSpeed), 0x01, 0x03, 0xFF }
							: new byte[] { _address, 0x01, 0x06, 0x01, Convert.ToByte(PanSpeed), Convert.ToByte(TiltSpeed), 0x03, 0x03, 0xFF };
						SendBytes(cmd);
						break;
					}
				case EDirection.PanRight:
					{
						// state ? [moving] : [stop]
						cmd = state
							? new byte[] { _address, 0x01, 0x06, 0x01, Convert.ToByte(PanSpeed), Convert.ToByte(TiltSpeed), 0x02, 0x03, 0xFF }
							: new byte[] { _address, 0x01, 0x06, 0x01, Convert.ToByte(PanSpeed), Convert.ToByte(TiltSpeed), 0x03, 0x03, 0xFF };
						SendBytes(cmd);
						break;
					}
				case EDirection.TiltUp:
					{
						// state ? [moving] : [stop]
						cmd = state
							? new byte[] { _address, 0x01, 0x06, 0x01, Convert.ToByte(PanSpeed), Convert.ToByte(TiltSpeed), 0x03, 0x01, 0xFF }
							: new byte[] { _address, 0x01, 0x06, 0x01, Convert.ToByte(PanSpeed), Convert.ToByte(TiltSpeed), 0x03, 0x03, 0xFF };
						SendBytes(cmd);
						break;
					}
				case EDirection.TiltDown:
					{
						// state ? [moving] : [stop]
						cmd = state
							? new byte[] { _address, 0x01, 0x06, 0x01, Convert.ToByte(PanSpeed), Convert.ToByte(TiltSpeed), 0x03, 0x02, 0xFF }
							: new byte[] { _address, 0x01, 0x06, 0x01, Convert.ToByte(PanSpeed), Convert.ToByte(TiltSpeed), 0x03, 0x03, 0xFF };
						SendBytes(cmd);
						break;
					}
				case EDirection.ZoomIn:
					{
						// state ? [moving] : [stop]
						cmd = state
							? new byte[] { _address, 0x01, 0x04, 0x07, Convert.ToByte(0x30 + ZoomSpeed), 0xFF }
							: new byte[] { _address, 0x01, 0x04, 0x07, 0x00, 0xFF };
						SendBytes(cmd);
						break;
					}
				case EDirection.ZoomOut:
					{
						// state ? [moving] : [stop]
						cmd = state
							? new byte[] { _address, 0x01, 0x04, 0x07, Convert.ToByte(0x20 + ZoomSpeed), 0xFF }
							: new byte[] { _address, 0x01, 0x04, 0x07, 0x00, 0xFF };
						SendBytes(cmd);
						break;
					}
				case EDirection.FocusAuto:
					{
						// state ? [moving] : [stop]
						cmd = state
							? new byte[] { _address, 0x01, 0x04, 0x38, 0x03, 0xFF }
							: new byte[] { _address, 0x01, 0x04, 0x38, 0x02, 0xFF };
						SendBytes(cmd);
						break;
					}
				case EDirection.FocusNear:
					{
						// state ? [moving] : [stop]
						cmd = state
							? new byte[] { _address, 0x01, 0x04, 0x08, Convert.ToByte(0x30 + FocusSpeed), 0xFF }
							: new byte[] { _address, 0x01, 0x04, 0x08, 0x02, 0xFF };
						SendBytes(cmd);
						break;
					}
				case EDirection.FocusFar:
					{
						// state ? [moving] : [stop]
						cmd = state
							? new byte[] { _address, 0x01, 0x04, 0x08, Convert.ToByte(0x20 + FocusSpeed), 0xFF }
							: new byte[] { _address, 0x01, 0x04, 0x08, 0x02, 0xFF };
						SendBytes(cmd);
						break;
					}
				case EDirection.PrivacyOn:
					{
						if (_privacyOnPreset == 0) return;
						RecallPreset(_privacyOnPreset);
						break;
					}
				case EDirection.PrivacyOff:
					{
						if (_privacyOffPreset == 0) return;
						RecallPreset(_privacyOffPreset);
						break;
					}
			}
		}

		/// <summary>
		/// Recall preset
		/// </summary>
		/// <param name="value">preset 1...16</param>
		public void RecallPreset(uint value)
		{
			if (value <= 0)
				return;

			byte[] cmd;

			// recall preset ? [serial cmd] : [ip cmd]
			// TODO [ ] Replace serial VISCA commands with VISCA over IP commands
			cmd = _commsIsSerial 
				? new byte[] { _address, 0x01, 0x04, 0x3F, 0x02, Convert.ToByte(value), 0xFF } 
				: new byte[] { _address };

			SendBytes(cmd);
		}

		/// <summary>
		/// Save preset
		/// </summary>
		/// <param name="value">preset 1...16</param>
		public void SavePreset(uint value)
		{
			if (value <= 0)
				return;

			byte[] cmd;

			// save preset ? [serial cmd] : [ip cmd]
			// TODO [ ] Replace serial VISCA commands with VISCA over IP commands
			cmd = _commsIsSerial 
				? new byte[] { _address, 0x01, 0x04, 0x3F, 0x00, Convert.ToByte(value), 0xFF } 
				: new byte[] { _address };

			SendBytes(cmd);
		}

		/// <summary>
		/// Sets the pan speed of the camera
		/// </summary>
		/// <param name="value">00...18(hex)</param>
		public void SetPanSpeed(uint value)
		{
			PanSpeed = value > PanSpeedMax ? PanSpeedDefault : value;
		}

		/// <summary>
		/// Sets the tilt speed of the camera
		/// </summary>
		/// <param name="value">00...18 (hex)</param>
		public void SetTiltSpeed(uint value)
		{
			TiltSpeed = value > TiltSpeedMax ? TiltSpeedDefault : value;
		}

		/// <summary>
		/// Sets the zoom speed of the camera
		/// </summary>
		/// <param name="value">00...07 (hex)</param>
		public void SetZoomSpeed(uint value)
		{
			ZoomSpeed = value > ZoomSpeedMax ? ZoomSpeedDefault : value;
		}

		/// <summary>
		/// Sets the focus speed of the camera
		/// </summary>
		/// <param name="value">00...07 (hex)</param>
		public void SetFocusSpeed(uint value)
		{
			FocusSpeed = value > FocusSpeedMax ? FocusSpeedDefault : value;
		}
	}
}

