using System;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronSockets;
using PepperDash.Core;
using PepperDash.Core.Logging;

namespace ViscaCameraPlugin
{
	/// <summary>
	/// UDP transport for VISCA over IP.
	/// </summary>
	/// <remarks>
	/// Cameras that follow Sony's VISCA-over-IP scheme all listen on one well known port (52381),
	/// and reply to whatever source port the datagram arrived from. Essentials' own UDP comms use a
	/// single port number for both the local socket and the destination, so the second camera on
	/// 52381 cannot bind and is silently dead. This binds an ephemeral local port instead, which
	/// lets any number of cameras share the destination port.
	/// </remarks>
	public class ViscaUdpTransport : Device, ISocketStatus
	{
		/// <summary>Let the stack pick the local port - see the class remarks.</summary>
		private const int EphemeralLocalPort = 0;

		private const int DefaultBufferSize = 2000;

		/// <summary>ISO 8859-1: maps every byte to the character of the same value, and back.</summary>
		private static readonly Encoding ByteEncoding = Encoding.GetEncoding(28591);

		private readonly string _hostname;
		private readonly int _remotePort;
		private readonly int _bufferSize;

		private UDPServer _server;
		private bool _isConnected;

		public event EventHandler<GenericCommMethodReceiveBytesArgs> BytesReceived;
		public event EventHandler<GenericCommMethodReceiveTextArgs> TextReceived;
		public event EventHandler<GenericSocketStatusChageEventArgs> ConnectionChange;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="key">device key</param>
		/// <param name="hostname">camera address</param>
		/// <param name="remotePort">port the camera listens on</param>
		/// <param name="bufferSize">receive buffer size, 0 for the default</param>
		public ViscaUdpTransport(string key, string hostname, int remotePort, int bufferSize)
			: base(key)
		{
			_hostname = hostname;
			_remotePort = remotePort;
			_bufferSize = bufferSize > 0 ? bufferSize : DefaultBufferSize;

			CrestronEnvironment.ProgramStatusEventHandler += programEventType =>
			{
				if (programEventType == eProgramStatusEventType.Stopping)
					Disconnect();
			};

			CrestronEnvironment.EthernetEventHandler += ethernetEventArgs =>
			{
				if (ethernetEventArgs.EthernetEventType == eEthernetEventType.LinkUp && _isConnected)
					Connect();
			};
		}

		public bool IsConnected
		{
			get { return _isConnected; }
		}

		public SocketStatus ClientStatus
		{
			get { return _server == null ? SocketStatus.SOCKET_STATUS_NO_CONNECT : _server.ServerStatus; }
		}

		public void Connect()
		{
			if (string.IsNullOrEmpty(_hostname))
			{
				this.LogWarning("No address configured, cannot open the UDP socket");
				return;
			}

			if (_remotePort < 1 || _remotePort > 65535)
			{
				this.LogWarning("Port {port} is not a valid port number, cannot open the UDP socket", _remotePort);
				return;
			}

			if (_server == null)
				_server = new UDPServer(_hostname, _remotePort, _bufferSize);

			var status = _server.EnableUDPServer(_hostname, EphemeralLocalPort, _remotePort);

			this.LogDebug("Opening UDP socket to {hostname}:{port} returned {status}", _hostname, _remotePort, status);

			SetConnected(status == SocketErrorCodes.SOCKET_OK);

			if (!_isConnected)
			{
				this.LogWarning("Failed to open the UDP socket to {hostname}:{port}: {status}", _hostname, _remotePort, status);
				return;
			}

			_server.ReceiveDataAsync(Receive);
		}

		public void Disconnect()
		{
			if (_server != null)
				_server.DisableUDPServer();

			SetConnected(false);
		}

		public void SendText(string text)
		{
			if (string.IsNullOrEmpty(text))
				return;

			SendBytes(ByteEncoding.GetBytes(text));
		}

		public void SendBytes(byte[] bytes)
		{
			if (bytes == null || bytes.Length == 0)
				return;

			if (!_isConnected || _server == null)
			{
				this.LogDebug("Not connected, dropping {length} bytes", bytes.Length);
				return;
			}

			var status = _server.SendData(bytes, bytes.Length);

			if (status != SocketErrorCodes.SOCKET_OK)
				this.LogWarning("Sending {length} bytes to {hostname}:{port} returned {status}", bytes.Length, _hostname, _remotePort, status);
		}

		private void Receive(UDPServer server, int numBytes)
		{
			try
			{
				if (numBytes <= 0)
					return;

				var bytes = server.IncomingDataBuffer.Take(numBytes).ToArray();

				var bytesHandler = BytesReceived;
				if (bytesHandler != null)
					bytesHandler(this, new GenericCommMethodReceiveBytesArgs(bytes));

				var textHandler = TextReceived;
				if (textHandler != null)
					textHandler(this, new GenericCommMethodReceiveTextArgs(ByteEncoding.GetString(bytes, 0, bytes.Length)));
			}
			catch (Exception ex)
			{
				this.LogError(ex, "Error handling {length} received bytes", numBytes);
			}
			finally
			{
				// keep listening, whatever this datagram turned out to be
				server.ReceiveDataAsync(Receive);
			}
		}

		private void SetConnected(bool isConnected)
		{
			if (_isConnected == isConnected)
				return;

			_isConnected = isConnected;

			var handler = ConnectionChange;
			if (handler != null)
				handler(this, new GenericSocketStatusChageEventArgs(this));
		}
	}
}
