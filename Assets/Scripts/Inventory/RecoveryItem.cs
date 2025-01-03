using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Item/Create new recovery item")]
public class RecoveryItem : ItemBase
{
    [Header("HP")]
    [SerializeField] int hpAmount;
    [SerializeField] bool restoreMaxHp;

    [Header("PP")]
    [SerializeField] int ppAmount;
    [SerializeField] bool restoreMaxPP;

    [Header("Status Conditions")]
    [SerializeField] ConditionID status;
    [SerializeField] bool recoveryAllStatus;

    [Header("Revive")]
    [SerializeField] bool revive;
    [SerializeField] bool maxRevive;

    public override bool Use(Monsters monsters)
    {
        // revice
        if(revive || maxRevive)
        {
            if(monsters.HP > 0)
            {
                return false;
            }
            if(revive)
            {
                monsters.IncreaseHP(monsters.HP/2);
            }
            else if(maxRevive)
            {
                monsters.IncreaseHP(monsters.MaxHp);
            }
            monsters.CureStatus();

            return true;
        }

        // no other items can be used on fainted monster
        if(monsters.HP == 0)
        {
            return false;
        }

        //restoreHp
        if(restoreMaxHp || hpAmount > 0)
        {
            if(monsters.HP == monsters.MaxHp)
            {
                return false;
            }
            if (restoreMaxHp)
            {
                monsters.IncreaseHP(monsters.MaxHp);
            }
            else
            {
                monsters.IncreaseHP(hpAmount);
            }
        }

        // Recover status
        if(recoveryAllStatus || status != ConditionID.none)
        {
            if(monsters.Status == null && monsters.VolatileStatus == null)
            {
                return false;
            }
            if (recoveryAllStatus)
            {
                monsters.CureStatus();
                monsters.CureVolatileStatus();
            }
            else
            {
                if(monsters.Status.Id == status)
                {
                    monsters.CureStatus();
                }
                else if(monsters.VolatileStatus.Id == status)
                {
                    monsters.CureVolatileStatus();
                }
                else
                {
                    return false ;
                }
            }
        }

        // Restone PP
        if (restoreMaxPP)
        {
            monsters.Moves.ForEach(m => m.IncresePP(m.Base.PP));
        }
        else if(ppAmount > 0)
        {
            monsters.Moves.ForEach(m => m.IncresePP(ppAmount));
        }

        return true;
    }
}
