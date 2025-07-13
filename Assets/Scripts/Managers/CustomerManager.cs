using System.Collections.Generic;
using Characters;
using Core;
using HelperManagers;
using Tools;
using UnityEngine;

namespace Managers
{
    public class CustomerManager: IManagerComponent
    {
        public CustomerFactory CustomerFactory;
        public int MaxCustomersPerDay = 10;
        public int MaxCustomersOnOneTime = 1;
        
        private List<Customer> _customers = new List<Customer>();
        private int _currentCustomersCount => _customers?.Count ?? 0;
        private bool _isActive = true;
        
        public void Initialize()
        {
            CustomerFactory = new CustomerFactory();
            EventBus.Subscribe<TimeOfDay>(OnDayCycleChanged);
            
            CustomLogger.LogGameLoop("CustomerManager Initialized");
        }
        
        private void OnDayCycleChanged(TimeOfDay newState)
        {
            CustomLogger.LogTemporary($"CustomerManager.OnDayCycleChanged New state: {newState}");
            if (newState == TimeOfDay.Day)
            {
                _isActive = true;
            }
            else if (newState == TimeOfDay.Evening)
            {
                _isActive = false;
                ClearCustomers();
            }
        }

        public void Update()
        {
            if (!_isActive) return;
            
            if (_currentCustomersCount < MaxCustomersOnOneTime)
            {
                SpawnCustomer();
            }
        }
        
        private void SpawnCustomer()
        {
            CustomLogger.LogGameLoop("CustomerManager.SpawnCustomer");
            if (_currentCustomersCount >= MaxCustomersPerDay) return;
            if (_currentCustomersCount >= MaxCustomersOnOneTime) return;

            var customer = CustomerFactory.CreateCustomer();
            customer.MoveToReception();
            _customers.Add(customer);
        }
        
        private void ClearCustomers()
        {
            foreach (var customer in _customers)
            {
                if (customer != null)
                {
                    customer.GoFromTavern();
                }
            }
            _customers.Clear();
        }
        
        public void Dispose()
        {
            EventBus.Unsubscribe<TimeOfDay>(OnDayCycleChanged);
        }
    }
}