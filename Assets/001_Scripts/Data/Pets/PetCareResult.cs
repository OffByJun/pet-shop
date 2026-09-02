using System;
using System.Collections.Generic;
using _001_Scripts.Data.Items;

namespace _001_Scripts.Data.Pets
{
    public sealed class PetCareResult
    {
        private readonly List<ItemStack> grantedItems = new List<ItemStack>();
        private readonly List<ItemStack> rejectedItems = new List<ItemStack>();

        public PetVariantDefinition Variant { get; }
        public PetCareAction Action { get; }
        public IReadOnlyList<ItemStack> GrantedItems => grantedItems;
        public IReadOnlyList<ItemStack> RejectedItems => rejectedItems;
        public bool GrantedAnything => grantedItems.Count > 0;

        public PetCareResult(PetVariantDefinition variant, PetCareAction action)
        {
            Variant = variant;
            Action = action;
        }

        internal void Add(ItemStack stack, bool granted)
        {
            (granted ? grantedItems : rejectedItems).Add(stack);
        }
    }
}
