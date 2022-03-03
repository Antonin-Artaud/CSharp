namespace ServerUnity.Network
{
    using System;

    public class TcpBuffer
    {
        private byte[] _buffer;
        private int _length;

        public ReadOnlySpan<byte> Span => new ReadOnlySpan<byte>(this._buffer, 0, this._length);
        public Memory<byte> Memory => new Memory<byte>(this._buffer, 0, this._length);
        public bool IsEmpty => this._length == 0;

        public TcpBuffer(int initCapacity)
        {
            this._buffer = new byte[initCapacity];
        }
        
        /// <summary>
        /// set buffer to null, and length 0
        /// </summary>
        public void Dispose()
        {
            this._buffer = null;
            this._length = 0;
        }

        /// <summary>
        /// Write buffer with blockCopy
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="length"></param>
        public void Write(byte[] buffer, int length)
        {
            if (this._buffer.Length < this._length + length)
            {
                byte[] biggestArray = new byte[this._length + length];
                Buffer.BlockCopy(this._buffer, 0, biggestArray, 0, this._length);
                this._buffer = biggestArray;
            }

            Buffer.BlockCopy(buffer, 0, this._buffer, this._length, length);
            this._length += length;
        }

        /// <summary>
        /// remove buffer
        /// </summary>
        /// <param name="length"></param>
        public void Remove(int length)
        {
            Buffer.BlockCopy(this._buffer, length, this._buffer, 0, this._length -= length);
        }
    }
}