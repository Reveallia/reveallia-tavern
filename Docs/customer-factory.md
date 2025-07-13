# CustomerFactory - Архітектура генерації клієнтів

## Загальна концепція

CustomerFactory відповідає за створення різноманітних клієнтів з унікальними характеристиками, замовленнями та поведінкою. Фабрика враховує багато факторів для створення цікавих та різноманітних клієнтів.

## Базова архітектура CustomerFactory

```csharp
public class CustomerFactory : MonoBehaviour
{
    public static CustomerFactory Instance { get; private set; }
    
    [Header("Customer Templates")]
    public List<CustomerTemplate> customerTemplates = new List<CustomerTemplate>();
    
    [Header("Generation Settings")]
    public CustomerGenerationSettings generationSettings;
    
    [Header("Time-based Factors")]
    public TimeOfDayModifier timeModifier;
    public WeatherModifier weatherModifier;
    public EventModifier eventModifier;
    
    [Header("Difficulty Scaling")]
    public DifficultyScaling difficultyScaling;
    
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
    
    public Customer CreateCustomer(CustomerGenerationContext context = null)
    {
        if (context == null)
        {
            context = CreateDefaultContext();
        }
        
        // Вибір шаблону клієнта
        CustomerTemplate template = SelectCustomerTemplate(context);
        
        // Створення клієнта
        Customer customer = InstantiateCustomer(template);
        
        // Налаштування характеристик
        ConfigureCustomer(customer, template, context);
        
        // Генерація замовлення
        GenerateOrderForCustomer(customer, context);
        
        // Налаштування поведінки
        ConfigureBehavior(customer, context);
        
        return customer;
    }
    
    protected virtual CustomerGenerationContext CreateDefaultContext()
    {
        return new CustomerGenerationContext
        {
            timeOfDay = GameManager.Instance.GetCurrentTimeOfDay(),
            weather = WeatherManager.Instance.GetCurrentWeather(),
            currentEvent = EventManager.Instance.GetCurrentEvent(),
            difficulty = GameManager.Instance.GetCurrentDifficulty(),
            tavernReputation = GameManager.Instance.GetTavernReputation(),
            dayOfWeek = GameManager.Instance.GetCurrentDayOfWeek()
        };
    }
    
    protected virtual CustomerTemplate SelectCustomerTemplate(CustomerGenerationContext context)
    {
        List<CustomerTemplate> availableTemplates = GetAvailableTemplates(context);
        
        // Розрахунок ваги для кожного шаблону
        Dictionary<CustomerTemplate, float> weights = new Dictionary<CustomerTemplate, float>();
        float totalWeight = 0f;
        
        foreach (var template in availableTemplates)
        {
            float weight = CalculateTemplateWeight(template, context);
            weights[template] = weight;
            totalWeight += weight;
        }
        
        // Випадковий вибір з урахуванням ваги
        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;
        
        foreach (var kvp in weights)
        {
            currentWeight += kvp.Value;
            if (randomValue <= currentWeight)
            {
                return kvp.Key;
            }
        }
        
        return availableTemplates[0]; // Fallback
    }
    
    protected virtual List<CustomerTemplate> GetAvailableTemplates(CustomerGenerationContext context)
    {
        return customerTemplates.Where(template => 
            template.IsAvailable(context) && 
            template.minDifficulty <= context.difficulty &&
            template.maxDifficulty >= context.difficulty
        ).ToList();
    }
    
    protected virtual float CalculateTemplateWeight(CustomerTemplate template, CustomerGenerationContext context)
    {
        float baseWeight = template.baseSpawnWeight;
        
        // Модифікатори залежно від контексту
        baseWeight *= timeModifier.GetModifier(template, context.timeOfDay);
        baseWeight *= weatherModifier.GetModifier(template, context.weather);
        baseWeight *= eventModifier.GetModifier(template, context.currentEvent);
        
        // Модифікатор репутації
        baseWeight *= GetReputationModifier(template, context.tavernReputation);
        
        // Модифікатор дня тижня
        baseWeight *= GetDayOfWeekModifier(template, context.dayOfWeek);
        
        return baseWeight;
    }
    
    protected virtual Customer InstantiateCustomer(CustomerTemplate template)
    {
        GameObject customerObj = new GameObject($"Customer_{template.customerName}_{System.Guid.NewGuid()}");
        Customer customer = customerObj.AddComponent<Customer>();
        
        // Базові налаштування
        customer.characterName = template.customerName;
        customer.customerType = template.customerType;
        customer.characterSprite = template.customerSprite;
        
        return customer;
    }
    
    protected virtual void ConfigureCustomer(Customer customer, CustomerTemplate template, CustomerGenerationContext context)
    {
        // Базові характеристики
        customer.patienceMultiplier = template.basePatienceMultiplier;
        customer.tipMultiplier = template.baseTipMultiplier;
        customer.stressGrowthRate = template.baseStressGrowthRate;
        
        // Модифікація залежно від контексту
        ApplyContextModifiers(customer, template, context);
        
        // Випадкові варіації
        ApplyRandomVariations(customer, template);
        
        // Налаштування візуальних елементів
        ConfigureVisualElements(customer, template);
    }
    
    protected virtual void ApplyContextModifiers(Customer customer, CustomerTemplate template, CustomerGenerationContext context)
    {
        // Модифікація терпіння залежно від часу дня
        float timeModifier = timeModifier.GetPatienceModifier(context.timeOfDay);
        customer.patienceMultiplier *= timeModifier;
        
        // Модифікація залежно від погоди
        float weatherModifier = weatherModifier.GetPatienceModifier(context.weather);
        customer.patienceMultiplier *= weatherModifier;
        
        // Модифікація залежно від події
        float eventModifier = eventModifier.GetPatienceModifier(context.currentEvent);
        customer.patienceMultiplier *= eventModifier;
        
        // Модифікація залежно від репутації таверни
        float reputationModifier = GetReputationPatienceModifier(context.tavernReputation);
        customer.patienceMultiplier *= reputationModifier;
    }
    
    protected virtual void ApplyRandomVariations(Customer customer, CustomerTemplate template)
    {
        // Випадкові варіації характеристик (±20%)
        float variation = Random.Range(0.8f, 1.2f);
        customer.patienceMultiplier *= variation;
        customer.tipMultiplier *= variation;
        
        // Випадкові особливості
        if (Random.Range(0f, 1f) < 0.1f) // 10% шанс
        {
            customer.patienceMultiplier *= 1.5f; // Дуже терплячий
        }
        
        if (Random.Range(0f, 1f) < 0.05f) // 5% шанс
        {
            customer.tipMultiplier *= 2f; // Дуже щедрий
        }
    }
    
    protected virtual void ConfigureVisualElements(Customer customer, CustomerTemplate template)
    {
        // Налаштування емодзі
        customer.emotionSprites = template.emotionSprites;
        
        // Налаштування кольору
        if (template.customerColor != Color.white)
        {
            customer.spriteRenderer.color = template.customerColor;
        }
        
        // Налаштування розміру
        if (template.sizeModifier != 1f)
        {
            customer.transform.localScale *= template.sizeModifier;
        }
    }
    
    protected virtual void GenerateOrderForCustomer(Customer customer, CustomerGenerationContext context)
    {
        // Вибір рецепту
        Recipe selectedRecipe = SelectRecipeForCustomer(customer, context);
        
        // Створення замовлення
        Order order = new Order(selectedRecipe);
        
        // Модифікація замовлення залежно від контексту
        ModifyOrderBasedOnContext(order, context);
        
        // Призначення замовлення клієнту
        customer.PlaceOrder(order);
    }
    
    protected virtual Recipe SelectRecipeForCustomer(Customer customer, CustomerGenerationContext context)
    {
        List<Recipe> availableRecipes = GetAvailableRecipesForCustomer(customer, context);
        
        if (availableRecipes.Count == 0)
        {
            return RecipeManager.Instance.GetDefaultRecipe();
        }
        
        // Розрахунок ваги для кожного рецепту
        Dictionary<Recipe, float> weights = new Dictionary<Recipe, float>();
        float totalWeight = 0f;
        
        foreach (var recipe in availableRecipes)
        {
            float weight = CalculateRecipeWeight(recipe, customer, context);
            weights[recipe] = weight;
            totalWeight += weight;
        }
        
        // Випадковий вибір
        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;
        
        foreach (var kvp in weights)
        {
            currentWeight += kvp.Value;
            if (randomValue <= currentWeight)
            {
                return kvp.Key;
            }
        }
        
        return availableRecipes[0];
    }
    
    protected virtual List<Recipe> GetAvailableRecipesForCustomer(Customer customer, CustomerGenerationContext context)
    {
        List<Recipe> recipes = new List<Recipe>();
        
        // Рецепти залежно від типу клієнта
        recipes.AddRange(RecipeManager.Instance.GetRecipesForCustomerType(customer.customerType));
        
        // Рецепти залежно від часу дня
        recipes.AddRange(RecipeManager.Instance.GetRecipesForTimeOfDay(context.timeOfDay));
        
        // Рецепти залежно від погоди
        recipes.AddRange(RecipeManager.Instance.GetRecipesForWeather(context.weather));
        
        // Рецепти залежно від події
        if (context.currentEvent != null)
        {
            recipes.AddRange(RecipeManager.Instance.GetRecipesForEvent(context.currentEvent));
        }
        
        // Видалення дублікатів
        return recipes.Distinct().ToList();
    }
    
    protected virtual float CalculateRecipeWeight(Recipe recipe, Customer customer, CustomerGenerationContext context)
    {
        float weight = recipe.baseOrderWeight;
        
        // Бонус за улюблений рецепт
        if (customer.preferredRecipes.Contains(recipe))
        {
            weight *= 2f;
        }
        
        // Бонус за сезонність
        if (recipe.IsSeasonal(context.timeOfDay))
        {
            weight *= 1.5f;
        }
        
        // Бонус за складність (залежно від рівня гравця)
        if (recipe.difficulty <= context.difficulty)
        {
            weight *= 1.2f;
        }
        
        return weight;
    }
    
    protected virtual void ModifyOrderBasedOnContext(Order order, CustomerGenerationContext context)
    {
        // Модифікація ціни залежно від події
        if (context.currentEvent != null)
        {
            order.basePrice *= context.currentEvent.priceMultiplier;
        }
        
        // Модифікація часу очікування залежно від погоди
        order.timeLimit *= weatherModifier.GetTimeModifier(context.weather);
    }
    
    protected virtual void ConfigureBehavior(Customer customer, CustomerGenerationContext context)
    {
        // Налаштування поведінки залежно від типу клієнта
        ConfigureCustomerBehavior(customer, context);
        
        // Налаштування діалогів
        ConfigureDialogues(customer, context);
        
        // Налаштування анімацій
        ConfigureAnimations(customer, context);
    }
    
    protected virtual void ConfigureCustomerBehavior(Customer customer, CustomerGenerationContext context)
    {
        // Різні типи поведінки залежно від типу клієнта
        switch (customer.customerType)
        {
            case CustomerType.Noble:
                customer.stressGrowthRate *= 1.5f; // Швидше нервуються
                customer.tipMultiplier *= 1.8f; // Більше платять
                break;
                
            case CustomerType.Traveler:
                customer.patienceMultiplier *= 1.3f; // Більш терплячі
                customer.stressGrowthRate *= 0.8f; // Повільніше нервуються
                break;
                
            case CustomerType.Local:
                customer.tipMultiplier *= 0.8f; // Менше платять
                customer.patienceMultiplier *= 1.1f; // Трохи терплячіші
                break;
                
            case CustomerType.Adventurer:
                customer.stressGrowthRate *= 0.7f; // Дуже терплячі
                customer.tipMultiplier *= 1.2f; // Хороші чаєві
                break;
        }
    }
    
    protected virtual void ConfigureDialogues(Customer customer, CustomerGenerationContext context)
    {
        // Налаштування діалогів залежно від контексту
        customer.dialogueLines = GetContextualDialogues(customer, context);
    }
    
    protected virtual void ConfigureAnimations(Customer customer, CustomerGenerationContext context)
    {
        // Налаштування анімацій залежно від типу клієнта
        customer.characterAnimator = GetAnimatorForCustomerType(customer.customerType);
    }
}
```

