using System;
using _001_Scripts.Core.Entity;
using _001_Scripts.Core.Pipes.Msgs;
using _001_Scripts.Core.Pipes.Pipes;
using _001_Scripts.Data;
using _001_Scripts.Data.Customers;
using _001_Scripts.Data.Economy;
using _001_Scripts.Data.Items;
using _001_Scripts.Data.Pets;
using _001_Scripts.Data.Progression;
using _001_Scripts.Data.Tools;

namespace _001_Scripts.Core.Pipes
{
    /// <summary>동기 Publish가 끝나기 전에 완료되는 한 번의 응답입니다. 수신자가 없으면 성공으로 처리하지 않습니다.</summary>
    public sealed class PipeReply<T>
    {
        private bool claimed;
        public bool Completed { get; private set; }
        public bool Succeeded { get; private set; }
        public T Value { get; private set; }

        public bool TryClaim()
        {
            if (claimed) return false;
            claimed = true;
            return true;
        }

        public void Complete(bool succeeded, T value = default)
        {
            if (!claimed || Completed) throw new InvalidOperationException("A pipe reply must be claimed and completed only once.");
            Succeeded = succeeded;
            Value = value;
            Completed = true;
        }
    }
}
