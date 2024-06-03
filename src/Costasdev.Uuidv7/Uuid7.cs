using System;
using System.Net;
using System.Security.Cryptography;

namespace Costasdev.Uuidv7
{
	public struct Uuid7
	{
		private byte[] _uuid;
		
		private Uuid7(byte[] uuid)
		{
			if (uuid.Length != 16)
			{
				throw new ArgumentException("UUID must be 16 bytes long");
			}
			_uuid = uuid;
		}
		
		public static Uuid7 NewUuid()
		{
			// Generate a random 16 byte array
			var uuid = new byte[16];
			using (var rng = RandomNumberGenerator.Create())
			{	
				rng.GetBytes(uuid);
			}

			// Overwrite the first 48 bits with the current time in milliseconds since Unix epoch
			var millis = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

			// Invert the byte order if the system is little-endian (most significant byte first)
			var millisLow = (int)millis;
			var millisHigh = (short)(millis >> 32);

			if (BitConverter.IsLittleEndian)
			{
				millisLow = IPAddress.HostToNetworkOrder(millisLow);
				millisHigh = IPAddress.HostToNetworkOrder(millisHigh);
			}

			Buffer.BlockCopy(BitConverter.GetBytes(millisHigh), 0, uuid, 0, 2);
			Buffer.BlockCopy(BitConverter.GetBytes(millisLow), 0, uuid, 2, 4);

			// Overwrite bits 48-51 (4 bits) with the version number (7) 
			uuid[6] = (byte)((uuid[6] & 0x0F) | 0x70);

			// Overwrite bits 64-65 (2 bits) with the variant (10)
			uuid[8] = (byte)((uuid[8] & 0x3F) | 0x80);

			return new Uuid7(uuid);
		}

		public override string ToString()
		{
			var hex = BitConverter.ToString(_uuid).Replace("-", "");
			return $"{hex.Substring(0, 8)}-{hex.Substring(8, 4)}-{hex.Substring(12, 4)}-{hex.Substring(16, 4)}-{hex.Substring(20)}";
		}
		
	}
}