using System.Collections.Generic;
using System.Linq;
using Characters;
using Data;
using Managers;
using UnityEngine;

namespace Core
{
    public class CustomerFactory
    {
        [Header("Customer Templates")]
        public List<CharacterData> customerTemplates => GameConfiguration.Instance.CustomerTemplates;
        
        //[Header("Generation Settings")]
        //public CustomerGenerationSettings generationSettings;
        
        public Customer CreateCustomer(CustomerGenerationContext context = null)
        {
            if (context == null)
            {
                context = CreateDefaultContext();
            }
            
            // Вибір шаблону клієнта
            CharacterData template = SelectCustomerTemplate(context);
            
            // Створення клієнта
            Customer customer = InstantiateCustomer(template);
            
            // Налаштування характеристик
            ConfigureCustomer(customer, template, context);
            
            // // Генерація замовлення
            // GenerateOrderForCustomer(customer, context);
            //
            // // Налаштування поведінки
            // ConfigureBehavior(customer, context);
            
            return customer;
        }
        
        protected virtual CustomerGenerationContext CreateDefaultContext()
        {
            return new CustomerGenerationContext
            {
                // timeOfDay = GameManager.Instance.GetCurrentTimeOfDay(),
                // weather = WeatherManager.Instance.GetCurrentWeather(),
                // currentEvent = EventManager.Instance.GetCurrentEvent(),
                // difficulty = GameManager.Instance.GetCurrentDifficulty(),
                // tavernReputation = GameManager.Instance.GetTavernReputation(),
                // dayOfWeek = GameManager.Instance.GetCurrentDayOfWeek()
            };
        }
        
