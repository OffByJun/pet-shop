using System;
using System.Collections.Generic;

namespace _001_Scripts.Data.Progression
{

    public sealed class ProgressionState
    {
        private readonly HashSet<string> unlockedIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> completedEndingIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public bool IsUnlocked(string id) => !string.IsNullOrWhiteSpace(id) && unlockedIds.Contains(id);
        public bool IsEndingCompleted(string id) => !string.IsNullOrWhiteSpace(id) && completedEndingIds.Contains(id);
        internal bool AddUnlock(string id) => unlockedIds.Add(id);
        internal bool AddEnding(string id) => completedEndingIds.Add(id);

        public ProgressionSnapshot CreateSnapshot()
        {
            var unlocks = new string[unlockedIds.Count];
            var endings = new string[completedEndingIds.Count];
            unlockedIds.CopyTo(unlocks);
            completedEndingIds.CopyTo(endings);
            return new ProgressionSnapshot { UnlockedIds = unlocks, CompletedEndingIds = endings };
        }

        public void Restore(ProgressionSnapshot snapshot)
        {
            unlockedIds.Clear();
            completedEndingIds.Clear();
            if (snapshot == null) return;
            AddRange(unlockedIds, snapshot.UnlockedIds);
            AddRange(completedEndingIds, snapshot.CompletedEndingIds);
        }

        private static void AddRange(HashSet<string> target, string[] values)
        {
            if (values == null) return;
            for (var i = 0; i < values.Length; i++)
                if (!string.IsNullOrWhiteSpace(values[i])) target.Add(values[i]);
        }
    }
}
