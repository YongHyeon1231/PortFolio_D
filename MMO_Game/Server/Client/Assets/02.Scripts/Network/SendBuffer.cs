using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
    public class SendBufferHelper
    {
        public static ThreadLocal<SendBuffer> CurrentBuffer = new ThreadLocal<SendBuffer>(() => { return null; });

        public static int ChunkSize { get; set; } = 65535;

        public static ArraySegment<byte> Open(int reserveSize)
        {
            //만약 null이면 아직 CurrentBuffer를 한번도 사용을 안한 셈이 되니까 하나를 만들어 줍니다.
            if (CurrentBuffer.Value == null)
            {
                CurrentBuffer.Value = new SendBuffer(ChunkSize);
            }

            // CurrentBuffer의 Value가 있기는 한데 FreeSize를 체크를 해보니까 너가 요구한 reserveSize보다 작다고 하면은
            // CurrentBuffer의 Value를 기존에 있던 Chunk를 날려버린 다음에 새로운 아이로 교체를 해주도록 합니다.
            if (CurrentBuffer.Value.FreeSize < reserveSize)
            {
                CurrentBuffer.Value = new SendBuffer(ChunkSize);
            }

            // 여기까지 오면 공간이 남아있다는 얘기니까
            return CurrentBuffer.Value.Open(reserveSize);
        }

        public static ArraySegment<byte> Close(int usedSize)
        {
            return CurrentBuffer.Value.Close(usedSize);
        }
    }

    public class SendBuffer
    {
        // [u][][][][][][][][][]
        byte[] _buffer;
        int _usedSize = 0;

        public int FreeSize {  get { return _buffer.Length - _usedSize; } }

        public SendBuffer(int chunkSize)
        {
            _buffer = new byte[chunkSize];
        }

        public ArraySegment<byte> Open(int reserveSize)
        {
            return new ArraySegment<byte>(_buffer, _usedSize, reserveSize);
        }

        // 버퍼를 최종적으로 다썼다고 반환을 하는 그런 개념이 되겠습니다.
        public ArraySegment<byte> Close(int usedSize)
        {
            ArraySegment<byte> segment = new ArraySegment<byte>(_buffer, _usedSize, usedSize);
            _usedSize += usedSize;
            return segment;
        }
    }
}
