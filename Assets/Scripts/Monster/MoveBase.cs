using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using MonstersNamespace;

[CreateAssetMenu(fileName = "Move", menuName = "Monster/Create new move")]
public class MoveBase : ScriptableObject
{
    [SerializeField] string name;

    [TextArea]
    [SerializeField] string description;

    [SerializeField] MonsterType type;
    [SerializeField] int power;
    [SerializeField] int accuracy;
    [SerializeField] bool alwaysHits;
    [SerializeField] int pp;
    [SerializeField] int priority;
    
    [SerializeField] MoveCategory category;
    [SerializeField] MoveEffects effects;
    [SerializeField] List<SecondaryEffects> secondaries;
    [SerializeField] MoverTarget target;

    [SerializeField] AudioClip sound;
    public MonsterType Type // Expose it as a property
    {
        get { return type; }
    }

    public string Name
    {
        get { return name; }
    }

    public string Description
    {
        get { return description; }
    }

    public int Power
    {
        get { return power; }
    }

    public int Accuracy
    {
        get { return accuracy; }
    }

    public bool AlwaysHits
    {
        get { return alwaysHits; }
    }

    public int PP
    {
        get { return pp; }
    }
    public int Priority
    {
        get { return priority; }
    }
    public MoveCategory Category 
    { 
        get { return category; } 
    } 
    
    public MoveEffects Effects
    {
        get { return effects; }
    }

    public List<SecondaryEffects> Secondaries
    {
        get { return secondaries; }
    }

    public MoverTarget Target
    {
        get { return target; }
    }

    public AudioClip Sound => sound;
}

[System.Serializable]

public class MoveEffects
{
    [SerializeField] List<StatBoost> boosts;
    [SerializeField] ConditionID status;
    [SerializeField] ConditionID volatileStatus;

    public List<StatBoost> Boosts
    {
        get { return boosts; }
    }

    public ConditionID Status
    {
        get { return status; }
    }

    public ConditionID VolatileStatus
    {
        get { return status; }
    }
}

[System.Serializable]
public class SecondaryEffects: MoveEffects
{
    [SerializeField] int chance;
    [SerializeField] MoverTarget target;

    public int Chance
    {
        get { return chance; }
    }

    public MoverTarget Target
    {
        get { return target; }
    }
}

[System.Serializable]
public class StatBoost
{
    public Stat stat;
    public int boost;
}

public enum MoveCategory
{
    Physical, Special, Status
}

public enum MoverTarget
{
    Foe,Self
}
