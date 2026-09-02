using System;

namespace _001_Scripts.Data.Customers
{
    [Serializable]
    public sealed class ServiceRequestState
    {
        public PetConditionDefinition Condition { get; }
        public ServiceRequestKind Kind { get; }
        public bool IsResolved { get; private set; }

        public ServiceRequestState(PetConditionDefinition condition, ServiceRequestKind kind)
        {
            Condition = condition ?? throw new ArgumentNullException(nameof(condition));
            Kind = kind;
        }

        internal bool Resolve()
        {
            if (IsResolved) return false;
            IsResolved = true;
            return true;
        }
    }
}
