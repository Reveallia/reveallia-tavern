# Технічна Реалізація - Unity 2D

## Структура проекту

### Папки та організація
```
Assets/
├── Scripts/
│   ├── Core/
│   │   ├── GameManager.cs
│   │   ├── TimeManager.cs
│   │   └── SaveSystem.cs
│   ├── Tavern/
│   │   ├── TavernManager.cs
│   │   ├── Customer.cs
│   │   └── Order.cs
│   ├── MiniGames/
│   │   ├── Base/
│   │   │   ├── IMiniGame.cs
│   │   │   └── MiniGameBase.cs
│   │   ├── Hunting/
│   │   ├── Gardening/
│   │   └── Cooking/
│   ├── Staff/
│   │   ├── StaffMember.cs
│   │   └── StaffManager.cs
│   └── UI/
│       ├── UIManager.cs
│       └── HUD.cs
├── ScriptableObjects/
│   ├── Recipes/
│   ├── Ingredients/
│   ├── Customers/
│   └── Staff/
├── Prefabs/
├── Sprites/
└── Audio/
```

## Ключові класи та системи

### 1. GameManager - Центральна система
```csharp
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Managers")]
    public TavernManager tavernManager;
    public TimeManager timeManager;
    public UIManager uiManager;
    
    [Header("Game State")]
    public GameState currentState;
    public float money;
    public int reputation;
    
    public enum GameState
    {
        Morning,    // Підготовка
        Working,    // Робочий час
        Evening,    // Вечірній час
        Night       // Відпочинок
    }
    
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    
    public void StartDay()
    {
        currentState = GameState.Morning;
        timeManager.StartDay();
        tavernManager.PrepareForCustomers();
    }
    
    public void CompleteOrder(Order order, float quality)
    {
        float reward = CalculateReward(order, quality);
        money += reward;
        reputation += CalculateReputationChange(quality);
        
        GameEvents.OnOrderCompleted?.Invoke(order);
        GameEvents.OnMoneyChanged?.Invoke(money);
    }
}
```

### 2. Система подій (Event System)
```csharp
public static class GameEvents
{
    // Клієнти
    public static event Action<Customer> OnCustomerArrived;
    public static event Action<Customer> OnCustomerLeft;
    public static event Action<Customer> OnCustomerAngry;
    
    // Замовлення
    public static event Action<Order> OnOrderReceived;
    public static event Action<Order> OnOrderCompleted;
    public static event Action<Order> OnOrderFailed;
    
    // Гроші та прогрес
    public static event Action<float> OnMoneyChanged;
    public static event Action<int> OnReputationChanged;
    
    // Міні-ігри
    public static event Action<IMiniGame> OnMiniGameStarted;
    public static event Action<IMiniGame> OnMiniGameCompleted;
    
    // Персонал
    public static event Action<StaffMember> OnStaffHired;
    public static event Action<StaffMember> OnRelationshipChanged;
}
```

### 3. Система міні-ігор
```csharp
public interface IMiniGame
{
    void StartGame();
    void EndGame();
    bool IsCompleted();
    float GetProgress();
    MiniGameResult GetResult();
}

public abstract class MiniGameBase : MonoBehaviour, IMiniGame
{
    [Header("Base Settings")]
    public string gameName;
    public float timeLimit = 60f;
    public bool isActive;
    
    protected float currentTime;
    protected float progress;
    protected bool completed;
    
    public virtual void StartGame()
    {
        isActive = true;
        currentTime = 0f;
        progress = 0f;
        completed = false;
        
        GameEvents.OnMiniGameStarted?.Invoke(this);
    }
    
    public virtual void EndGame()
    {
        isActive = false;
        GameEvents.OnMiniGameCompleted?.Invoke(this);
    }
    
    public virtual bool IsCompleted() => completed;
    public virtual float GetProgress() => progress;
    
    protected virtual void Update()
    {
        if (!isActive) return;
        
        currentTime += Time.deltaTime;
        if (currentTime >= timeLimit)
        {
            EndGame();
        }
    }
}

public class MiniGameResult
{
    public bool success;
    public float quality;
    public List<IngredientData> collectedIngredients;
    public float timeSpent;
}
```