## Допоміжні класи

### CustomerGenerationContext
```csharp
[System.Serializable]
public class CustomerGenerationContext
{
    public TimeOfDay timeOfDay;
    public WeatherType weather;
    public GameEvent currentEvent;
    public int difficulty;
    public float tavernReputation;
    public DayOfWeek dayOfWeek;
    public bool isSpecialOccasion;
    public float timeModifier;
    public float weatherModifier;
    public float eventModifier;
}

public enum TimeOfDay
{
    Morning,    // 6:00 - 12:00
    Afternoon,  // 12:00 - 18:00
    Evening,    // 18:00 - 22:00
    Night       // 22:00 - 6:00
}

public enum WeatherType
{
    Sunny,
    Cloudy,
    Rainy,
    Stormy,
    Foggy,
    Snowy
}

public enum DayOfWeek
{
    Monday,
    Tuesday,
    Wednesday,
    Thursday,
    Friday,
    Saturday,
    Sunday
}
```

### CustomerTemplate
```csharp
[CreateAssetMenu(fileName = "New Customer Template", menuName = "Tavern/Customer Template")]
public class CustomerTemplate : ScriptableObject
{
    [Header("Basic Info")]
    public string customerName;
    public CustomerType customerType;
    public Sprite customerSprite;
    
    [Header("Spawn Settings")]
    public float baseSpawnWeight = 1f;
    public int minDifficulty = 1;
    public int maxDifficulty = 10;
    public bool isRare = false;
    
    [Header("Base Stats")]
    public float basePatienceMultiplier = 1f;
    public float baseTipMultiplier = 1f;
    public float baseStressGrowthRate = 1f;
    
    [Header("Visual")]
    public List<Sprite> emotionSprites;
    public Color customerColor = Color.white;
    public float sizeModifier = 1f;
    
    [Header("Preferences")]
    public List<Recipe> preferredRecipes;
    public List<IngredientType> dislikedIngredients;
    public List<Recipe> hatedRecipes;
    
    [Header("Time Preferences")]
    public List<TimeOfDay> preferredTimes;
    public List<TimeOfDay> avoidedTimes;
    
    [Header("Weather Preferences")]
    public List<WeatherType> preferredWeather;
    public List<WeatherType> avoidedWeather;
    
    [Header("Event Preferences")]
    public List<GameEvent> preferredEvents;
    public List<GameEvent> avoidedEvents;
    
    [Header("Behavior")]
    public CustomerBehaviorType behaviorType;
    public float patienceVariation = 0.2f;
    public float tipVariation = 0.2f;
    
    [Header("Dialogues")]
    public List<string> arrivalDialogues;
    public List<string> waitingDialogues;
    public List<string> servedDialogues;
    public List<string> angryDialogues;
    
    public bool IsAvailable(CustomerGenerationContext context)
    {
        // Перевірка часу дня
        if (avoidedTimes.Contains(context.timeOfDay))
            return false;
            
        // Перевірка погоди
        if (avoidedWeather.Contains(context.weather))
            return false;
            
        // Перевірка подій
        if (context.currentEvent != null && avoidedEvents.Contains(context.currentEvent))
            return false;
            
        return true;
    }
}

public enum CustomerBehaviorType
{
    Normal,
    Impatient,
    Patient,
    Demanding,
    Easygoing,
    VIP
}
```

