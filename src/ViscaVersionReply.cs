namespace ViscaCameraPlugin
{
	/// <summary>
	/// The VISCA version inquiry (CAM_VersionInq) and its reply.
	/// </summary>
	/// <remarks>
	/// The inquiry is <c>8x 09 00 02 FF</c>; the reply is <c>y0 50 GG GG HH HH JJ JJ KK FF</c> - vendor
	/// ID, model ID, ROM revision, maximum socket count. A VISCA reply does not say which inquiry it
	/// answers, so this one is told apart from the power reply (<c>y0 50 0p FF</c>) by its length.
	///
	/// Free of Crestron types so it can be tested on its own.
	/// </remarks>
	public static class ViscaVersionReply
	{
		/// <summary>The version inquiry for the camera at this VISCA address byte (0x81 for camera 1).</summary>
		public static byte[] Inquiry(byte address)
		{
			return new byte[] { address, 0x09, 0x00, 0x02, 0xFF };
		}

		/// <summary>
		/// True when the payload is a version reply, with or without its closing 0xFF.
		/// </summary>
		public static bool TryParse(byte[] payload, out ushort vendor, out ushort model, out ushort rom)
		{
			vendor = 0;
			model = 0;
			rom = 0;

			if (payload == null)
			{
				return false;
			}

			var length = payload.Length;
			if (length > 0 && payload[length - 1] == 0xFF)
			{
				length--;
			}

			// y0 is 0x90 to 0xF0: the replying camera's address plus eight, low nibble zero.
			if (length != 9 || payload[0] < 0x90 || (payload[0] & 0x0F) != 0 || payload[1] != 0x50)
			{
				return false;
			}

			vendor = (ushort)((payload[2] << 8) | payload[3]);
			model = (ushort)((payload[4] << 8) | payload[5]);
			rom = (ushort)((payload[6] << 8) | payload[7]);
			return true;
		}

		/// <summary>
		/// The reply as a firmware string: the ROM revision first, since that is what changes with a
		/// firmware update, then the vendor and model codes that say which camera it belongs to.
		/// </summary>
		public static string Describe(ushort vendor, ushort model, ushort rom)
		{
			return string.Format("ROM {0:X4} (vendor {1:X4}, model {2:X4})", rom, vendor, model);
		}
	}
}
