# Архітектура персонажів Reveallia Tavern

## Загальна структура

У грі є два основні типи персонажів:
1. **Персонал** - працівники таверни з пасивними здібностями
2. **Клієнти** - відвідувачі з замовленнями та емоційними станами

## Базовий клас Character

```csharp
public abstract class Character : MonoBehaviour
{
    [Header("Basic Info")]
    public string characterName;
    public Sprite characterSprite;
    public CharacterType characterType;
    
    [Header("Visual")]
    public Animator characterAnimator;
    public SpriteRenderer spriteRenderer;
    
    [Header("State")]
    public CharacterState currentState;
    public float currentHealth;
    public float maxHealth;
    
    // Events
    public UnityEvent<Character> OnStateChanged = new UnityEvent<Character>();
    public UnityEvent<Character> OnHealthChanged = new UnityEvent<Character>();
    
    protected virtual void Awake()
    {
        InitializeCharacter();
    }
    
    protected virtual void InitializeCharacter()
    {
        currentHealth = maxHealth;
        currentState = CharacterState.Idle;
    }
    
    public virtual void ChangeState(CharacterState newState)
    {
        currentState = newState;
        OnStateChanged?.Invoke(this);
    }
    
    public virtual void TakeDamage(float damage)
    {
        currentHealth = Mathf.Max(0, currentHealth - damage);
        OnHealthChanged?.Invoke(this);
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    protected virtual void Die()
    {
        ChangeState(CharacterState.Dead);
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
```

## Архітектура персоналу (Staff)

### Базовий клас Staff
```csharp
public abstract class Staff : Character
{
    [Header("Staff Info")]
    public StaffType staffType;
    public float salary;
    public int experienceLevel;
    public float experiencePoints;
    
    [Header("Abilities")]
    public List<StaffAbility> abilities = new List<StaffAbility>();
    public List<PassiveEffect> passiveEffects = new List<PassiveEffect>();
    
    [Header("Work")]
    public bool isWorking;
    public float workEfficiency;
    public Transform workStation;
    
    [Header("Relationships")]
    public Dictionary<Staff, float> relationships = new Dictionary<Staff, float>();
    
    protected override void InitializeCharacter()
    {
        base.InitializeCharacter();
        characterType = CharacterType.Staff;
        InitializeAbilities();
    }
    
    protected virtual void InitializeAbilities()
    {
        // Базові здібності залежно від типу персоналу
        switch (staffType)
        {
            case StaffType.Cook:
                abilities.Add(new CookingAbility());
                passiveEffects.Add(new CookingSpeedBoost());
                break;
            case StaffType.Waiter:
                abilities.Add(new CustomerServiceAbility());
                passiveEffects.Add(new CustomerPatienceBoost());
                break;
            case StaffType.Hunter:
                abilities.Add(new HuntingAbility());
                passiveEffects.Add(new PassiveMeatGeneration());
                break;
            case StaffType.Gardener:
                abilities.Add(new GardeningAbility());
                passiveEffects.Add(new PassivePlantGeneration());
                break;
        }
    }
    
    public virtual void StartWork()
    {
        isWorking = true;
        ChangeState(CharacterState.Working);
        ApplyPassiveEffects();
    }
    
    public virtual void StopWork()
    {
        isWorking = false;
        ChangeState(CharacterState.Idle);
        RemovePassiveEffects();
    }
    
    protected virtual void ApplyPassiveEffects()
    {
        foreach (var effect in passiveEffects)
        {
            effect.Apply(this);
        }
    }
    
    protected virtual void RemovePassiveEffects()
    {
        foreach (var effect in passiveEffects)
        {
            effect.Remove(this);
        }
    }
    
    public virtual void GainExperience(float points)
    {
        experiencePoints += points;
        CheckLevelUp();
    }
    
    protected virtual void CheckLevelUp()
    {
        float requiredExp = GetRequiredExperienceForLevel(experienceLevel + 1);
        if (experiencePoints >= requiredExp)
        {
            LevelUp();
        }
    }
    
    protected virtual void LevelUp()
    {
        experienceLevel++;
        UnlockNewAbilities();
        GameEvents.OnStaffLevelUp?.Invoke(this);
    }
    
    protected virtual void UnlockNewAbilities()
    {
        // Розблокування нових здібностей з рівнем
        var newAbilities = GetAbilitiesForLevel(experienceLevel);
        foreach (var ability in newAbilities)
        {
            if (!abilities.Contains(ability))
            {
                abilities.Add(ability);
            }
        }
    }
    
    public virtual void ImproveRelationship(Staff otherStaff, float amount)
    {
        if (!relationships.ContainsKey(otherStaff))
        {
            relationships[otherStaff] = 0f;
        }
        
        relationships[otherStaff] = Mathf.Clamp(relationships[otherStaff] + amount, -100f, 100f);
    }
}

public enum StaffType
{
    Cook,       // Кухар
    Waiter,     // Офіціант
    Hunter,     // Мисливець
    Gardener,   // Садівник
    Manager     // Менеджер
}
```

