using System;
using _001_Scripts.Core.Entity;
using _001_Scripts.Core.Pipes;
using _001_Scripts.Core.Pipes.Msgs;
using _001_Scripts.Data.Customers;
using _001_Scripts.Managers.Interfaces;
using _001_Scripts.UI.Components;
using _001_Scripts.UI.UILib;
using UnityEngine;

namespace _001_Scripts.Managers
{
    /// <summary>손님 접수, 대화, 인계와 화면 입력을 연결합니다.</summary>
    public sealed class ReceptionManager : ServiceManagerBase<ReceptionManager>, IReceptionService
    {

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
        public ReceptionFlow State { get; private set; }
        public float StateProgress => Mathf.Clamp01((Time.unscaledTime - stateStarted) / .8f);
        public bool CanInteract => State == ReceptionFlow.Talking && !handoff.IsRunning && !handoff.IsReady &&
            (!ShopRoutineManager.HasInstance || ShopRoutineManager.Instance.Game.Status == _001_Scripts.Data.DayStatus.CustomerArrived);

        protected override void ProvideServices()
        {
            Provide<IReceptionService>();
        }

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
            if (ShopRoutineManager.HasInstance &&
                ShopRoutineManager.Instance.Game.Status != _001_Scripts.Data.DayStatus.CustomerArrived &&
                ShopRoutineManager.Instance.Game.Status != _001_Scripts.Data.DayStatus.PetInCare) return;
            if (State == ReceptionFlow.Arriving && customerActor.HasArrived) SetState(ReceptionFlow.Talking);
            if (CanInteract)
            {
                dialogue.DrainPatience(Time.deltaTime);
                if (dialogue.HasGivenUp) GiveUp();
            }
            if (State == ReceptionFlow.Leaving && customerActor.HasExited) BeginNextCustomer();
            if (handoff.IsReady && !readyLineShown)
            {
                readyLineShown = true;
                dialogue.SayCustomer(Line(dialogue.Lines.HandoffLine));
            }
        }

        private void LateUpdate()
        {
            if (dialogue?.Order == null) return;
            view.Render(new ReceptionViewModel(dialogue, CanInteract, handoff.IsReady));
            customerActor.SetMood(dialogue.Mood);
        }

        public void Ask(int index)
        {
            if (CanInteract) dialogue.Ask(index);
        }

        public void Accept()
        {
            if (!CanInteract) return;
            if (ShopRoutineManager.HasInstance && !ShopRoutineManager.Instance.AcceptPet()) return;
            dialogue.SayPlayer(Line(dialogue.Lines.AcceptLine));
            readyLineShown = false;
            transition.Prepare(CurrentOrder);
            handoff.Begin();
            customerActor.BeginHandoff();
        }

        public void Reject()
        {
            if (!CanInteract) return;
            dialogue.SayCustomer(Line(dialogue.Lines.RejectLine));
            SetState(ReceptionFlow.Leaving);
            customerActor.Exit();
        }

        /// <summary>인내심이 바닥난 손님은 주문을 맡기지 않고 돌아갑니다.</summary>
        private void GiveUp()
        {
            dialogue.SayCustomer(Line(dialogue.Lines.GiveUpLine));
            SetState(ReceptionFlow.Leaving);
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
            ServiceOrder order;
            if (ShopRoutineManager.HasInstance)
            {
                var game = ShopRoutineManager.Instance.Game;
                if (CurrentOrder != null && ReferenceEquals(CurrentOrder, game.CurrentOrder)) game.SkipCurrentCustomer();
                if (game.Status != _001_Scripts.Data.DayStatus.CustomerArrived) return;
                order = game.CurrentOrder;
            }
            else if (!GamePipe.TryCreateOrder(null, true, out order))
                throw new InvalidOperationException("No active game manager could create the next order.");
            CurrentOrder = order;
            dialogue.Begin(CurrentOrder);
            SetState(ReceptionFlow.Arriving);
            customerActor.Enter(CurrentOrder);
        }

        /// <summary>현재 주문 기준으로 문구의 토큰을 채웁니다.</summary>
        private string Line(string template) => CurrentOrder == null
            ? template
            : _001_Scripts.Data.Customers.ReceptionDialogueTable.Fill(template,
                pet: CurrentOrder.Pet.DisplayName, customer: CurrentOrder.Customer.DisplayName);

        private void SetState(ReceptionFlow next)
        {
            State = next;
            stateStarted = Time.unscaledTime;
        }

    }
}
