using _001_Scripts.Core.Pipes;
using _001_Scripts.Core.Pipes.Msgs;
using _001_Scripts.Core.Entity.Pets;
using _001_Scripts.Data;
using _001_Scripts.Managers.Interfaces;
using _001_Scripts.UI.Components;
using _001_Scripts.UI.UILib;
using UnityEngine;

namespace _001_Scripts.Managers
{
    /// <summary>Coordinates the care use case. Rendering and pointer input are uGUI adapters.</summary>
    public sealed class CareManager : ServiceManagerBase<CareManager>, ICareService
    {
        [SerializeField] private CareUIComponent view;
        [SerializeField] private CareStageInput stageInput;

        private CareSession session;
        private readonly CareFlowState flow = new CareFlowState();
        private readonly PetBondState bond = new PetBondState();
        private readonly CareEventDirector eventDirector = new CareEventDirector();
        private CareInspection inspection;
        private _001_Scripts.Core.Entity.Pets.PetSurface surface;
        private readonly System.Collections.Generic.List<RoutineCareRule> routineRules = new System.Collections.Generic.List<RoutineCareRule>();
        private CareEventEncounter activeEvent;
        public CareSession Session => session;
        private CareToolKind selectedTool = CareToolKind.Sprayer;
        private int selectedCondition = -1;
        private string message = "펫의 모습을 살펴보고 수상한 부위를 직접 눌러 보세요.";

        protected override void ProvideServices()
        {
            Provide<ICareService>();
        }

        protected override void SubscribeGamePipes()
        {
            Listen<CareInputRequest>(request =>
            {
                if (request.Input == CareInput.Inspect)
                {
                    if (request.Source == stageInput) InspectCondition(request.Index, request.ScreenPosition);
                    return;
                }
                if (request.Input == CareInput.Stroke)
                {
                    if (request.Source == stageInput) ApplyStroke(request.Index, request.Amount, request.ScreenPosition);
                    return;
                }
                if (request.Source != view) return;
                switch (request.Input)
                {
                    case CareInput.SelectTool: SelectTool(request.Index); break;
                    case CareInput.SelectCondition: SelectCondition(request.Index); break;
                    case CareInput.Reset: ResetCare(); break;
                    case CareInput.EventChoice: ResolveCareEvent(request.Index); break;
                }
            });
        }
        private void Start()
        {
            surface = UnityEngine.Object.FindAnyObjectByType<_001_Scripts.Core.Entity.Pets.PetSurface>();
            ResetCare();
        }

        /// <summary>선택한 도구를 실제 표면 시뮬레이션에 흘려보냅니다.</summary>
        private void PaintSurface(float distance, Vector2 screenPosition)
        {
            if (surface == null || !surface.IsReady) return;
            if (!surface.TryUv(screenPosition, null, out var uv)) return;
            var strength = Mathf.Clamp01(.04f + distance * .010f);
            var direction = screenPosition - lastPaintPosition;
            lastPaintPosition = screenPosition;

            switch (selectedTool)
            {
                case CareToolKind.Sprayer:
                    surface.Apply(new ToolStamp(uv, .16f, strength, direction, SurfaceToolKind.Water));
                    break;
                case CareToolKind.WashBrush:
                    // 문지르면 거품이 일고, 그 거품이 오염을 벗겨 냅니다.
                    surface.Apply(new ToolStamp(uv, .12f, strength, direction, SurfaceToolKind.Soap));
                    surface.Apply(new ToolStamp(uv, .10f, strength, direction, SurfaceToolKind.Brush));
                    break;
                case CareToolKind.Comb:
                case CareToolKind.Scissors:
                    surface.Apply(new ToolStamp(uv, .09f, strength, direction, SurfaceToolKind.Trim));
                    break;
            }
        }

        private void LateUpdate()
        {
            if (session != null)
            {
                flow.Tick(Time.unscaledDeltaTime, Time.unscaledTime);
                view.Render(new CareViewModel(session, selectedTool, selectedCondition, message, flow, activeEvent,
                    inspection, surface == null ? _001_Scripts.Core.Entity.Pets.PetSurfaceState.Fresh : surface.State, bond));
            }
        }

        private Vector2 lastPaintPosition;

        private void SelectTool(int index)
        {
            if (!System.Enum.IsDefined(typeof(CareToolKind), index)) return;
            selectedTool = (CareToolKind)index;
            message = $"{CarePresentation.ToolLabel(selectedTool)} 선택";
        }

        private void SelectCondition(int index)
        {
            if (session == null || index < 0 || index >= session.Conditions.Count) return;
            if (!session.Conditions[index].IsDiscovered)
            {
                selectedCondition = -1;
                message = "아직 모르는 증상입니다. 펫의 외형에서 이상한 부위를 찾아 눌러 보세요.";
                return;
            }
            selectedCondition = index;
            message = $"{session.Conditions[index].Name} 상태를 선택했습니다.";
        }