### Система здібностей персоналу
```csharp
public abstract class StaffAbility : ScriptableObject
{
    public string abilityName;
    public string description;
    public Sprite abilityIcon;
    public float cooldown;
    public int requiredLevel;
    
    protected float lastUseTime;
    
    public virtual bool CanUse(Staff staff)
    {
        return Time.time - lastUseTime >= cooldown && staff.experienceLevel >= requiredLevel;
    }
    
    public virtual void Use(Staff staff)
    {
        if (CanUse(staff))
        {
            ExecuteAbility(staff);
            lastUseTime = Time.time;
        }
    }
    
    protected abstract void ExecuteAbility(Staff staff);
}

// Приклади здібностей
[CreateAssetMenu(fileName = "Cooking Ability", menuName = "Tavern/Staff/Cooking Ability")]
public class CookingAbility : StaffAbility
{
    public float cookingSpeedBonus = 0.2f;
    public float qualityBonus = 0.1f;
    
    protected override void ExecuteAbility(Staff staff)
    {
        // Збільшення швидкості готування
        staff.workEfficiency += cookingSpeedBonus;
        
        // Тимчасовий бонус до якості страв
        GameManager.Instance.AddTemporaryEffect(new QualityBoostEffect(qualityBonus, 30f));
    }
}

[CreateAssetMenu(fileName = "Customer Service Ability", menuName = "Tavern/Staff/Customer Service Ability")]
public class CustomerServiceAbility : StaffAbility
{
    public float patienceBoost = 0.3f;
    
    protected override void ExecuteAbility(Staff staff)
    {
        // Зменшення швидкості росту стресу у клієнтів
        GameManager.Instance.AddTemporaryEffect(new CustomerPatienceEffect(patienceBoost, 60f));
    }
}
```

### Система пасивних ефектів
```csharp
public abstract class PassiveEffect : ScriptableObject
{
    public string effectName;
    public string description;
    public bool isActive;
    
    public virtual void Apply(Staff staff)
    {
        isActive = true;
        OnApply(staff);
    }
    
    public virtual void Remove(Staff staff)
    {
        isActive = false;
        OnRemove(staff);
    }
    
    protected abstract void OnApply(Staff staff);
    protected abstract void OnRemove(Staff staff);
}

[CreateAssetMenu(fileName = "Cooking Speed Boost", menuName = "Tavern/Staff/Passive/Cooking Speed Boost")]
public class CookingSpeedBoost : PassiveEffect
{
    public float speedMultiplier = 0.2f;
    
    protected override void OnApply(Staff staff)
    {
        staff.workEfficiency += speedMultiplier;
    }
    
    protected override void OnRemove(Staff staff)
    {
        staff.workEfficiency -= speedMultiplier;
    }
}

[CreateAssetMenu(fileName = "Passive Meat Generation", menuName = "Tavern/Staff/Passive/Meat Generation")]
public class PassiveMeatGeneration : PassiveEffect
{
    public float generationInterval = 300f; // 5 хвилин
    public int meatAmount = 1;
    
    private float lastGenerationTime;
    
    protected override void OnApply(Staff staff)
    {
        StartCoroutine(GenerateMeatRoutine(staff));
    }
    
    protected override void OnRemove(Staff staff)
    {
        StopCoroutine(GenerateMeatRoutine(staff));
    }
    
    private IEnumerator GenerateMeatRoutine(Staff staff)
    {
        while (isActive)
        {
            yield return new WaitForSeconds(generationInterval);
            
            if (isActive)
            {
                // Додати м'ясо до інвентаря
                InventoryManager.Instance.AddIngredient(IngredientType.Meat, meatAmount);
                GameEvents.OnIngredientCollected?.Invoke(GetMeatIngredient());
            }
        }
    }
}
```

