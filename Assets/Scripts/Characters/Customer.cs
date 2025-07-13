using System.Collections;
using System.Collections.Generic;
using Data;
using Events;
using HelperManagers;
using Managers;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

namespace Characters
{
    public class Customer : Character
    {
        public CustomerType CustomerType => CharacterData.CustomerType;
        public CharacterDestination Destination { get; private set; }
        
        private Transform _transform; 
        private Coroutine _moveToCoroutine;
        
        public bool TestingMode = false;
        
        public void Initialize(CharacterData data)
        {
            CharacterData = data;
            _transform = transform;  
            SpriteRenderer.sprite = CharacterData.CharacterSprite;
            
            _transform.position = GameConfiguration.Instance.GetDestinationData(CharacterDestination.Exit).Position;
        }
        
        [ShowIf("TestingMode")]
        [Button("Move to Reception")]
        public void MoveToReception()
        {
            MoveTo(CharacterDestination.Reception);
        }
        
        [ShowIf("TestingMode")]
        [Button("Go from Tavern")]
        public void GoFromTavern()
        {
            MoveTo(CharacterDestination.Exit);
        }
        
        private void MoveTo(CharacterDestination destination)
        {
            if(_moveToCoroutine != null)
            {
                StopCoroutine(_moveToCoroutine);
                EventBus.Publish(new DestinationStatusChanged(this, destination, ProgressState.Failed));
            }
            _moveToCoroutine = StartCoroutine(MoveToCoroutine(destination));
            Destination = destination;
            EventBus.Publish(new DestinationStatusChanged(this, destination, ProgressState.InProgress));
        }
        
