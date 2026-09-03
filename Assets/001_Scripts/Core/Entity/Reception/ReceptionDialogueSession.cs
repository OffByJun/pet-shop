using System;
using System.Collections.Generic;
using _001_Scripts.Core;
using _001_Scripts.Data.Customers;
using _001_Scripts.UI.UILib;
using UnityEngine;

namespace _001_Scripts.Core.Entity
{
    /// <summary>Conversation state only; wording is delegated to a composer policy.</summary>
    public sealed class ReceptionDialogueSession : GameBehaviour
    {
        private readonly List<ReceptionQuestion> questions = new List<ReceptionQuestion>();
        private DefaultReceptionDialogueComposer composer;

        public ServiceOrder Order { get; private set; }
        public IReadOnlyList<ReceptionQuestion> Questions => questions;
        public string Speaker { get; private set; } = string.Empty;
        public string Line { get; private set; } = string.Empty;
        public float Patience { get; private set; } = 1f;
        public int AskedCount { get; private set; }

        private void Awake() => composer ??= new DefaultReceptionDialogueComposer();

        public void SetComposer(DefaultReceptionDialogueComposer value) =>
            composer = value ?? throw new ArgumentNullException(nameof(value));

        public void Begin(ServiceOrder order)
        {
            Order = order ?? throw new ArgumentNullException(nameof(order));
            composer ??= new DefaultReceptionDialogueComposer();
            questions.Clear();
            Patience = 1f;
            AskedCount = 0;
            Speaker = order.Customer.DisplayName;
            Line = composer.Greeting(order);

            for (var i = 0; i < order.Requests.Count; i++)
            {
                var condition = order.Requests[i].Condition;
                questions.Add(new ReceptionQuestion(
                    condition,
                    composer.Question(condition),
                    composer.Reply(condition),
                    condition.DisplayName));
            }
        }

        public bool Ask(int index)
        {
            if (index < 0 || index >= questions.Count) return false;
            var question = questions[index];
            if (!question.TryMarkAsked()) return false;
            AskedCount++;
            Patience = Mathf.Max(.2f, Patience - .12f);
            Speaker = Order.Customer.DisplayName;
            Line = question.Reply;
            return true;
        }

        public void SayPlayer(string line)
        {
            Speaker = "나";
            Line = line ?? string.Empty;
        }

        public void SayCustomer(string line)
        {
            Speaker = Order == null ? string.Empty : Order.Customer.DisplayName;
            Line = line ?? string.Empty;
        }
    }
}
