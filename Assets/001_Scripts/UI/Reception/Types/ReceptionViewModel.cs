using System;
using System.Collections.Generic;
using _001_Scripts.Core.Entity;
using _001_Scripts.Data.Customers;

namespace _001_Scripts.UI.UILib
{
    public sealed class ReceptionQuestion
    {
        public PetConditionDefinition Condition { get; }
        public string Prompt { get; }
        public string Reply { get; }
        public string Reveal { get; }
        public bool Asked { get; private set; }

        public ReceptionQuestion(PetConditionDefinition condition, string prompt, string reply, string reveal)
        {
            Condition = condition ?? throw new ArgumentNullException(nameof(condition));
            Prompt = prompt ?? string.Empty;
            Reply = reply ?? string.Empty;
            Reveal = reveal ?? string.Empty;
        }

        public bool TryMarkAsked()
        {
            if (Asked) return false;
            Asked = true;
            return true;
        }
    }

    public sealed class ReceptionViewModel
    {
        public ServiceOrder Order { get; }
        public IReadOnlyList<ReceptionQuestion> Questions { get; }
        public string Speaker { get; }
        public string Line { get; }
        public float Patience { get; }
        /// <summary>손님과의 관계 배지 문구입니다. 없으면 빈 문자열입니다.</summary>
        public string BondLabel { get; }
        public int VisitNumber { get; }
        public bool CanInteract { get; }
        public bool CanEnterCare { get; }

        public ReceptionViewModel(ReceptionDialogueSession dialogue, bool canInteract, bool canEnterCare)
        {
            if (dialogue == null) throw new ArgumentNullException(nameof(dialogue));
            Order = dialogue.Order;
            Questions = dialogue.Questions;
            Speaker = dialogue.Speaker;
            Line = dialogue.Line;
            Patience = dialogue.Patience;
            var relationship = dialogue.Relationship;
            VisitNumber = relationship == null ? 0 : relationship.Visits + 1;
            BondLabel = relationship != null && _001_Scripts.Managers.ShopRoutineManager.HasInstance
                ? _001_Scripts.Managers.ShopRoutineManager.Instance.BondWith(Order.Customer).Label
                : string.Empty;
            CanInteract = canInteract;
            CanEnterCare = canEnterCare;
        }
    }
}
