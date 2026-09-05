using System;
using _001_Scripts.Core;
using _001_Scripts.Core.Pipes;
using _001_Scripts.Core.Pipes.Msgs;
using _001_Scripts.Data;
using _001_Scripts.UI.UILib;
using UnityEngine;
using UnityEngine.UI;

namespace _001_Scripts.UI.Components
{
    /// <summary>Passive uGUI care view. All referenced objects are serialized in the scene.</summary>
    public sealed class CareUIComponent : GameBehaviour
    {
        [SerializeField] private Text titleText;
        [SerializeField] private Text remainingText;
        [SerializeField] private Text messageText;
        [SerializeField] private Text byproductText;
        [SerializeField] private RectTransform stage;
        [SerializeField] private PetLayeredVisual petVisual;
        private Image petPortrait;
        [SerializeField] private Button[] toolButtons;
        [SerializeField] private Image[] toolBackgrounds;
        [SerializeField] private Button[] conditionButtons;
        [SerializeField] private Text[] conditionNames;
        [SerializeField] private Text[] conditionCareLabels;
        [SerializeField] private Slider[] conditionProgress;
        [SerializeField] private RectTransform[] conditionMarks;
        [SerializeField] private Image[] conditionMarkImages;
        [SerializeField] private Text[] conditionMarkLabels;
        [Tooltip("마커 안에 들어가는 케어 아이콘입니다. 비어 있으면 예전 글리프를 씁니다.")]
        [SerializeField] private Image[] conditionMarkIcons = new Image[0];
        [SerializeField] private _001_Scripts.UI.Theme.UITheme theme;
        private UnityEngine.UI.Image scanTrack;
        private UnityEngine.UI.Image scanFill;
        private UnityEngine.UI.Image confidenceFill;
        private Text scanLabel;
        private Text surfaceLabel;
        [SerializeField] private Button resetButton;
        [SerializeField] private GameObject completionPanel;
        [SerializeField] private Text completionByproductText;
        private CareFeedbackVfx feedback;
        private CareEventPanel eventPanel;

        private void Awake()
        {
            feedback = GetComponent<CareFeedbackVfx>();
            if (feedback == null) feedback = gameObject.AddComponent<CareFeedbackVfx>();
            feedback.SetTheme(theme);
            feedback.Initialize(stage != null ? stage : (RectTransform)transform);
            eventPanel = GetComponent<CareEventPanel>();
            if (eventPanel == null) eventPanel = gameObject.AddComponent<CareEventPanel>();
            eventPanel.SetTheme(theme);
            eventPanel.Initialize(this);
            for (var i = 0; i < toolButtons.Length; i++)
            {
                var captured = i;
                toolButtons[i].onClick.AddListener(() => GamePipe.Publish(new CareInputRequest(this, CareInput.SelectTool, captured, 0f)));
            }
            for (var i = 0; i < conditionButtons.Length; i++)
            {
                var captured = i;
                conditionButtons[i].onClick.AddListener(() => GamePipe.Publish(new CareInputRequest(this, CareInput.SelectCondition, captured, 0f)));
            }
            resetButton.onClick.AddListener(() => GamePipe.Publish(new CareInputRequest(this, CareInput.Reset, -1, 0f)));
        }

        /// <summary>부위 좌표를 표면 UV로 옮길 때 기준이 되는 스테이지입니다.</summary>
        public RectTransform StageRect => stage;

