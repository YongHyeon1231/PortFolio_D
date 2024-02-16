using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerCore;

namespace Server
{
    struct JobTimerElem : IComparable<JobTimerElem>
    {
        public int execTick; // 실행 시간
        public Action action;

        public int CompareTo(JobTimerElem other)
        {
            return other.execTick - execTick;    
        }
    }

    internal class JobTimer
    {
        PriorityQueue<JobTimerElem> _pq = new PriorityQueue<JobTimerElem>();
        object _lock = new object();

        public static JobTimer Instance { get; } = new JobTimer();

        public void Push(Action action, int tickAfter = 0)
        {
            JobTimerElem job;
            job.execTick = System.Environment.TickCount + tickAfter; //언제 실행하는지
            job.action = action; // 무엇을 실행 하는지

            lock (_lock)
            {
                _pq.Push(job);
            }
        }

        public void Flush()
        {
            while (true)
            {
                int now = System.Environment.TickCount;

                JobTimerElem job;

                lock (_lock)
                {
                    if (_pq.Count == 0)
                        break;

                    job = _pq.Peek();
                    // 살짝만 엿보는 상태에서
                    // 내가 실행해야 되는 시간이 언제인지 봤는데
                    // 아직 먼 미래다 라고 하면은 break를 합니다.
                    if (job.execTick > now)
                        break;

                    _pq.Pop();
                }

                job.action.Invoke();
            }
        }
    }
}