### 4. Система клієнтів
```csharp
[System.Serializable]
public class Customer : MonoBehaviour
{
    [Header("Customer Info")]
    public string customerName;
    public CustomerType type;
    public float patience;
    public List<RecipeData> favoriteDishes;
    
    [Header("Current State")]
    public Order currentOrder;
    public float waitTime;
    public float stressLevel;
    public CustomerMood mood;
    
    public enum CustomerType
    {
        Regular,    // Звичайний
        Impatient,  // Нетерплячий
        Picky,      // Вибагливий
        VIP         // Важливий
    }
    
    public enum CustomerMood
    {
        Happy,
        Neutral,
        Annoyed,
        Angry,
        Leaving
    }
    
    private void Start()
    {
        StartCoroutine(StressCoroutine());
    }
    
    private IEnumerator StressCoroutine()
    {
        while (currentOrder != null && !currentOrder.isCompleted)
        {
            yield return new WaitForSeconds(1f);
            waitTime += 1f;
            UpdateStressLevel();
        }
    }
    
    private void UpdateStressLevel()
    {
        stressLevel = waitTime / patience;
        
        if (stressLevel >= 1f)
        {
            mood = CustomerMood.Leaving;
            GameEvents.OnCustomerAngry?.Invoke(this);
        }
        else if (stressLevel >= 0.7f)
        {
            mood = CustomerMood.Angry;
        }
        else if (stressLevel >= 0.4f)
        {
            mood = CustomerMood.Annoyed;
        }
        else if (stressLevel >= 0.2f)
        {
            mood = CustomerMood.Neutral;
        }
        else
        {
            mood = CustomerMood.Happy;
        }
        
        UpdateVisuals();
    }
}
```

### 5. Система замовлень
```csharp
[System.Serializable]
public class Order
{
    public Customer customer;
    public RecipeData recipe;
    public float timeReceived;
    public bool isCompleted;
    public float quality;
    
    public Order(Customer customer, RecipeData recipe)
    {
        this.customer = customer;
        this.recipe = recipe;
        this.timeReceived = Time.time;
        this.isCompleted = false;
        this.quality = 0f;
    }
    
    public float GetWaitTime()
    {
        return Time.time - timeReceived;
    }
    
    public bool CanBeCooked(List<IngredientData> availableIngredients)
    {
        foreach (var required in recipe.requiredIngredients)
        {
            if (!availableIngredients.Any(i => i.id == required.ingredient.id && i.quantity >= required.quantity))
                return false;
        }
        return true;
    }
}
```

### 6. Scriptable Objects для даних

#### RecipeData
```csharp
[CreateAssetMenu(fileName = "New Recipe", menuName = "Tavern/Recipe")]
public class RecipeData : ScriptableObject
{
    [Header("Basic Info")]
    public string recipeName;
    public string description;
    public Sprite icon;
    public float basePrice;
    public float cookingTime;
    
    [Header("Ingredients")]
    public List<IngredientRequirement> requiredIngredients;
    
    [Header("Difficulty")]
    public int difficulty;
    public MiniGameType cookingMiniGame;
    
    [System.Serializable]
    public class IngredientRequirement
    {
        public IngredientData ingredient;
        public int quantity;
    }
}
```

#### IngredientData
```csharp
[CreateAssetMenu(fileName = "New Ingredient", menuName = "Tavern/Ingredient")]
public class IngredientData : ScriptableObject
{
    [Header("Basic Info")]
    public string ingredientName;
    public string description;
    public Sprite icon;
    public IngredientType type;
    
    [Header("Collection")]
    public MiniGameType collectionMiniGame;
    public float collectionTime;
    public int maxQuantity;
    
    [Header("Seasonal")]
    public List<Season> availableSeasons;
    
    public enum IngredientType
    {
        Vegetable,
        Meat,
        Fish,
        Herb,
        Spice,
        Dairy
    }
    
    public enum Season
    {
        Spring,
        Summer,
        Autumn,
        Winter
    }
}
```

## UI Система

