using System;
using System.Collections.Generic;
using UnityEngine;

namespace _001_Scripts.Data
{
    public sealed class DefaultCareConditionSource : ICareConditionSource
    {
        // Zones are normalised to the care stage and line up with the layered pet art
        // under "Pet Work Stage/Pet Visual" (light_phome parts).
        public IReadOnlyList<CareConditionState> Create(Func<string, bool> includeCondition)
        {
            var all = new List<CareConditionState>
            {
                new CareConditionState("mud", "진흙", CareKind.Wash, new Rect(.407f, .593f, .186f, .233f),
                    new[] { CareToolKind.Sprayer, CareToolKind.WashBrush }, true),
                new CareConditionState("tangle", "엉킨 털", CareKind.Brush, new Rect(.477f, .202f, .139f, .140f),
                    new[] { CareToolKind.Comb }, byproduct: "부드러운 털 x1"),
                new CareConditionState("wound", "작은 상처", CareKind.Treat, new Rect(.523f, .798f, .079f, .121f),
                    new[] { CareToolKind.Medicine }),
                new CareConditionState("crystal", "수정 조각", CareKind.Remove, new Rect(.491f, .277f, .112f, .140f),
                    new[] { CareToolKind.Tweezers }, byproduct: "수정 조각 x1"),
                new CareConditionState("long_fur", "긴 털", CareKind.Trim, new Rect(.314f, .481f, .099f, .335f),
                    new[] { CareToolKind.Scissors }, byproduct: "긴 털 뭉치 x1")
            };
            if (includeCondition != null) all.RemoveAll(item => !includeCondition(item.Id));
            return all;
        }
    }
}
