using System.Text;
using _001_Scripts.Core;
using _001_Scripts.Data;
using _001_Scripts.Managers;
using _001_Scripts.UI.Theme;
using UnityEngine;
using UnityEngine.UI;

namespace _001_Scripts.UI.Shell
{
    /// <summary>하루 정산 화면입니다. 글 덩어리 대신 항목별 타일로 보여 줍니다.</summary>
    public sealed class DaySettlementPanel : GameBehaviour
    {
        [SerializeField] private UITheme theme;

        private ShopRoutineManager routine;
        private GameObject root;
        private Text title;
        private Text served;
        private Text perfect;
        private Text revenue;
        private Text unserved;
        private Text serviceLine;
        private Text byproductLine;
        private Text totalLine;
        private Text reputationLine;
        private Text goalLine;
        private Text supplyLine;
        private Text stockList;

        public void Initialize(ShopRoutineManager owner, Transform parent, UITheme themeAsset)
        {
            routine = owner;
            if (themeAsset != null) theme = themeAsset;
            if (theme == null || root != null) return;
            Build(parent);
            root.SetActive(false);
        }

        public void Render(bool visible)
        {
            if (root == null) return;
            root.SetActive(visible);
            if (!visible || routine == null) return;

            var game = routine.Game;
            var summary = game.BuildSummary();
            title.text = $"DAY {summary.DayNumber:00} 정산";
            served.text = $"{summary.CompletedOrders + summary.PerfectOrders} / {summary.TotalCustomers}";
            perfect.text = summary.PerfectOrders.ToString();
            revenue.text = $"{summary.TotalRevenue:N0} G";
            unserved.text = summary.UnservedOrders.ToString();

            var goal = routine.Settings.DailyGoalFor(Mathf.Max(1, summary.DayNumber));
            goalLine.text = routine.LastGoalMet
                ? $"오늘 목표 {goal:N0} G 달성"
                : $"오늘 목표 {goal:N0} G 미달 · {goal - summary.TotalRevenue:N0} G 부족" +
                  (routine.LastMissFee > 0 ? $" · 유지비 −{routine.LastMissFee:N0} G" : "");
            goalLine.color = routine.LastGoalMet ? theme.SageDeep : theme.Blush;

            serviceLine.text = $"서비스 수입      {summary.ServiceRevenue:N0} G";
            byproductLine.text = $"부산물 판매      {summary.ByproductRevenue:N0} G";
            totalLine.text = $"오늘 총수입      {summary.TotalRevenue:N0} G";

            var gain = routine.Reputation.LastGain;
            reputationLine.text = $"평판  {(gain > 0 ? "+" + gain : gain.ToString())}  →  " +
                                  $"{routine.Reputation.Points}점 · {routine.ReputationTier.Title} " +
                                  $"(내일 손님 {(routine.ReputationTier.ExtraCustomers > 0 ? "+" : "")}" +
                                  $"{routine.ReputationTier.ExtraCustomers}명)";

            var supplies = routine.Settings.Supplies;
            var line = new StringBuilder();
            for (var i = 0; i < supplies.Count; i++)
            {
                if (i > 0) line.Append("     ");
                line.Append($"{supplies[i].DisplayName} {routine.Stock.Get(supplies[i])}");
            }
            supplyLine.text = line.Length == 0 ? string.Empty : "남은 보급     " + line;

            var stacks = routine.Inventory.Stacks;
            if (stacks.Count == 0)
            {
                stockList.text = "보관 중인 부산물이 없습니다.";
                return;
            }
            var items = new StringBuilder();
            for (var i = 0; i < stacks.Count; i++)
                items.AppendLine($"{stacks[i].Item.DisplayName} ×{stacks[i].Amount}" +
                                 $"      {stacks[i].Item.BaseSellPrice * stacks[i].Amount:N0} G");
            stockList.text = items.ToString();
        }

