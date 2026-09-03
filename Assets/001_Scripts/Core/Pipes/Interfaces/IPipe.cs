using System;

namespace _001_Scripts.Core.Pipes.Msgs
{
    public interface IPipe : IDisposable
    {
        Type MessageType { get; }
    }
}