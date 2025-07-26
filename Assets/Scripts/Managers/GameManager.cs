using System.Collections.Generic;
using Tools;
using UnityEngine;

namespace Managers
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;
        
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
        
        
        private List<IManagerComponent> _components;
        
        public void Initialize()
        {
            CustomLogger.LogGameLoop("Initialize GameManager");
            _components = new List<IManagerComponent>();
            
            var customerManager = new CustomerManager();
            var dayCycleManager = new DayCycleManager();
            var tablesManager = new TablesManager();
            
            GameManagerContext.CustomerManager = customerManager;
            GameManagerContext.DayCycleManager = dayCycleManager;
            GameManagerContext.TablesManager = tablesManager;
            
            _components.Add(customerManager);
            _components.Add(dayCycleManager);
            _components.Add(tablesManager);
            
            foreach (var component in _components)
            {
                component.Initialize();
            }
            CustomLogger.LogGameLoop("GameManager Initialized");
        }
        
        public void Update()
        {
            foreach (var component in _components)
            {
                component.Update();
            }
        }
    }
}