        private IEnumerator MoveToCoroutine(CharacterDestination destination)
        {
            var targetPosition = GameConfiguration.Instance.GetDestinationData(destination).Position;
            while (Vector2.Distance(_transform.position, targetPosition) > 0.1f)
            {
                _transform.position = Vector2.MoveTowards(_transform.position, targetPosition, GameConfiguration.Instance.Speed * Time.deltaTime);
                yield return null;
            }
            
            EventBus.Publish(new DestinationStatusChanged(this, destination, ProgressState.Completed));
        }
        
        
        // [Header("Customer Info")]
        // public CustomerType customerType;
        // public float patienceMultiplier = 1f;
        // public float tipMultiplier = 1f;
        //
        // [Header("Order")]
        // public Order currentOrder;
        // public float orderStartTime;
        // public float maxWaitTime = 300f; // 5 хвилин
        //
        // [Header("Emotional State")]
        // public CustomerEmotion currentEmotion;
        // public float patienceLevel = 100f;
        // public float stressGrowthRate = 1f;
        //
        // [Header("Visual")]
        // public SpriteRenderer emotionBubble;
        // public Image patienceBar;
        // public List<Sprite> emotionSprites;
        //
        // private Coroutine stressCoroutine;
        //
        // protected override void InitializeCharacter()
        // {
        //     base.InitializeCharacter();
        //     characterType = CharacterType.Customer;
        //     InitializeCustomer();
        // }
        //
        // protected virtual void InitializeCustomer()
        // {
        //     currentEmotion = CustomerEmotion.Happy;
        //     patienceLevel = 100f;
        //     UpdateEmotionVisual();
        //     StartStressCoroutine();
        // }
        //
        // public virtual void PlaceOrder(Order order)
        // {
        //     currentOrder = order;
        //     orderStartTime = Time.time;
        //     ChangeState(CharacterState.Interacting);
        //     
        //     // Показати замовлення гравцю
        //     GameEvents.OnCustomerOrderPlaced?.Invoke(this, order);
        // }
        //
        // public virtual void ReceiveOrder(Order deliveredOrder)
        // {
        //     if (currentOrder != null && currentOrder.IsSameAs(deliveredOrder))
        //     {
        //         // Розрахунок винагороди
        //         float waitTime = Time.time - orderStartTime;
        //         float reward = CalculateReward(waitTime);
        //         
        //         // Видача винагороди
        //         GameManager.Instance.AddMoney(reward);
        //         
        //         // Зміна емоції
        //         ChangeEmotion(CustomerEmotion.Happy);
        //         
        //         // Подія обслуговування
        //         GameEvents.OnCustomerServed?.Invoke(this);
        //         
        //         // Покинути таверну
        //         LeaveTavern();
        //     }
        //     else
        //     {
        //         // Неправильне замовлення
        //         ChangeEmotion(CustomerEmotion.Angry);
        //         GameEvents.OnCustomerDissatisfied?.Invoke(this);
        //     }
        // }
        //
        // protected virtual float CalculateReward(float waitTime)
        // {
        //     float baseReward = currentOrder.basePrice;
        //     float patienceMultiplier = patienceLevel / 100f;
        //     float timeMultiplier = Mathf.Max(0.1f, 1f - (waitTime / maxWaitTime));
        //     
        //     return baseReward * patienceMultiplier * timeMultiplier * tipMultiplier;
        // }
        //
        // protected virtual void StartStressCoroutine()
        // {
        //     stressCoroutine = StartCoroutine(StressGrowthRoutine());
        // }
        //
        // protected virtual IEnumerator StressGrowthRoutine()
        // {
        //     while (patienceLevel > 0)
        //     {
        //         yield return new WaitForSeconds(1f);
        //         
        //         // Зростання стресу
        //         patienceLevel -= stressGrowthRate * Time.deltaTime;
        //         
        //         // Оновлення візуальних елементів
        //         UpdatePatienceBar();
        //         UpdateEmotionBasedOnPatience();
        //         
        //         // Перевірка на втрату терпіння
        //         if (patienceLevel <= 0)
        //         {
        //             OnPatienceLost();
        //             break;
        //         }
        //     }
        // }
        //
        // protected virtual void UpdateEmotionBasedOnPatience()
        // {
        //     CustomerEmotion newEmotion = patienceLevel switch
        //     {
        //         > 80f => CustomerEmotion.Happy,
        //         > 60f => CustomerEmotion.Neutral,
        //         > 40f => CustomerEmotion.Impatient,
        //         > 20f => CustomerEmotion.Stressed,
        //         _ => CustomerEmotion.Angry
        //     };
        //     
        //     if (newEmotion != currentEmotion)
        //     {
        //         ChangeEmotion(newEmotion);
        //     }
        // }
        //
        // protected virtual void ChangeEmotion(CustomerEmotion newEmotion)
        // {
        //     currentEmotion = newEmotion;
        //     UpdateEmotionVisual();
        //     //GameEvents.OnCustomerEmotionChanged?.Invoke(this, newEmotion);
        // }
        //
        // protected virtual void UpdateEmotionVisual()
        // {
        //     if (emotionBubble != null && emotionSprites.Count > 0)
        //     {
        //         int emotionIndex = (int)currentEmotion;
        //         if (emotionIndex < emotionSprites.Count)
        //         {
        //             emotionBubble.sprite = emotionSprites[emotionIndex];
        //         }
        //     }
        // }
        //
        // protected virtual void UpdatePatienceBar()
        // {
        //     if (patienceBar != null)
        //     {
        //         patienceBar.fillAmount = patienceLevel / 100f;
        //     }
        // }
        //
        // protected virtual void OnPatienceLost()
        // {
        //     ChangeEmotion(CustomerEmotion.Angry);
        //     //GameEvents.OnCustomerLeftAngry?.Invoke(this);
        //     LeaveTavern();
        // }
        //
        // protected virtual void LeaveTavern()
        // {
        //     if (stressCoroutine != null)
        //     {
        //         StopCoroutine(stressCoroutine);
        //     }
        //     
        //     ChangeState(CharacterState.Moving);
        //     // Анімація виходу з таверни
        //     StartCoroutine(LeaveTavernCoroutine());
        // }
        //
        // protected virtual IEnumerator LeaveTavernCoroutine()
        // {
        //     // Анімація руху до виходу
        //     yield return new WaitForSeconds(2f);
        //     
        //     // Видалення клієнта
        //     GameManager.Instance.CustomerManager.RemoveCustomer(this);
        //     Destroy(gameObject);
        // }
    }

    public enum CustomerType
    {
        Local,          // Місцевий
        Traveler,       // Мандрівник
        Noble,          // Аристократ
        Merchant,       // Торговець
        Adventurer      // Шукач пригод
    }

    public enum CustomerEmotion
    {
        Happy,      // 😊
        Neutral,    // 🙂
        Impatient,  // 😐
        Stressed,   // 😤
        Angry       // 😠
    }
}