        public void Render(CareViewModel model)
        {
            titleText.text = CareHandoffContext.HasActiveVisit
                ? $"{CareHandoffContext.PetName}의 케어룸"
                : "포근포근 케어룸";
            remainingText.text = model.Completed
                ? "모든 케어 완료"
                : $"발견 {model.DiscoveredCount} / {model.Conditions.Count}  ·  남은 케어 {model.RemainingCount}";
            messageText.text = model.Message;
            byproductText.text = model.Byproducts.Count == 0 ? "아직 없음" : string.Join(" · ", model.Byproducts);

            for (var i = 0; i < toolButtons.Length; i++)
                toolBackgrounds[i].color = i == (int)model.SelectedTool
                    ? new Color32(210, 238, 228, 255)
                    : new Color32(247, 244, 235, 255);

            for (var i = 0; i < conditionButtons.Length; i++)
            {
                var visible = i < model.Conditions.Count;
                conditionButtons[i].gameObject.SetActive(visible);
                conditionMarks[i].gameObject.SetActive(visible && !model.Conditions[i].Resolved);
                if (!visible) continue;

                var condition = model.Conditions[i];
                PositionMark(conditionMarks[i], condition.Zone);
                if (!condition.IsDiscovered)
                {
                    conditionNames[i].text = "???";
                    conditionCareLabels[i].text = "펫에서 직접 찾기";
                    conditionProgress[i].value = 0f;
                    conditionMarkImages[i].color = Color.clear;
                    conditionMarkLabels[i].text = string.Empty;
                    // The icon has no sprite until the condition is found; leaving it enabled
                    // would draw a blank white quad on the pet.
                    SetMarkIcon(i, null);
                    continue;
                }

                conditionNames[i].text = condition.Name;
                conditionCareLabels[i].text = condition.Resolved
                    ? "해결"
                    : $"{CarePresentation.CareLabel(condition.Care)} {condition.CurrentPass}/{condition.RequiredPasses} · {condition.CurrentStageName}";
                conditionProgress[i].value = condition.Progress01;
                conditionMarkImages[i].color = MarkColor(condition.Care, model.SelectedCondition == i);
                if (theme != null && i < conditionMarkIcons.Length && conditionMarkIcons[i] != null)
                    SetMarkIcon(i, theme.CareIcon(condition.Care));
                else conditionMarkLabels[i].text = MarkGlyph(condition.Care);
            }

            RenderInspection(model);
            RenderSurface(model);

            RenderPet(model);
            feedback.Render(model.Flow, model.Bond);
            eventPanel.Render(model.ActiveEvent);

            completionPanel.SetActive(model.Completed);
            completionByproductText.text = (model.Byproducts.Count == 0
                ? "획득한 부산물이 없습니다."
                : "획득 부산물  " + string.Join(" · ", model.Byproducts)) +
                (model.Completed && model.Flow != null ? $"\n케어 등급  {model.Flow.Grade}  ·  {model.Flow.Score:N0}점" : string.Empty);
        }

        /// <summary>
        /// 주문에 전용 초상이 있는 일반 펫은 그 이미지를 사용합니다. 기존 크리스탈 강아지는
        /// 상태별 파츠가 있는 조립형 비주얼을 그대로 유지합니다.
        /// </summary>
        private void RenderPet(CareViewModel model)
        {
            var pet = CareHandoffContext.ActiveOrder?.Pet;
            var usePortrait = pet != null && pet.Icon != null && pet.VariantId != "crystal_dog";

            if (petVisual != null)
            {
                petVisual.gameObject.SetActive(!usePortrait);
                if (!usePortrait) petVisual.Render(model);
            }

            if (!usePortrait)
            {
                if (petPortrait != null) petPortrait.gameObject.SetActive(false);
                return;
            }

            EnsurePetPortrait();
            petPortrait.sprite = pet.Icon;
            petPortrait.gameObject.SetActive(true);
        }

