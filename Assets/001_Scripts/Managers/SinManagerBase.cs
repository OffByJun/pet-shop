using _001_Scripts.Core;
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
