using UnityEngine;

namespace Core
{
    public class PrefabsManager : MonoBehaviour
    {
        public static PrefabsManager Instance { get; private set; }
        public GameObject CustomerPrefab;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        public GameObject GetCustomerPrefab()
        {
            return CustomerPrefab;
        }
    }
}