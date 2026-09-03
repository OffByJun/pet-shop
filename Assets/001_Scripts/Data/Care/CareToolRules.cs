using System;
using System.Collections.Generic;
using UnityEngine;

namespace _001_Scripts.Data
{
    public static class CareToolRules
    {
        public static float Effort(CareToolKind tool) => tool switch
        {
            CareToolKind.Medicine => 300f,
            CareToolKind.Tweezers => 240f,
            CareToolKind.Scissors => 320f,
            _ => 420f
        };
    }
}