        private void InspectCondition(int index, Vector2 screenPosition)
        {
            if (session == null || session.IsCompleted) return;
            if (index < 0 || index >= session.Conditions.Count || session.Conditions[index].Resolved)
            {
                selectedCondition = -1;
                message = "이 부위에서는 특별한 증상을 찾지 못했습니다.";
                view.PlayInspection(screenPosition, false);
                return;
            }

            var condition = session.Conditions[index];
            var discovered = condition.Discover();
            selectedCondition = index;
            message = discovered
                ? $"증상 발견! {condition.Name} · {CarePresentation.CareLabel(condition.Care)} 처치가 필요합니다. · {bond.RegisterDiscovery()}"
                : $"확인된 증상: {condition.Name} · {CarePresentation.CareLabel(condition.Care)}";
            view.PlayInspection(screenPosition, discovered);
        }

        private void ApplyStroke(int index, float distance, Vector2 screenPosition)
        {
            if (session == null || session.IsCompleted) return;
            if (activeEvent != null)
            {
                message = "먼저 발생한 케어 이벤트에 대응해 주세요.";
                return;
            }
            PaintSurface(distance, screenPosition);
            // Anywhere that is not an already-found symptom, dragging searches instead of treating.
            if (index < 0 || index >= session.Conditions.Count || !session.Conditions[index].IsDiscovered)
            {
                Sweep(distance, screenPosition);
                return;
            }
            if (ShopRoutineManager.HasInstance) { ApplyRoutineStroke(index, distance, screenPosition); return; }
            selectedCondition = index;
            var result = session.ApplyStroke(session.Conditions[index], selectedTool,
                distance * flow.ProgressMultiplier * bond.ProgressMultiplier);
            message = CarePresentation.InteractionMessage(result);
            ResolveFlowFeedback(result.Status, distance, screenPosition);
            if (result.Status == CareInteractionStatus.StageCompleted)
                TryStartCareEvent(result.Condition);
        }

        /// <summary>케어 솜씨를 주문에 남겨 정산에서 보상으로 바뀌게 합니다.</summary>
        private void RecordCareResult(ShopRoutineManager routine)
        {
            var order = routine.Game.CurrentOrder;
            if (order == null) return;
            var grade = routine.Settings.CareQualityFor(flow.BestCombo);
            order.RecordCareResult(grade.PayoutMultiplier * bond.RewardMultiplier,
                $"{grade.Label} · 신뢰 {Mathf.RoundToInt(bond.Trust)}");
        }

        // 부위별로 직전에 잰 오염입니다. 씻겨 나간 차이만큼만 진행도로 바꿉니다.
        private readonly System.Collections.Generic.Dictionary<int, float> lastZoneDirt =
            new System.Collections.Generic.Dictionary<int, float>();

        /// <summary>이 부위에서 방금 씻어낸 만큼의 진행도입니다. 세척 상태가 아니면 -1입니다.</summary>
        private float MeasuredWashProgress(int index, CareConditionState state, ShopRoutineManager routine)
        {
            if (state.Care != CareKind.Wash) return -1f;
            var dirt = ZoneDirt(index, state);
            if (dirt < 0f) return -1f;
            var previous = lastZoneDirt.TryGetValue(index, out var stored) ? stored : dirt;
            lastZoneDirt[index] = dirt;
            return Mathf.Max(0f, previous - dirt) * routine.Settings.WashProgressGain;
        }

        private bool IsZoneClean(int index, ShopRoutineManager routine)
        {
            var dirt = ZoneDirt(index, session.Conditions[index]);
            return dirt < 0f || dirt <= routine.Settings.WashCleanThreshold;
        }

        private float ZoneDirt(int index, CareConditionState state)
        {
            if (surface == null || !surface.IsReady || view == null) return -1f;
            var stage = view.StageRect;
            if (stage == null) return -1f;
            return surface.TryUvFromStage(stage, state.Zone, out var uvRect) ? surface.SampleDirt(uvRect) : -1f;
        }

