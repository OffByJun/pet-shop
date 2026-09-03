using System;
using System.Collections.Generic;

namespace _001_Scripts.Data.Progression
{
    [Serializable]
    public sealed class ProgressionSnapshot
    {
        public string[] UnlockedIds = Array.Empty<string>();
        public string[] CompletedEndingIds = Array.Empty<string>();
    }
}
