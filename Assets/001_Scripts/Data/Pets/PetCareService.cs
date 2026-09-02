using System;
using _001_Scripts.Core;
using _001_Scripts.Data.Items;
using UnityEngine;

namespace _001_Scripts.Data.Pets
{
    /// <summary>케어 행동에 맞는 부산물을 판정해 아이템 획득 서비스로 전달합니다.</summary>
    public sealed class PetCareService : GameBehaviour, IPetCareService
    {
        [Tooltip("IItemAcquisitionService를 구현한 컴포넌트")]
        [SerializeField] private MonoBehaviour itemAcquisitionProvider;
        private IItemAcquisitionService acquisitionService;
        private IPetByproductRandom random = new UnityPetByproductRandom();

        public event Action<PetCareResult> CareCompleted;

        private void Awake()
        {
            if (acquisitionService == null) acquisitionService = itemAcquisitionProvider as IItemAcquisitionService;
            if (acquisitionService == null) acquisitionService = GetComponent<IItemAcquisitionService>();
        }

        public void SetAcquisitionService(IItemAcquisitionService service) => acquisitionService = service;
        public void SetRandom(IPetByproductRandom source) => random = source ?? throw new ArgumentNullException(nameof(source));

        public bool TryCare(PetInstance pet, PetCareAction action, out PetCareResult result)
        {
            result = new PetCareResult(pet == null ? null : pet.Variant, action);
            if (pet == null || pet.Variant == null || acquisitionService == null) return false;

            var rules = pet.Variant.Byproducts;
            for (var i = 0; i < rules.Count; i++)
            {
                var rule = rules[i];
                if (!rule.IsValid || rule.CareAction != action || !pet.TryConsumeByproductRule(i)) continue;
                if (random.Value > rule.Chance) continue;
                var amount = random.RangeInclusive(rule.MinAmount, rule.MaxAmount);
                var stack = new ItemStack(rule.Item, amount);
                result.Add(stack, acquisitionService.TryGrant(rule.Item, amount));
            }

            CareCompleted?.Invoke(result);
            return true;
        }
    }
}
