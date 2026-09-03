using System;
using System.Collections.Generic;
using _001_Scripts.Data.Customers;

namespace PetShop.Care
{
    public interface IReceptionOrderProvider
    {
        ServiceOrder CreateNext();
    }

    public interface IReceptionDialogue
    {
        ServiceOrder Order { get; }
        IReadOnlyList<ReceptionQuestion> Questions { get; }
        string Speaker { get; }
        string Line { get; }
        float Patience { get; }
        void Begin(ServiceOrder order);
        bool Ask(int index);
        void SayPlayer(string line);
        void SayCustomer(string line);
    }

    public interface IReceptionDialogueComposer
    {
        string Greeting(ServiceOrder order);
        string Question(PetConditionDefinition condition);
        string Reply(PetConditionDefinition condition);
    }

    public interface IReceptionCustomerActor
    {
        bool HasArrived { get; }
        bool HasExited { get; }
        void Enter(ServiceOrder order);
        void Exit();
        void BeginHandoff();
    }

    public interface IReceptionHandoff
    {
        bool IsRunning { get; }
        bool IsReady { get; }
        float Progress { get; }
        void Begin();
        void ResetState();
    }

    public interface ICareSceneTransition
    {
        void Prepare(ServiceOrder order);
        void EnterCareScene();
        void ResetState();
    }

    public interface IReceptionView
    {
        event Action<int> QuestionRequested;
        event Action AcceptRequested;
        event Action RejectRequested;
        event Action NextRequested;
        event Action CareRequested;
        void Render(ReceptionViewModel model);
    }
}
