using System;
using System.Collections.Generic;
using Tools;

namespace HelperManagers
{
    public static class EventBus
    {
        private static readonly Dictionary<Type, List<Delegate>> _subscribers = new();

        public static void Subscribe<T>(Action<T> callback)
        {
            var type = typeof(T);
            if (!_subscribers.ContainsKey(type))
                _subscribers[type] = new List<Delegate>();

            _subscribers[type].Add(callback);
        }

        public static void Unsubscribe<T>(Action<T> callback)
        {
            var type = typeof(T);
            if (_subscribers.ContainsKey(type))
                _subscribers[type].Remove(callback);
        }

        public static void Publish<T>(T evt)
        {
            CustomLogger.LogEvents(evt);
            var type = typeof(T);
            if (!_subscribers.ContainsKey(type))
                return;

            foreach (var callback in _subscribers[type])
                ((Action<T>)callback).Invoke(evt);

            
        }

        public static void Clear()
        {
            _subscribers.Clear();
        }
    }

}