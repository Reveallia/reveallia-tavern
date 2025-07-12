# Технічна документація Reveallia Tavern

## Архітектура проекту

### Структура папок
```
Assets/
├── Scripts/
│   ├── Core/           # Основні системи
│   ├── Managers/       # Менеджери гри
│   ├── UI/            # Інтерфейс користувача
│   ├── MiniGames/     # Міні-ігри
│   ├── Characters/    # Персонажі та клієнти
│   ├── Cooking/       # Система готування
│   └── Data/          # ScriptableObjects
├── Prefabs/           # Префаби
├── Scenes/            # Сцени
├── Art/               # Графічні ресурси
└── Audio/             # Аудіо ресурси
```

## Основні системи

### GameManager
Центральний менеджер гри, що контролює:
- Стан гри (меню, гра, пауза)
- Переходи між сценами
- Збереження/завантаження прогресу

```csharp
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    public GameState CurrentState { get; private set; }
    public DayCycleManager DayCycle { get; private set; }
    public CustomerManager CustomerManager { get; private set; }
    
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
}
```

### CustomerManager
Управляє клієнтами та їх замовленнями:
- Генерація клієнтів
- Система черги
- Емоційні стани
- Таймери очікування

### DayCycleManager
Контролює денний цикл:
- День/вечір/ніч
- Переходи між режимами
- Система часу

### InventoryManager
Управляє інвентарем гравця:
- Зберігання інгредієнтів
- Обмеження місця
- Сортування та фільтрація

## ScriptableObjects

### Recipe
```csharp
[CreateAssetMenu(fileName = "New Recipe", menuName = "Tavern/Recipe")]
public class Recipe : ScriptableObject
{
    public string recipeName;
    public Sprite recipeIcon;
    public List<IngredientRequirement> ingredients;
    public float basePrice;
    public float cookingTime;
    public RecipeDifficulty difficulty;
    public List<CookingStep> cookingSteps;
}
```

### Ingredient
```csharp
[CreateAssetMenu(fileName = "New Ingredient", menuName = "Tavern/Ingredient")]
public class Ingredient : ScriptableObject
{
    public string ingredientName;
    public Sprite ingredientIcon;
    public IngredientType type;
    public float basePrice;
    public string description;
}
```

### Customer
```csharp
[CreateAssetMenu(fileName = "New Customer", menuName = "Tavern/Customer")]
public class Customer : ScriptableObject
{
    public string customerName;
    public Sprite customerSprite;
    public CustomerType type;
    public float patienceMultiplier;
    public float tipMultiplier;
    public List<Recipe> preferredRecipes;
}
```

## Система подій

Використання Unity Events для комунікації між системами:

```csharp
public static class GameEvents
{
    public static UnityEvent<Customer> OnCustomerArrived = new UnityEvent<Customer>();
    public static UnityEvent<Customer> OnCustomerServed = new UnityEvent<Customer>();
    public static UnityEvent<Recipe> OnRecipeCompleted = new UnityEvent<Recipe>();
    public static UnityEvent<Ingredient> OnIngredientCollected = new UnityEvent<Ingredient>();
    public static UnityEvent<DayPhase> OnDayPhaseChanged = new UnityEvent<DayPhase>();
}
```

## Міні-ігри

### Базовий клас для міні-ігор
```csharp
public abstract class MiniGame : MonoBehaviour
{
    [SerializeField] protected MiniGameData gameData;
    
    public UnityEvent<MiniGameResult> OnGameCompleted = new UnityEvent<MiniGameResult>();
    
    public abstract void StartGame();
    public abstract void EndGame();
    protected abstract MiniGameResult CalculateResult();
}
```

### Полювання
```csharp
public class HuntingMiniGame : MiniGame
{
    [SerializeField] private Transform targetSpawner;
    [SerializeField] private GameObject arrowPrefab;
    
    private List<HuntingTarget> activeTargets = new List<HuntingTarget>();
    private int score = 0;
    
    public override void StartGame()
    {
        StartCoroutine(SpawnTargets());
        StartCoroutine(GameTimer());
    }
    
    private IEnumerator SpawnTargets()
    {
        while (gameData.gameTime > 0)
        {
            SpawnTarget();
            yield return new WaitForSeconds(gameData.spawnInterval);
        }
    }
}
```

### Садівництво
```csharp
public class GardeningMiniGame : MiniGame
{
    [SerializeField] private PlantPlot[] plantPlots;
    [SerializeField] private List<PlantData> availablePlants;
    
    public override void StartGame()
    {
        InitializePlots();
        StartCoroutine(GrowthCycle());
    }
    
    private void InitializePlots()
    {
        foreach (var plot in plantPlots)
        {
            plot.OnPlantHarvested.AddListener(OnPlantHarvested);
        }
    }
}
```

