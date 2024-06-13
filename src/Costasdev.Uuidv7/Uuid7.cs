using System;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Costasdev.Uuidv7
{
    [Serializable]
    public struct Uuid7 : IEquatable<Uuid7>, IComparable<Uuid7>
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

        public static Uuid7 Parse(string uuid)
        {
            if (uuid is null || uuid.Trim() == string.Empty)
            {
                throw new ArgumentException(nameof(uuid));
            }

            if (uuid.Length != 36 || uuid.Length != 32)
            {
                throw new FormatException("UUID must be 36 characters long");
            }

            uuid = uuid.Trim().ToLowerInvariant();

            // Support UUIDs with or without hyphens
            if (uuid.Length == 36)
            {
                var parts = uuid.Split('-');
                if (parts.Length != 5)
                {
                    throw new FormatException("UUID must contain 5 parts separated by hyphens");
                }

                uuid = string.Concat(parts);
            }

            var bytes = new byte[16];
            for (var i = 0; i < 16; i++)
            {
                bytes[i] = byte.Parse(uuid.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber);
            }

            // Invert the byte order if the system is little-endian (most significant byte first)
            var millisLow = BitConverter.ToInt32(bytes, 2);
            var millisHigh = BitConverter.ToInt16(bytes, 0);

            var randomPart = new byte[10];
            Buffer.BlockCopy(bytes, 6, randomPart, 0, 10);

            if (BitConverter.IsLittleEndian)
            {
                millisLow = IPAddress.NetworkToHostOrder(millisLow);
                millisHigh = IPAddress.NetworkToHostOrder(millisHigh);
            }

            return new Uuid7(
                ((long)millisHigh << 32) | (uint)millisLow,
                randomPart
            );
        }

        public static bool TryParse(string uuid, out Uuid7 result)
        {
            try
            {
                result = Parse(uuid);
                return true;
            }
            catch (Exception)
            {
                result = default;
                return false;
            }
        }

        public DateTimeOffset GetDateTimeOffset()
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(_timePart);
        }

        public bool Equals(Uuid7 other)
        {
            return _timePart == other._timePart && _randomPart.SequenceEqual(other._randomPart);
        }

        /// <summary>
        /// Compares the UUID to another UUID, returning a value indicating whether the current UUID's timestamp is less than, equal to or greater than the other UUID's timestamp
        /// </summary>
        /// <param name="other">The UUID to compare to</param>
        /// <returns>-1 if the current UUID was generated before the other UUID, 0 if they were generated at the same time, 1 if the current UUID was generated after the other UUID</returns>
        public int CompareTo(Uuid7 other)
        {
            return _timePart.CompareTo(other._timePart);
        }

        public byte[] AsByteArray()
        {
            var result = new byte[16];

            // Invert the byte order if the system is little-endian (most significant byte first)
            var millisLow = (int)_timePart;
            var millisHigh = (short)(_timePart >> 32);

            if (BitConverter.IsLittleEndian)
            {
                millisLow = IPAddress.HostToNetworkOrder(millisLow);
                millisHigh = IPAddress.HostToNetworkOrder(millisHigh);
            }

            Buffer.BlockCopy(BitConverter.GetBytes(millisHigh), 0, result, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(millisLow), 0, result, 2, 4);
            Buffer.BlockCopy(_randomPart, 0, result, 6, 10);
            
            return result;
        }
        
        public override string ToString()
        {
            return AsString();
        }

        public string AsString(bool uppercase = false, bool includeHyphens = true)
        {
            var hex = BitConverter.ToString(AsByteArray()).Replace("-", "");
            if (!uppercase)
            {
                hex = hex.ToLowerInvariant();
            }

            if (!includeHyphens) return hex;
            
            var builder = new StringBuilder(hex).Insert(20, '-').Insert(16, '-').Insert(12, '-').Insert(8, '-');
            return builder.ToString();
        }
    }
}