### HUD (Heads Up Display)
```csharp
public class HUD : MonoBehaviour
{
    [Header("UI Elements")]
    public Text moneyText;
    public Text reputationText;
    public Text timeText;
    public Slider stressSlider;
    public Image customerEmoji;
    
    [Header("Customer UI")]
    public GameObject customerPanel;
    public Text customerNameText;
    public Text orderText;
    public Image customerSprite;
    
    private void Start()
    {
        GameEvents.OnMoneyChanged += UpdateMoney;
        GameEvents.OnCustomerArrived += ShowCustomerInfo;
        GameEvents.OnCustomerLeft += HideCustomerInfo;
    }
    
    private void UpdateMoney(float newAmount)
    {
        moneyText.text = $"${newAmount:F0}";
    }
    
    private void ShowCustomerInfo(Customer customer)
    {
        customerPanel.SetActive(true);
        customerNameText.text = customer.customerName;
        orderText.text = customer.currentOrder.recipe.recipeName;
        customerSprite.sprite = customer.GetComponent<SpriteRenderer>().sprite;
    }
    
    private void HideCustomerInfo(Customer customer)
    {
        customerPanel.SetActive(false);
    }
}
```

## Оптимізація та продуктивність

### Object Pooling для клієнтів
```csharp
public class CustomerPool : MonoBehaviour
{
    [Header("Pool Settings")]
    public GameObject customerPrefab;
    public int poolSize = 20;
    
    private Queue<GameObject> customerPool;
    
    private void Start()
    {
        InitializePool();
    }
    
    private void InitializePool()
    {
        customerPool = new Queue<GameObject>();
        
        for (int i = 0; i < poolSize; i++)
        {
            GameObject customer = Instantiate(customerPrefab, transform);
            customer.SetActive(false);
            customerPool.Enqueue(customer);
        }
    }
    
    public GameObject GetCustomer()
    {
        if (customerPool.Count > 0)
        {
            GameObject customer = customerPool.Dequeue();
            customer.SetActive(true);
            return customer;
        }
        
        return Instantiate(customerPrefab);
    }
    
    public void ReturnCustomer(GameObject customer)
    {
        customer.SetActive(false);
        customerPool.Enqueue(customer);
    }
}
```

## Збереження прогресу

### SaveSystem
```csharp
public class SaveSystem : MonoBehaviour
{
    [System.Serializable]
    public class GameData
    {
        public float money;
        public int reputation;
        public int currentDay;
        public List<string> unlockedRecipes;
        public List<string> hiredStaff;
        public Dictionary<string, int> ingredientInventory;
    }
    
    public void SaveGame()
    {
        GameData data = new GameData
        {
            money = GameManager.Instance.money,
            reputation = GameManager.Instance.reputation,
            currentDay = GameManager.Instance.timeManager.currentDay,
            unlockedRecipes = GetUnlockedRecipes(),
            hiredStaff = GetHiredStaff(),
            ingredientInventory = GetInventory()
        };
        
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("SaveData", json);
        PlayerPrefs.Save();
    }
    
    public void LoadGame()
    {
        if (PlayerPrefs.HasKey("SaveData"))
        {
            string json = PlayerPrefs.GetString("SaveData");
            GameData data = JsonUtility.FromJson<GameData>(json);
            
            GameManager.Instance.money = data.money;
            GameManager.Instance.reputation = data.reputation;
            GameManager.Instance.timeManager.currentDay = data.currentDay;
            
            LoadUnlockedRecipes(data.unlockedRecipes);
            LoadHiredStaff(data.hiredStaff);
            LoadInventory(data.ingredientInventory);
        }
    }
}
```

## Наступні кроки розробки

1. **Створіть базову сцену** з таверною та UI
2. **Реалізуйте GameManager** та базові системи
3. **Створіть одну міні-гру** (наприклад, садівництво)
4. **Додайте систему клієнтів** та замовлень
5. **Реалізуйте базове готування**
6. **Додайте систему грошей** та винагород
7. **Тестуйте та балансуйте** геймплей
8. **Поступово додавайте** нові функції

Ця технічна документація дає вам основу для початку розробки. Почніть з MVP та поступово розширюйте функціонал!