        /// <summary>돋보기로 펫을 훑습니다. 가까울수록 반응이 뜨거워지고, 머무르면 증상을 찾아냅니다.</summary>
        private void Sweep(float travel, Vector2 screenPosition)
        {
            if (inspection == null || stageInput == null) return;
            if (inspection.Exhausted)
            {
                message = "오늘은 더 살펴볼 기운이 없어요. 찾은 증상부터 처치해 주세요.";
                return;
            }
            if (!stageInput.TryStageNormalized(screenPosition, out var point)) return;
            var stageWidth = Mathf.Max(1f, stageInput.StageSize.x);
            var found = inspection.Scan(point, travel / stageWidth, Time.unscaledDeltaTime, session.Conditions);
            if (found == null)
            {
                message = CareInspection.HeatLabel(inspection.Heat) +
                          (inspection.Heat == InspectHeat.Hot
                              ? $"  ·  확신 {Mathf.RoundToInt(inspection.Confidence * 100)}%"
                              : string.Empty);
                return;
            }

            found.Discover();
            for (var i = 0; i < session.Conditions.Count; i++)
                if (ReferenceEquals(session.Conditions[i], found)) selectedCondition = i;
            message = $"증상 발견! {found.Name} · {CarePresentation.CareLabel(found.Care)} 처치가 필요합니다. · {bond.RegisterDiscovery()}";
            flow.GrantMomentum(2, Time.unscaledTime);
            view.PlayInspection(screenPosition, true);
        }

        private void ResetCare()
        {
            if (ShopRoutineManager.HasInstance)
            {
                // Reset must never create a second reward-bearing session for an accepted pet.
                if (session != null) return;
                var routine = ShopRoutineManager.Instance;
                var states = new System.Collections.Generic.List<CareConditionState>();
                foreach (var request in routine.Game.CurrentOrder.Requests)
                {
                    var rule = routine.Settings.FindCare(request.Condition);
                    if (rule == null) throw new System.InvalidOperationException($"Missing care rule: {request.Condition.ConditionId}");
                    routineRules.Add(rule);
                    states.Add(rule.CreateState());
                }
                session = new CareSession(states);
                inspection = routine.Settings.CreateInspection();
                flow.Reset();
                bond.Reset(routine.Game.CurrentOrder?.Pet?.VariantId);
                lastZoneDirt.Clear();
                if (surface != null) surface.ResetSurface();
                eventDirector.Reset();
                activeEvent = null;
                return;
            }
            inspection = ShopRoutineManager.HasInstance
                ? ShopRoutineManager.Instance.Settings.CreateInspection()
                : new CareInspection(100f, .30f, .085f, 26f, 1.35f, .9f);
            var source = new DefaultCareConditionSource();
            session = new CareSession(source.Create(
                CareHandoffContext.HasActiveVisit ? CareHandoffContext.HasCondition : null));
            flow.Reset();
            bond.Reset(CareHandoffContext.ActiveOrder?.Pet?.VariantId);
            eventDirector.Reset();
            activeEvent = null;
            selectedTool = CareToolKind.Sprayer;
            selectedCondition = -1;
            message = "펫의 모습을 살펴보고 수상한 부위를 직접 눌러 보세요.";
        }

