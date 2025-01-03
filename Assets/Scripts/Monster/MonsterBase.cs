using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Monster", menuName = "Monster/Create new Monster")]
public class MonsterBase : ScriptableObject
{
    [SerializeField] string name;

    [TextArea]
    [SerializeField] string description;

    [SerializeField] Sprite frontSprite;
    [SerializeField] Sprite backSprite;

    [SerializeField] MonsterType type1;
    [SerializeField] MonsterType type2;

    // Base Stats
    [SerializeField] int maxHP;
    [SerializeField] int attack;
    [SerializeField] int defense;
    [SerializeField] int spAttack;
    [SerializeField] int spDefense;
    [SerializeField] int speed;

    [SerializeField] int expYield;
    [SerializeField] GrowthRate growthRate;

    [SerializeField] int catchRate = 255;

    [SerializeField] List<LearnableMove> learnableMoves;
    [SerializeField] List<MoveBase> learnableByItems;

    [SerializeField] List<Evolution> evolutions;

    public static int MaxNumOfMoves { get; set; } = 4;

    public int GetExpForLevel(int level)
    {
        if(growthRate == GrowthRate.Fast)
        {
            return 4*(level * level * level)/5;
        }
        else if(growthRate == GrowthRate.Mediumfast)
        {
            return level * level * level;
        }

        return -1;
    }
    public string Name
    {
        get { return name; }
    }

    public string Description
    {
        get { return description; }
    }

    public int MaxHp
    {
        get { return maxHP; }
    }

    public int Attack
    {
        get { return attack; }
    }

    public int Defense
    {
        get { return defense; }
    }

    public int SpAttack
    {
        get { return spAttack; }
    }

    public int SpDefense
    {
        get { return spDefense; }
    }

    public int Speed
    {
        get { return speed; }
    }

    public MonsterType Type1
    {
        get { return type1; }
    }

    public MonsterType Type2
    {
        get { return type2; }
    }

    public List<LearnableMove> LearnableMoves
    {
        get { return learnableMoves; }
    }

    public List<MoveBase> LearnableByItems => learnableByItems;

    public List<Evolution> Evolutions => evolutions;

    public int CatchRate => catchRate;

    public int ExpYield => expYield;

    public GrowthRate GrowthRate => growthRate;


    [System.Serializable]
    public class LearnableMove
    {
        [SerializeField] MoveBase moveBase;
        [SerializeField] int level;

        public MoveBase Base
        {
            get { return moveBase; }
        }

        public int Level
        {
            get { return level; }
        }
    }

    public Sprite FrontSprite
    {
        get { return frontSprite; }
    }

    public Sprite BackSprite
    {
        get { return backSprite; }
    }

}

[System.Serializable]
public class Evolution
{
    [SerializeField] MonsterBase evolvesInto;
    [SerializeField] int requiresdLevel;
    [SerializeField] EvolutonItem requiresdItem;

    public MonsterBase EvolvesInto => evolvesInto;
    public int RequiredLevel => requiresdLevel;
    public EvolutonItem RequiredItem => requiresdItem;
}

// Đặt MonsterType bên ngoài lớp MonsterBase
public enum MonsterType
{
    None,
    Normal,
    Fire,
    Water,
    Electric,
    Grass,
    Ice,
    Fighting,
    Poison,
    Ground,
    Flying,
    Psychic,
    Bug,
    Rock,
    Ghost,
    Dragon
}

public enum GrowthRate
{
    Fast, Mediumfast
}

public enum Stat
{
    Attack,
    Defense,
    SpAttack,
    SpDefense,
    Speed,

    // These 2 are not actual stats. they're used to boost the moveAccuracy
    Accuracy,
    Evasion
}

