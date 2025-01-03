using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCController : MonoBehaviour, Interactable
{
    [SerializeField] Dialog dialog;
    [SerializeField] List<Vector2> movementPattern;
    [SerializeField] float timeBetweenPattern;

    NPCState state;
    float idleTimer = 0f;
    int currentPattern = 0;

    Character character;
    ItemGiver itemGiver;

    private void Awake()
    {
        character = GetComponent<Character>();
        itemGiver = GetComponent<ItemGiver>();
    }

    public IEnumerator Interact(Transform initiator)
    {
        if(state == NPCState.Idle)
        {
            state = NPCState.Dialog;
            character.LookTowards(initiator.position);

            if(itemGiver != null  && itemGiver.CanBeGiven())
            {
                yield return itemGiver.GiverItem(initiator.GetComponent<PlayerController>());
            }
            else
            {
                yield return DialogManager.i.ShowDialog(dialog);
            }
            idleTimer = 0f;
            state = NPCState.Idle;
        }
    }

    private void Update()
    {

        if(state == NPCState.Idle)
        {
            idleTimer += Time.deltaTime;
            if(idleTimer > timeBetweenPattern)
            {
                idleTimer = 0f;
                if(movementPattern.Count > 0 )
                {
                    StartCoroutine(Walk());
                }
            }
        }

        character.HandleUpdate();
    }
    IEnumerator Walk()
    {
        state = NPCState.Walking;

        var oldPos = transform.position;

        yield return character.Move(movementPattern[currentPattern], () => {
            Debug.Log("NPC đã hoàn thành bước đi.");
        });

        if(transform.position != oldPos)
        {
            currentPattern = (currentPattern + 1) % movementPattern.Count;
        }

        state = NPCState.Idle;
    }
}

public enum NPCState { Idle, Walking, Dialog}
