using System;
using _001_Scripts.Data.Pets;
using UnityEngine;

namespace _001_Scripts.Data.Customers
{
    /// <summary>접수대에서 쓰이는 모든 문구입니다. 토큰: {pet} {customer} {condition} {action} {clue}</summary>
    [CreateAssetMenu(fileName = "ReceptionDialogueTable", menuName = "PetShop/Customers/Reception Dialogue Table")]
    public sealed class ReceptionDialogueTable : ScriptableObject
    {
        [Serializable]
        public struct ArchetypeLine
        {
            public CustomerArchetype Archetype;
            [TextArea(1, 3)] public string Line;
        }

        [Serializable]
        public struct CategoryLine
        {
            public PetConditionCategory Category;
            [TextArea(1, 3)] public string Line;
        }

        [Serializable]
        public struct ConditionLine
        {
            public PetConditionDefinition Condition;
            [Tooltip("비어 있으면 카테고리 문구를 씁니다.")]
            [TextArea(1, 3)] public string Clue;
            [Tooltip("비어 있으면 카테고리 문구를 씁니다.")]
            [TextArea(1, 3)] public string Question;
        }

        [Serializable]
        public struct CareActionLabel
        {
            public PetCareAction Action;
            public string Label;
        }

        [Header("인사 · {pet} {clue} {customer}")]
        [SerializeField] private ArchetypeLine[] greetings = DefaultGreetings();
        [TextArea(1, 3)] [SerializeField] private string defaultGreeting = "안녕하세요. {pet}이(가) {clue}. 한번 봐주시겠어요?";

        [Header("증상 힌트 · 인사에 {clue}로 들어갑니다")]
        [SerializeField] private CategoryLine[] clues = DefaultClues();
        [TextArea(1, 3)] [SerializeField] private string defaultClue = "평소와 조금 달라 보여요";

        [Header("추가 질문 · {condition}")]
        [SerializeField] private CategoryLine[] questions = DefaultQuestions();
        [TextArea(1, 3)] [SerializeField] private string defaultQuestion = "{condition}에 대해 더 알려주세요.";

        [Header("상태별 개별 문구 (카테고리보다 우선)")]
        [SerializeField] private ConditionLine[] conditionOverrides = Array.Empty<ConditionLine>();

        [Header("질문에 대한 답 · {condition} {action}")]
        [TextArea(1, 3)] [SerializeField] private string replyFormat = "자세히 보니 ‘{condition}’ 상태예요. {action} 케어가 필요해 보여요.";
        [SerializeField] private CareActionLabel[] actionLabels = DefaultActionLabels();
        [SerializeField] private string defaultActionLabel = "상태에 맞는";

        [Header("진행 문구 · {pet} {customer}")]
        [SerializeField] private string playerSpeakerName = "나";
        [TextArea(1, 3)] [SerializeField] private string acceptLine = "확인했습니다. {pet}의 상태부터 살펴볼게요.";
        [TextArea(1, 3)] [SerializeField] private string rejectLine = "아쉽지만 알겠어요. 다음에 다시 찾아올게요.";
        [TextArea(1, 3)] [SerializeField] private string handoffLine = "잘 부탁드릴게요. {pet}을(를) 맡길게요.";
        [TextArea(1, 3)] [SerializeField] private string giveUpLine = "너무 오래 기다렸네요. 다음에 다시 올게요.";

        public string PlayerSpeakerName => string.IsNullOrWhiteSpace(playerSpeakerName) ? "나" : playerSpeakerName;
        public string AcceptLine => acceptLine;
        public string RejectLine => rejectLine;
        public string HandoffLine => handoffLine;
        public string GiveUpLine => giveUpLine;

        public string Greeting(CustomerArchetype archetype)
        {
            for (var i = 0; i < greetings.Length; i++)
                if (greetings[i].Archetype == archetype && !string.IsNullOrWhiteSpace(greetings[i].Line))
                    return greetings[i].Line;
            return defaultGreeting;
        }

        public string Clue(PetConditionDefinition condition)
        {
            if (condition == null) return defaultClue;
            var over = Override(condition, true);
            if (!string.IsNullOrWhiteSpace(over)) return over;
            return Category(clues, condition.Category, defaultClue);
        }

        public string Question(PetConditionDefinition condition)
        {
            if (condition == null) return defaultQuestion;
            var over = Override(condition, false);
            if (!string.IsNullOrWhiteSpace(over)) return over;
            return Category(questions, condition.Category, defaultQuestion);
        }

