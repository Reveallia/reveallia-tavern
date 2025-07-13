using System.Collections.Generic;

namespace Characters
{
    [System.Serializable]
    public class Order
    {
        // public string orderId;
        // public Recipe recipe;
        // public float basePrice;
        // public float timeLimit;
        // public List<IngredientRequirement> requiredIngredients;
        // public OrderPriority priority;
        //
        // public Order(Recipe recipe)
        // {
        //     this.recipe = recipe;
        //     this.basePrice = recipe.basePrice;
        //     this.timeLimit = recipe.cookingTime * 2f; // Подвійний час для терпіння
        //     this.requiredIngredients = new List<IngredientRequirement>(recipe.ingredients);
        //     this.orderId = System.Guid.NewGuid().ToString();
        // }
        //
        // public bool IsSameAs(Order other)
        // {
        //     return recipe == other.recipe;
        // }
        //
        // public bool HasRequiredIngredients(List<Ingredient> availableIngredients)
        // {
        //     foreach (var requirement in requiredIngredients)
        //     {
        //         bool hasIngredient = availableIngredients.Any(i => 
        //             i.ingredientType == requirement.ingredientType && 
        //             i.quantity >= requirement.quantity);
        //         
        //         if (!hasIngredient)
        //             return false;
        //     }
        //     return true;
        // }
    }

    public enum OrderPriority
    {
        Low,
        Normal,
        High,
        Urgent
    }
}