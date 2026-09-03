using System;
using System.Collections.Generic;
using UnityEngine;

namespace _001_Scripts.Data
{
    public interface ICareConditionSource
    {
        IReadOnlyList<CareConditionState> Create(Func<string, bool> includeCondition);
    }
}
