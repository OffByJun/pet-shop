using System;
using System.Collections.Generic;
using _001_Scripts.Data.Customers;
using _001_Scripts.Data.Items;
using _001_Scripts.Data.Pets;
using _001_Scripts.Data.Progression;

namespace _001_Scripts.Managers
{
    /// <summary>손님 성향을 사용해 펫과 필수/선택 상태를 뽑습니다.</summary>
    public sealed class ServiceOrderGenerator
    {
        private readonly ServiceOrderCatalog catalog;
        private readonly IServiceOrderRandom random;
        private readonly Func<string, bool> isContentUnlocked;
        private readonly Func<PetConditionDefinition, bool> conditionSupported;
        private readonly int minimumCareRequests;
        private readonly int maximumCareRequests;

        public ServiceOrderGenerator(
            ServiceOrderCatalog catalog,
            IServiceOrderRandom random = null,
            Func<string, bool> isContentUnlocked = null,
            Func<PetConditionDefinition, bool> conditionSupported = null,
            int minimumCareRequests = 3,
            int maximumCareRequests = 5)
        {
            this.catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            this.random = random ?? new UnityServiceOrderRandom();
            this.isContentUnlocked = isContentUnlocked;
            this.conditionSupported = conditionSupported;
            this.minimumCareRequests = Math.Max(1, minimumCareRequests);
            this.maximumCareRequests = Math.Max(this.minimumCareRequests, maximumCareRequests);
        }

        public ServiceOrder CreateOrder(CustomerTypeDefinition customer = null)
        {
            customer ??= SelectCustomer();
            var pet = SelectPet(customer);
            var pool = BuildConditionPool(customer);
            if (pool.Count == 0) throw new InvalidOperationException("No pet conditions are configured for this customer.");

            var requiredCount = random.Range(customer.MinimumRequiredRequests, customer.MaximumRequiredRequests + 1);
            requiredCount = Math.Max(requiredCount, Math.Min(2, pool.Count));
            requiredCount = Math.Min(requiredCount, pool.Count);
            // maximumCareRequests is a hard ceiling: the care screen can only show that many rows,
            // and a request it cannot show would make the order impossible to complete.
            requiredCount = Math.Max(1, Math.Min(requiredCount, maximumCareRequests));
            var required = DrawUnique(pool, requiredCount);
            var optionalCount = random.Range(customer.MinimumOptionalCare, customer.MaximumOptionalCare + 1);
            var targetTotal = random.Range(minimumCareRequests, maximumCareRequests + 1);
            optionalCount = Math.Max(optionalCount, targetTotal - required.Count);
            optionalCount = Math.Min(optionalCount, maximumCareRequests - required.Count);
            optionalCount = Math.Max(0, Math.Min(optionalCount, pool.Count));
            var optional = DrawUnique(pool, optionalCount);
            return new ServiceOrder(customer, pet, required, optional, catalog.PerfectOptionalCompletionRatio);
        }

        private CustomerTypeDefinition SelectCustomer()
        {
            var customers = catalog.CustomerTypes;
            if (customers.Count == 0) throw new InvalidOperationException("No customer types are configured.");
            var total = 0f;
            for (var i = 0; i < customers.Count; i++) if (customers[i] != null) total += customers[i].AppearanceWeight;
            if (total <= 0f) throw new InvalidOperationException("Customer appearance weights must be positive.");
            var roll = random.Value * total;
            CustomerTypeDefinition fallback = null;
            for (var i = 0; i < customers.Count; i++)
            {
                if (customers[i] == null) continue;
                fallback = customers[i];
                roll -= customers[i].AppearanceWeight;
                if (roll <= 0f) return customers[i];
            }
            return fallback;
        }

        private PetVariantDefinition SelectPet(CustomerTypeDefinition customer)
        {
            var all = catalog.PetVariants;
            if (all.Count == 0) throw new InvalidOperationException("No pet variants are configured.");
            var candidates = new List<PetVariantDefinition>();
            var preferRare = random.Value < customer.RareByproductChance;
            var preferElemental = random.Value < customer.ElementalPetChance;

            for (var i = 0; i < all.Count; i++)
            {
                var pet = all[i];
                if (pet == null || !IsAvailable(pet.RequiredProgressionContentId)) continue;
                var isElemental = pet.Attribute != null && pet.Attribute.Element != PetElement.None;
                if (preferElemental && !isElemental) continue;
                if (preferRare && !HasRareByproduct(pet)) continue;
                candidates.Add(pet);
            }
            if (candidates.Count == 0)
                for (var i = 0; i < all.Count; i++)
                    if (all[i] != null && IsAvailable(all[i].RequiredProgressionContentId)) candidates.Add(all[i]);
            if (candidates.Count == 0) throw new InvalidOperationException("No unlocked pet variants are available.");
            return candidates[random.Range(0, candidates.Count)];
        }

        private List<WeightedCondition> BuildConditionPool(CustomerTypeDefinition customer)
        {
            var result = new List<WeightedCondition>();
            var preferences = customer.ConditionPreferences;
            for (var i = 0; i < preferences.Count; i++)
                if (preferences[i].Condition != null && preferences[i].Weight > 0f &&
                    IsAvailable(preferences[i].Condition.RequiredProgressionContentId) && IsSupported(preferences[i].Condition))
                    result.Add(new WeightedCondition(preferences[i].Condition, preferences[i].Weight));
            var conditions = catalog.Conditions;
            for (var i = 0; i < conditions.Count; i++)
                if (conditions[i] != null && IsAvailable(conditions[i].RequiredProgressionContentId) &&
                    IsSupported(conditions[i]) && !ContainsCategory(result, conditions[i].Category))
                    result.Add(new WeightedCondition(conditions[i], result.Count == 0 ? 1f : .35f));
            return result;
        }

        private static bool ContainsCategory(List<WeightedCondition> pool, PetConditionCategory category)
        {
            for (var i = 0; i < pool.Count; i++) if (pool[i].Condition.Category == category) return true;
            return false;
        }

        private bool IsAvailable(string contentId) => isContentUnlocked == null || isContentUnlocked(contentId);
        private bool IsSupported(PetConditionDefinition condition) => conditionSupported == null || conditionSupported(condition);

        private List<PetConditionDefinition> DrawUnique(List<WeightedCondition> pool, int count)
        {
            var result = new List<PetConditionDefinition>(count);
            for (var draw = 0; draw < count && pool.Count > 0; draw++)
            {
                var total = 0f;
                for (var i = 0; i < pool.Count; i++) total += pool[i].Weight;
                var roll = random.Value * total;
                var selected = pool.Count - 1;
                for (var i = 0; i < pool.Count; i++)
                {
                    roll -= pool[i].Weight;
                    if (roll <= 0f) { selected = i; break; }
                }
                var selectedCondition = pool[selected].Condition;
                result.Add(selectedCondition);
                for (var i = pool.Count - 1; i >= 0; i--)
                    if (pool[i].Condition.Category == selectedCondition.Category) pool.RemoveAt(i);
            }
            return result;
        }

        private static bool HasRareByproduct(PetVariantDefinition pet)
        {
            var byproducts = pet.Byproducts;
            for (var i = 0; i < byproducts.Count; i++)
                if (byproducts[i].Item != null && byproducts[i].Item.Rarity >= ItemRarity.Rare) return true;
            return false;
        }

        private readonly struct WeightedCondition
        {
            public readonly PetConditionDefinition Condition;
            public readonly float Weight;
            public WeightedCondition(PetConditionDefinition condition, float weight) { Condition = condition; Weight = weight; }
        }
    }
}
