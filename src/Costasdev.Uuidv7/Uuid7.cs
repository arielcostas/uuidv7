using System;
using System.Net;
using System.Security.Cryptography;

namespace Costasdev.Uuidv7
{
    public struct Uuid7
    {
        /// <summary>
        /// Little-endian timestamp in milliseconds since Unix epoch
        /// </summary>
        private long _timePart;

        /// <summary>
        /// The 10 bytes containing version number, variant and random data
        /// </summary>
        private byte[] _randomPart;

        /// <summary>
        /// Creates a new UUID with the specified time and random parts
        /// </summary>
        /// <param name="timePart">The timestamp in milliseconds since Unix epoch</param>
        /// <param name="randomPart">A 10 byte array containing the version number, variant and random data</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private Uuid7(long timePart, byte[] randomPart)
        {
            if (timePart < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(_timePart), "Time part must be a non-negative number");
            }

            if (randomPart.Length != 10)
            {
                throw new ArgumentOutOfRangeException(nameof(_randomPart), "Random part must be 10 bytes long");
            }
            
            if (randomPart[0] >> 4 != 7)
            {
                throw new ArgumentOutOfRangeException(nameof(_randomPart), "Version number must be 7");
            }
            
            // Check variant is 8, 9, A or B
            if ((randomPart[2] >> 6) != 2)
            {
                throw new ArgumentOutOfRangeException(nameof(_randomPart), "Variant number must be 10");
            }
            
            _timePart = timePart;
            _randomPart = randomPart;
        }

        /// <summary>
        /// Generates a new UUID with the current DateTime in UTC
        /// </summary>
        /// <returns>A new UUID</returns>
        public static Uuid7 NewUuid()
        {
            return NewUuid(DateTimeOffset.UtcNow);
        }

        /// <summary>
        /// Generates a new UUID with the specified DateTime
        /// </summary>
        /// <param name="dateTime">The DateTime to use as the timestamp. It will be converted to UTC</param>
        /// <returns></returns>
        public static Uuid7 NewUuid(DateTime dateTime)
        {
            return NewUuid(new DateTimeOffset(dateTime.ToUniversalTime()));
        }

        /// <summary>
        /// Generates a new UUID with the specified DateTimeOffset
        /// </summary>
        /// <param name="dateTimeOffset">The DateTimeOffset to use as the timestamp. It will be converted to UTC</param>
        /// <returns>A new UUID</returns>
        public static Uuid7 NewUuid(DateTimeOffset dateTimeOffset)
        {
            // Generate a random 16 byte array
            var randomBytes = new byte[10];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }

            // Overwrite bits 48-51 (4 bits) with the version number (7) 
            randomBytes[0] = (byte)((randomBytes[6] & 0x0F) | 0x70);

            // Overwrite bits 64-65 (2 bits) with the variant (10)
            randomBytes[2] = (byte)((randomBytes[8] & 0x3F) | 0x80);

            return new Uuid7(
                dateTimeOffset.ToUnixTimeMilliseconds(),
                randomBytes
            );
        }

        /// <summary>
        /// Converts the UUID to a string representation
        /// </summary>
        /// <returns>The UUID as a string in the format xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx</returns>
        public override string ToString()
        {
            var uuid = new byte[16];

            // Invert the byte order if the system is little-endian (most significant byte first)
            var millisLow = (int)_timePart;
            var millisHigh = (short)(_timePart >> 32);

            if (BitConverter.IsLittleEndian)
            {
                millisLow = IPAddress.HostToNetworkOrder(millisLow);
                millisHigh = IPAddress.HostToNetworkOrder(millisHigh);
            }

            Buffer.BlockCopy(BitConverter.GetBytes(millisHigh), 0, uuid, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(millisLow), 0, uuid, 2, 4);
            Buffer.BlockCopy(_randomPart, 0, uuid, 6, 10);

            var hex = BitConverter.ToString(uuid).Replace("-", "");
            return
                $"{hex.Substring(0, 8)}-{hex.Substring(8, 4)}-{hex.Substring(12, 4)}-{hex.Substring(16, 4)}-{hex.Substring(20)}";
        }
    }
}