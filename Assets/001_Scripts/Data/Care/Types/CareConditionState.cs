using System;
using System.Collections.Generic;
using UnityEngine;

namespace _001_Scripts.Data
{
    public sealed class CareConditionState
    {
        private static readonly string[] WashStages = { "오염 불리기", "깊은 세척", "헹굼", "보송 마무리" };
        private static readonly string[] BrushStages = { "엉킴 풀기", "속털 정돈", "결 빗기", "윤기 마무리" };
        private static readonly string[] TreatStages = { "부위 소독", "상태 확인", "집중 치료", "보호 마무리" };
        private static readonly string[] RemoveStages = { "주변 정리", "방향 확인", "조심스런 제거", "잔여물 확인" };
        private static readonly string[] TrimStages = { "길이 맞추기", "형태 잡기", "세부 정리", "마무리 손질" };
        private readonly HashSet<CareToolKind> acceptedTools;

        public string Id { get; }
        public string Name { get; }
        public CareKind Care { get; }
        public Rect Zone { get; }
        public bool NeedsWater { get; }
        public string Byproduct { get; }
        public float Remaining { get; private set; } = 1f;
        public float Wetness { get; private set; }
        public bool IsDiscovered { get; private set; }
        public int CompletedPasses { get; private set; }
        public int RequiredPasses { get; }
        public int CurrentPass => Mathf.Min(CompletedPasses + 1, RequiredPasses);
        public float Progress01 => Mathf.Clamp01((CompletedPasses + (1f - Remaining)) / RequiredPasses);
        public bool Resolved => CompletedPasses >= RequiredPasses;
        public string CurrentStageName => StageName(Care, CurrentPass);

        public CareConditionState(
            string id, string name, CareKind care, Rect zone,
            IEnumerable<CareToolKind> tools, bool needsWater = false, string byproduct = null,
            int requiredPasses = 3)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Name = name ?? string.Empty;
            Care = care;
            Zone = zone;
            NeedsWater = needsWater;
            Byproduct = byproduct ?? string.Empty;
            RequiredPasses = Mathf.Max(2, requiredPasses);
            acceptedTools = new HashSet<CareToolKind>(tools ?? throw new ArgumentNullException(nameof(tools)));
        }

        public bool Accepts(CareToolKind tool) => acceptedTools.Contains(tool);
        public bool Discover()
        {
            if (IsDiscovered) return false;
            IsDiscovered = true;
            return true;
        }

        public void ApplyWater(float amount) => Wetness = Mathf.Clamp01(Wetness + Mathf.Max(0f, amount));

        public void ApplyAssistProgress(float amount)
        {
            if (Resolved || amount <= 0f) return;
            Remaining = Mathf.Max(.15f, Remaining - amount);
        }

        public bool ApplyProgress(float amount)
        {
            if (Resolved || amount <= 0f) return false;
            Remaining = Mathf.Clamp01(Remaining - amount);
            if (Remaining > .001f) return false;
            CompletedPasses++;
            if (Resolved)
            {
                Remaining = 0f;
                return true;
            }
            Remaining = 1f;
            return false;
        }

        private static string StageName(CareKind care, int pass)
        {
            var stages = care switch
            {
                CareKind.Wash => WashStages,
                CareKind.Brush => BrushStages,
                CareKind.Treat => TreatStages,
                CareKind.Remove => RemoveStages,
                CareKind.Trim => TrimStages,
                _ => null
            };
            return stages == null ? $"케어 {pass}단계" : stages[Mathf.Clamp(pass - 1, 0, stages.Length - 1)];
        }
    }
}