### Модифікатори

#### TimeOfDayModifier
```csharp
[System.Serializable]
public class TimeOfDayModifier
{
    [Header("Spawn Weights")]
    public float morningWeight = 1f;
    public float afternoonWeight = 1.2f;
    public float eveningWeight = 1.5f;
    public float nightWeight = 0.5f;
    
    [Header("Patience Modifiers")]
    public float morningPatience = 1f;
    public float afternoonPatience = 0.9f;
    public float eveningPatience = 0.8f;
    public float nightPatience = 1.2f;
    
    public float GetModifier(CustomerTemplate template, TimeOfDay timeOfDay)
    {
        return timeOfDay switch
        {
            TimeOfDay.Morning => morningWeight,
            TimeOfDay.Afternoon => afternoonWeight,
            TimeOfDay.Evening => eveningWeight,
            TimeOfDay.Night => nightWeight,
            _ => 1f
        };
    }
    
    public float GetPatienceModifier(TimeOfDay timeOfDay)
    {
        return timeOfDay switch
        {
            TimeOfDay.Morning => morningPatience,
            TimeOfDay.Afternoon => afternoonPatience,
            TimeOfDay.Evening => eveningPatience,
            TimeOfDay.Night => nightPatience,
            _ => 1f
        };
    }
}
```

