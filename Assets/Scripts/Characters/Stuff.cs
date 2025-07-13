using System.Collections.Generic;
using UnityEngine;

namespace Characters
{
    public class Staff : Character
{
    [Header("Staff Info")]
    public StaffType staffType;
    
    // [Header("Abilities")]
    // public List<StaffAbility> abilities = new List<StaffAbility>();
    // public List<PassiveEffect> passiveEffects = new List<PassiveEffect>();
    
    // [Header("Work")]
    // public bool isWorking;
    // public float workEfficiency;
    // public Transform workStation;
    //
    // [Header("Relationships")]
    // public Dictionary<Staff, float> relationships = new Dictionary<Staff, float>();
    
    protected override void InitializeCharacter()
    {
        base.InitializeCharacter();
        //InitializeAbilities();
    }
    
    // protected virtual void InitializeAbilities()
    // {
    //     // Базові здібності залежно від типу персоналу
    //     switch (staffType)
    //     {
    //         case StaffType.Cook:
    //             abilities.Add(new CookingAbility());
    //             passiveEffects.Add(new CookingSpeedBoost());
    //             break;
    //         case StaffType.Waiter:
    //             abilities.Add(new CustomerServiceAbility());
    //             passiveEffects.Add(new CustomerPatienceBoost());
    //             break;
    //         case StaffType.Hunter:
    //             abilities.Add(new HuntingAbility());
    //             passiveEffects.Add(new PassiveMeatGeneration());
    //             break;
    //         case StaffType.Gardener:
    //             abilities.Add(new GardeningAbility());
    //             passiveEffects.Add(new PassivePlantGeneration());
    //             break;
    //     }
    // }
    
    // public virtual void StartWork()
    // {
    //     isWorking = true;
    //     ChangeState(CharacterState.Working);
    //     //ApplyPassiveEffects();
    // }
    //
    // public virtual void StopWork()
    // {
    //     isWorking = false;
    //     ChangeState(CharacterState.Idle);
    //     //RemovePassiveEffects();
    // }
    
    // protected virtual void ApplyPassiveEffects()
    // {
    //     foreach (var effect in passiveEffects)
    //     {
    //         effect.Apply(this);
    //     }
    // }
    //
    // protected virtual void RemovePassiveEffects()
    // {
    //     foreach (var effect in passiveEffects)
    //     {
    //         effect.Remove(this);
    //     }
    // }
    
    // public virtual void GainExperience(float points)
    // {
    //     experiencePoints += points;
    //     CheckLevelUp();
    // }
    
    // protected virtual void CheckLevelUp()
    // {
    //     float requiredExp = GetRequiredExperienceForLevel(experienceLevel + 1);
    //     if (experiencePoints >= requiredExp)
    //     {
    //         LevelUp();
    //     }
    // }
    //
    // protected virtual void LevelUp()
    // {
    //     experienceLevel++;
    //     UnlockNewAbilities();
    //     GameEvents.OnStaffLevelUp?.Invoke(this);
    // }
    //
    // protected virtual void UnlockNewAbilities()
    // {
    //     // Розблокування нових здібностей з рівнем
    //     var newAbilities = GetAbilitiesForLevel(experienceLevel);
    //     foreach (var ability in newAbilities)
    //     {
    //         if (!abilities.Contains(ability))
    //         {
    //             abilities.Add(ability);
    //         }
    //     }
    // }
    
    // public virtual void ImproveRelationship(Staff otherStaff, float amount)
    // {
    //     if (!relationships.ContainsKey(otherStaff))
    //     {
    //         relationships[otherStaff] = 0f;
    //     }
    //     
    //     relationships[otherStaff] = Mathf.Clamp(relationships[otherStaff] + amount, -100f, 100f);
    // }
}

public enum StaffType
{
    Cook,       // Кухар
    Waiter,     // Офіціант
    Hunter,     // Мисливець
    Gardener,   // Садівник
    Manager     // Менеджер
}
}