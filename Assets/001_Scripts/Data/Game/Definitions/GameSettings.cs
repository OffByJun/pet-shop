using UnityEngine;

namespace _001_Scripts.Data
{
    [CreateAssetMenu(fileName = "GameSettings", menuName = "PetShop/Game Settings")]
    public sealed class GameSettings : ScriptableObject
    {
        [SerializeField, Min(1)] private int minimumCustomers = 5;
        [SerializeField, Min(1)] private int maximumCustomers = 8;
        [SerializeField, Min(1)] private float businessDurationSeconds = 300f;

        public int MinimumCustomers => minimumCustomers;
        public int MaximumCustomers => maximumCustomers;
        public float BusinessDurationSeconds => Mathf.Max(1f, businessDurationSeconds);

        private void OnValidate()
        {
            minimumCustomers = Mathf.Max(1, minimumCustomers);
            maximumCustomers = Mathf.Max(minimumCustomers, maximumCustomers);
        }
    }
}