public class TypeChart
{
    static float[][] chart =
{
    //                    NOR    FIR    WAT     ELE    GRS    ICE    FIG    POI    GRO    FLY    PSY    BUG    RCK    GHO    DRA    DRK    STL    FAI
    /*NOR*/  new float[] { 1f,   1f,    1f,     1f,    1f,    1f,    1f,    1f,    1f,    1f,    1f,    1f,    0.5f,  0f,    1f,    1f,    0.5f,  1f },
    /*FIR*/  new float[] { 1f,   0.5f,  0.5f,   1f,    2f,    2f,    1f,    1f,    1f,    1f,    1f,    2f,    0.5f,  1f,    0.5f,  1f,    2f,    1f },
    /*WAT*/  new float[] { 1f,   2f,    0.5f,   1f,    0.5f,  1f,    1f,    1f,    2f,    1f,    1f,    1f,    2f,    1f,    0.5f,  1f,    1f,    1f },
    /*ELE*/  new float[] { 1f,   1f,    2f,     0.5f,  0.5f,  1f,    1f,    1f,    0f,    2f,    1f,    1f,    1f,    1f,    1f,    1f,    0.5f,  1f },
    /*GRS*/  new float[] { 1f,   0.5f,  2f,     1f,    0.5f,  1f,    1f,    0.5f,  2f,    0.5f,  1f,    0.5f,  2f,    1f,    1f,    1f,    0.5f,  1f },
    /*ICE*/  new float[] { 1f,   0.5f,  0.5f,   1f,    2f,    0.5f,  1f,    1f,    2f,    2f,    1f,    1f,    1f,    1f,    2f,    1f,    0.5f,  1f },
    /*FIG*/  new float[] { 2f,   1f,    1f,     1f,    1f,    1f,    1f,    0.5f,  1f,    0.5f,  0.5f,  0.5f,  1f,    0f,    1f,    2f,    2f,    0.5f },
    /*POI*/  new float[] { 1f,   1f,    1f,     1f,    2f,    1f,    1f,    0.5f,  0.5f,  1f,    1f,    1f,    0.5f,  0.5f,  1f,    1f,    0f,    2f },
    /*GRO*/  new float[] { 1f,   2f,    1f,     2f,    0.5f,  1f,    1f,    2f,    1f,    0f,    1f,    0.5f,  2f,    1f,    1f,    1f,    2f,    1f },
    /*FLY*/  new float[] { 1f,   1f,    1f,     0.5f,  2f,    1f,    2f,    1f,    1f,    1f,    1f,    2f,    0.5f,  1f,    1f,    1f,    0.5f,  1f },
    /*PSY*/  new float[] { 1f,   1f,    1f,     1f,    1f,    1f,    2f,    1f,    1f,    1f,    0.5f,  1f,    1f,    1f,    1f,    0f,    0.5f,  1f },
    /*BUG*/  new float[] { 1f,   0.5f,  1f,     1f,    2f,    1f,    0.5f,  0.5f,  1f,    0.5f,  1f,    1f,    1f,    0.5f,  1f,    2f,    0.5f,  0.5f },
    /*RCK*/  new float[] { 1f,   2f,    1f,     1f,    1f,    2f,    0.5f,  1f,    0.5f,  2f,    1f,    1f,    1f,    1f,    1f,    1f,    0.5f,  1f },
    /*GHO*/  new float[] { 0f,   1f,    1f,     1f,    1f,    1f,    1f,    1f,    1f,    1f,    1f,    1f,    1f,    2f,    1f,    0.5f,  1f,    1f },
    /*DRA*/  new float[] { 1f,   1f,    1f,     1f,    1f,    1f,    1f,    1f,    1f,    1f,    1f,    1f,    1f,    1f,    2f,    1f,    0.5f,  0f },
    /*DRK*/  new float[] { 1f,   1f,    1f,     1f,    1f,    1f,    0.5f,  1f,    1f,    1f,    2f,    1f,    1f,    0.5f,  1f,    0.5f,  1f,    0.5f },
    /*STL*/  new float[] { 1f,   0.5f,  0.5f,   0.5f,  1f,    2f,    1f,    1f,    1f,    1f,    1f,    1f,    1f,    1f,    1f,    1f,    0.5f,  2f },
    /*FAI*/  new float[] { 1f,   0.5f,  1f,     1f,    1f,    1f,    2f,    0.5f,  1f,    1f,    1f,    1f,    1f,    1f,    2f,    2f,    0.5f,  1f }
};


    public static float GetEffectiveness(MonsterType attackType, MonsterType defenderType)
    {
        if (attackType == MonsterType.None || defenderType == MonsterType.None)
            return 1;
        
        int row = (int)attackType - 1;
        int col = (int)defenderType - 1;
        return chart[row][col];
    }
}