        public string ActionLabel(PetCareAction action)
        {
            for (var i = 0; i < actionLabels.Length; i++)
                if (actionLabels[i].Action == action && !string.IsNullOrWhiteSpace(actionLabels[i].Label))
                    return actionLabels[i].Label;
            return defaultActionLabel;
        }

        public string Reply(PetConditionDefinition condition) => replyFormat;

        /// <summary>문구의 토큰을 실제 값으로 바꿉니다. 비어 있는 값은 빈 문자열로 들어갑니다.</summary>
        public static string Fill(string template, string pet = null, string customer = null,
            string condition = null, string action = null, string clue = null)
        {
            if (string.IsNullOrEmpty(template)) return string.Empty;
            return template
                .Replace("{pet}", pet ?? string.Empty)
                .Replace("{customer}", customer ?? string.Empty)
                .Replace("{condition}", condition ?? string.Empty)
                .Replace("{action}", action ?? string.Empty)
                .Replace("{clue}", clue ?? string.Empty);
        }

        private string Override(PetConditionDefinition condition, bool wantClue)
        {
            for (var i = 0; i < conditionOverrides.Length; i++)
            {
                if (conditionOverrides[i].Condition != condition) continue;
                return wantClue ? conditionOverrides[i].Clue : conditionOverrides[i].Question;
            }
            return null;
        }

        private static string Category(CategoryLine[] lines, PetConditionCategory category, string fallback)
        {
            for (var i = 0; i < lines.Length; i++)
                if (lines[i].Category == category && !string.IsNullOrWhiteSpace(lines[i].Line))
                    return lines[i].Line;
            return fallback;
        }

        private static ArchetypeLine[] DefaultGreetings() => new[]
        {
            Greet(CustomerArchetype.Adventurer, "긴 여행을 마치고 왔어요. {pet}이(가) {clue}."),
            Greet(CustomerArchetype.Wizard, "요즘 {pet}이(가) {clue}. 원인을 살펴봐 주세요."),
            Greet(CustomerArchetype.Merchant, "깔끔한 관리가 필요해요. {pet}이(가) {clue}."),
            Greet(CustomerArchetype.Noble, "우리 {pet}이(가) {clue}. 세심히 부탁드려요.")
        };

        private static CategoryLine[] DefaultClues() => new[]
        {
            Cat(PetConditionCategory.Contamination, "몸이 무겁고 바닥에 자국을 남겨요"),
            Cat(PetConditionCategory.Injury, "한쪽을 만지면 움찔해요"),
            Cat(PetConditionCategory.Coat, "털이 빗자루처럼 엉켜 있어요"),
            Cat(PetConditionCategory.Growth, "몸에 작은 것이 자란 것 같아요"),
            Cat(PetConditionCategory.ForeignObject, "몸 한쪽이 유난히 반짝여요"),
            Cat(PetConditionCategory.Nails, "걸을 때 바닥을 긁는 소리가 나요"),
            Cat(PetConditionCategory.Hunger, "기운이 없어 보여요"),
            Cat(PetConditionCategory.Stress, "구석에 오래 숨어 있어요")
        };

        private static CategoryLine[] DefaultQuestions() => new[]
        {
            Cat(PetConditionCategory.Contamination, "어디가 가장 더러워졌나요?"),
            Cat(PetConditionCategory.Injury, "다친 곳을 자세히 보셨나요?"),
            Cat(PetConditionCategory.Coat, "털 상태를 조금 더 설명해 주세요."),
            Cat(PetConditionCategory.Growth, "몸에 자란 것이 정확히 무엇인가요?"),
            Cat(PetConditionCategory.ForeignObject, "붙어 있는 물체가 무엇인가요?"),
            Cat(PetConditionCategory.Nails, "발톱 상태는 어떤가요?")
        };

        private static CareActionLabel[] DefaultActionLabels() => new[]
        {
            Act(PetCareAction.Wash, "세척"), Act(PetCareAction.Brush, "빗질"), Act(PetCareAction.Treat, "치료"),
            Act(PetCareAction.Extract, "제거"), Act(PetCareAction.Trim, "손질"), Act(PetCareAction.Clip, "발톱 손질"),
            Act(PetCareAction.Feed, "급식"), Act(PetCareAction.Play, "놀이")
        };

        private static ArchetypeLine Greet(CustomerArchetype archetype, string line) =>
            new ArchetypeLine { Archetype = archetype, Line = line };
        private static CategoryLine Cat(PetConditionCategory category, string line) =>
            new CategoryLine { Category = category, Line = line };
        private static CareActionLabel Act(PetCareAction action, string label) =>
            new CareActionLabel { Action = action, Label = label };
    }
}
