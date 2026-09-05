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
        public int DiscoveredCount { get; }
        public bool Completed { get; }
        public string Message { get; }
        public CareFlowState Flow { get; }
        public CareEventEncounter ActiveEvent { get; }
        public CareInspection Inspection { get; }
        public PetBondState Bond { get; }
        public _001_Scripts.Core.Entity.Pets.PetSurfaceState Surface { get; }

        public CareViewModel(
            CareSession session, CareToolKind selectedTool, int selectedCondition, string message,
            CareFlowState flow = null, CareEventEncounter activeEvent = null, CareInspection inspection = null,
            _001_Scripts.Core.Entity.Pets.PetSurfaceState surface = default, PetBondState bond = null)
        {
            Conditions = session.Conditions;
            Byproducts = session.Byproducts;
            SelectedTool = selectedTool;
            SelectedCondition = selectedCondition;
            RemainingCount = session.RemainingCount;
            for (var i = 0; i < Conditions.Count; i++)
                if (Conditions[i].IsDiscovered) DiscoveredCount++;
            Completed = session.IsCompleted;
            Message = message ?? string.Empty;
            Flow = flow;
            ActiveEvent = activeEvent;
            Inspection = inspection;
            Surface = surface;
            Bond = bond;
        }
    }

}
