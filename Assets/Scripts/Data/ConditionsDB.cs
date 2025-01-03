//using MonstersNamespace;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionsDB
{
    public static void Init()
    {
        foreach(var kvp in Conditions)
        {
            var conditionId = kvp.Key;
            var condition = kvp.Value;

            condition.Id = conditionId;
        }
    }
    public static Dictionary<ConditionID, Condition> Conditions { get; set; } = new Dictionary<ConditionID, Condition>()
    {
        {
            ConditionID.psn,
            new Condition()
            {
                Name = "Poison",
                StartMessage = "has been poisoned",
                OnAfterTurn = (Monsters monster) =>
                {
                    monster.DecreaseHP(monster.MaxHp/8);
                    monster.StatusChanges.Enqueue($"{monster.Base.Name} hurt itself due to poison");
                }
            }
        },

        {
            ConditionID.brn,
            new Condition()
            {
                Name = "Burn",
                StartMessage = "has been Burned",
                OnAfterTurn = (Monsters monster) =>
                {
                    monster.DecreaseHP(monster.MaxHp/16);
                    monster.StatusChanges.Enqueue($"{monster.Base.Name} hurt itself due to burn");
                }
            }
        },

        {
            ConditionID.par,
            new Condition()
            {
                Name = "Paralyzed",
                StartMessage = "has been paralyzed",
                OnBeforeMove = (Monsters monster) =>
                {
                    if (Random.Range(1,5) == 1)
                    {
                        monster.StatusChanges.Enqueue($"{monster.Base.Name}'s paralyzed and cant't move");
                        return false;
                    }
                    return true;
                }
            }
        },

        {
            ConditionID.frz,
            new Condition()
            {
                Name = "Freeze",
                StartMessage = "has been frozen",
                OnBeforeMove = (Monsters monster) =>
                {
                    if (Random.Range(1,5) == 1)
                    {
                        monster.CureStatus();
                        monster.StatusChanges.Enqueue($"{monster.Base.Name}'s is not frozen anymore");
                        return true;
                    }
                    return false;
                }
            }
        },

        {
            ConditionID.slp,
            new Condition()
            {
                Name = "Sleep",
                StartMessage = "has been asleep",
                OnStart = (Monsters monster) =>
                {
                    // Sleep for 1-3 turn
                    monster.StatusTime = Random.Range(1,4);
                    Debug.Log($"Will be asleep for {monster.StatusTime} moves");
                }, 

                OnBeforeMove = (Monsters monster) =>
                {
                    if(monster.StatusTime < 0)
                    {
                        monster.CureStatus();
                        monster.StatusChanges.Enqueue($"{monster.Base.Name} woke up!");
                        return true;
                    }
                   monster.StatusTime--;
                   monster.StatusChanges.Enqueue($"{monster.Base.Name} is sleeping");
                   return false;
                }
            }
        },

        // Volatile Status Condition
        {
            ConditionID.confusion,
            new Condition()
            {
                Name = "Confution",
                StartMessage = "has been confued",
                OnStart = (Monsters monster) =>
                {
                    // Confution for 1-4 turn
                    monster.VolatileStatusTime = Random.Range(1,5);
                    Debug.Log($"Will be confued for {monster.VolatileStatus} moves");
                },

                OnBeforeMove = (Monsters monster) =>
                {
                    if(monster.VolatileStatusTime <= 0)
                    {
                        monster.CureVolatileStatus();
                        monster.StatusChanges.Enqueue($"{monster.Base.Name} kicked out of confution!");
                        return true;
                    }
                   monster.VolatileStatusTime--;

                    // 50% chance to do a move
                    if(Random.Range(1,3) == 1)
                        return true;

                    // Hurt by confution
                   monster.StatusChanges.Enqueue($"{monster.Base.Name} is confused");
                   monster.DecreaseHP(monster.MaxHp / 8);
                   monster.StatusChanges.Enqueue($"It hurt itself due to confusion");
                   return false;
                }
            }
        }
    };

    public static float GetStatusBonus(Condition condition)
    {
        if(condition == null)
        {
            return 1f;
        }
        else if(condition.Id == ConditionID.slp || condition.Id == ConditionID.frz)
        {
            return 2f;
        }
        else if (condition.Id == ConditionID.par || condition.Id == ConditionID.psn || condition.Id == ConditionID.brn)
        {
            return 1.5f;
        }
        return 1f;
    }
}

public enum ConditionID
{
    none, psn, brn, slp, par, frz, confusion
}
