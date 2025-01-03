using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class LongGrass : MonoBehaviour, IPlayerTriggerable
{
    public void OnPlayerTrigged(PlayerController player)
    {
        if (UnityEngine.Random.Range(1, 101) <= 10)
        {
            player.Character.Animator.IsMoving = false;
            GameController.i.StartBattle(BattleTrigger.LongGrass);
        }
    }

    public bool TriggerRepetedly => true;
}
