using System;
using _001_Scripts.Core.Pipes.Pipes;
using _001_Scripts.UI.Components;

namespace _001_Scripts.UI.UILib
{
    // ─────────────────────────────────────────────
    // Requests : UIManager가 구독하여 처리하는 요청 신호
    // ─────────────────────────────────────────────

    /// <summary>
    /// 등록된 UI를 보여달라는 요청입니다.
    /// </summary>
    public readonly struct UIShowRequest : IPipeMsg
    {
        public readonly string ServiceId;

        public UIShowRequest(string serviceId)
        {
            ServiceId = serviceId;
        }
    }

    /// <summary>
    /// 등록된 UI를 숨겨달라는 요청입니다.
    /// </summary>
    public readonly struct UIHideRequest : IPipeMsg
    {
        public readonly string ServiceId;

        public UIHideRequest(string serviceId)
        {
            ServiceId = serviceId;
        }
    }

    /// <summary>
    /// 진행 중인 전환을 취소해달라는 요청입니다.
    /// </summary>
    public readonly struct UICancelRequest : IPipeMsg
    {
        public readonly string ServiceId;

        public UICancelRequest(string serviceId)
        {
            ServiceId = serviceId;
        }
    }

    /// <summary>
    /// 애니메이션 없이 즉시 표시 상태를 바꿔달라는 요청입니다.
    /// </summary>
    public readonly struct UISetInstantRequest : IPipeMsg
    {
        public readonly string ServiceId;
        public readonly bool Visible;

        public UISetInstantRequest(string serviceId, bool visible)
        {
            ServiceId = serviceId;
            Visible = visible;
        }
    }

    /// <summary>
    /// UIComponent가 스스로를 레지스트리에 등록해달라는 요청입니다.
    /// </summary>
    public readonly struct UIComponentRegisterRequest : IPipeMsg
    {
        public readonly UIComponent Component;

        public UIComponentRegisterRequest(UIComponent component)
        {
            Component = component;
        }
    }

    /// <summary>
    /// UIComponent가 스스로를 레지스트리에서 빼달라는 요청입니다.
    /// </summary>
    public readonly struct UIComponentUnregisterRequest : IPipeMsg
    {
        public readonly UIComponent Component;

        public UIComponentUnregisterRequest(UIComponent component)
        {
            Component = component;
        }
    }

    // ─────────────────────────────────────────────
    // Notifications : UIManager가 발행하는 결과 신호
    // ─────────────────────────────────────────────

    /// <summary>
    /// UIComponent가 레지스트리에 등록되었습니다.
    /// </summary>
    public readonly struct UIComponentRegistered : IPipeMsg
    {
        public readonly string ServiceId;
        public readonly UIComponent Component;

        public UIComponentRegistered(string serviceId, UIComponent component)
        {
            ServiceId = serviceId;
            Component = component;
        }
    }

    /// <summary>
    /// UIComponent가 레지스트리에서 제거되었습니다.
    /// </summary>
    public readonly struct UIComponentUnregistered : IPipeMsg
    {
        public readonly string ServiceId;
        public readonly UIComponent Component;

        public UIComponentUnregistered(string serviceId, UIComponent component)
        {
            ServiceId = serviceId;
            Component = component;
        }
    }

    /// <summary>
    /// 전환이 시작되었습니다.
    /// </summary>
    public readonly struct UITransitionStarted : IPipeMsg
    {
        public readonly string ServiceId;
        public readonly UITransition Transition;

        public UITransitionStarted(string serviceId, UITransition transition)
        {
            ServiceId = serviceId;
            Transition = transition;
        }
    }

    /// <summary>
    /// 전환이 정상적으로 끝났습니다.
    /// </summary>
    public readonly struct UITransitionCompleted : IPipeMsg
    {
        public readonly string ServiceId;
        public readonly UITransition Transition;

        public UITransitionCompleted(string serviceId, UITransition transition)
        {
            ServiceId = serviceId;
            Transition = transition;
        }
    }

    /// <summary>
    /// 전환이 취소되었습니다.
    /// </summary>
    public readonly struct UITransitionCanceled : IPipeMsg
    {
        public readonly string ServiceId;
        public readonly UITransition Transition;

        public UITransitionCanceled(string serviceId, UITransition transition)
        {
            ServiceId = serviceId;
            Transition = transition;
        }
    }

    /// <summary>
    /// 전환 도중 예외가 발생했거나 대상 UI를 찾지 못했습니다.
    /// </summary>
    public readonly struct UITransitionFailed : IPipeMsg
    {
        public readonly string ServiceId;
        public readonly UITransition Transition;
        public readonly Exception Error;

        public UITransitionFailed(string serviceId, UITransition transition, Exception error)
        {
            ServiceId = serviceId;
            Transition = transition;
            Error = error;
        }
    }

    /// <summary>
    /// UI의 확정된 표시 상태가 바뀌었습니다.
    /// </summary>
    public readonly struct UIVisibilityChanged : IPipeMsg
    {
        public readonly string ServiceId;
        public readonly UIVisibilityState State;

        public UIVisibilityChanged(string serviceId, UIVisibilityState state)
        {
            ServiceId = serviceId;
            State = state;
        }
    }
}
