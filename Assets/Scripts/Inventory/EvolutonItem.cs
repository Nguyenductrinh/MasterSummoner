using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Item/Create new evolution item")]
public class EvolutonItem : ItemBase
{
    public override bool Use(Monsters monsters)
    {
        return true;
    }
}
