using _001_Scripts.Core.Pipes;
using _001_Scripts.Core.Pipes.Msgs;
using _001_Scripts.Data;
using _001_Scripts.UI.Components;
using _001_Scripts.UI.UILib;
using UnityEngine;

namespace _001_Scripts.Managers
{
    /// <summary>Coordinates the care use case. Rendering and pointer input are uGUI adapters.</summary>
    public sealed class CareManager : SinManagerBase<CareManager>
    {
        [SerializeField] private CareUIComponent view;
        [SerializeField] private CareStageInput stageInput;

        private CareSession session;
        private CareToolKind selectedTool = CareToolKind.Sprayer;
        private int selectedCondition = -1;
        private string message = "상태를 확인하고 알맞은 도구를 선택하세요.";

        protected override void SubscribeGamePipes()
        {
            Listen<CareInputRequest>(request =>
            {
                if (request.Input == CareInput.Stroke)
                {
                    if (request.Source == stageInput) ApplyStroke(request.Index, request.Amount);
                    return;
                }
                if (request.Source != view) return;
                switch (request.Input)
                {
                    case CareInput.SelectTool: SelectTool(request.Index); break;
                    case CareInput.SelectCondition: SelectCondition(request.Index); break;
                    case CareInput.Reset: ResetCare(); break;
                }
            });
        }
        private void Start() => ResetCare();

        private void LateUpdate()
        {
            if (session != null)
                view.Render(new CareViewModel(session, selectedTool, selectedCondition, message));
        }

        private void SelectTool(int index)
        {
            if (!System.Enum.IsDefined(typeof(CareToolKind), index)) return;
            selectedTool = (CareToolKind)index;
            message = $"{CarePresentation.ToolLabel(selectedTool)} 선택";
        }

        private void SelectCondition(int index)
        {
            if (session == null || index < 0 || index >= session.Conditions.Count) return;
            selectedCondition = index;
            message = $"{session.Conditions[index].Name} 상태를 선택했습니다.";
        }

        private void ApplyStroke(int index, float distance)
        {
            if (session == null || session.IsCompleted || index < 0 || index >= session.Conditions.Count) return;
            selectedCondition = index;
            var result = session.ApplyStroke(session.Conditions[index], selectedTool, distance);
            message = CarePresentation.InteractionMessage(result);
        }

        private void ResetCare()
        {
            var source = new DefaultCareConditionSource();
            session = new CareSession(source.Create(
                CareHandoffContext.HasActiveVisit ? CareHandoffContext.HasCondition : null));
            selectedTool = CareToolKind.Sprayer;
            selectedCondition = -1;
            message = "상태를 확인하고 알맞은 도구를 선택하세요.";
        }

        public void Configure(CareUIComponent careView, CareStageInput input)
        {
            view = careView;
            stageInput = input;
        }
    }
}
