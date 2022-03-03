namespace ProtocolShared
{
    using System;
    using System.Linq;
    using System.Runtime.CompilerServices;

    public class TcpPacket
    {
        private const int HeaderSize = 5;
        
        private readonly byte[] _commandData;
        private readonly CommandId _commandId;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="commandData"></param>
        /// <param name="commandId"></param>
        private TcpPacket(byte[] commandData, CommandId commandId)
        {
            this._commandData = commandData;
            this._commandId = commandId;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        public TcpPacket(ICommand command)
        {
            this._commandData = CommandSerializer.Serialize(command);
            this._commandId = command.Id;
        }

        /// <summary>
        /// Decode command
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public ICommand DecodeCommand()
        {
            Type commandType = CommandMap.GetCommandTypeById(this._commandId);

            if (commandType == null)
                throw new Exception("Command type not found. (id: " + this._commandId + ")");

            return CommandSerializer.Deserialize(commandType, this._commandData);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public byte[] ToArray()
        {
            byte[] packet = new byte[this._commandData.Length + TcpPacket.HeaderSize];
            
            TcpPacket.WriteHeader(packet, this._commandId, this._commandData.Length);
            Buffer.BlockCopy(this._commandData, 0, packet, TcpPacket.HeaderSize, this._commandData.Length);

            return packet;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="commandId"></param>
        /// <param name="dataLength"></param>
        /// <exception cref="IndexOutOfRangeException"></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void WriteHeader(byte[] packet, CommandId commandId, int dataLength)
        {
            if (packet.Length < TcpPacket.HeaderSize)
                throw new IndexOutOfRangeException("packet must be greater than " + TcpPacket.HeaderSize);

            packet[0] = (byte) commandId;
            packet[1] = (byte) (dataLength >> 24);
            packet[2] = (byte) (dataLength >> 16);
            packet[3] = (byte) (dataLength >> 8);
            packet[4] = (byte) (dataLength);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="commandId"></param>
        /// <param name="dataLength"></param>
        /// <exception cref="IndexOutOfRangeException"></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ReadHeader(byte[] packet, out CommandId commandId, out int dataLength)
        {
            if (packet.Length < TcpPacket.HeaderSize)
                throw new IndexOutOfRangeException("packet must be greater than " + TcpPacket.HeaderSize);
            
            commandId = (CommandId) packet[0];
            dataLength = packet[4] | packet[3] << 8 | packet[2] << 16 | packet[1] << 24;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="commandId"></param>
        /// <param name="dataLength"></param>
        /// <exception cref="IndexOutOfRangeException"></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ReadHeader(ReadOnlySpan<byte> packet, out CommandId commandId, out int dataLength)
        {
            if (packet.Length < TcpPacket.HeaderSize)
                throw new IndexOutOfRangeException("packet must be greater than " + TcpPacket.HeaderSize);
            
            commandId = (CommandId) packet[0];
            dataLength = packet[4] | packet[3] << 8 | packet[2] << 16 | packet[1] << 24;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="tcpPacket"></param>
        /// <returns></returns>
        public static DecodeResult TryDecode(byte[] packet, out TcpPacket tcpPacket)
        {
            if (packet.Length < TcpPacket.HeaderSize)
            {
                tcpPacket = null;
                return DecodeResult.NotTerminated;
            }
            
            TcpPacket.ReadHeader(packet, out CommandId commandId, out int dataLength);

            if (dataLength < 0)
            {
                tcpPacket = null;
                return DecodeResult.BadData;
            }

            if (packet.Length < TcpPacket.HeaderSize + dataLength)
            {
                tcpPacket = null;
                return DecodeResult.NotTerminated;
            }

            tcpPacket = new TcpPacket(packet.Skip(TcpPacket.HeaderSize).Take(dataLength).ToArray(), commandId);
            return DecodeResult.Success + dataLength + TcpPacket.HeaderSize;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="tcpPacket"></param>
        /// <returns></returns>
        public static DecodeResult TryDecode(ReadOnlySpan<byte> packet, out TcpPacket tcpPacket)
        {
            if (packet.Length < TcpPacket.HeaderSize)
            {
                tcpPacket = null;
                return DecodeResult.NotTerminated;
            }
            
            TcpPacket.ReadHeader(packet, out CommandId commandId, out int dataLength);

            if (dataLength < 0)
            {
                tcpPacket = null;
                return DecodeResult.BadData;
            }

            if (packet.Length < TcpPacket.HeaderSize + dataLength)
            {
                tcpPacket = null;
                return DecodeResult.NotTerminated;
            }

            tcpPacket = new TcpPacket(packet.Slice(TcpPacket.HeaderSize, dataLength).ToArray(), commandId);
            return DecodeResult.Success + dataLength + TcpPacket.HeaderSize;
        }
        
        public enum DecodeResult
        {
            BadData = -2,
            NotTerminated = -1,
            Success = 0
        }
    }
}