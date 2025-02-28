using System.Collections.Generic;
using Newtonsoft.Json;
using PepperDash.Essentials.Core;

namespace ViscaCameraPlugin
{
	public class Camera
	{
		[JsonProperty("control")]
		public EssentialsControlPropertiesConfig Control { get; set; }

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
		public int PollTimeMs { get; set; }

		[JsonProperty("presets")]
		public List<Preset> Presets { get; set; }

		public Camera()
		{
			Presets = new List<Preset>();
		}
	}

	public class Preset
	{
		[JsonProperty("name")]
		public string Name { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }


	}
}