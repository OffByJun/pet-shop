using System;
using System.Collections.Generic;
using UnityEngine;

namespace _001_Scripts.Data
{
    public sealed class DefaultCareConditionSource : ICareConditionSource
    {
        public IReadOnlyList<CareConditionState> Create(Func<string, bool> includeCondition)
        {
            var all = new List<CareConditionState>
            {
                new CareConditionState("mud", "진흙", CareKind.Wash, new Rect(.22f, .52f, .25f, .22f),
                    new[] { CareToolKind.Sprayer, CareToolKind.WashBrush }, true),
                new CareConditionState("tangle", "엉킨 털", CareKind.Brush, new Rect(.49f, .31f, .28f, .20f),
                    new[] { CareToolKind.Comb }, byproduct: "부드러운 털 x1"),
                new CareConditionState("wound", "작은 상처", CareKind.Treat, new Rect(.42f, .67f, .19f, .16f),
                    new[] { CareToolKind.Medicine }),
                new CareConditionState("crystal", "수정 조각", CareKind.Remove, new Rect(.69f, .51f, .15f, .22f),
                    new[] { CareToolKind.Tweezers }, byproduct: "수정 조각 x1"),
                new CareConditionState("long_fur", "긴 털", CareKind.Trim, new Rect(.13f, .29f, .18f, .20f),
                    new[] { CareToolKind.Scissors }, byproduct: "긴 털 뭉치 x1")
            };
            if (includeCondition != null) all.RemoveAll(item => !includeCondition(item.Id));
            return all;
        }
    }
}
