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
        [Tooltip("인내심과 표정 규칙입니다. 비어 있으면 기본값으로 동작합니다.")]
        [SerializeField] private ReceptionSettings settings;
        private readonly List<ReceptionQuestion> questions = new List<ReceptionQuestion>();
        private DefaultReceptionDialogueComposer composer;

        public ServiceOrder Order { get; private set; }
        public IReadOnlyList<ReceptionQuestion> Questions => questions;
        public string Speaker { get; private set; } = string.Empty;
        public string Line { get; private set; } = string.Empty;
        public float Patience { get; private set; } = 1f;
        public int AskedCount { get; private set; }
        public ReceptionSettings Settings => settings;
        /// <summary>이 손님과 쌓인 관계입니다. 루틴이 없으면 비어 있습니다.</summary>
        public CustomerRelationship Relationship { get; private set; }
        public CustomerMood Mood => settings.MoodFor(Patience);
        public bool HasGivenUp => settings.LeaveWhenPatienceEmpty && Patience <= 0f;

        public ReceptionDialogueTable Lines => composer.Table;

        private void Awake()
        {
            // A scene without authored assets still needs sane numbers and wording to run on.
            if (settings == null) settings = ScriptableObject.CreateInstance<ReceptionSettings>();
            composer ??= new DefaultReceptionDialogueComposer(settings.Lines);
        }

        public void SetComposer(DefaultReceptionDialogueComposer value) =>
            composer = value ?? throw new ArgumentNullException(nameof(value));

        public void Begin(ServiceOrder order)
        {
            Order = order ?? throw new ArgumentNullException(nameof(order));
            composer ??= new DefaultReceptionDialogueComposer(settings == null ? null : settings.Lines);
            questions.Clear();
            Patience = settings.StartingPatience;
            AskedCount = 0;
            Relationship = _001_Scripts.Managers.ShopRoutineManager.HasInstance
                ? _001_Scripts.Managers.ShopRoutineManager.Instance.RelationshipWith(order.Customer)
                : null;
            Speaker = order.Customer.CharacterName;
            Line = composer.Greeting(order, Relationship);

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
            // Questions alone never push the customer out of the door; only waiting does.
            Patience = Mathf.Max(Mathf.Min(settings.QuestionFloor, Patience), Patience - settings.QuestionCost);
            Speaker = Order.Customer.CharacterName;
            Line = question.Reply;
            return true;
        }

        /// <summary>응대를 기다리는 동안 인내심을 흘려보냅니다.</summary>
        public void DrainPatience(float deltaSeconds)
        {
            if (Order == null || deltaSeconds <= 0f || settings.DrainPerSecond <= 0f) return;
            Patience = Mathf.Max(0f, Patience - settings.DrainPerSecond * deltaSeconds);
        }

        public void SayPlayer(string line)
        {
            Speaker = Lines.PlayerSpeakerName;
            Line = line ?? string.Empty;
        }

        public void SayCustomer(string line)
        {
            Speaker = Order == null ? string.Empty : Order.Customer.CharacterName;
            Line = line ?? string.Empty;
        }
    }
}
