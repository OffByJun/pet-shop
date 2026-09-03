using System;
using _001_Scripts.Data.Pets;

namespace _001_Scripts.Data.Tools
{
    public static class PetToolCapabilityMap
    {
        public static PetToolCapability FromCareAction(PetCareAction action)
        {
            return action switch
            {
                PetCareAction.Wash => PetToolCapability.Clean,
                PetCareAction.Brush => PetToolCapability.Groom,
                PetCareAction.Treat => PetToolCapability.Treat,
                PetCareAction.Extract => PetToolCapability.Extract,
                PetCareAction.Trim => PetToolCapability.Trim,
                PetCareAction.Clip => PetToolCapability.Clip,
                _ => PetToolCapability.None
            };
        }

        public static PetToolInteractionSupport ToSupport(this PetToolInteractionMode mode)
            => (PetToolInteractionSupport)(1 << (int)mode);
    }
}