## Архітектура клієнтів (Customer)

### Базовий клас Customer
```csharp
public class Customer : Character
{
    [Header("Customer Info")]
    public CustomerType customerType;
    public float patienceMultiplier = 1f;
    public float tipMultiplier = 1f;
    
    [Header("Order")]
    public Order currentOrder;
    public float orderStartTime;
    public float maxWaitTime = 300f; // 5 хвилин
    
    [Header("Emotional State")]
    public CustomerEmotion currentEmotion;
    public float patienceLevel = 100f;
    public float stressGrowthRate = 1f;
    
    [Header("Visual")]
    public SpriteRenderer emotionBubble;
    public Image patienceBar;
    public List<Sprite> emotionSprites;
    
    private Coroutine stressCoroutine;
    
    protected override void InitializeCharacter()
    {
        base.InitializeCharacter();
        characterType = CharacterType.Customer;
        InitializeCustomer();
    }
    
    protected virtual void InitializeCustomer()
    {
        currentEmotion = CustomerEmotion.Happy;
        patienceLevel = 100f;
        UpdateEmotionVisual();
        StartStressCoroutine();
    }
    
    public virtual void PlaceOrder(Order order)
    {
        currentOrder = order;
        orderStartTime = Time.time;
        ChangeState(CharacterState.Interacting);
        
        // Показати замовлення гравцю
        GameEvents.OnCustomerOrderPlaced?.Invoke(this, order);
    }
    
    public virtual void ReceiveOrder(Order deliveredOrder)
    {
        if (currentOrder != null && currentOrder.IsSameAs(deliveredOrder))
        {
            // Розрахунок винагороди
            float waitTime = Time.time - orderStartTime;
            float reward = CalculateReward(waitTime);
            
            // Видача винагороди
            GameManager.Instance.AddMoney(reward);
            
            // Зміна емоції
            ChangeEmotion(CustomerEmotion.Happy);
            
            // Подія обслуговування
            GameEvents.OnCustomerServed?.Invoke(this);
            
            // Покинути таверну
            LeaveTavern();
        }
        else
        {
            // Неправильне замовлення
            ChangeEmotion(CustomerEmotion.Angry);
            GameEvents.OnCustomerDissatisfied?.Invoke(this);
        }
    }
    
    protected virtual float CalculateReward(float waitTime)
    {
        float baseReward = currentOrder.basePrice;
        float patienceMultiplier = patienceLevel / 100f;
        float timeMultiplier = Mathf.Max(0.1f, 1f - (waitTime / maxWaitTime));
        
        return baseReward * patienceMultiplier * timeMultiplier * tipMultiplier;
    }
    
    protected virtual void StartStressCoroutine()
    {
        stressCoroutine = StartCoroutine(StressGrowthRoutine());
    }
    
    protected virtual IEnumerator StressGrowthRoutine()
    {
        while (patienceLevel > 0)
        {
            yield return new WaitForSeconds(1f);
            
            // Зростання стресу
            patienceLevel -= stressGrowthRate * Time.deltaTime;
            
            // Оновлення візуальних елементів
            UpdatePatienceBar();
            UpdateEmotionBasedOnPatience();
            
            // Перевірка на втрату терпіння
            if (patienceLevel <= 0)
            {
                OnPatienceLost();
                break;
            }
        }
    }
    
    protected virtual void UpdateEmotionBasedOnPatience()
    {
        CustomerEmotion newEmotion = patienceLevel switch
        {
            > 80f => CustomerEmotion.Happy,
            > 60f => CustomerEmotion.Neutral,
            > 40f => CustomerEmotion.Impatient,
            > 20f => CustomerEmotion.Stressed,
            _ => CustomerEmotion.Angry
        };
        
        if (newEmotion != currentEmotion)
        {
            ChangeEmotion(newEmotion);
        }
    }
    
    protected virtual void ChangeEmotion(CustomerEmotion newEmotion)
    {
        currentEmotion = newEmotion;
        UpdateEmotionVisual();
        GameEvents.OnCustomerEmotionChanged?.Invoke(this, newEmotion);
    }
    
    protected virtual void UpdateEmotionVisual()
    {
        if (emotionBubble != null && emotionSprites.Count > 0)
        {
            int emotionIndex = (int)currentEmotion;
            if (emotionIndex < emotionSprites.Count)
            {
                emotionBubble.sprite = emotionSprites[emotionIndex];
            }
        }
    }
    
    protected virtual void UpdatePatienceBar()
    {
        if (patienceBar != null)
        {
            patienceBar.fillAmount = patienceLevel / 100f;
        }
    }
    
    protected virtual void OnPatienceLost()
    {
        ChangeEmotion(CustomerEmotion.Angry);
        GameEvents.OnCustomerLeftAngry?.Invoke(this);
        LeaveTavern();
    }
    
    protected virtual void LeaveTavern()
    {
        if (stressCoroutine != null)
        {
            StopCoroutine(stressCoroutine);
        }
        
        ChangeState(CharacterState.Moving);
        // Анімація виходу з таверни
        StartCoroutine(LeaveTavernCoroutine());
    }
    
    protected virtual IEnumerator LeaveTavernCoroutine()
    {
        // Анімація руху до виходу
        yield return new WaitForSeconds(2f);
        
        // Видалення клієнта
        CustomerManager.Instance.RemoveCustomer(this);
        Destroy(gameObject);
    }
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
```

