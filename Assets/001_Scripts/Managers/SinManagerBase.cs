using System;
using System.Collections.Generic;
using _001_Scripts.Core;
using _001_Scripts.Core.Pipes;
using _001_Scripts.Core.Pipes.Pipes;
using UnityEngine;

namespace _001_Scripts.Managers
{
    /// <summary>
    /// Manager 타입별 단일 인스턴스와 공통 수명을 관리하는 기반 클래스입니다.
    /// </summary>
    public abstract class SinManagerBase<TManager> : GameBehaviour
        where TManager : SinManagerBase<TManager>
    {
        [SerializeField]
        private bool persistAcrossScenes = true;

        public static TManager Instance { get; private set; }

        public static bool HasInstance => Instance != null;

        public bool IsPrimaryInstance { get; private set; }

        private readonly List<IDisposable> gamePipeSubscriptions = new List<IDisposable>();

        protected virtual void OnEnable()
        {
            if (IsPrimaryInstance) SubscribeGamePipes();
        }

        protected virtual void OnDisable() => DisposeGamePipes();

        protected virtual void SubscribeGamePipes() { }

        protected void Listen<T>(Action<T> handler) where T : struct, IPipeMsg
        {
            var subscription = GamePipe.Subscribe<T>(message =>
            {
                if (IsPrimaryInstance && isActiveAndEnabled) handler(message);
            });
            if (subscription != null) gamePipeSubscriptions.Add(subscription);
        }

        private void DisposeGamePipes()
        {
            foreach (var subscription in gamePipeSubscriptions) subscription.Dispose();
            gamePipeSubscriptions.Clear();
        }

        protected virtual void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = (TManager)this;
            IsPrimaryInstance = true;

            if (persistAcrossScenes)
            {
                DontDestroyOnLoad(transform.root.gameObject);
            }

            OnManagerAwake();
        }

        protected virtual void OnDestroy()
        {
            if (!IsPrimaryInstance || Instance != this)
            {
                return;
            }

            DisposeGamePipes();
            OnManagerDestroying();
            Instance = null;
            IsPrimaryInstance = false;
        }

        protected virtual void OnManagerAwake()
        {
        }

        protected virtual void OnManagerDestroying()
        {
        }
    }
}
