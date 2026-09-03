using System.Threading;
using System.Threading.Tasks;
using _001_Scripts.Core;
using UnityEngine;

namespace _001_Scripts.UI.UILib
{
    public abstract class UIActionComponent : GameBehaviour, IUIAction
    {
        [SerializeField] private UIActionTiming timing = UIActionTiming.AfterShow;
        [SerializeField] private int order;

        public int Order => order;

        public bool RunsAt(UIActionTiming targetTiming)
        {
            return (timing & targetTiming) != 0;
        }

        public abstract Task ExecuteAsync(UIActionContext context, CancellationToken cancellationToken);
    }
}
