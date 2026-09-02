using System;
using _001_Scripts.Core.Pipes.Msgs;
using _001_Scripts.Core.Pipes.Pipes;
using MessagePipe;

namespace _001_Scripts.Core.Pipes.Msgs
{
    public sealed class Pipe<T> : IPipe
        where T : struct, IPipeMsg
    {
        private IDisposablePublisher<T> _publisher;
        private ISubscriber<T> _subscriber;

        public Type MessageType => typeof(T);

        public Pipe(
            IDisposablePublisher<T> publisher,
            ISubscriber<T> subscriber)
        {
            _publisher = publisher;
            _subscriber = subscriber;
        }

        public void Publish(in T message)
        {
            _publisher.Publish(message);
        }

        public IDisposable Subscribe(Action<T> action)
        {
            return _subscriber.Subscribe(action);
        }

        public void Dispose()
        {
            _publisher?.Dispose();

            _publisher = null;
            _subscriber = null;
        }
    }
}