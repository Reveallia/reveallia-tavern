using System;
using System.Collections.Generic;
using Characters;
using NaughtyAttributes;
using UnityEngine;

namespace Data
{
    public class GameConfiguration: MonoBehaviour
    {
        public static GameConfiguration Instance;
        
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

        [BoxGroup("General Settings")] public float DayDuration = 15f;//60f * 5f;
        [BoxGroup("General Settings")] public float NightDuration = 15f;//60f * 5f;
        
        [BoxGroup("Customer Settings")] public float Speed;
        [BoxGroup("Customer Settings")] [ReorderableList] public List<DestinationData> Destinations;
        [BoxGroup("Customer Settings")] [ReorderableList] public List<CharacterData> CustomerTemplates;
        
        public DestinationData GetDestinationData(CharacterDestination destination)
        {
            return Destinations.Find(d => d.Destination == destination);
        }
        
    }
    
    [Serializable]
    public struct DestinationData
    {
        public CharacterDestination Destination;
        public Transform Target;
        public Vector2 Position => Target.position;
    }
}