### Система замовлень
```csharp
[System.Serializable]
public class Order
{
    public string orderId;
    public Recipe recipe;
    public float basePrice;
    public float timeLimit;
    public List<IngredientRequirement> requiredIngredients;
    public OrderPriority priority;
    
    public Order(Recipe recipe)
    {
        this.recipe = recipe;
        this.basePrice = recipe.basePrice;
        this.timeLimit = recipe.cookingTime * 2f; // Подвійний час для терпіння
        this.requiredIngredients = new List<IngredientRequirement>(recipe.ingredients);
        this.orderId = System.Guid.NewGuid().ToString();
    }
    
    public bool IsSameAs(Order other)
    {
        return recipe == other.recipe;
    }
    
    public bool HasRequiredIngredients(List<Ingredient> availableIngredients)
    {
        foreach (var requirement in requiredIngredients)
        {
            bool hasIngredient = availableIngredients.Any(i => 
                i.ingredientType == requirement.ingredientType && 
                i.quantity >= requirement.quantity);
                
            if (!hasIngredient)
                return false;
        }
        return true;
    }
}

public enum OrderPriority
{
    Low,
    Normal,
    High,
    Urgent
}

[System.Serializable]
public class IngredientRequirement
{
    public IngredientType ingredientType;
    public int quantity;
    public IngredientQuality minimumQuality;
}
```

## Менеджери персонажів

