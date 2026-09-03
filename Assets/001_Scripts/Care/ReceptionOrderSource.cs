using System;
using _001_Scripts.Data.Customers;
using _001_Scripts.Data.Pets;
using UnityEngine;

namespace PetShop.Care
{
    /// <summary>Produces reception orders through the project's existing order system.</summary>
    public sealed class ReceptionOrderSource : MonoBehaviour, IReceptionOrderProvider
    {
        [SerializeField] private ServiceOrderService orderService;
        [SerializeField] private ServiceOrderCatalog fallbackCatalog;
        [SerializeField] private PetCareAction[] supportedActions =
        {
            PetCareAction.Wash,
            PetCareAction.Brush,
            PetCareAction.Treat,
            PetCareAction.Extract,
            PetCareAction.Trim,
            PetCareAction.Clip
        };
        [SerializeField, Min(1)] private int maximumGenerationAttempts = 20;

        private ServiceOrderGenerator fallbackGenerator;

        public ServiceOrder CreateNext()
        {
            if (orderService != null)
            {
                for (var attempt = 0; attempt < maximumGenerationAttempts; attempt++)
                {
                    var generated = orderService.CreateOrder();
                    if (CanEnterCareRoom(generated)) return generated;
                }
                throw new InvalidOperationException("Could not generate an order supported by the care room.");
            }
            if (fallbackCatalog == null)
                throw new InvalidOperationException("Assign ServiceOrderService or ServiceOrderCatalog.");
            fallbackGenerator ??= new ServiceOrderGenerator(fallbackCatalog);
            for (var attempt = 0; attempt < maximumGenerationAttempts; attempt++)
            {
                var generated = fallbackGenerator.CreateOrder();
                if (CanEnterCareRoom(generated)) return generated;
            }
            throw new InvalidOperationException("Could not generate an order supported by the care room.");
        }

        private bool CanEnterCareRoom(ServiceOrder order)
        {
            for (var i = 0; i < order.Requests.Count; i++)
            {
                var action = order.Requests[i].Condition.ResolvedBy;
                var supported = false;
                for (var j = 0; j < supportedActions.Length; j++)
                    if (supportedActions[j] == action)
                    {
                        supported = true;
                        break;
                    }
                if (!supported) return false;
            }
            return true;
        }

        public void Configure(ServiceOrderCatalog catalog, ServiceOrderService service = null)
        {
            fallbackCatalog = catalog;
            orderService = service;
            fallbackGenerator = null;
        }
    }
}
