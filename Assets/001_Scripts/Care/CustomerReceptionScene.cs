using System;
using _001_Scripts.Data.Customers;
using UnityEngine;

namespace PetShop.Care
{
    /// <summary>Application coordinator. It depends only on reception ports.</summary>
    public sealed class CustomerReceptionScene : MonoBehaviour
    {
        public enum FlowState { Arriving, Talking, Leaving }

        [SerializeField] private MonoBehaviour orderProviderComponent;
        [SerializeField] private MonoBehaviour dialogueComponent;
        [SerializeField] private MonoBehaviour handoffComponent;
        [SerializeField] private MonoBehaviour customerActorComponent;
        [SerializeField] private MonoBehaviour transitionComponent;
        [SerializeField] private MonoBehaviour viewComponent;

        private IReceptionOrderProvider orderProvider;
        private IReceptionDialogue dialogue;
        private IReceptionHandoff handoff;
        private IReceptionCustomerActor customerActor;
        private ICareSceneTransition transition;
        private IReceptionView view;
        private float stateStarted;
        private bool readyLineShown;

        public ServiceOrder CurrentOrder { get; private set; }
        public FlowState State { get; private set; }
        public float StateProgress => Mathf.Clamp01((Time.unscaledTime - stateStarted) / .8f);
        public bool CanInteract => State == FlowState.Talking && !handoff.IsRunning && !handoff.IsReady;

        private void Awake()
        {
            orderProvider = Require<IReceptionOrderProvider>(orderProviderComponent, nameof(orderProviderComponent));
            dialogue = Require<IReceptionDialogue>(dialogueComponent, nameof(dialogueComponent));
            handoff = Require<IReceptionHandoff>(handoffComponent, nameof(handoffComponent));
            customerActor = Require<IReceptionCustomerActor>(customerActorComponent, nameof(customerActorComponent));
            transition = Require<ICareSceneTransition>(transitionComponent, nameof(transitionComponent));
            view = Require<IReceptionView>(viewComponent, nameof(viewComponent));
        }

        private void OnEnable()
        {
            if (view == null) return;
            view.QuestionRequested += Ask;
            view.AcceptRequested += Accept;
            view.RejectRequested += Reject;
            view.NextRequested += SkipToNext;
            view.CareRequested += EnterCare;
        }

        private void OnDisable()
        {
            if (view == null) return;
            view.QuestionRequested -= Ask;
            view.AcceptRequested -= Accept;
            view.RejectRequested -= Reject;
            view.NextRequested -= SkipToNext;
            view.CareRequested -= EnterCare;
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
            CurrentOrder = orderProvider.CreateNext();
            dialogue.Begin(CurrentOrder);
            SetState(FlowState.Arriving);
            customerActor.Enter(CurrentOrder);
        }

        private void SetState(FlowState next)
        {
            State = next;
            stateStarted = Time.unscaledTime;
        }

        public void Configure(
            ReceptionOrderSource source,
            ReceptionDialogueSession dialogueSession,
            ReceptionHandoff handoffTimer,
            ReceptionCustomerActor actor,
            ReceptionCareSceneTransition sceneTransition,
            ReceptionUIComponent receptionView)
        {
            orderProviderComponent = source;
            dialogueComponent = dialogueSession;
            handoffComponent = handoffTimer;
            customerActorComponent = actor;
            transitionComponent = sceneTransition;
            viewComponent = receptionView;
        }

        private static T Require<T>(MonoBehaviour component, string fieldName) where T : class
        {
            if (component is T contract) return contract;
            throw new InvalidOperationException($"{fieldName} must implement {typeof(T).Name}.");
        }
    }
}