        private void EnsurePetPortrait()
        {
            if (petPortrait != null || stage == null) return;

            petPortrait = new GameObject("Active Pet Portrait", typeof(RectTransform),
                    typeof(CanvasRenderer), typeof(Image))
                .GetComponent<Image>();
            var rect = petPortrait.rectTransform;
            rect.SetParent(stage, false);
            rect.anchorMin = new Vector2(.08f, .04f);
            rect.anchorMax = new Vector2(.92f, .96f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.pivot = new Vector2(.5f, .5f);
            petPortrait.preserveAspect = true;
            petPortrait.raycastTarget = false;

            // 증상 마커가 초상 위에 그려지도록 조립형 펫 바로 다음 순서에 둡니다.
            var sibling = petVisual == null ? 0 : petVisual.transform.GetSiblingIndex() + 1;
            rect.SetSiblingIndex(sibling);
        }

        public void PlayFeedback(Vector2 screenPosition, CareInteractionStatus status,
            CareFlowFeedback flowFeedback, CareFlowState flow)
        {
            if (feedback != null) feedback.Emit(screenPosition, status, flowFeedback, flow);
        }

        public void PlayInspection(Vector2 screenPosition, bool discovered)
        {
            if (feedback != null) feedback.EmitInspection(screenPosition, discovered);
        }

        public void PlayEventOutcome(CareConditionState condition, CareFlowFeedback flowFeedback, CareFlowState flow)
        {
            if (feedback == null || condition == null || stage == null) return;
            var eventCamera = GetComponentInParent<Canvas>()?.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : GetComponentInParent<Canvas>()?.worldCamera;
            var screenPosition = RectTransformUtility.WorldToScreenPoint(eventCamera, stage.position);
            feedback.Emit(screenPosition, CareInteractionStatus.Progressed, flowFeedback, flow);
        }

        /// <summary>진찰력과 반응 온도를 스테이지 아래 띠로 보여 줍니다.</summary>
        private void RenderInspection(CareViewModel model)
        {
            if (theme == null || stage == null) return;
            if (scanTrack == null) BuildInspectionStrip();
            var inspection = model.Inspection;
            var show = inspection != null && !model.Completed && model.DiscoveredCount < model.Conditions.Count;
            scanTrack.transform.parent.gameObject.SetActive(show);
            if (!show) return;

            scanFill.rectTransform.anchorMax = new Vector2(inspection.Stamina01, 1f);
            scanFill.color = inspection.Exhausted ? theme.Blush
                : inspection.Stamina01 < .3f ? theme.Gold : theme.SageDeep;
            confidenceFill.rectTransform.anchorMax = new Vector2(inspection.Confidence, 1f);
            confidenceFill.color = HeatColor(inspection.Heat);
            scanLabel.text = inspection.Exhausted
                ? "진찰력 소진 · 찾은 증상부터 처치하세요"
                : $"진찰력 {Mathf.RoundToInt(inspection.Stamina01 * 100)}%   ·   {CareInspection.HeatLabel(inspection.Heat)}";
            scanLabel.color = inspection.Heat == InspectHeat.Hot ? theme.Ink : theme.InkSoft;
        }

        /// <summary>표면 시뮬레이션에서 되읽은 실제 수치입니다.</summary>
        private void RenderSurface(CareViewModel model)
        {
            if (surfaceLabel == null) return;
            var surface = model.Surface;
            surfaceLabel.text =
                $"청결도 {surface.Cleanliness:P0}   ·   건조도 {surface.Dryness:P0}   ·   " +
                $"정돈도 {surface.FurOrder:P0}   ·   거품 {surface.Foam:P0}";
        }

        private Color HeatColor(InspectHeat heat) => heat switch
        {
            InspectHeat.Hot => theme.Blush,
            InspectHeat.Warm => theme.Gold,
            InspectHeat.Cool => theme.SageDeep,
            _ => new Color(theme.InkFaint.r, theme.InkFaint.g, theme.InkFaint.b, .45f)
        };

        private void BuildInspectionStrip()
        {
            var root = _001_Scripts.UI.Theme.ThemedUIBuilder.Surface("Inspection Strip", stage.parent, theme,
                theme.Card, theme.PaperWarm, new Vector2(0f, -318f), new Vector2(716f, 74f));
            root.raycastTarget = false;

            scanLabel = _001_Scripts.UI.Theme.ThemedUIBuilder.Label("Hint", root.transform, theme, "", 14,
                theme.InkSoft, TextAnchor.MiddleLeft, new Vector2(2f, 17f), new Vector2(660f, 22f));

            var readout = _001_Scripts.UI.Theme.ThemedUIBuilder.Surface("Surface Readout", stage.parent, theme,
                theme.Card, theme.PaperWarm, new Vector2(0f, 262f), new Vector2(716f, 34f));
            readout.raycastTarget = false;
            surfaceLabel = _001_Scripts.UI.Theme.ThemedUIBuilder.Label("Values", readout.transform, theme, "", 14,
                theme.InkSoft, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(690f, 26f));

            scanTrack = _001_Scripts.UI.Theme.ThemedUIBuilder.Surface("Stamina Track", root.transform, theme,
                theme.Chip, new Color(theme.Ink.r, theme.Ink.g, theme.Ink.b, .10f),
                new Vector2(0f, -6f), new Vector2(664f, 10f));
            scanTrack.pixelsPerUnitMultiplier = 3.4f;
            scanFill = Bar(scanTrack.transform, "Stamina Fill", theme.SageDeep);

            var confidenceTrack = _001_Scripts.UI.Theme.ThemedUIBuilder.Surface("Confidence Track", root.transform,
                theme, theme.Chip, new Color(theme.Ink.r, theme.Ink.g, theme.Ink.b, .08f),
                new Vector2(0f, -24f), new Vector2(664f, 8f));
            confidenceTrack.pixelsPerUnitMultiplier = 3.4f;
            confidenceFill = Bar(confidenceTrack.transform, "Confidence Fill", theme.Blush);
        }

        private UnityEngine.UI.Image Bar(Transform parent, string name, Color color)
        {
            var image = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(UnityEngine.UI.Image))
                .GetComponent<UnityEngine.UI.Image>();
            var rect = image.rectTransform;
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = new Vector2(0f, 1f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            image.sprite = theme.Chip;
            image.type = UnityEngine.UI.Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = 3.4f;
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private void PositionMark(RectTransform mark, Rect zone)
        {
            var size = stage.rect.size;
            mark.anchorMin = Vector2.zero;
            mark.anchorMax = Vector2.zero;
            mark.pivot = Vector2.zero;
            mark.anchoredPosition = new Vector2(zone.x * size.x, (1f - zone.y - zone.height) * size.y);
            mark.sizeDelta = new Vector2(zone.width * size.x, zone.height * size.y);
        }

        /// <summary>마커 아이콘을 갈아 끼우고, 스프라이트가 없으면 숨깁니다.</summary>
        private void SetMarkIcon(int index, Sprite sprite)
        {
            if (index >= conditionMarkIcons.Length) return;
            var icon = conditionMarkIcons[index];
            if (icon == null) return;
            icon.sprite = sprite;
            icon.enabled = sprite != null;
        }

        private Color MarkColor(CareKind care, bool selected)
        {
            if (theme != null)
            {
                var themed = theme.CareColor(care);
                themed.a = selected ? .78f : .52f;
                return themed;
            }
            var color = care switch
            {
                CareKind.Wash => new Color32(74, 158, 210, 220),
                CareKind.Brush => new Color32(242, 190, 83, 220),
                CareKind.Treat => new Color32(238, 126, 104, 220),
                CareKind.Remove => new Color32(131, 117, 181, 220),
                CareKind.Trim => new Color32(105, 192, 166, 220),
                _ => new Color32(80, 90, 100, 220)
            };
            if (selected) color.a = 255;
            return color;
        }

        private static string MarkGlyph(CareKind care) => care switch
        {
            CareKind.Wash => "泥",
            CareKind.Brush => "털",
            CareKind.Treat => "+",
            CareKind.Remove => "◆",
            CareKind.Trim => "긴",
            _ => "?"
        };

        public void Configure(
            Text title, Text remaining, Text message, Text byproducts, RectTransform stageRoot,
            Button[] tools, Image[] toolImages, Button[] conditions, Text[] names, Text[] careLabels,
            Slider[] progress, RectTransform[] marks, Image[] markImages, Text[] markLabels,
            Button reset, GameObject completion, Text completionByproducts, PetLayeredVisual pet = null)
        {
            petVisual = pet;
            titleText = title;
            remainingText = remaining;
            messageText = message;
            byproductText = byproducts;
            stage = stageRoot;
            toolButtons = tools;
            toolBackgrounds = toolImages;
            conditionButtons = conditions;
            conditionNames = names;
            conditionCareLabels = careLabels;
            conditionProgress = progress;
            conditionMarks = marks;
            conditionMarkImages = markImages;
            conditionMarkLabels = markLabels;
            resetButton = reset;
            completionPanel = completion;
            completionByproductText = completionByproducts;
        }
    }
}
