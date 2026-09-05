using System;
using System.Collections.Generic;
using UnityEngine;

namespace _001_Scripts.Data
{
    public enum CareKind { Wash, Brush, Treat, Remove, Trim }

    public enum CareToolKind { Sprayer, WashBrush, Comb, Medicine, Tweezers, Scissors }

    public enum CareInteractionStatus { WrongTool, Wetting, NeedsWater, Progressed, StageCompleted, Resolved }

    public readonly struct CareInteractionResult
    {
        public CareInteractionStatus Status { get; }
        public CareConditionState Condition { get; }

        public CareInteractionResult(CareInteractionStatus status, CareConditionState condition)
        {
            Status = status;
            Condition = condition;
        }
    }
}