### StaffManager
```csharp
public class StaffManager : MonoBehaviour
{
    public static StaffManager Instance { get; private set; }
    
    [Header("Staff")]
    public List<Staff> hiredStaff = new List<Staff>();
    public List<Staff> availableStaff = new List<Staff>();
    
    [Header("Staff Generation")]
    public List<StaffData> staffTemplates;
    public float staffGenerationInterval = 3600f; // 1 година
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public virtual void HireStaff(Staff staff)
    {
        if (hiredStaff.Count < GetMaxStaffCapacity())
        {
            hiredStaff.Add(staff);
            availableStaff.Remove(staff);
            
            // Запустити роботу
            staff.StartWork();
            
            GameEvents.OnStaffHired?.Invoke(staff);
        }
    }
    
    public virtual void FireStaff(Staff staff)
    {
        if (hiredStaff.Contains(staff))
        {
            staff.StopWork();
            hiredStaff.Remove(staff);
            availableStaff.Add(staff);
            
            GameEvents.OnStaffFired?.Invoke(staff);
        }
    }
    
    public virtual void GenerateNewStaff()
    {
        if (availableStaff.Count < 5) // Максимум 5 доступних кандидатів
        {
            StaffData template = staffTemplates[Random.Range(0, staffTemplates.Count)];
            Staff newStaff = CreateStaffFromTemplate(template);
            availableStaff.Add(newStaff);
        }
    }
    
    protected virtual Staff CreateStaffFromTemplate(StaffData template)
    {
        // Створення персоналу з шаблону
        GameObject staffObj = new GameObject($"Staff_{template.staffName}");
        Staff staff = staffObj.AddComponent<Staff>();
        
        // Налаштування з шаблону
        staff.characterName = template.staffName;
        staff.staffType = template.staffType;
        staff.salary = template.baseSalary;
        
        return staff;
    }
    
    protected virtual int GetMaxStaffCapacity()
    {
        // Залежить від рівня таверни
        return 5 + GameManager.Instance.tavernLevel * 2;
    }
}
```

### CustomerManager
```csharp
public class CustomerManager : MonoBehaviour
{
    public static CustomerManager Instance { get; private set; }
    
    [Header("Customers")]
    public List<Customer> activeCustomers = new List<Customer>();
    public Queue<Customer> customerQueue = new Queue<Customer>();
    
    [Header("Customer Generation")]
    public List<CustomerData> customerTemplates;
    public float customerSpawnInterval = 30f;
    public int maxCustomers = 10;
    
    [Header("Spawn Points")]
    public Transform[] spawnPoints;
    public Transform[] waitingPositions;
    
    private Coroutine spawnCoroutine;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        spawnCoroutine = StartCoroutine(CustomerSpawnRoutine());
    }
    
    protected virtual IEnumerator CustomerSpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(customerSpawnInterval);
            
            if (activeCustomers.Count < maxCustomers)
            {
                SpawnCustomer();
            }
        }
    }
    
    protected virtual void SpawnCustomer()
    {
        // Вибір шаблону клієнта
        CustomerData template = customerTemplates[Random.Range(0, customerTemplates.Count)];
        
        // Створення клієнта
        GameObject customerObj = new GameObject($"Customer_{template.customerName}");
        Customer customer = customerObj.AddComponent<Customer>();
        
        // Налаштування з шаблону
        customer.characterName = template.customerName;
        customer.customerType = template.customerType;
        customer.patienceMultiplier = template.patienceMultiplier;
        customer.tipMultiplier = template.tipMultiplier;
        
        // Розміщення
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        customer.transform.position = spawnPoint.position;
        
        // Додавання до черги
        customerQueue.Enqueue(customer);
        activeCustomers.Add(customer);
        
        // Генерація замовлення
        GenerateOrderForCustomer(customer);
        
        GameEvents.OnCustomerArrived?.Invoke(customer);
    }
    
    protected virtual void GenerateOrderForCustomer(Customer customer)
    {
        // Вибір рецепту залежно від типу клієнта
        Recipe recipe = GetRandomRecipeForCustomer(customer.customerType);
        Order order = new Order(recipe);
        
        customer.PlaceOrder(order);
    }
    
    protected virtual Recipe GetRandomRecipeForCustomer(CustomerType customerType)
    {
        List<Recipe> availableRecipes = RecipeManager.Instance.GetRecipesForCustomerType(customerType);
        return availableRecipes[Random.Range(0, availableRecipes.Count)];
    }
    
    public virtual void RemoveCustomer(Customer customer)
    {
        activeCustomers.Remove(customer);
        
        // Переміщення наступного клієнта з черги
        if (customerQueue.Count > 0)
        {
            Customer nextCustomer = customerQueue.Dequeue();
            MoveCustomerToWaitingPosition(nextCustomer);
        }
    }
    
    protected virtual void MoveCustomerToWaitingPosition(Customer customer)
    {
        // Знайти вільну позицію очікування
        Transform waitingPos = GetAvailableWaitingPosition();
        if (waitingPos != null)
        {
            customer.transform.position = waitingPos.position;
        }
    }
    
    protected virtual Transform GetAvailableWaitingPosition()
    {
        foreach (Transform pos in waitingPositions)
        {
            // Перевірити чи позиція вільна
            Collider2D[] colliders = Physics2D.OverlapPointAll(pos.position);
            if (colliders.Length == 0)
            {
                return pos;
            }
        }
        return null;
    }
}
```

