using System;
using _001_Scripts.Core.Entity;
using _001_Scripts.Core.Pipes;
using _001_Scripts.Core.Pipes.Msgs;
using _001_Scripts.Data.Customers;
using _001_Scripts.UI.Components;
using _001_Scripts.UI.UILib;
using UnityEngine;

namespace _001_Scripts.Managers
{
    /// <summary>손님 접수, 대화, 인계와 화면 입력을 연결합니다.</summary>
    public sealed class ReceptionManager : SinManagerBase<ReceptionManager>
    {
        public enum FlowState { Arriving, Talking, Leaving }

        [UnityEngine.Serialization.FormerlySerializedAs("dialogueComponent")]
        [SerializeField] private ReceptionDialogueSession dialogue;
        [UnityEngine.Serialization.FormerlySerializedAs("handoffComponent")]
        [SerializeField] private ReceptionHandoff handoff;
        [UnityEngine.Serialization.FormerlySerializedAs("customerActorComponent")]
        [SerializeField] private ReceptionCustomerActor customerActor;
        [UnityEngine.Serialization.FormerlySerializedAs("transitionComponent")]
        [SerializeField] private ReceptionCareSceneTransition transition;
        [UnityEngine.Serialization.FormerlySerializedAs("viewComponent")]
        [SerializeField] private ReceptionUIComponent view;
        private float stateStarted;
        private bool readyLineShown;

        public ServiceOrder CurrentOrder { get; private set; }
        public FlowState State { get; private set; }
        public float StateProgress => Mathf.Clamp01((Time.unscaledTime - stateStarted) / .8f);
        public bool CanInteract => State == FlowState.Talking && !handoff.IsRunning && !handoff.IsReady;

        protected override void SubscribeGamePipes()
        {
            Listen<ReceptionInputRequest>(request =>
            {
                if (request.Source != view) return;
                switch (request.Input)
                {
                    case ReceptionInput.Ask: Ask(request.Index); break;
                    case ReceptionInput.Accept: Accept(); break;
                    case ReceptionInput.Reject: Reject(); break;
                    case ReceptionInput.Next: SkipToNext(); break;
                    case ReceptionInput.EnterCare: EnterCare(); break;
                }
            });
        }
        private void Start() => BeginNextCustomer();

        private void Update()
        {
            if (State == FlowState.Arriving && customerActor.HasArrived) SetState(FlowState.Talking);
            if (State == FlowState.Leaving && customerActor.HasExited) BeginNextCustomer();
            if (handoff.IsReady && !readyLineShown)
            {
                readyLineShown = true;
                dialogue.SayCustomer($"잘 부탁드릴게요. {CurrentOrder.Pet.DisplayName}을(를) 맡길게요.");
            }
        }

        private void LateUpdate()
        {
            if (dialogue?.Order != null)
                view.Render(new ReceptionViewModel(dialogue, CanInteract, handoff.IsReady));
        }

        public void Ask(int index)
        {
            if (CanInteract) dialogue.Ask(index);
        }

        public void Accept()
        {
            if (!CanInteract) return;
            dialogue.SayPlayer($"확인했습니다. {CurrentOrder.Pet.DisplayName}의 상태부터 살펴볼게요.");
            readyLineShown = false;
            transition.Prepare(CurrentOrder);
            handoff.Begin();
            customerActor.BeginHandoff();
        }

        public void Reject()
        {
            if (!CanInteract) return;
            dialogue.SayCustomer("아쉽지만 알겠어요. 다음에 다시 찾아올게요.");
            SetState(FlowState.Leaving);
            customerActor.Exit();
        }

        public void SkipToNext()
        {
            if (CanInteract) BeginNextCustomer();
        }

        public void EnterCare()
        {
            if (handoff.IsReady) transition.EnterCareScene();
        }

        private void BeginNextCustomer()
        {
            handoff.ResetState();
            transition.ResetState();
            if (!GamePipe.TryCreateOrder(null, true, out var order))
                throw new InvalidOperationException("No active game manager could create the next order.");
            CurrentOrder = order;
            dialogue.Begin(CurrentOrder);
            SetState(FlowState.Arriving);
            customerActor.Enter(CurrentOrder);
        }

        private void SetState(FlowState next)
        {
            State = next;
            stateStarted = Time.unscaledTime;
        }

    }
}
