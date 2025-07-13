using Data;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;

namespace Characters
{
    public abstract class Character : MonoBehaviour
    {
        [Header("Basic Info")]
        public CharacterData CharacterData;
    
        [Header("Visual")]
        [JsonIgnore]
        public SpriteRenderer SpriteRenderer;
    
        [Header("State")]
        public CharacterState currentState;
    
        // Events
        public UnityEvent<Character> OnStateChanged = new UnityEvent<Character>();
    
        protected virtual void Awake()
        {
            InitializeCharacter();
        }
    
        protected virtual void InitializeCharacter()
        {
            currentState = CharacterState.Idle;
        }
    
        public virtual void ChangeState(CharacterState newState)
        {
            currentState = newState;
            OnStateChanged?.Invoke(this);
        }
    }

    public enum CharacterType
    {
        Staff,      // Персонал
        Customer    // Клієнт
    }

    public enum CharacterState
    {
        Idle,
        Working,
        Moving,
        Interacting,
        Stressed,
        Happy,
        Dead
    }
}