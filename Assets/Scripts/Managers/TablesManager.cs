using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

namespace Managers
{
    public class TablesManager: IManagerComponent
    {
        public void Initialize()
        {
            
        }

        public void Update()
        {
            
        }

        public void Dispose()
        {
            
        }
    }

    [Serializable]
    public struct Table
    {
        public string Id;
        public List<Chair> Chairs;
        
        public Table(List<Chair> chairs)
        {
            Id = System.Guid.NewGuid().ToString();
            Chairs = chairs;
        }
    }
    
    [Serializable]
    public struct Chair
    {
        public string Id;
        public bool IsFree;
        [HideIf("IsFree")]
        public string CharacterId;
        public Vector3 Position;
        
        public Chair(Vector3 position)
        {
            Id = System.Guid.NewGuid().ToString();
            IsFree = true;
            CharacterId = null;
            Position = position;
        }
        
        public void SetOccupied(string characterId)
        {
            IsFree = false;
            CharacterId = characterId;
        }
    }
}