using System;
using System.Collections.Generic;
using UnityEngine;

namespace _001_Scripts.Data
{
    public static class CareToolRules
    {
        public static float Effort(CareToolKind tool) => tool switch
        {
            CareToolKind.Medicine => 420f,
            CareToolKind.Tweezers => 360f,
            CareToolKind.Scissors => 480f,
            _ => 540f
        };
    }
}
