using System;
using _001_Scripts.Core.Pipes.Msgs;
using _001_Scripts.Core.Pipes.Pipes;

namespace _001_Scripts.Core.Pipes
{
    public interface IPipeHub
    {
        void Register<T>()
            where T : struct, IPipeMsg;

        void Unregister<T>()
            where T : struct, IPipeMsg;

        bool IsRegistered<T>()
            where T : struct, IPipeMsg;

        Pipe<T> GetPipe<T>()
            where T : struct, IPipeMsg;

        void Publish<T>(in T message)
            where T : struct, IPipeMsg;

        IDisposable Subscribe<T>(Action<T> action)
            where T : struct, IPipeMsg;
    }
}