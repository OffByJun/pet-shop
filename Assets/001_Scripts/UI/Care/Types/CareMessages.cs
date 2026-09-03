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
    public enum CareInput { SelectTool, SelectCondition, Reset, Stroke }

    public readonly struct CareInputRequest : IPipeMsg
    {
        public readonly UnityEngine.Object Source;
        public readonly CareInput Input;
        public readonly int Index;
        public readonly float Amount;

        public CareInputRequest(UnityEngine.Object source, CareInput input, int index, float amount)
        {
            Source = source;
            Input = input;
            Index = index;
            Amount = amount;
        }
    }
}