        private void ApplyRoutineStroke(int index, float distance, Vector2 screenPosition)
        {
            var routine = ShopRoutineManager.Instance;
            if (routine.Game.Status != DayStatus.PetInCare || distance <= 0f || float.IsNaN(distance) || float.IsInfinity(distance)) return;
            var state = session.Conditions[index];
            if (state.Resolved) return;
            var rule = routineRules[index];
            selectedCondition = index;
            if (!state.Accepts(selectedTool))
            {
                message = "상태에 맞는 도구를 선택하세요.";
                ResolveFlowFeedback(CareInteractionStatus.WrongTool, distance, screenPosition);
                return;
            }
            if (!routine.HasSupplyFor(rule))
            {
                // Refuse before any effort is spent so the player never loses work to an empty shelf.
                message = $"{rule.Supply.DisplayName}이(가) 떨어졌어요. 정산 후 보급을 채워 주세요.";
                ResolveFlowFeedback(CareInteractionStatus.WrongTool, distance, screenPosition);
                return;
            }
            if (state.NeedsWater && selectedTool == CareToolKind.Sprayer)
            {
                state.ApplyWater(distance / (rule.WaterEffort * routine.Settings.CareDurationMultiplier));
                message = state.Wetness >= 1f ? "충분히 적셨습니다. 세척솔을 사용하세요." : $"물 적시기 {state.Wetness:P0}";
                ResolveFlowFeedback(CareInteractionStatus.Wetting, distance, screenPosition);
                return;
            }
            if (state.NeedsWater && state.Wetness < 1f)
            {
                message = "먼저 분무기로 적셔 주세요.";
                ResolveFlowFeedback(CareInteractionStatus.NeedsWater, distance, screenPosition);
                return;
            }
            var amount = distance / (rule.Effort * routine.Settings.CareDurationMultiplier) *
                         routine.CareSpeedMultiplier * flow.ProgressMultiplier * bond.ProgressMultiplier;

            // 세척은 추상 진행도가 아니라 표면에서 실제로 씻겨 나간 오염만큼 나아갑니다.
            var washed = MeasuredWashProgress(index, state, routine);
            if (washed >= 0f)
                amount = IsZoneClean(index, routine)
                    // 이미 깨끗해진 자리는 더 벗겨낼 때가 없으므로 마무리 단계가 막히면 안 됩니다.
                    ? Mathf.Max(amount, 1f)
                    : washed * flow.ProgressMultiplier * bond.ProgressMultiplier;

            if (amount < state.Remaining)
            {
                state.ApplyProgress(amount);
                message = $"{state.Name} 케어 중";
                ResolveFlowFeedback(CareInteractionStatus.Progressed, distance, screenPosition);
                return;
            }
            if (washed >= 0f && !IsZoneClean(index, routine))
            {
                // 진행도만 채우고 실제로는 때가 남은 상태에서는 끝낼 수 없습니다.
                state.ApplyProgress(Mathf.Max(0f, state.Remaining - .02f));
                message = "아직 때가 남았어요. 거품을 내고 남은 자리를 문질러 주세요.";
                ResolveFlowFeedback(CareInteractionStatus.Progressed, distance, screenPosition);
                return;
            }
            if (state.CompletedPasses + 1 < state.RequiredPasses)
            {
                state.ApplyProgress(1f);
                message = $"{state.Name} 단계 완료! 다음은 {state.CurrentStageName}입니다.";
                ResolveFlowFeedback(CareInteractionStatus.StageCompleted, distance, screenPosition);
                TryStartCareEvent(state);
                return;
            }
            var request = routine.Game.CurrentOrder.Requests[index];
            if (!routine.ConsumeSupplyFor(rule))
            {
                message = $"{rule.Supply.DisplayName}이(가) 떨어졌어요.";
                return;
            }
            if (!routine.Game.TryBeginTool(rule.DomainTool, request, out var toolSession)) return;
            if (!toolSession.IsReadyToComplete)
                routine.Game.ApplyToolInput(request.Condition.InteractionMode, 1f);
            if (!routine.Game.TryCompleteTool(out var result)) { routine.Game.CancelActiveTool(); return; }
            state.ApplyProgress(1f);
            session.RegisterResolved(state);
            foreach (var stack in result.CareReward.GrantedItems)
                session.RecordByproduct($"{stack.Item.DisplayName} x{stack.Amount}");
            foreach (var stack in result.CareReward.RejectedItems)
                session.RecordByproduct($"보관함 부족: {stack.Item.DisplayName} x{stack.Amount} 미획득");
            if (session.IsCompleted) RecordCareResult(routine);
            message = session.IsCompleted ? "케어 완료! 아래에서 펫을 돌려주세요." : $"{state.Name} 해결";
            ResolveFlowFeedback(CareInteractionStatus.Resolved, distance, screenPosition);
        }

        private void ResolveFlowFeedback(CareInteractionStatus status, float distance, Vector2 screenPosition)
        {
            var correct = status == CareInteractionStatus.Wetting ||
                          status == CareInteractionStatus.Progressed ||
                          status == CareInteractionStatus.StageCompleted ||
                          status == CareInteractionStatus.Resolved;
            var feedback = correct
                ? flow.RegisterSuccess(distance, Time.unscaledTime, status == CareInteractionStatus.Resolved)
                : flow.BreakCombo();
            var reaction = bond.RegisterInteraction(status, distance);
            if (feedback == CareFlowFeedback.Fever)
                message = "피버 타임! 지금은 케어 속도가 1.75배입니다.";
            if (!string.IsNullOrEmpty(reaction)) message += " · " + reaction;
            view.PlayFeedback(screenPosition, status, feedback, flow);
        }

        private void TryStartCareEvent(CareConditionState condition)
        {
            if (activeEvent != null || !eventDirector.TryCreate(condition, out activeEvent)) return;
            message = $"케어 이벤트 발생 · {activeEvent.Title}";
        }

        private void ResolveCareEvent(int choiceIndex)
        {
            if (activeEvent == null || !activeEvent.TryChoose(choiceIndex, out var choice)) return;
            var condition = activeEvent.Condition;
            condition.ApplyAssistProgress(choice.AssistProgress);
            var feedbackResult = flow.GrantMomentum(choice.FlowBeats, Time.unscaledTime);
            message = choice.Result + " · " + bond.RegisterEventChoice(choice.AssistProgress, choice.FlowBeats);
            view.PlayEventOutcome(condition, feedbackResult, flow);
            activeEvent = null;
        }

        public void Configure(CareUIComponent careView, CareStageInput input)
        {
            view = careView;
            stageInput = input;
        }
    }
}
