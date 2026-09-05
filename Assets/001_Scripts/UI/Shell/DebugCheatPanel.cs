using System;
using System.Collections.Generic;
using _001_Scripts.Core;
using _001_Scripts.Data;
using _001_Scripts.Managers;
using _001_Scripts.UI.Theme;
using UnityEngine;
using UnityEngine.UI;

namespace _001_Scripts.UI.Shell
{
    /// <summary>개발용 치트 패널입니다. F1으로 열고 닫으며, 릴리즈 빌드에서는 아무것도 만들지 않습니다.</summary>
    public sealed class DebugCheatPanel : GameBehaviour
    {
        [SerializeField] private UITheme theme;
        [Tooltip("패널을 여닫는 키입니다.")]
        [SerializeField] private UnityEngine.InputSystem.Key toggleKey = UnityEngine.InputSystem.Key.F1;

        private GameObject root;
        private Text status;
        private readonly List<Action> perFrame = new List<Action>();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void Awake()
        {
            if (theme == null) return;
            Build();
            root.SetActive(false);
        }

        private void Update()
        {
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard == null || root == null) return;
            if (keyboard[toggleKey].wasPressedThisFrame) root.SetActive(!root.activeSelf);
            if (!root.activeSelf) return;
            for (var i = 0; i < perFrame.Count; i++) perFrame[i]();
        }
#endif

        private void Build()
        {
            var canvas = ThemedUIBuilder.Overlay("Debug Canvas", 950);
            canvas.transform.SetParent(transform, false);
            root = canvas.gameObject;

            var card = ThemedUIBuilder.Surface("Debug Card", root.transform, theme, theme.Card,
                new Color(theme.Paper.r, theme.Paper.g, theme.Paper.b, .97f),
                new Vector2(-548f, 0f), new Vector2(440f, 800f));

            ThemedUIBuilder.Label("Title", card.transform, theme, "디버그 · 치트", 20, theme.Ink,
                TextAnchor.MiddleLeft, new Vector2(4f, 368f), new Vector2(380f, 30f), true);
            ThemedUIBuilder.Label("Hint", card.transform, theme, "F1로 여닫기 · 개발 빌드 전용", 12, theme.InkFaint,
                TextAnchor.MiddleLeft, new Vector2(4f, 344f), new Vector2(380f, 22f));

            status = ThemedUIBuilder.Label("Status", card.transform, theme, "", 13, theme.InkSoft,
                TextAnchor.UpperLeft, new Vector2(4f, 286f), new Vector2(392f, 80f));
            perFrame.Add(RefreshStatus);

            var y = 228f;
            Section("돈 · 평판", ref y, card.transform);
            Row(card.transform, ref y, "+1,000 G", () => Inventory(i => i.Add(1000)));
            Row(card.transform, ref y, "+10,000 G", () => Inventory(i => i.Add(10000)));
            Row(card.transform, ref y, "평판 +10", () => Reputation(10));
            Row(card.transform, ref y, "평판 −10", () => Reputation(-10));

            Section("보급", ref y, card.transform);
            Row(card.transform, ref y, "모든 보급 +20", () => Supplies(20));
            Row(card.transform, ref y, "모든 보급 0개", () => Supplies(int.MinValue));

            Section("하루", ref y, card.transform);
            Row(card.transform, ref y, "영업 시간 −60초", () => Day(g => g.TickBusiness(60f)));
            Row(card.transform, ref y, "마감 즉시", () => Day(g => g.TickBusiness(99999f)));
            Row(card.transform, ref y, "손님 인내심 0", ExhaustPatience);

            Section("케어", ref y, card.transform);
            Row(card.transform, ref y, "상태 전부 발견", () => Care(true, false));
            Row(card.transform, ref y, "상태 전부 해결", () => Care(true, true));

            Section("이동", ref y, card.transform);
            Row(card.transform, ref y, "메인 메뉴로", () => { GamePause.Clear(); if (ShopRoutineManager.HasInstance) ShopRoutineManager.Instance.ReturnToMainMenu(); });
        }

        private void Section(string label, ref float y, Transform parent)
        {
            ThemedUIBuilder.Label("Section " + label, parent, theme, label, 12, theme.Oak,
                TextAnchor.MiddleLeft, new Vector2(4f, y), new Vector2(380f, 22f));
            y -= 26f;
        }

        private void Row(Transform parent, ref float y, string label, UnityEngine.Events.UnityAction action)
        {
            var button = ThemedUIBuilder.Capsule("Cheat " + label, parent, theme, label, false,
                new Vector2(0f, y), new Vector2(392f, 38f));
            button.onClick.AddListener(action);
            y -= 42f;
        }

        private void RefreshStatus()
        {
            if (!ShopRoutineManager.HasInstance)
            {
                status.text = "루틴 매니저 없음 · 씬 단독 실행 중";
                return;
            }
            var routine = ShopRoutineManager.Instance;
            var game = routine.Game;
            status.text =
                $"DAY {game.DayNumber:00} · {game.Status}\n" +
                $"{routine.Inventory.Balance:N0} G · 평판 {routine.Reputation.Points} ({routine.ReputationTier.Title})\n" +
                $"남은 시간 {Mathf.CeilToInt(game.RemainingBusinessSeconds)}초 · 보관함 {routine.Inventory.Stacks.Count}/{routine.Inventory.Capacity}";
        }

        private static void Inventory(Action<InventoryManager> action)
        {
            if (ShopRoutineManager.HasInstance) action(ShopRoutineManager.Instance.Inventory);
            else if (InventoryManager.HasInstance) action(InventoryManager.Instance);
        }

        private static void Reputation(int delta)
        {
            if (!ShopRoutineManager.HasInstance) return;
            var reputation = ShopRoutineManager.Instance.Reputation;
            var property = typeof(ShopReputation).GetProperty("Points");
            var setter = property == null ? null : property.GetSetMethod(true);
            if (setter != null) setter.Invoke(reputation, new object[] { Mathf.Max(0, reputation.Points + delta) });
        }

        private static void Supplies(int delta)
        {
            if (!ShopRoutineManager.HasInstance) return;
            var routine = ShopRoutineManager.Instance;
            foreach (var supply in routine.Settings.Supplies)
            {
                if (supply == null) continue;
                if (delta == int.MinValue) routine.Stock.TryConsume(supply, routine.Stock.Get(supply));
                else routine.Stock.Add(supply, delta);
            }
        }

        private static void Day(Action<GameManager> action)
        {
            if (ShopRoutineManager.HasInstance) action(ShopRoutineManager.Instance.Game);
            else if (GameManager.HasInstance) action(GameManager.Instance);
        }

        private static void ExhaustPatience()
        {
            var dialogue = FindAnyObjectByType<Core.Entity.ReceptionDialogueSession>();
            if (dialogue != null) dialogue.DrainPatience(999f);
        }

        /// <summary>케어 중인 상태를 한 번에 발견하거나 해결합니다.</summary>
        private static void Care(bool discover, bool resolve)
        {
            if (!CareManager.HasInstance) return;
            var session = CareManager.Instance.Session;
            if (session == null) return;
            foreach (var condition in session.Conditions)
            {
                if (discover)
                {
                    var property = condition.GetType().GetProperty("IsDiscovered");
                    var setter = property == null ? null : property.GetSetMethod(true);
                    if (setter != null) setter.Invoke(condition, new object[] { true });
                }
                if (!resolve || condition.Resolved) continue;
                condition.ApplyWater(2f);
                while (!condition.Resolved) condition.ApplyProgress(1f);
                session.RegisterResolved(condition);
            }
        }
    }
}
