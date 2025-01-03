using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Item/Create new monsterball")]
public class MonsterballItem : ItemBase
{
    [SerializeField] float catchRateModifier = 1;
    public override bool Use(Monsters monster)
    {
        return true;
    }

    public override bool CanUseOutsideBattle => false;

    public float CatchRateModifier => catchRateModifier;
}
