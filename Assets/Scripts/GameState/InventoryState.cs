using GDEUtils.StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryState : State<GameController>
{
    [SerializeField] InventoryUI inventoryUI;
    public static InventoryState i {  get; set; }

    // Output
    public ItemBase SelectedItem { get; private set; }
    private void Awake()
    {
        i = this;
    }
    Inventory inventory;
    private void Start()
    {
        inventory = Inventory.GetInventory();
    }

    GameController gc;
    public override void Enter(GameController owner)
    {
        gc = owner;

        SelectedItem = null;

        inventoryUI.gameObject.SetActive(true);
        inventoryUI.OnSelected += OnItemSelected;
        inventoryUI.OnBack += OnBack;
    }

    public override void Execute()
    {
        inventoryUI.HandleUpdate();
    }

    public override void Exit()
    {
        inventoryUI.gameObject.SetActive(false);
        inventoryUI.OnSelected -= OnItemSelected;
        inventoryUI.OnBack -= OnBack;
    }

    void OnItemSelected(int selection)
    {
        SelectedItem = inventoryUI.SelectedItem;
        StartCoroutine(SelectMonsterAndItem());
    }
    public void OnBack()
    {
        SelectedItem = null;
        gc.StateMachine.Pop();
    }

    IEnumerator SelectMonsterAndItem()
    {
        var prevState = gc.StateMachine.GetPrevState();
        if (prevState == BattleState.i)
        {
            // In Battle
            if (!SelectedItem.CanUseInBattle)
            {
                yield return DialogManager.i.ShowDialogText("This item cannot be used in battle");
                yield break;
            }
        }
        else
        {
            // Out
            if (!SelectedItem.CanUseOutsideBattle)
            {
                yield return DialogManager.i.ShowDialogText("This item cannot be used outside battle");
                yield break;
            }
        }
        if(SelectedItem is MonsterballItem)
        {
            inventory.UseItem(SelectedItem, null);
            gc.StateMachine.Pop();
            yield break;
        }
        yield return gc.StateMachine.PushAndWait(PartyState.i);

        
        if(prevState == BattleState.i)
        {
            if (UseItemState.i.ItemUsed)
            {
                gc.StateMachine.Pop();
            }
        }
    }
}
