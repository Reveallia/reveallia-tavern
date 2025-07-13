using System;
using System.Collections.Generic;
using Characters;
using NaughtyAttributes;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "Character", menuName = "Character", order = 0)]
    public class CharacterData : ScriptableObject
    {
        [HorizontalLine(color: EColor.Orange)]
        [BoxGroup("General Info")]
        public string CharacterName;
        [ShowAssetPreview(256, 256)] [BoxGroup("General Info")]
        public Sprite CharacterSprite;
        [BoxGroup("General Info")]
        public CharacterType CharacterType;
        
        [ShowIf("CharacterType", CharacterType.Customer)] 
        [BoxGroup("Customer Info")] 
        [HorizontalLine(color: EColor.Orange)]
        public CustomerType CustomerType;
        [ShowIf("CharacterType", CharacterType.Customer)] [BoxGroup("Customer Info")]
        public CustomerTemplate CustomerTemplate;
    }
    
    [Serializable]
    public struct CustomerTemplate
    {
        public float BaseSpawnWeight;
    }
}