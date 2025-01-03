using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using TMPro.Examples;
using System.Linq;

public class PlayerController : MonoBehaviour, ISavable
{
    [SerializeField] string name;
    [SerializeField] Sprite sprite;

    private Vector2 input;

    private Character character;
    public static PlayerController i { get; private set; }

    private void Awake()
    {
        if (i == null)
        {
            i = this;
        }
        else
        {
            Debug.LogWarning("Multiple PlayerController instances detected!");
            Destroy(gameObject);
        }
        character = GetComponent<Character>();
    }

    public void HandleUpdate()
    {
        if (!character.IsMoving) // Chỉ di chuyển khi không có hành động di chuyển trước đó
        {
           input.x = Input.GetAxisRaw("Horizontal");
           input.y = Input.GetAxisRaw("Vertical");

        if(input.x != 0)
        {
           input.y = 0;
        }

        if(input != Vector2.zero)
        {
           StartCoroutine(character.Move(input, OnMoveOver));
        }
    }
        character.HandleUpdate();

        if(Input.GetKeyDown(KeyCode.Z))
        {
            StartCoroutine(Interact());
        }
    }

    IEnumerator Interact()
    {
        var faceDir = new Vector3(character.Animator.MoveX, character.Animator.MoveY);
        var interactPos =  transform.position + faceDir;

        var collider = Physics2D.OverlapCircle(interactPos, 0.5f, GameLayers.i.InteractebLayer);
        if(collider != null)
        {
            yield return collider.GetComponent<Interactable>()?.Interact(transform);
        }
    }

    private void OnMoveOver()
    {
        var colliders = Physics2D.OverlapCircleAll(transform.position - new Vector3(0, character.OffsetY), 0.2f, GameLayers.i.TriggerableLayer);

        foreach (var collider in colliders)
        {
            var triggerable =  collider.GetComponent<IPlayerTriggerable>();
            if(triggerable != null)
            {
                triggerable.OnPlayerTrigged(this);
                break;
            }
        }
    }

    public object CaptureState()
    {
        var saveData = new PlayerSaveData()
        {
            position = new float[] { transform.position.x, transform.position.y },
            monsters = GetComponent<MonsterParty>().Monster.Select(p => p.GetSaveData()).ToList()
        };

        return saveData;
    }

    public void RestoreState(object state)
    {
        var saveData = (PlayerSaveData)state;

        // Restor Position
        var pos = saveData.position;
        transform.position = new Vector3(pos[0], pos[1]);

        // Restor Party
        GetComponent<MonsterParty>().Monster =  saveData.monsters.Select(s => new Monsters(s)).ToList();
    }

    public string Name
    {
        get => name;
    }

    public Sprite Sprite
    {
        get => sprite;
    }

    public Character Character => character;
}

[Serializable]
public class PlayerSaveData
{
    public float[] position;
    public List<MonsterSaveData> monsters;
}
