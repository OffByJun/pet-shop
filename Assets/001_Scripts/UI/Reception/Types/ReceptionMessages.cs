using _001_Scripts.Core.Entity;
using _001_Scripts.Core.Pipes.Pipes;
using _001_Scripts.Data;
using _001_Scripts.Data.Customers;
using _001_Scripts.Data.Economy;
using _001_Scripts.Data.Items;
using _001_Scripts.Data.Pets;
using _001_Scripts.Data.Progression;
using _001_Scripts.Data.Tools;

namespace _001_Scripts.Core.Pipes.Msgs
{
    public enum ReceptionInput { Ask, Accept, Reject, Next, EnterCare }

    public readonly struct ReceptionInputRequest : IPipeMsg
    {
        public readonly UnityEngine.Object Source;
        public readonly ReceptionInput Input;
        public readonly int Index;

        public ReceptionInputRequest(UnityEngine.Object source, ReceptionInput input, int index)
        {
            Source = source;
            Input = input;
            Index = index;
        }
    }
}
