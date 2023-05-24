using System.Collections.Generic;
using Newtonsoft.Json;
using PepperDash.Essentials.Core;

namespace ViscaCameraPlugin
{
	public class ViscaCameraConfig
	{
		[JsonProperty("control")]
		public EssentialsControlPropertiesConfig Control { get; set; }

		[JsonProperty("deviceId")]
		public long DeviceId { get; set; }

		[JsonProperty("enabled")]
		public bool Enabled { get; set; }

		[JsonProperty("address")]
		public uint Address { get; set; }

		[JsonProperty("panSpeed")]
		public uint PanSpeed { get; set; }

		[JsonProperty("tiltSpeed")]
		public uint TiltSpeed { get; set; }

		[JsonProperty("ZoomSpeed")]
		public uint ZoomSpeed { get; set; }

		[JsonProperty("FocusSpeed")]
		public uint FocusSpeed { get; set; }

		[JsonProperty("PrivacyOnPreset")]
		public uint PrivacyOnPreset { get; set; }

		[JsonProperty("PrivacyOffPreset")]
		public uint PrivacyOffPreset { get; set; }

		[JsonProperty("pollTimeMs")]
		public long PollTimeMs { get; set; }

		[JsonProperty("warningTimeoutMs")]
		public long WarningTimeoutMs { get; set; }

		[JsonProperty("errorTimeoutMs")]
		public long ErrorTimeoutMs { get; set; }

		[JsonProperty("presets")]
		public Dictionary<uint, ViscaCameraPresetConfig> Presets { get; set; }

		public ViscaCameraConfig()
		{
			Presets = new Dictionary<uint, ViscaCameraPresetConfig>();
		}
	}

	public class ViscaCameraPresetConfig
	{
		[JsonProperty("name")]
		public string Name { get; set; }

        [JsonProperty("index")]
        public uint Index { get; set; }


	}
}