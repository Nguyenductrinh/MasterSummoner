using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Item/Create new TM or HM ")]
public class TmItems : ItemBase
{
    [SerializeField] MoveBase move;
    [SerializeField] bool isHM;

    public override string Name => base.Name + $": {move.Name}";

    public override bool Use(Monsters monsters)
    {
        // Learning move is handled from Inventory Ui, If it was learned then return true
        return monsters.HasMove(move);
    }

    public bool CanBeTaught(Monsters monster)
    {
        return monster.Base.LearnableByItems.Contains(move);
    }

    public override bool IsReusable => isHM;

    public override bool CanUseInBattle => false;

    public MoveBase Move => move;
    public bool IsHM => isHM;
}