#### WeatherModifier
```csharp
[System.Serializable]
public class WeatherModifier
{
    [Header("Spawn Weights")]
    public float sunnyWeight = 1f;
    public float cloudyWeight = 1.1f;
    public float rainyWeight = 0.8f;
    public float stormyWeight = 0.3f;
    public float foggyWeight = 0.7f;
    public float snowyWeight = 0.6f;
    
    [Header("Patience Modifiers")]
    public float sunnyPatience = 1f;
    public float cloudyPatience = 1.1f;
    public float rainyPatience = 1.3f;
    public float stormyPatience = 0.7f;
    public float foggyPatience = 1.2f;
    public float snowyPatience = 1.4f;
    
    [Header("Time Modifiers")]
    public float sunnyTime = 1f;
    public float cloudyTime = 1f;
    public float rainyTime = 1.2f;
    public float stormyTime = 0.8f;
    public float foggyTime = 1.1f;
    public float snowyTime = 1.3f;
    
    public float GetModifier(CustomerTemplate template, WeatherType weather)
    {
        return weather switch
        {
            WeatherType.Sunny => sunnyWeight,
            WeatherType.Cloudy => cloudyWeight,
            WeatherType.Rainy => rainyWeight,
            WeatherType.Stormy => stormyWeight,
            WeatherType.Foggy => foggyWeight,
            WeatherType.Snowy => snowyWeight,
            _ => 1f
        };
    }
    
    public float GetPatienceModifier(WeatherType weather)
    {
        return weather switch
        {
            WeatherType.Sunny => sunnyPatience,
            WeatherType.Cloudy => cloudyPatience,
            WeatherType.Rainy => rainyPatience,
            WeatherType.Stormy => stormyPatience,
            WeatherType.Foggy => foggyPatience,
            WeatherType.Snowy => snowyPatience,
            _ => 1f
        };
    }
    
    public float GetTimeModifier(WeatherType weather)
    {
        return weather switch
        {
            WeatherType.Sunny => sunnyTime,
            WeatherType.Cloudy => cloudyTime,
            WeatherType.Rainy => rainyTime,
            WeatherType.Stormy => stormyTime,
            WeatherType.Foggy => foggyTime,
            WeatherType.Snowy => snowyTime,
            _ => 1f
        };
    }
}
```

