using System;

namespace _001_Scripts.UI.UILib
{
    /// <summary>
    /// Selects when a component-style UI action runs.
    /// Multiple timings can be combined in the Inspector.
    /// </summary>
    [Flags]
    public enum UIActionTiming
    {
        None = 0,
        BeforeShow = 1 << 0,
        AfterShow = 1 << 1,
        BeforeHide = 1 << 2,
        AfterHide = 1 << 3
    }
}