        private void Build(Transform parent)
        {
            var rect = ThemedUIBuilder.Rect("Day Settlement", parent, new Vector2(0f, 60f), new Vector2(1240f, 604f));
            root = rect.gameObject;

            title = ThemedUIBuilder.Label("Title", rect, theme, "DAY 00 정산", 34, theme.Ink,
                TextAnchor.MiddleLeft, new Vector2(-430f, 262f), new Vector2(360f, 48f), true);

            var tile = new Vector2(286f, 92f);
            served = ThemedUIBuilder.StatTile("Served", rect, theme, "완료한 손님",
                new Color(theme.Sage.r, theme.Sage.g, theme.Sage.b, .42f), new Vector2(-465f, 172f), tile);
            perfect = ThemedUIBuilder.StatTile("Perfect", rect, theme, "완벽 케어",
                new Color(theme.Gold.r, theme.Gold.g, theme.Gold.b, .24f), new Vector2(-155f, 172f), tile);
            revenue = ThemedUIBuilder.StatTile("Revenue", rect, theme, "오늘 총수입",
                theme.PaperWarm, new Vector2(155f, 172f), tile);
            unserved = ThemedUIBuilder.StatTile("Unserved", rect, theme, "미응대",
                new Color(theme.Blush.r, theme.Blush.g, theme.Blush.b, .22f), new Vector2(465f, 172f), tile);

            var ledger = ThemedUIBuilder.Surface("Ledger", rect, theme, theme.Card, theme.PaperWarm,
                new Vector2(-310f, -8f), new Vector2(596f, 236f));
            ThemedUIBuilder.Label("Head", ledger.transform, theme, "수입", 13, theme.InkFaint,
                TextAnchor.MiddleLeft, new Vector2(2f, 90f), new Vector2(540f, 24f));
            serviceLine = ThemedUIBuilder.Label("Service", ledger.transform, theme, "", 17, theme.InkSoft,
                TextAnchor.MiddleLeft, new Vector2(2f, 65f), new Vector2(540f, 26f));
            byproductLine = ThemedUIBuilder.Label("Byproduct", ledger.transform, theme, "", 17, theme.InkSoft,
                TextAnchor.MiddleLeft, new Vector2(2f, 35f), new Vector2(540f, 26f));
            totalLine = ThemedUIBuilder.Label("Total", ledger.transform, theme, "", 20, theme.Ink,
                TextAnchor.MiddleLeft, new Vector2(2f, -1f), new Vector2(540f, 30f), true);
            goalLine = ThemedUIBuilder.Label("Goal", ledger.transform, theme, "", 16, theme.SageDeep,
                TextAnchor.MiddleLeft, new Vector2(2f, -36f), new Vector2(540f, 26f), true);
            reputationLine = ThemedUIBuilder.Label("Reputation", ledger.transform, theme, "", 15, theme.InkSoft,
                TextAnchor.MiddleLeft, new Vector2(2f, -64f), new Vector2(540f, 26f));
            supplyLine = ThemedUIBuilder.Label("Supplies", ledger.transform, theme, "", 14, theme.InkFaint,
                TextAnchor.MiddleLeft, new Vector2(2f, -90f), new Vector2(540f, 24f));

            var storage = ThemedUIBuilder.Surface("Storage", rect, theme, theme.Card, theme.Memo,
                new Vector2(310f, -8f), new Vector2(596f, 236f));
            ThemedUIBuilder.Label("Head", storage.transform, theme, "보관 중인 부산물 · 다음 날에도 유지", 13,
                theme.InkFaint, TextAnchor.MiddleLeft, new Vector2(2f, 90f), new Vector2(540f, 24f));
            stockList = ThemedUIBuilder.Label("Items", storage.transform, theme, "", 15, theme.InkSoft,
                TextAnchor.UpperLeft, new Vector2(2f, -18f), new Vector2(540f, 176f));
        }
    }
}