## ScriptableObjects для даних

### StaffData
```csharp
[CreateAssetMenu(fileName = "New Staff Data", menuName = "Tavern/Staff Data")]
public class StaffData : ScriptableObject
{
    [Header("Basic Info")]
    public string staffName;
    public StaffType staffType;
    public Sprite staffSprite;
    
    [Header("Stats")]
    public float baseSalary;
    public float baseEfficiency;
    public int startingLevel;
    
    [Header("Abilities")]
    public List<StaffAbility> startingAbilities;
    public List<PassiveEffect> startingPassives;
    
    [Header("Personality")]
    public string personalityDescription;
    public List<string> dialogueLines;
}
```

### CustomerData
```csharp
[CreateAssetMenu(fileName = "New Customer Data", menuName = "Tavern/Customer Data")]
public class CustomerData : ScriptableObject
{
    [Header("Basic Info")]
    public string customerName;
    public CustomerType customerType;
    public Sprite customerSprite;
    
    [Header("Stats")]
    public float patienceMultiplier = 1f;
    public float tipMultiplier = 1f;
    public float stressGrowthRate = 1f;
    
    [Header("Preferences")]
    public List<Recipe> preferredRecipes;
    public List<IngredientType> dislikedIngredients;
    
    [Header("Visual")]
    public List<Sprite> emotionSprites;
    public Color customerColor = Color.white;
}
```

## Події (Events)

```csharp
public static class GameEvents
{
    // Staff Events
    public static UnityEvent<Staff> OnStaffHired = new UnityEvent<Staff>();
    public static UnityEvent<Staff> OnStaffFired = new UnityEvent<Staff>();
    public static UnityEvent<Staff> OnStaffLevelUp = new UnityEvent<Staff>();
    
    // Customer Events
    public static UnityEvent<Customer> OnCustomerArrived = new UnityEvent<Customer>();
    public static UnityEvent<Customer> OnCustomerServed = new UnityEvent<Customer>();
    public static UnityEvent<Customer> OnCustomerLeftAngry = new UnityEvent<Customer>();
    public static UnityEvent<Customer> OnCustomerDissatisfied = new UnityEvent<Customer>();
    public static UnityEvent<Customer, CustomerEmotion> OnCustomerEmotionChanged = new UnityEvent<Customer, CustomerEmotion>();
    
    // Order Events
    public static UnityEvent<Customer, Order> OnCustomerOrderPlaced = new UnityEvent<Customer, Order>();
    public static UnityEvent<Order> OnOrderCompleted = new UnityEvent<Order>();
}
```

## Висновок

Ця архітектура забезпечує:

1. **Модульність** - кожен тип персонажа має свій клас
2. **Розширюваність** - легко додавати нові типи персоналу та клієнтів
3. **Гнучкість** - система здібностей та пасивних ефектів
4. **Візуальність** - емоції клієнтів та стани персоналу
5. **Масштабованість** - можна легко додавати нові механіки

Ключові особливості:
- **Персонал** має пасивні здібності та може вчитися
- **Клієнти** мають емоційні стани та систему терпіння
- **Система подій** для комунікації між системами
- **ScriptableObjects** для легкого налаштування даних 