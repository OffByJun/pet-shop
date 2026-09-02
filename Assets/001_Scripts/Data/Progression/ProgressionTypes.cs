using System;
using UnityEngine;

namespace _001_Scripts.Data.Progression
{
    public enum ProgressionStageId { Early, Middle, Expansion, Late, Final }

    public enum ProgressionBenefitType
    {
        ProcessingSpeed,
        EffectiveRange,
        InteractionAssist,
        StorageCapacity,
        ShopVisual,
        ShopFunction,
        ContentPool
    }

    [Serializable]
    public struct ProgressionBenefit
    {
        [SerializeField] private ProgressionBenefitType type;
        [SerializeField] private float value;
        [SerializeField] private string contentId;

        public ProgressionBenefitType Type => type;
        public float Value => value;
        public string ContentId => contentId;
    }

    [Serializable]
    public struct ProgressionStageDefinition
    {
        [SerializeField] private ProgressionStageId stage;
        [SerializeField] private string displayName;
        [TextArea(2, 4)] [SerializeField] private string description;

        public ProgressionStageId Stage => stage;
        public string DisplayName => displayName;
        public string Description => description;
    }
}
