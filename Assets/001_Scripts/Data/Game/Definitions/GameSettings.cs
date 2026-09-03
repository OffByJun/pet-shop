using UnityEngine;

namespace _001_Scripts.Data
{
    [CreateAssetMenu(fileName = "GameSettings", menuName = "PetShop/Game Settings")]
    public sealed class GameSettings : ScriptableObject
    {
        [SerializeField, Min(1)] private int minimumCustomers = 5;
        [SerializeField, Min(1)] private int maximumCustomers = 8;

        public int MinimumCustomers => minimumCustomers;
        public int MaximumCustomers => maximumCustomers;

        private void OnValidate()
        {
            minimumCustomers = Mathf.Max(1, minimumCustomers);
            maximumCustomers = Mathf.Max(minimumCustomers, maximumCustomers);
        }
    }
}