## UI Система

### UI Manager
```csharp
public class UIManager : MonoBehaviour
{
    [SerializeField] private CustomerOrderUI orderUI;
    [SerializeField] private InventoryUI inventoryUI;
    [SerializeField] private CookingUI cookingUI;
    [SerializeField] private DayCycleUI dayCycleUI;
    
    private void Start()
    {
        GameEvents.OnCustomerArrived.AddListener(ShowOrderUI);
        GameEvents.OnDayPhaseChanged.AddListener(UpdateDayCycleUI);
    }
    
    private void ShowOrderUI(Customer customer)
    {
        orderUI.ShowOrder(customer.CurrentOrder);
    }
}
```

### UI Toolkit
Використання UI Toolkit для гнучкого інтерфейсу:
- CustomerOrderPanel.uxml
- InventoryPanel.uxml
- CookingPanel.uxml

## Система збереження

### SaveSystem
```csharp
public class SaveSystem : MonoBehaviour
{
    [System.Serializable]
    public class GameData
    {
        public int money;
        public List<IngredientData> inventory;
        public List<RecipeData> unlockedRecipes;
        public List<StaffData> hiredStaff;
        public int currentDay;
        public float reputation;
    }
    
    public void SaveGame(GameData data)
    {
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("SaveData", json);
        PlayerPrefs.Save();
    }
    
    public GameData LoadGame()
    {
        if (PlayerPrefs.HasKey("SaveData"))
        {
            string json = PlayerPrefs.GetString("SaveData");
            return JsonUtility.FromJson<GameData>(json);
        }
        return new GameData();
    }
}
```

## Оптимізація

### Object Pooling
Для часто створюваних об'єктів (стріли, звірі, UI елементи):
```csharp
public class ObjectPool : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private int poolSize = 20;
    
    private Queue<GameObject> objectPool = new Queue<GameObject>();
    
    private void Start()
    {
        for (int i = 0; i < poolSize; i++)
        {
            CreateNewObject();
        }
    }
    
    public GameObject GetObject()
    {
        if (objectPool.Count == 0)
        {
            CreateNewObject();
        }
        
        GameObject obj = objectPool.Dequeue();
        obj.SetActive(true);
        return obj;
    }
    
    public void ReturnObject(GameObject obj)
    {
        obj.SetActive(false);
        objectPool.Enqueue(obj);
    }
}
```

### Кешування компонентів
```csharp
public class CachedMonoBehaviour : MonoBehaviour
{
    protected Transform cachedTransform;
    protected Rigidbody2D cachedRigidbody;
    
    protected virtual void Awake()
    {
        cachedTransform = transform;
        cachedRigidbody = GetComponent<Rigidbody2D>();
    }
}
```

## Тестування

### Unit Tests
```csharp
public class RecipeTests
{
    [Test]
    public void Recipe_WithValidIngredients_ShouldBeCookable()
    {
        // Arrange
        Recipe recipe = ScriptableObject.CreateInstance<Recipe>();
        recipe.ingredients = new List<IngredientRequirement>();
        
        // Act
        bool canCook = recipe.CanCook(new List<Ingredient>());
        
        // Assert
        Assert.IsTrue(canCook);
    }
}
```

### Integration Tests
```csharp
public class CustomerServiceTests
{
    [Test]
    public void Customer_WhenServedQuickly_ShouldGiveBonus()
    {
        // Arrange
        Customer customer = ScriptableObject.CreateInstance<Customer>();
        Recipe recipe = ScriptableObject.CreateInstance<Recipe>();
        
        // Act
        float reward = CustomerManager.CalculateReward(customer, recipe, 10f);
        
        // Assert
        Assert.Greater(reward, recipe.basePrice);
    }
}
```

## Рекомендації по реалізації

### Пріоритети розробки:
1. **Core Game Loop** - базовий цикл "клієнт → замовлення → готування"
2. **UI System** - основні інтерфейси
3. **Save System** - збереження прогресу
4. **Mini-games** - по одній міні-грі за раз
5. **Staff System** - система найму персоналу
6. **Polish** - анімації, звуки, ефекти

### Технічні поради:
- Використовувати ScriptableObjects для всіх даних
- Реалізувати систему подій для слабкого зв'язку
- Створити модульну архітектуру
- Використовувати Object Pooling для продуктивності
- Писати тести для критичних систем 