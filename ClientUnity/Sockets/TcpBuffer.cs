using System;

namespace Akatsuki.Server.Network
{
    public class TcpBuffer
    {
        public byte[] Buffer { get; private set; }
        public int Length { get; private set; }

        public TcpBuffer(int initCapacity)
        {
            Buffer = new byte[initCapacity];
        }

        public ReadOnlySpan<byte> Span => new ReadOnlySpan<byte>(Buffer, 0, Length);
        public Memory<byte> Memory => new Memory<byte>(Buffer, 0, Length);
        public bool IsEmpty => Length == 0;

        public void Dispose()
        {
            Buffer = null;
            Length = 0;
        }

        public void Write(byte[] buffer, int length)
        {
            if (Buffer.Length < Length + length)
            {
                var biggestArray = new byte[Length + length];
                System.Buffer.BlockCopy(Buffer, 0, biggestArray, 0, Length);
                Buffer = biggestArray;
            }

            System.Buffer.BlockCopy(buffer, 0, Buffer, Length, length);
            Length += length;
        }

        public void Remove(int length)
        {
            System.Buffer.BlockCopy(Buffer, length, Buffer, 0, Length -= length);
        }
    }
}