#### EventModifier
```csharp
[System.Serializable]
public class EventModifier
{
    [Header("Event Modifiers")]
    public List<EventModifierData> eventModifiers = new List<EventModifierData>();
    
    public float GetModifier(CustomerTemplate template, GameEvent gameEvent)
    {
        if (gameEvent == null) return 1f;
        
        var modifier = eventModifiers.Find(m => m.eventType == gameEvent.eventType);
        return modifier?.spawnWeight ?? 1f;
    }
    
    public float GetPatienceModifier(GameEvent gameEvent)
    {
        if (gameEvent == null) return 1f;
        
        var modifier = eventModifiers.Find(m => m.eventType == gameEvent.eventType);
        return modifier?.patienceModifier ?? 1f;
    }
}

[System.Serializable]
public class EventModifierData
{
    public EventType eventType;
    public float spawnWeight = 1f;
    public float patienceModifier = 1f;
    public float priceMultiplier = 1f;
}
```

### DifficultyScaling
```csharp
[System.Serializable]
public class DifficultyScaling
{
    [Header("Customer Count")]
    public AnimationCurve customerCountCurve;
    
    [Header("Customer Types")]
    public AnimationCurve nobleChanceCurve;
    public AnimationCurve adventurerChanceCurve;
    public AnimationCurve travelerChanceCurve;
    
    [Header("Order Complexity")]
    public AnimationCurve complexOrderChanceCurve;
    public AnimationCurve multipleOrderChanceCurve;
    
    public int GetCustomerCount(int difficulty)
    {
        float normalizedDifficulty = Mathf.Clamp01(difficulty / 10f);
        return Mathf.RoundToInt(customerCountCurve.Evaluate(normalizedDifficulty) * 10);
    }
    
    public float GetNobleChance(int difficulty)
    {
        float normalizedDifficulty = Mathf.Clamp01(difficulty / 10f);
        return nobleChanceCurve.Evaluate(normalizedDifficulty);
    }
    
    public float GetComplexOrderChance(int difficulty)
    {
        float normalizedDifficulty = Mathf.Clamp01(difficulty / 10f);
        return complexOrderChanceCurve.Evaluate(normalizedDifficulty);
    }
}
```

