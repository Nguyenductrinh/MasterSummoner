using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainerFov : MonoBehaviour, IPlayerTriggerable
{
    public void OnPlayerTrigged(PlayerController player)
    {
        player.Character.Animator.IsMoving = false;
        GameController.i.OnEnterTrainerView(GetComponentInParent<TrainerController>());
    }
}
