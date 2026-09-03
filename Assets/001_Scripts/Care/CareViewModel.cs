using System.Collections.Generic;

namespace PetShop.Care
{
    public sealed class CareViewModel
    {
        public IReadOnlyList<CareConditionState> Conditions { get; }
        public IReadOnlyList<string> Byproducts { get; }
        public CareToolKind SelectedTool { get; }
        public int SelectedCondition { get; }
        public int RemainingCount { get; }
        public bool Completed { get; }
        public string Message { get; }

        public CareViewModel(
            CareSession session, CareToolKind selectedTool, int selectedCondition, string message)
        {
            Conditions = session.Conditions;
            Byproducts = session.Byproducts;
            SelectedTool = selectedTool;
            SelectedCondition = selectedCondition;
            RemainingCount = session.RemainingCount;
            Completed = session.IsCompleted;
            Message = message ?? string.Empty;
        }
    }

    public static class CarePresentation
    {
        public static string ToolLabel(CareToolKind tool) => tool switch
        {
            CareToolKind.Sprayer => "물뿌리개",
            CareToolKind.WashBrush => "세척 브러시",
            CareToolKind.Comb => "빗",
            CareToolKind.Medicine => "치료 도구",
            CareToolKind.Tweezers => "집게",
            CareToolKind.Scissors => "가위",
            _ => tool.ToString()
        };

        public static string CareLabel(CareKind care) => care switch
        {
            CareKind.Wash => "세척",
            CareKind.Brush => "빗질",
            CareKind.Treat => "치료",
            CareKind.Remove => "제거",
            CareKind.Trim => "손질",
            _ => care.ToString()
        };

        public static string InteractionMessage(CareInteractionResult result) => result.Status switch
        {
            CareInteractionStatus.WrongTool =>
                $"{result.Condition.Name}에는 {CareLabel(result.Condition.Care)} 도구가 필요해요.",
            CareInteractionStatus.Wetting when result.Condition.Wetness >= 1f =>
                "충분히 젖었어요! 이제 세척 브러시로 문질러 주세요.",
            CareInteractionStatus.Wetting => "진흙을 물로 충분히 적시는 중입니다.",
            CareInteractionStatus.NeedsWater => "마른 진흙이에요. 물뿌리개로 먼저 적셔 주세요.",
            CareInteractionStatus.Progressed =>
                $"{result.Condition.Name} 처리 중 · 남은 진행도 {UnityEngine.Mathf.CeilToInt(result.Condition.Remaining * 100)}%",
            CareInteractionStatus.Resolved when !string.IsNullOrWhiteSpace(result.Condition.Byproduct) =>
                $"{result.Condition.Name} 해결! 부산물: {result.Condition.Byproduct}",
            CareInteractionStatus.Resolved => $"{result.Condition.Name} 해결!",
            _ => string.Empty
        };
    }
}
