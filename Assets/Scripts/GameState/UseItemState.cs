using GDEUtils.StateMachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class UseItemState : State<GameController>
{
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] InventoryUI inventoryUI;

    //Output
    public bool ItemUsed {  get; private set; }
    
    public static UseItemState i { get; private set; }
    Inventory inventory;

    private void Awake()
    {
        i = this;
        inventory = Inventory.GetInventory();
    }
    GameController gc;
    public override void Enter(GameController owner)
    {
        gc = owner;

        ItemUsed = false;

        StartCoroutine(UseItem()); 
    }
    IEnumerator UseItem()
    {
        var item = inventoryUI.SelectedItem;
        var monster = partyScreen.SelectedMember;

        if (item == null || monster == null)
        {
            gc.StateMachine.Pop();
            yield break;
        }

        // Evolution
        if (item is TmItems)
        {
            yield return HandleTmItems();
        }
        else 
        {
            if (item is EvolutonItem)
            {
                var evolution = monster.CheckForEvolution(item);
                if (evolution != null)
                {
                    yield return EvolutionState.i.Evolve(monster, evolution);
                }
                else
                {
                    yield return DialogManager.i.ShowDialogText("It won't have any effect!");
                    gc.StateMachine.Pop();
                    yield break;
                }
            }

            var usedItem = inventory.UseItem(item,partyScreen.SelectedMember);
            if (usedItem != null)
            {
                ItemUsed = true;

                if(usedItem is RecoveryItem)
                {
                    yield return DialogManager.i.ShowDialogText($"The player used {usedItem.Name}");
                }

            }
            else
            {
                if(inventoryUI.SelectedCategory == (int)ItemCategory.Items)
                {
                    yield return DialogManager.i.ShowDialogText("It won't have any effect!");
                }
            }
        }
        gc.StateMachine.Pop();
    }
    IEnumerator HandleTmItems()
    {
        var tmItem = inventoryUI.SelectedItem as TmItems;
        if (tmItem == null)
        {
            yield break;
        }
        var monster = partyScreen.SelectedMember;

        if (monster.HasMove(tmItem.Move))
        {
            yield return DialogManager.i.ShowDialogText($"{monster.Base.Name} already know {tmItem.Move.Name}");
            yield break;
        }

        if (!tmItem.CanBeTaught(monster))
        {
            yield return DialogManager.i.ShowDialogText($"{monster.Base.Name} can't learn {tmItem.Move.Name}");
            yield break;
        }

        if (monster.Moves.Count < MonsterBase.MaxNumOfMoves)
        {
            monster.LearnMove(tmItem.Move);
            yield return DialogManager.i.ShowDialogText($"{monster.Base.Name} learned {tmItem.Move.Name}");
        }
        else
        {
            yield return DialogManager.i.ShowDialogText($"{monster.Base.Name} is trying to learn {tmItem.Move.Name}");
            yield return DialogManager.i.ShowDialogText($"But it cannot learn more than {MonsterBase.MaxNumOfMoves} moves");
            
            yield return DialogManager.i.ShowDialogText($"Choose a move you wan't tp forget", true, false);

            MoveToForgetState.i.NewMove = tmItem.Move;
            MoveToForgetState.i.CurrentMoves = monster.Moves.Select(m => m.Base).ToList();
            yield return gc.StateMachine.PushAndWait(MoveToForgetState.i);

            var moveIndex = MoveToForgetState.i.Selection;
            if (moveIndex == MonsterBase.MaxNumOfMoves || moveIndex == -1)
            {
                // Don't learn the new move
                yield return DialogManager.i.ShowDialogText($"{monster.Base.Name} did not learn {tmItem.Move.Name}");
            }
            else
            {
                // Forget the selected move and learn new move
                var selectedMove = monster.Moves[moveIndex].Base;
                yield return DialogManager.i.ShowDialogText($"{monster.Base.Name} forgot {selectedMove.Name} and learned {tmItem.Move.Name}");

                monster.Moves[moveIndex] = new Move(tmItem.Move);
            }
        }
    }
}
