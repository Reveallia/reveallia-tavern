using System.Collections;
using Data;
using DG.Tweening;
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
        public Vector2 BreatheScaleOffset = new Vector2(0.025f, 0.05f);
        public float BreatheSpeed = 1f;
    
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
            Invoke(nameof(IdleBreatheAnimation), Random.Range(0f, BreatheSpeed));
        }
    
        public virtual void ChangeState(CharacterState newState)
        {
            currentState = newState;
            OnStateChanged?.Invoke(this);
        }
        
        private void IdleBreatheAnimation()
        {
            // DOTween sprite scale animation
            Vector2 startScale = SpriteRenderer.transform.localScale;
            Vector2 endScale = startScale + BreatheScaleOffset;
            
            Sequence sequence = DOTween.Sequence();
            sequence.Append(SpriteRenderer.transform.DOScale(endScale, BreatheSpeed)
                .SetEase(Ease.InOutSine))
                .Append(SpriteRenderer.transform.DOScale(startScale, BreatheSpeed)
                .SetEase(Ease.InOutSine))
                .OnComplete(IdleBreatheAnimation);
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