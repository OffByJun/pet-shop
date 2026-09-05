using System.Collections.Generic;
using System.Text;
using _001_Scripts.Data;
using _001_Scripts.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace _001_Scripts.UI
{
    /// <summary>Serialized uGUI shell around the existing reception and care scenes.</summary>
    public sealed class ShopRoutineUI : MonoBehaviour
    {
        private const string BackgroundSpritePath = "UI/ShopTheme/cozy-shop-background";
        private const string ButtonSpritePath = "UI/ShopTheme/paw-button";
        private const string CrestSpritePath = "UI/ShopTheme/paw-header-crest";
        private static readonly Color Ink = new Color32(45, 65, 60, 255);
        private static readonly Color WarmCream = new Color32(255, 249, 232, 218);
        private static readonly Color WarmShadow = new Color32(88, 59, 37, 90);
        private static Sprite croppedButtonSprite;
        [Tooltip("설정하면 버튼 스킨을 UITheme에 맡기고 여기서는 배경만 꾸밉니다.")]
        [SerializeField] private _001_Scripts.UI.Theme.UITheme theme;
        [SerializeField] private ShopRoutineManager routine;
        [SerializeField] private GameObject panel;
        [SerializeField] private Text title;
        [SerializeField] private Text body;
        [SerializeField] private Text hud;
        [SerializeField] private Image accent;
        [SerializeField] private Image decorationArt;
        [SerializeField] private Button primary;
        [SerializeField] private Text primaryLabel;
        [SerializeField] private Button sell;
        [SerializeField] private Button returnPet;
        [SerializeField] private Button mainMenu;
        [SerializeField] private Transform upgradeList;
        [SerializeField] private Transform decorationList;
        [SerializeField] private Transform supplyList;
        [SerializeField] private Button cardTemplate;
        private readonly List<Button> upgradeButtons = new List<Button>();
        private readonly List<Button> decorationButtons = new List<Button>();
        private readonly List<Button> supplyButtons = new List<Button>();
        private _001_Scripts.UI.Shell.DaySettlementPanel settlement;
        private bool cardsBuilt;

        private void Start()
        {
            primary.onClick.AddListener(Advance);
            sell.onClick.AddListener(routine.SellByproducts);
            returnPet.onClick.AddListener(() => routine.ReturnPet());
            if (mainMenu != null) mainMenu.onClick.AddListener(() => routine.ReturnToMainMenu());
            BuildCards();
            ApplyGeneratedTheme();
            if (theme != null)
            {
                settlement = GetComponent<_001_Scripts.UI.Shell.DaySettlementPanel>();
                if (settlement == null) settlement = gameObject.AddComponent<_001_Scripts.UI.Shell.DaySettlementPanel>();
                settlement.Initialize(routine, panel.transform, theme);
            }
        }

        private void ApplyGeneratedTheme()
        {
            var panelImage = panel.GetComponent<Image>();
            if (panelImage != null)
            {
                panelImage.color = Color.clear;
                panelImage.raycastTarget = false;
            }

            var backgroundSprite = Resources.Load<Sprite>(BackgroundSpritePath);
            if (backgroundSprite != null && panel.transform.Find("Generated Shop Background") == null)
            {
                // Keep the hub backdrop under the hub panel so it is hidden together with the
                // panel while reception and care scenes are active.
                var background = CreateImage(panel.transform, "Generated Shop Background", backgroundSprite);
                Stretch(background.rectTransform, Vector2.zero, Vector2.zero);
                background.preserveAspect = false;
                background.transform.SetAsFirstSibling();
            }

            if (panel.transform.Find("Generated Journal Card") == null)
            {
                var card = CreateImage(panel.transform, "Generated Journal Card", null);
                card.color = WarmCream;
                card.raycastTarget = false;
                var rect = card.rectTransform;
                rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
                rect.anchoredPosition = new Vector2(0f, 28f);
                rect.sizeDelta = new Vector2(1320f, 700f);
                var shadow = card.gameObject.AddComponent<Shadow>();
                shadow.effectColor = WarmShadow;
                shadow.effectDistance = new Vector2(0f, -10f);
                card.transform.SetSiblingIndex(1);
            }

            var crestSprite = Resources.Load<Sprite>(CrestSpritePath);
            if (crestSprite != null && panel.transform.Find("Generated Paw Crest") == null)
            {
                var crest = CreateImage(panel.transform, "Generated Paw Crest", crestSprite);
                var rect = crest.rectTransform;
                rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
                rect.anchoredPosition = new Vector2(-220f, 365f);
                rect.sizeDelta = new Vector2(54f, 54f);
                crest.preserveAspect = true;
                crest.raycastTarget = false;
            }

            // With a theme asset the buttons are already skinned by the editor pass, and re-skinning
            // every one with the ornate paw plaque is what flattened the hierarchy in the first place.
            if (theme == null)
            {
                var buttonSprite = LoadButtonSprite();
                var buttons = panel.transform.parent.GetComponentsInChildren<Button>(true);
                for (var i = 0; i < buttons.Length; i++) SkinButton(buttons[i], buttonSprite);
            }

            SetButtonLayout(primary, new Vector2(0f, -275f), new Vector2(640f, 82f));
            SetButtonLayout(sell, new Vector2(0f, -210f), new Vector2(440f, 70f));
            SetButtonLayout(mainMenu, new Vector2(510f, 335f), new Vector2(220f, 60f));
            SetButtonLayout(returnPet, new Vector2(625f, 0f), new Vector2(280f, 56f));

            if (theme != null) return;
            var labels = panel.GetComponentsInChildren<Text>(true);
            for (var i = 0; i < labels.Length; i++)
            {
                labels[i].color = Ink;
                if (labels[i].GetComponent<Shadow>() != null) continue;
                var shadow = labels[i].gameObject.AddComponent<Shadow>();
                shadow.effectColor = new Color32(255, 253, 241, 180);
                shadow.effectDistance = new Vector2(1f, -1f);
            }
        }

        private static Image CreateImage(Transform parent, string name, Sprite sprite)
        {
            var image = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image))
                .GetComponent<Image>();
            image.transform.SetParent(parent, false);
            image.sprite = sprite;
            return image;
        }

        private static void Stretch(RectTransform rect, Vector2 minimumOffset, Vector2 maximumOffset)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = minimumOffset;
            rect.offsetMax = maximumOffset;
        }

        private static Sprite LoadButtonSprite()
        {
            if (croppedButtonSprite != null) return croppedButtonSprite;
            var source = Resources.Load<Sprite>(ButtonSpritePath);
            if (source == null || source.texture == null) return source;
            var texture = source.texture;
            // The painted source intentionally has generous transparent export padding. Crop that
            // padding at runtime so short buttons retain their wooden rim when nine-sliced.
            var rect = new Rect(texture.width * .015f, texture.height * .16f,
                texture.width * .97f, texture.height * .71f);
            var border = new Vector4(rect.width * .14f, rect.height * .34f,
                rect.width * .14f, rect.height * .34f);
            croppedButtonSprite = Sprite.Create(texture, rect, new Vector2(.5f, .5f),
                source.pixelsPerUnit, 0, SpriteMeshType.FullRect, border);
            croppedButtonSprite.name = "paw-button-cropped";
            croppedButtonSprite.hideFlags = HideFlags.HideAndDontSave;
            return croppedButtonSprite;
        }

        private static void SetButtonLayout(Button button, Vector2 position, Vector2 size)
        {
            if (button == null) return;
            var rect = button.GetComponent<RectTransform>();
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static void SkinButton(Button button, Sprite sprite)
        {
            if (button == null) return;
            var image = button.targetGraphic as Image ?? button.GetComponent<Image>();
            if (image != null && sprite != null)
            {
                image.sprite = sprite;
                image.type = Image.Type.Sliced;
                image.color = Color.white;
                image.pixelsPerUnitMultiplier = 1f;
            }
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color32(255, 245, 218, 255);
            colors.pressedColor = new Color32(220, 205, 169, 255);
            colors.selectedColor = colors.highlightedColor;
            colors.disabledColor = new Color32(180, 180, 180, 145);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = .12f;
            button.colors = colors;
            if (button.GetComponent<Shadow>() == null)
            {
                var shadow = button.gameObject.AddComponent<Shadow>();
                shadow.effectColor = WarmShadow;
                shadow.effectDistance = new Vector2(0f, -4f);
            }
        }

        private void BuildCards()
        {
            if (cardsBuilt) return;
            cardsBuilt = true;
            if (routine.Game.ProgressionCatalog != null)
                foreach (var unlock in routine.Game.ProgressionCatalog.Unlocks)
                {
                    var captured = unlock;
                    var button = Instantiate(cardTemplate, upgradeList);
                    button.gameObject.SetActive(true);
                    button.GetComponentInChildren<Text>().text = $"{unlock.DisplayName}  ·  {unlock.Cost:N0} G";
                    button.onClick.AddListener(() => routine.PurchaseUpgrade(captured));
                    upgradeButtons.Add(button);
                }
            foreach (var decoration in routine.Settings.Decorations)
            {
                var captured = decoration;
                var button = Instantiate(cardTemplate, decorationList);
                button.gameObject.SetActive(true);
                button.GetComponentInChildren<Text>().text = $"{decoration.DisplayName}  ·  {decoration.Cost:N0} G";
                button.onClick.AddListener(() => routine.SelectDecoration(captured));
                decorationButtons.Add(button);
            }
            if (supplyList == null) return;
            foreach (var supply in routine.Settings.Supplies)
            {
                var captured = supply;
                var button = Instantiate(cardTemplate, supplyList);
                button.gameObject.SetActive(true);
                button.GetComponentInChildren<Text>().text = supply.DisplayName;
                button.onClick.AddListener(() => routine.PurchaseSupply(captured));
                supplyButtons.Add(button);
            }
        }

        private void Update()
        {
            var game = routine.Game;
            var state = game.Status;
            var improving = routine.IsImproving;
            var full = state != DayStatus.CustomerArrived && state != DayStatus.PetInCare;
            panel.SetActive(full || routine.IsLoading);
            upgradeList.gameObject.SetActive(improving);
            decorationList.gameObject.SetActive(improving);
            if (supplyList != null) supplyList.gameObject.SetActive(improving);
            primary.gameObject.SetActive(full);
            primary.interactable = !routine.IsLoading;
            sell.gameObject.SetActive(state == DayStatus.EndOfDaySettlement);
            sell.interactable = routine.Inventory.Stacks.Count > 0;
            returnPet.gameObject.SetActive(state == DayStatus.PetInCare && !routine.IsLoading);
            // Leaving is only offered between customers, never while a pet is on the care table.
            if (mainMenu != null)
            {
                mainMenu.gameObject.SetActive(full && !routine.IsLoading);
                mainMenu.interactable = !routine.IsLoading;
            }
            returnPet.interactable = game.CurrentOrder != null && game.CurrentOrder.ResolvedRequiredCount == game.CurrentOrder.RequiredCount && game.ActiveToolSession == null;
            var seconds = Mathf.CeilToInt(game.RemainingBusinessSeconds);
            hud.text = $"DAY {game.DayNumber:00}     {seconds / 60:00}:{seconds % 60:00}     {routine.Inventory.Balance:N0} G     보관함 {routine.Inventory.Stacks.Count}/{routine.Inventory.Capacity}     평판 {routine.Reputation.Points} · {routine.ReputationTier.Title}";
            var goal = routine.DailyGoal;
            var earned = routine.DailyEarned;
            if (state != DayStatus.Closed)
                hud.text += $"     목표 {earned:N0} / {goal:N0} G" + (earned >= goal ? " 달성" : "");
            var shortage = LowSupplies();
            if (!string.IsNullOrEmpty(shortage)) hud.text += $"     보급 부족: {shortage}";
            if (game.IsClosingTime && state == DayStatus.PetInCare) hud.text += "     마감 · 맡은 펫 케어 후 정산";
            if (routine.Decoration != null)
            {
                accent.color = routine.Decoration.AccentColor;
                decorationArt.sprite = routine.Decoration.Artwork;
                decorationArt.gameObject.SetActive(decorationArt.sprite != null);
                hud.text += $"     {routine.Decoration.DisplayName}";
            }
            if (settlement != null)
                settlement.Render(state == DayStatus.EndOfDaySettlement && !routine.IsLoading);
            if (routine.IsLoading) { title.text = "가게로 이동 중…"; body.text = ""; return; }
            switch (state)
            {
                case DayStatus.Closed:
                    title.text = improving ? "내일을 준비하는 시간" : "포근포근 펫샵";
                    body.text = improving
                        ? $"보급을 채우고 가게를 손보세요.\n보유금 {routine.Inventory.Balance:N0} G · 케어 속도 {routine.CareSpeedMultiplier:P0}\n평판 {routine.Reputation.Points} · {routine.ReputationTier.Title} · 내일 손님 {Signed(routine.ReputationTier.ExtraCustomers)}명\n\n{SupplyLine()}"
                        : "작은 돌봄이 쌓여, 나만의 가게가 됩니다.\n\n손님 맞이 → 펫 케어 → 부산물 획득\n펫 돌려주기 → 결제 → 하루 정산 → 보급과 꾸미기";
                    primaryLabel.text = improving ? "다음 날 영업 시작" : "영업 시작";
                    break;
                case DayStatus.AwaitingPayment:
                    title.text = "펫을 손님에게 돌려주었습니다";
                    body.text = $"{game.CurrentOrder.Customer.DisplayName} · {game.CurrentOrder.Pet.DisplayName}\n필수 케어 {game.CurrentOrder.ResolvedRequiredCount}/{game.CurrentOrder.RequiredCount}\n선택 케어 {game.CurrentOrder.ResolvedOptionalCount}/{game.CurrentOrder.OptionalCount}\n\n서비스 대금을 받으세요.";
                    primaryLabel.text = "돈 받기";
                    break;
                case DayStatus.CustomerSettlement:
                    title.text = "감사합니다. 다음에 또 만나요!";
                    body.text = $"서비스 수입 +{game.CurrentOrder.Completion.Reward.Currency:N0} G\n보유금 {routine.Inventory.Balance:N0} G";
                    primaryLabel.text = "계속";
                    break;
                case DayStatus.WaitingForClose:
                    title.text = "오늘의 예약 손님을 모두 맞이했어요";
                    body.text = $"남은 영업 시간 {seconds / 60:00}:{seconds % 60:00}\n더 올 손님이 없으니 지금 마감해도 손해는 없어요.";
                    primaryLabel.text = "지금 마감하기";
                    break;
                case DayStatus.EndOfDaySettlement when settlement != null:
                    // The structured settlement panel replaces the text dump.
                    title.text = string.Empty;
                    body.text = string.Empty;
                    primaryLabel.text = "정산 완료 · 보급과 꾸미기";
                    break;
                case DayStatus.EndOfDaySettlement:
                    title.text = $"DAY {game.DayNumber:00} · 하루 정산";
                    var summary = game.BuildSummary();
                    var text = new StringBuilder($"완료 {summary.CompletedOrders + summary.PerfectOrders} / 예약 {summary.TotalCustomers} · 미응대 {summary.UnservedOrders}\n서비스 수입 {summary.ServiceRevenue:N0} G · 부산물 판매 {summary.ByproductRevenue:N0} G\n오늘 총수입 {summary.TotalRevenue:N0} G\n\n보관 중인 부산물 (다음 날에도 유지)\n");
                    foreach (var stack in routine.Inventory.Stacks) text.AppendLine($"{stack.Item.DisplayName} x{stack.Amount} · {stack.Item.BaseSellPrice * stack.Amount:N0} G");
                    text.AppendLine();
                    text.AppendLine($"평판 {Signed(routine.Reputation.LastGain)} → {routine.Reputation.Points}점 · {routine.ReputationTier.Title}");
                    text.AppendLine(SupplyLine());
                    body.text = text.ToString();
                    primaryLabel.text = "정산 완료 · 업그레이드 / 꾸미기";
                    break;
            }
            if (improving)
            {
                for (var i = 0; i < upgradeButtons.Count; i++)
                {
                    var unlock = game.ProgressionCatalog.Unlocks[i];
                    var owned = game.State.IsUnlocked(unlock.UnlockId);
                    upgradeButtons[i].interactable = !routine.IsLoading && game.CanUnlock(unlock);
                    upgradeButtons[i].GetComponentInChildren<Text>().text = $"{unlock.DisplayName} · {(owned ? "구매 완료" : unlock.Cost.ToString("N0") + " G")}";
                }
                for (var i = 0; i < decorationButtons.Count; i++)
                {
                    var decoration = routine.Settings.Decorations[i];
                    decorationButtons[i].interactable = !routine.IsLoading && (routine.Owns(decoration) || routine.Inventory.CanPurchase(decoration.Quote));
                    decorationButtons[i].GetComponentInChildren<Text>().text = $"{decoration.DisplayName} · {(routine.Decoration == decoration ? "적용 중" : routine.Owns(decoration) ? "적용하기" : decoration.Cost.ToString("N0") + " G")}";
                }
                for (var i = 0; i < supplyButtons.Count; i++)
                {
                    var supply = routine.Settings.Supplies[i];
                    supplyButtons[i].interactable = !routine.IsLoading && routine.Inventory.CanPurchase(supply.Quote);
                    supplyButtons[i].GetComponentInChildren<Text>().text =
                        $"{supply.DisplayName} {routine.Stock.Get(supply)}개 · +{supply.PackSize} / {supply.PackCost:N0} G";
                }
            }
        }

        private static string Signed(int value) => value > 0 ? "+" + value : value.ToString();

        /// <summary>보급 잔량을 한 줄로 정리합니다.</summary>
        private string SupplyLine()
        {
            var supplies = routine.Settings.Supplies;
            if (supplies.Count == 0) return string.Empty;
            var line = new StringBuilder("보급  ");
            for (var i = 0; i < supplies.Count; i++)
            {
                if (i > 0) line.Append("  ·  ");
                line.Append($"{supplies[i].DisplayName} {routine.Stock.Get(supplies[i])}");
            }
            return line.ToString();
        }

        /// <summary>바닥난 보급품 이름만 모읍니다.</summary>
        private string LowSupplies()
        {
            var supplies = routine.Settings.Supplies;
            var names = string.Empty;
            for (var i = 0; i < supplies.Count; i++)
            {
                if (routine.Stock.Get(supplies[i]) > 0) continue;
                names += (names.Length == 0 ? string.Empty : ", ") + supplies[i].DisplayName;
            }
            return names;
        }

        private void Advance()
        {
            switch (routine.Game.Status)
            {
                case DayStatus.Closed: routine.StartDay(); break;
                case DayStatus.AwaitingPayment: routine.CollectPayment(); break;
                case DayStatus.CustomerSettlement: routine.ContinueAfterPayment(); break;
                case DayStatus.WaitingForClose: routine.CloseEarly(); break;
                case DayStatus.EndOfDaySettlement: routine.FinishDay(); break;
            }
        }
    }
}
