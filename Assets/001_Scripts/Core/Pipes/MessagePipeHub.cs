using System;
using System.Collections.Generic;
using _001_Scripts.Core.Pipes.Msgs;
using _001_Scripts.Core.Pipes.Pipes;
using MessagePipe;
using UnityEngine;

namespace _001_Scripts.Core.Pipes
{
    /// <summary>
    /// MessagePipe 파이프들의 생성, 조회 및 수명을 관리합니다.
    /// </summary>
    [DefaultExecutionOrder(-10_000)]
    public sealed class MessagePipeHub : MonoBehaviour, IPipeHub
    {
        public static MessagePipeHub Instance { get; private set; }

        private readonly Dictionary<Type, IPipe> _pipes = new();

        private EventFactory _eventFactory;
        private IServiceProvider _provider;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void CreateGlobalHub()
        {
            if (FindAnyObjectByType<MessagePipeHub>() != null)
                return;

            var hubObject = new GameObject("[Core] MessagePipeHub");

            hubObject.AddComponent<MessagePipeHub>();

            DontDestroyOnLoad(hubObject);
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            DontDestroyOnLoad(gameObject);

            InitializeMessagePipe();
        }

        private void InitializeMessagePipe()
        {
            var builder = new BuiltinContainerBuilder();

            builder.AddMessagePipe();

            _provider = builder.BuildServiceProvider();

            _eventFactory =
                _provider.GetService(typeof(EventFactory)) as EventFactory;

            if (_eventFactory == null)
            {
                throw new InvalidOperationException(
                    "MessagePipe EventFactory could not be created.");
            }

            GlobalMessagePipe.SetProvider(_provider);
        }

        // ─────────────────────────────────────────────
        // Register
        // ─────────────────────────────────────────────

        public void Register<T>()
            where T : struct, IPipeMsg
        {
            var type = typeof(T);

            if (_pipes.ContainsKey(type))
                return;

            var (publisher, subscriber) =
                _eventFactory.CreateEvent<T>();

            var pipe = new Pipe<T>(
                publisher,
                subscriber);

            _pipes.Add(type, pipe);
        }

        // ─────────────────────────────────────────────
        // Get
        // ─────────────────────────────────────────────

        public Pipe<T> GetPipe<T>()
            where T : struct, IPipeMsg
        {
            if (!_pipes.TryGetValue(typeof(T), out var pipe))
            {
                throw new InvalidOperationException(
                    $"Pipe<{typeof(T).Name}> is not registered.");
            }

            return (Pipe<T>)pipe;
        }

        public bool TryGetPipe<T>(out Pipe<T> pipe)
            where T : struct, IPipeMsg
        {
            if (_pipes.TryGetValue(typeof(T), out var rawPipe))
            {
                pipe = (Pipe<T>)rawPipe;
                return true;
            }

            pipe = null;
            return false;
        }

        public bool IsRegistered<T>()
            where T : struct, IPipeMsg
        {
            return _pipes.ContainsKey(typeof(T));
        }

        // ─────────────────────────────────────────────
        // Publish / Subscribe
        // ─────────────────────────────────────────────

        public void Publish<T>(in T message)
            where T : struct, IPipeMsg
        {
            GetPipe<T>().Publish(in message);
        }

        public IDisposable Subscribe<T>(Action<T> action)
            where T : struct, IPipeMsg
        {
            return GetPipe<T>().Subscribe(action);
        }

        // ─────────────────────────────────────────────
        // Unregister
        // ─────────────────────────────────────────────

        public void Unregister<T>()
            where T : struct, IPipeMsg
        {
            var type = typeof(T);

            if (!_pipes.Remove(type, out var pipe))
                return;

            pipe.Dispose();
        }

        // ─────────────────────────────────────────────
        // Lifetime
        // ─────────────────────────────────────────────

        private void OnDestroy()
        {
            if (Instance != this)
                return;

            foreach (var pipe in _pipes.Values)
            {
                pipe.Dispose();
            }

            _pipes.Clear();

            Instance = null;
        }
    }
}