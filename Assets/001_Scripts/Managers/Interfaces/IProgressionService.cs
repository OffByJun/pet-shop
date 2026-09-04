using _001_Scripts.Core.Services;
using _001_Scripts.Data.Progression;

namespace _001_Scripts.Managers.Interfaces
{
    /// <summary>상점 진행도와 해금입니다. 콘텐츠 잠금 판정은 이 계약만 봅니다.</summary>
    public interface IProgressionService : IService
    {
        ProgressionState State { get; }
        ProgressionStageId CurrentStage { get; }

        bool IsContentUnlocked(string contentId);
        bool CanUnlock(ProgressionUnlockDefinition definition);
        bool TryUnlock(ProgressionUnlockDefinition definition);
        bool CanCompleteEnding(SettlementGoalDefinition goal);
        bool TryCompleteEnding(SettlementGoalDefinition goal);
    }
}
