using System.Threading;
using System.Threading.Tasks;
using _001_Scripts.Core;

namespace _001_Scripts.UI.UILib
{
    public abstract class UIAnimatorComponent : GameBehaviour, IUIAnimator
    {
        public abstract Task PlayAsync(UIAnimationContext context, CancellationToken cancellationToken);

        public abstract void ApplyInstant(UIAnimationContext context);
    }
}
