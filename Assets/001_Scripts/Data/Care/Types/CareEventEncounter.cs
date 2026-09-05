using System;
using System.Collections.Generic;

namespace _001_Scripts.Data
{
    public sealed class CareEventChoice
    {
        public string Label { get; }
        public string Hint { get; }
        public string Result { get; }
        public float AssistProgress { get; }
        public int FlowBeats { get; }

        public CareEventChoice(string label, string hint, string result, float assistProgress, int flowBeats)
        {
            Label = label ?? string.Empty;
            Hint = hint ?? string.Empty;
            Result = result ?? string.Empty;
            AssistProgress = Math.Max(0f, assistProgress);
            FlowBeats = Math.Max(0, flowBeats);
        }
    }

    public sealed class CareEventEncounter
    {
        private readonly CareEventChoice[] choices;

        public string EventId { get; }
        public string Title { get; }
        public string Description { get; }
        public CareConditionState Condition { get; }
        public IReadOnlyList<CareEventChoice> Choices => choices;
        public bool IsResolved { get; private set; }

        public CareEventEncounter(string eventId, string title, string description,
            CareConditionState condition, CareEventChoice first, CareEventChoice second)
        {
            EventId = eventId ?? throw new ArgumentNullException(nameof(eventId));
            Title = title ?? string.Empty;
            Description = description ?? string.Empty;
            Condition = condition ?? throw new ArgumentNullException(nameof(condition));
            choices = new[] { first ?? throw new ArgumentNullException(nameof(first)),
                second ?? throw new ArgumentNullException(nameof(second)) };
        }

        public bool TryChoose(int index, out CareEventChoice choice)
        {
            choice = null;
            if (IsResolved || index < 0 || index >= choices.Length) return false;
            IsResolved = true;
            choice = choices[index];
            return true;
        }
    }
}
