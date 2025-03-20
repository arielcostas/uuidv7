using System;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

// ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator

namespace Costasdev.Uuidv7
{
    /// <summary>
    /// Represents a Universally Unique Identifier (UUID) with version 7, as defined in RFC 9562
    /// </summary>
    [Serializable]
    public struct Uuid7 : IEquatable<Uuid7>, IComparable<Uuid7>
    {
        /// <summary>
        /// Little-endian timestamp in milliseconds since Unix epoch
        /// </summary>
        private ulong _timePart;

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
        public Uuid7(ulong timePart, byte[] randomPart)
        {
            if (randomPart.Length != 10)
            {
                throw new ArgumentOutOfRangeException(nameof(_randomPart), "Random part must be 10 bytes long");
            }

            if (randomPart[0] >> 4 != 7)
            {
                throw new ArgumentOutOfRangeException(nameof(_randomPart), "Version number must be 7");
            }

            // Check variant is 8, 9, A or B
            if (randomPart[2] >> 6 != 2)
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
                (ulong)dateTimeOffset.ToUnixTimeMilliseconds(),
                randomBytes
            );
        }

#if NET8_0_OR_GREATER
        /// <summary>
        /// Parses a UUIDv7 from a string, with or without hyphens
        /// </summary>
        /// <param name="uuid">The UUID string to parse</param>
        /// <returns>A new UUID</returns>
        /// <exception cref="ArgumentException">If the UUID is not provided</exception>
        /// <exception cref="FormatException">If the UUID is not in the correct format</exception>
        /// <exception cref="ArgumentOutOfRangeException">If the version number or variant number is incorrect</exception>
        public static Uuid7 Parse(string uuid)
        {
            if (string.IsNullOrWhiteSpace(uuid))
            {
                throw new ArgumentException(nameof(uuid));
            }

            Span<char> chars = stackalloc char[32];

            // Remove unwanted characters
            int i = 0;
            foreach (char c in uuid)
            {
                if (c is ' ' or '-')
                {
                    continue;
                }

                chars[i] = c;
                i++;
            }

            if (i != 32)
            {
                throw new FormatException($"UUID has invalid size of {i}");
            }

            // Parse the 32 hex chars into 16 bytes
            Span<byte> bytes = stackalloc byte[16];
            for (i = 0; i < 32; i += 2)
            {
                bytes[i / 2] = ParseHexByte(chars[i], chars[i+1]);
            }

            var millisLow = MemoryMarshal.Read<int>(bytes[2..]);
            var millisHigh = MemoryMarshal.Read<short>(bytes);

            if (BitConverter.IsLittleEndian)
            {
                millisLow = IPAddress.NetworkToHostOrder(millisLow);
                millisHigh = IPAddress.NetworkToHostOrder(millisHigh);
            }

            var randomPart = bytes.Slice(6, 10).ToArray();
            return new Uuid7(
                (ulong)((long)millisHigh << 32) | (uint)millisLow,
                randomPart
            );
        }
#else
        /// <summary>
        /// Parses a UUIDv7 from a string, with or without hyphens
        /// </summary>
        /// <param name="uuid">The UUID string to parse</param>
        /// <returns>A new UUID</returns>
        /// <exception cref="ArgumentException">If the UUID is not provided</exception>
        /// <exception cref="FormatException">If the UUID is not in the correct format</exception>
        /// <exception cref="ArgumentOutOfRangeException">If the version number or variant number is incorrect</exception>
        public static Uuid7 Parse(string uuid)
        {
            if (uuid is null || uuid.Trim() == string.Empty)
            {
                throw new ArgumentException(nameof(uuid));
            }

            if (uuid.Length != 36 && uuid.Length != 32)
            {
                throw new FormatException("UUID must be 32 or 36 characters long");
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

            if (BitConverter.IsLittleEndian)
            {
                millisLow = IPAddress.NetworkToHostOrder(millisLow);
                millisHigh = IPAddress.NetworkToHostOrder(millisHigh);
            }

            var randomPart = new byte[10];
            Buffer.BlockCopy(bytes, 6, randomPart, 0, 10);

            return new Uuid7(
                (ulong)((long)millisHigh << 32) | (uint)millisLow,
                randomPart
            );
        }
#endif

        /// <summary>
        /// Attempts to parse a UUID from a string, with or without hyphens.
        /// </summary>
        /// <seealso cref="Parse(string)"/>
        /// <param name="uuid">The UUID string to parse</param>
        /// <param name="result">The UUID if parsing was successful</param>
        /// <returns>True if parsing was successful, false otherwise</returns>
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

        /// <summary>
        /// Gets the timestamp of the UUID as a long
        /// </summary>
        public long GetTimestamp()
        {
            return (long)_timePart;
        }

        /// <summary>
        /// Gets the timestamp of the UUID as a DateTimeOffset
        /// </summary>
        public DateTimeOffset GetDateTimeOffset()
        {
            return DateTimeOffset.FromUnixTimeMilliseconds((long)_timePart);
        }

        /// <summary>
        /// Compares the UUID to another UUID, returning a value indicating whether the current UUID is equal to the other UUID
        /// </summary>
        /// <param name="other">The UUID to compare to</param>
        /// <returns>True if the UUIDs are equal, false otherwise</returns>
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

        /// <summary>
        /// Returns the UUID as a 16-byte array
        /// </summary>
        /// <returns>A byte[] with size 16</returns>
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

        /// <summary>
        /// Returns the UUID as a string
        /// </summary>
        /// <returns>The UUID as a string</returns>
        /// <seealso cref="AsString(bool, bool)"/>
        public override string ToString()
        {
            return AsString();
        }

        /// <summary>
        /// Returns the UUID as a string
        /// </summary>
        /// <param name="uppercase">Whether to use uppercase hex characters. Default is false</param>
        /// <param name="includeHyphens">Whether to include hyphens in the UUID. Default is true</param>
        /// <returns>The UUID as a string with the specified options</returns>
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

        private static byte ParseHexByte(char high, char low)
        {
            int highNibble = (high >= '0' && high <= '9')
                ? high - '0'
                : (char.ToLowerInvariant(high) - 'a' + 10);

            int lowNibble = (low >= '0' && low <= '9')
                ? low - '0'
                : (char.ToLowerInvariant(low) - 'a' + 10);

            return (byte)((highNibble << 4) | lowNibble);
        }
    }
}
