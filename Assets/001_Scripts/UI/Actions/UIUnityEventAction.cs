using System.Threading;
using System.Threading.Tasks;
using _001_Scripts.UI.UILib;
using UnityEngine;
using UnityEngine.Events;

namespace _001_Scripts.UI.Components
{
    [DisallowMultipleComponent]
    public sealed class UIUnityEventAction : UIActionComponent
    {
        [SerializeField] private UnityEvent onExecute = new UnityEvent();

        public override Task ExecuteAsync(UIActionContext context, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            onExecute.Invoke();
            return Task.CompletedTask;
        }
    }
}