## Приклади використання

### Створення клієнта
```csharp
// Простий виклик
Customer customer = CustomerFactory.Instance.CreateCustomer();

// З контекстом
var context = new CustomerGenerationContext
{
    timeOfDay = TimeOfDay.Evening,
    weather = WeatherType.Rainy,
    difficulty = 5,
    tavernReputation = 0.8f
};

Customer customer = CustomerFactory.Instance.CreateCustomer(context);
```

### Налаштування шаблонів
```csharp
// Створення шаблону аристократа
[CreateAssetMenu(fileName = "Noble Customer", menuName = "Tavern/Customers/Noble")]
public class NobleCustomerTemplate : CustomerTemplate
{
    private void Awake()
    {
        customerType = CustomerType.Noble;
        basePatienceMultiplier = 0.7f; // Менш терплячі
        baseTipMultiplier = 2.0f; // Більше платять
        baseStressGrowthRate = 1.5f; // Швидше нервуються
        
        preferredTimes = new List<TimeOfDay> { TimeOfDay.Evening };
        avoidedTimes = new List<TimeOfDay> { TimeOfDay.Night };
        
        behaviorType = CustomerBehaviorType.Demanding;
    }
}
```

## Висновок

CustomerFactory забезпечує:

1. **Різноманітність** - кожен клієнт унікальний
2. **Контекстну генерацію** - враховує час, погоду, події
3. **Масштабованість** - легко додавати нові типи клієнтів
4. **Балансування** - система ваг для контролю частоти появи
5. **Гнучкість** - можна налаштовувати всі аспекти клієнтів

Ключові особливості:
- **Контекстна генерація** - клієнти залежать від часу, погоди, подій
- **Система ваг** - контролює частоту появи різних типів
- **Випадкові варіації** - кожен клієнт трохи відрізняється
- **Модифікатори** - легко налаштовувати поведінку
- **ScriptableObjects** - для легкого створення нових типів 