        protected virtual CharacterData SelectCustomerTemplate(CustomerGenerationContext context)
        {
            List<CharacterData> availableTemplates = GetAvailableTemplates(context);
            
            Dictionary<CharacterData, float> weights = new Dictionary<CharacterData, float>();
            float totalWeight = 0f;
            
            foreach (var template in availableTemplates)
            {
                float weight = CalculateTemplateWeight(template, context);
                weights[template] = weight;
                totalWeight += weight;
            }
            
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
        
        protected virtual List<CharacterData> GetAvailableTemplates(CustomerGenerationContext context)
        {
            // return customerTemplates.Where(template => 
            //     template.IsAvailable(context) && 
            //     template.minDifficulty <= context.difficulty &&
            //     template.maxDifficulty >= context.difficulty
            // ).ToList();
            return customerTemplates;
        }
        
        protected virtual float CalculateTemplateWeight(CharacterData template, CustomerGenerationContext context)
        {
            float baseWeight = template.CustomerTemplate.BaseSpawnWeight;
            
            // Модифікатори залежно від контексту
            // baseWeight *= timeModifier.GetModifier(template, context.timeOfDay);
            // baseWeight *= weatherModifier.GetModifier(template, context.weather);
            // baseWeight *= eventModifier.GetModifier(template, context.currentEvent);
            //
            // // Модифікатор репутації
            // baseWeight *= GetReputationModifier(template, context.tavernReputation);
            //
            // // Модифікатор дня тижня
            // baseWeight *= GetDayOfWeekModifier(template, context.dayOfWeek);
            
            return baseWeight;
        }
        
        protected virtual Customer InstantiateCustomer(CharacterData template)
        {
            GameObject customerObj = Object.Instantiate(PrefabsManager.Instance.CustomerPrefab);
            Customer customer = customerObj.GetComponent<Customer>();
            return customer;
        }
        
        protected virtual void ConfigureCustomer(Customer customer, CharacterData template, CustomerGenerationContext context)
        {
            // Базові характеристики
            customer.Initialize(template);
            
            // Модифікація залежно від контексту
            //ApplyContextModifiers(customer, template, context);
            
            // Випадкові варіації
            //ApplyRandomVariations(customer, template);
            
            // Налаштування візуальних елементів
            //ConfigureVisualElements(customer, template);
        }
        
        // protected virtual void ApplyContextModifiers(Customer customer, CustomerTemplate template, CustomerGenerationContext context)
        // {
        //     // Модифікація терпіння залежно від часу дня
        //     float timeModifier = timeModifier.GetPatienceModifier(context.timeOfDay);
        //     customer.patienceMultiplier *= timeModifier;
        //     
        //     // Модифікація залежно від погоди
        //     float weatherModifier = weatherModifier.GetPatienceModifier(context.weather);
        //     customer.patienceMultiplier *= weatherModifier;
        //     
        //     // Модифікація залежно від події
        //     float eventModifier = eventModifier.GetPatienceModifier(context.currentEvent);
        //     customer.patienceMultiplier *= eventModifier;
        //     
        //     // Модифікація залежно від репутації таверни
        //     float reputationModifier = GetReputationPatienceModifier(context.tavernReputation);
        //     customer.patienceMultiplier *= reputationModifier;
        // }
        
        // protected virtual void ApplyRandomVariations(Customer customer, CustomerTemplate template)
        // {
        //     // Випадкові варіації характеристик (±20%)
        //     float variation = Random.Range(0.8f, 1.2f);
        //     customer.patienceMultiplier *= variation;
        //     customer.tipMultiplier *= variation;
        //     
        //     // Випадкові особливості
        //     if (Random.Range(0f, 1f) < 0.1f) // 10% шанс
        //     {
        //         customer.patienceMultiplier *= 1.5f; // Дуже терплячий
        //     }
        //     
        //     if (Random.Range(0f, 1f) < 0.05f) // 5% шанс
        //     {
        //         customer.tipMultiplier *= 2f; // Дуже щедрий
        //     }
        // }
        
        // protected virtual void ConfigureVisualElements(Customer customer, CustomerTemplate template)
        // {
        //     // Налаштування емодзі
        //     customer.emotionSprites = template.emotionSprites;
        //     
        //     // Налаштування кольору
        //     if (template.customerColor != Color.white)
        //     {
        //         customer.spriteRenderer.color = template.customerColor;
        //     }
        //     
        //     // Налаштування розміру
        //     if (template.sizeModifier != 1f)
        //     {
        //         customer.transform.localScale *= template.sizeModifier;
        //     }
        // }
        //
        // protected virtual void GenerateOrderForCustomer(Customer customer, CustomerGenerationContext context)
        // {
        //     // Вибір рецепту
        //     Recipe selectedRecipe = SelectRecipeForCustomer(customer, context);
        //     
        //     // Створення замовлення
        //     Order order = new Order(selectedRecipe);
        //     
        //     // Модифікація замовлення залежно від контексту
        //     ModifyOrderBasedOnContext(order, context);
        //     
        //     // Призначення замовлення клієнту
        //     customer.PlaceOrder(order);
        // }
        //
        // protected virtual Recipe SelectRecipeForCustomer(Customer customer, CustomerGenerationContext context)
        // {
        //     List<Recipe> availableRecipes = GetAvailableRecipesForCustomer(customer, context);
        //     
        //     if (availableRecipes.Count == 0)
        //     {
        //         return RecipeManager.Instance.GetDefaultRecipe();
        //     }
        //     
        //     // Розрахунок ваги для кожного рецепту
        //     Dictionary<Recipe, float> weights = new Dictionary<Recipe, float>();
        //     float totalWeight = 0f;
        //     
        //     foreach (var recipe in availableRecipes)
        //     {
        //         float weight = CalculateRecipeWeight(recipe, customer, context);
        //         weights[recipe] = weight;
        //         totalWeight += weight;
        //     }
        //     
        //     // Випадковий вибір
        //     float randomValue = Random.Range(0f, totalWeight);
        //     float currentWeight = 0f;
        //     
        //     foreach (var kvp in weights)
        //     {
        //         currentWeight += kvp.Value;
        //         if (randomValue <= currentWeight)
        //         {
        //             return kvp.Key;
        //         }
        //     }
        //     
        //     return availableRecipes[0];
        // }
        //
        // protected virtual List<Recipe> GetAvailableRecipesForCustomer(Customer customer, CustomerGenerationContext context)
        // {
        //     List<Recipe> recipes = new List<Recipe>();
        //     
        //     // Рецепти залежно від типу клієнта
        //     recipes.AddRange(RecipeManager.Instance.GetRecipesForCustomerType(customer.customerType));
        //     
        //     // Рецепти залежно від часу дня
        //     recipes.AddRange(RecipeManager.Instance.GetRecipesForTimeOfDay(context.timeOfDay));
        //     
        //     // Рецепти залежно від погоди
        //     recipes.AddRange(RecipeManager.Instance.GetRecipesForWeather(context.weather));
        //     
        //     // Рецепти залежно від події
        //     if (context.currentEvent != null)
        //     {
        //         recipes.AddRange(RecipeManager.Instance.GetRecipesForEvent(context.currentEvent));
        //     }
        //     
        //     // Видалення дублікатів
        //     return recipes.Distinct().ToList();
        // }
        //
        // protected virtual float CalculateRecipeWeight(Recipe recipe, Customer customer, CustomerGenerationContext context)
        // {
        //     float weight = recipe.baseOrderWeight;
        //     
        //     // Бонус за улюблений рецепт
        //     if (customer.preferredRecipes.Contains(recipe))
        //     {
        //         weight *= 2f;
        //     }
        //     
        //     // Бонус за сезонність
        //     if (recipe.IsSeasonal(context.timeOfDay))
        //     {
        //         weight *= 1.5f;
        //     }
        //     
        //     // Бонус за складність (залежно від рівня гравця)
        //     if (recipe.difficulty <= context.difficulty)
        //     {
        //         weight *= 1.2f;
        //     }
        //     
        //     return weight;
        // }
        //
        // protected virtual void ModifyOrderBasedOnContext(Order order, CustomerGenerationContext context)
        // {
        //     // Модифікація ціни залежно від події
        //     if (context.currentEvent != null)
        //     {
        //         order.basePrice *= context.currentEvent.priceMultiplier;
        //     }
        //     
        //     // Модифікація часу очікування залежно від погоди
        //     order.timeLimit *= weatherModifier.GetTimeModifier(context.weather);
        // }
        //
        // protected virtual void ConfigureBehavior(Customer customer, CustomerGenerationContext context)
        // {
        //     // Налаштування поведінки залежно від типу клієнта
        //     ConfigureCustomerBehavior(customer, context);
        //     
        //     // Налаштування діалогів
        //     ConfigureDialogues(customer, context);
        //     
        //     // Налаштування анімацій
        //     ConfigureAnimations(customer, context);
        // }
        //
        // protected virtual void ConfigureCustomerBehavior(Customer customer, CustomerGenerationContext context)
        // {
        //     // Різні типи поведінки залежно від типу клієнта
        //     switch (customer.customerType)
        //     {
        //         case CustomerType.Noble:
        //             customer.stressGrowthRate *= 1.5f; // Швидше нервуються
        //             customer.tipMultiplier *= 1.8f; // Більше платять
        //             break;
        //             
        //         case CustomerType.Traveler:
        //             customer.patienceMultiplier *= 1.3f; // Більш терплячі
        //             customer.stressGrowthRate *= 0.8f; // Повільніше нервуються
        //             break;
        //             
        //         case CustomerType.Local:
        //             customer.tipMultiplier *= 0.8f; // Менше платять
        //             customer.patienceMultiplier *= 1.1f; // Трохи терплячіші
        //             break;
        //             
        //         case CustomerType.Adventurer:
        //             customer.stressGrowthRate *= 0.7f; // Дуже терплячі
        //             customer.tipMultiplier *= 1.2f; // Хороші чаєві
        //             break;
        //     }
        // }
        //
        // protected virtual void ConfigureDialogues(Customer customer, CustomerGenerationContext context)
        // {
        //     // Налаштування діалогів залежно від контексту
        //     customer.dialogueLines = GetContextualDialogues(customer, context);
        // }
        //
        // protected virtual void ConfigureAnimations(Customer customer, CustomerGenerationContext context)
        // {
        //     // Налаштування анімацій залежно від типу клієнта
        //     customer.characterAnimator = GetAnimatorForCustomerType(customer.customerType);
        // }
    }
}