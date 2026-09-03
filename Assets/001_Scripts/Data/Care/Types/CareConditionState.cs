using System;
using System.Collections.Generic;
using UnityEngine;

namespace _001_Scripts.Data
{
    public sealed class CareConditionState
    {
        private readonly HashSet<CareToolKind> acceptedTools;

        public string Id { get; }
        public string Name { get; }
        public CareKind Care { get; }
        public Rect Zone { get; }
        public bool NeedsWater { get; }
        public string Byproduct { get; }
        public float Remaining { get; private set; } = 1f;
        public float Wetness { get; private set; }
        public bool Resolved => Remaining <= .001f;

        public CareConditionState(
            string id, string name, CareKind care, Rect zone,
            IEnumerable<CareToolKind> tools, bool needsWater = false, string byproduct = null)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Name = name ?? string.Empty;
            Care = care;
            Zone = zone;
            NeedsWater = needsWater;
            Byproduct = byproduct ?? string.Empty;
            acceptedTools = new HashSet<CareToolKind>(tools ?? throw new ArgumentNullException(nameof(tools)));
        }

        public bool Accepts(CareToolKind tool) => acceptedTools.Contains(tool);
        public void ApplyWater(float amount) => Wetness = Mathf.Clamp01(Wetness + Mathf.Max(0f, amount));

        public bool ApplyProgress(float amount)
        {
            if (Resolved || amount <= 0f) return false;
            Remaining = Mathf.Clamp01(Remaining - amount);
            return Resolved;
        }
    }
}
