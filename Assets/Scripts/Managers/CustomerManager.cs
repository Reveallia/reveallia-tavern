using System.Collections.Generic;
using Characters;
using Core;
using Events;
using HelperManagers;
using Tools;
using UnityEngine;

namespace Managers
{
    public class CustomerManager: IManagerComponent
    {
        public CustomerFactory CustomerFactory;
        public OrderFactory OrderFactory;
        public int MaxCustomersPerDay = 10;
        public int MaxCustomersOnOneTime = 1;
        
        private List<Customer> _customers = new List<Customer>();
        
        public List<Order> AvailableOrders { get; private set; } = new List<Order>();
        public List<Order> ActiveOrders { get; private set; } = new List<Order>();
        private int _currentCustomersCount => _customers?.Count ?? 0;
        private bool _isActive = true;
        
        public void Initialize()
        {
            CustomerFactory = new CustomerFactory();
            OrderFactory = new OrderFactory();
            EventBus.Subscribe<TimeOfDay>(OnDayCycleChanged);
            EventBus.Subscribe<DestinationStatusChanged>(OnDestinationStatusChanged);
            
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

        private void OnDestinationStatusChanged(DestinationStatusChanged data)
        {
            if (data.IsCustomer() && data.IsReceptionCompleted())
            {
                PublishOrder(OrderFactory.CreateOrder((Customer)data.Character));
            }
        }
        
        private void PublishOrder(Order order)
        {
            CustomLogger.LogGameLoop("CustomerManager.PublishOrder");
            if (AvailableOrders.Contains(order)) return;
            AvailableOrders.Add(order);
            EventBus.Publish(new OrderPublished(order));
        }
        
        public void TakeOrder(Order order)
        {
            CustomLogger.LogGameLoop("CustomerManager.TakeOrder");
            if (AvailableOrders.Contains(order))
            {
                AvailableOrders.Remove(order);
                ActiveOrders.Add(order);
                EventBus.Publish(new OrderAccepted(order));
            }
        }
        
        public void FinishOrder(Order order)
        {
            CustomLogger.LogGameLoop("CustomerManager.FinishOrder");
            if (ActiveOrders.Contains(order))
            {
                ActiveOrders.Remove(order);
                order.Customer.MoveToReception();
            }
        }
        
        public void CompleteOrder(Order order)
        {
            CustomLogger.LogGameLoop("CustomerManager.CompleteOrder");
            if (ActiveOrders.Contains(order))
            {
                ActiveOrders.Remove(order);
                order.Customer.GoFromTavern();
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