using System;
using MessagePipe;
using UnityEngine;

namespace _001_Scripts.Core.Pipes
{
    /// <summary>
    /// MessagePipe 내장 컨테이너로 파이프의 생성과 수명만 관리합니다.
    /// </summary>
    [DefaultExecutionOrder(-10_000)]
    public sealed class MessagePipeHub : MonoBehaviour
    {
        public static MessagePipeHub Instance { get; private set; }

        [SerializeField]
        private bool createInGamePipeOnStart = true;

        private IDisposablePublisher<GamePipeMessage> _gamePublisher;
        private ISubscriber<GamePipeMessage> _gameSubscriber;

        private IDisposablePublisher<InGamePipeMessage> _inGamePublisher;
        private ISubscriber<InGamePipeMessage> _inGameSubscriber;

        private IDisposablePublisher<InternalPipeMessage> _internalPublisher;
        private ISubscriber<InternalPipeMessage> _internalSubscriber;

        public IPublisher<GamePipeMessage> GamePublisher => _gamePublisher;
        public ISubscriber<GamePipeMessage> GameSubscriber => _gameSubscriber;

        public IPublisher<InGamePipeMessage> InGamePublisher => _inGamePublisher;
        public ISubscriber<InGamePipeMessage> InGameSubscriber => _inGameSubscriber;

        public bool HasInGamePipe => _inGamePublisher != null;

        internal IPublisher<InternalPipeMessage> InternalPublisher => _internalPublisher;
        internal ISubscriber<InternalPipeMessage> InternalSubscriber => _internalSubscriber;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void CreateGlobalHub()
        {
            if (FindAnyObjectByType<MessagePipeHub>() != null)
            {
                return;
            }

            var hubObject = new GameObject("[Core] MessagePipeHub");
            hubObject.AddComponent<MessagePipeHub>();
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

            var builder = new BuiltinContainerBuilder();
            builder.AddMessagePipe();

            var provider = builder.BuildServiceProvider();
            var eventFactory = provider.GetService(typeof(EventFactory)) as EventFactory;
            if (eventFactory == null)
            {
                throw new InvalidOperationException("MessagePipe EventFactory could not be created.");
            }

            GlobalMessagePipe.SetProvider(provider);
            (_gamePublisher, _gameSubscriber) = eventFactory.CreateEvent<GamePipeMessage>();
            (_internalPublisher, _internalSubscriber) = eventFactory.CreateEvent<InternalPipeMessage>();
        }

        private void Start()
        {
            if (createInGamePipeOnStart)
            {
                BeginInGamePipe();
            }
        }

        /// <summary>
        /// 인게임 파이프를 생성합니다. 이미 생성되어 있으면 아무 작업도 하지 않습니다.
        /// </summary>
        public void BeginInGamePipe()
        {
            if (_inGamePublisher != null)
            {
                return;
            }

            (_inGamePublisher, _inGameSubscriber) = GlobalMessagePipe.CreateEvent<InGamePipeMessage>();
        }

        /// <summary>
        /// 인게임 파이프와 연결된 모든 구독을 종료합니다.
        /// </summary>
        public void EndInGamePipe()
        {
            _inGamePublisher?.Dispose();
            _inGamePublisher = null;
            _inGameSubscriber = null;
        }

        private void OnDestroy()
        {
            if (Instance != this)
            {
                return;
            }

            EndInGamePipe();

            _internalPublisher?.Dispose();
            _internalPublisher = null;
            _internalSubscriber = null;

            _gamePublisher?.Dispose();
            _gamePublisher = null;
            _gameSubscriber = null;

            Instance = null;
        }
    }
}
