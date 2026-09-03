using System.Collections.Generic;
using _001_Scripts.Data;

namespace _001_Scripts.UI.UILib
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

}
