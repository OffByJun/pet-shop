using System;
using _001_Scripts.Data.Pets;

namespace _001_Scripts.Data.Tools
{
    [Flags]
    public enum PetToolCapability
    {
        None = 0,
        Clean = 1 << 0,
        Groom = 1 << 1,
        Extract = 1 << 2,
        Treat = 1 << 3,
        Trim = 1 << 4,
        Clip = 1 << 5
    }

    [Flags]
    public enum PetToolInteractionSupport
    {
        None = 0,
        Instant = 1 << 0,
        Hold = 1 << 1,
        Pull = 1 << 2,
        Cut = 1 << 3
    }

    public enum PetToolInteractionMode { Instant, Hold, Pull, Cut }

}
