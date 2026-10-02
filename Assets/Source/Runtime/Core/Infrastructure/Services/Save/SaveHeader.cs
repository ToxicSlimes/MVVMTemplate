using System;
using System.Buffers.Binary;

namespace Core.Infrastructure.Services.Save
{
    [Flags]
    internal enum SaveFlags : ushort
    {
        None = 0,
        Encrypted = 1 << 0,
        Compressed = 1 << 1,
    }

    internal readonly struct SaveHeader
    {
        public const uint Magic = 0x554B494F;
        public const int HeaderSize = 8;
        public const ushort CurrentVersion = 1;

        public SaveHeader(SaveFlags flags, ushort version = CurrentVersion)
        {
            Version = version;
            Flags = flags;
        }

        public ushort Version { get; }
        public SaveFlags Flags { get; }

        public byte[] ToBytes()
        {
            var buffer = new byte[HeaderSize];
            BinaryPrimitives.WriteUInt32LittleEndian(buffer.AsSpan(0, 4), Magic);
            BinaryPrimitives.WriteUInt16LittleEndian(buffer.AsSpan(4, 2), Version);
            BinaryPrimitives.WriteUInt16LittleEndian(buffer.AsSpan(6, 2), (ushort)Flags);
            return buffer;
        }

        public static bool TryParse(byte[] data, out SaveHeader header)
        {
            header = default;

            if (data == null || data.Length < HeaderSize)
                return false;

            if (BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(0, 4)) != Magic)
                return false;

            var version = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(4, 2));
            if (version != CurrentVersion)
                return false;

            var flags = (SaveFlags)BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(6, 2));
            if ((flags & ~(SaveFlags.Encrypted | SaveFlags.Compressed)) != 0)
                return false;

            header = new SaveHeader(flags, version);
            return true;
        }
    }
}
