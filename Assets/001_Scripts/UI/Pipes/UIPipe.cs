using System;
using _001_Scripts.Core.Pipes;
using _001_Scripts.Core.Pipes.Pipes;
using _001_Scripts.UI.Components;
using UnityEngine;

namespace _001_Scripts.UI.UILib
{
    /// <summary>
    /// UI 메시지를 MessagePipeHub로 주고받기 위한 진입점입니다.
    /// 호출자는 UIManager를 직접 참조하지 않고 이 클래스만 사용합니다.
    /// </summary>
    public static class UIPipe
    {
        public static bool IsAvailable => MessagePipeHub.Instance != null;

        /// <summary>
        /// 허브와 파이프가 아직 없으면 만듭니다. 만들 수 없으면 false를 돌려줍니다.
        /// </summary>
        public static bool EnsurePipe<T>()
            where T : struct, IPipeMsg
        {
            MessagePipeHub.EnsureInstance();

            MessagePipeHub hub = MessagePipeHub.Instance;
            if (hub == null)
            {
                return false;
            }

            if (!hub.IsRegistered<T>())
            {
                hub.Register<T>();
            }

            return true;
        }

        /// <summary>
        /// 허브가 준비되지 않았으면 조용히 무시하고 false를 돌려줍니다.
        /// </summary>
        public static bool Publish<T>(in T message)
            where T : struct, IPipeMsg
        {
            if (!EnsurePipe<T>())
            {
                if (!MessagePipeHub.IsShuttingDown)
                {
                    Debug.LogWarning($"MessagePipeHub is not ready. {typeof(T).Name} was dropped.");
                }

                return false;
            }

            MessagePipeHub.Instance.Publish(in message);
            return true;
        }

        /// <summary>
        /// 허브가 준비되지 않았으면 null을 돌려줍니다.
        /// </summary>
        public static IDisposable Subscribe<T>(Action<T> action)
            where T : struct, IPipeMsg
        {
            if (action == null || !EnsurePipe<T>())
            {
                return null;
            }

            return MessagePipeHub.Instance.Subscribe(action);
        }

        // ─────────────────────────────────────────────
        // Requests
        // ─────────────────────────────────────────────

        public static void Show(string serviceId)
        {
            Publish(new UIShowRequest(serviceId));
        }

        public static void Hide(string serviceId)
        {
            Publish(new UIHideRequest(serviceId));
        }

        public static void Cancel(string serviceId)
        {
            Publish(new UICancelRequest(serviceId));
        }

        public static void SetInstant(string serviceId, bool visible)
        {
            Publish(new UISetInstantRequest(serviceId, visible));
        }

        public static void Register(UIComponent component)
        {
            Publish(new UIComponentRegisterRequest(component));
        }

        public static void Unregister(UIComponent component)
        {
            Publish(new UIComponentUnregisterRequest(component));
        }
    }
}
