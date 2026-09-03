using System;
using System.Collections.Generic;
using _001_Scripts.Data.Customers;

namespace PetShop.Care
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
        public bool CanInteract { get; }
        public bool CanEnterCare { get; }

        public ReceptionViewModel(IReceptionDialogue dialogue, bool canInteract, bool canEnterCare)
        {
            if (dialogue == null) throw new ArgumentNullException(nameof(dialogue));
            Order = dialogue.Order;
            Questions = dialogue.Questions;
            Speaker = dialogue.Speaker;
            Line = dialogue.Line;
            Patience = dialogue.Patience;
            CanInteract = canInteract;
            CanEnterCare = canEnterCare;
        }
    }
}
