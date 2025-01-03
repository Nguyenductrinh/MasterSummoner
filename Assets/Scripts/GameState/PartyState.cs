using GDEUtils.StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GDE.GenericSelectionUI;

public class PartyState : State<GameController>
{
    [SerializeField] PartyScreen partyScreen;

    public Monsters SelectedMonster { get; private set; }
    public static PartyState i { get; private set; }

    private void Awake()
    {
        i = this;
    }

    private GameController gc;

    public override void Enter(GameController owner)
    {
        gc = owner;

        SelectedMonster = null;

        partyScreen.gameObject.SetActive(true);
        partyScreen.OnSelected += OnMonsterSelected;
        partyScreen.OnBack += OnBack;
    }
    public override void Execute()
    {
        partyScreen.HandleUpdate();
    }

    public override void Exit()
    {
        partyScreen.gameObject.SetActive(false);
        partyScreen.OnSelected -= OnMonsterSelected;
        partyScreen.OnBack -= OnBack;
    }

    private void OnMonsterSelected(int selection)
    {
        SelectedMonster = partyScreen.SelectedMember;
        var prevState = gc.StateMachine.GetPrevState();

        if (prevState == InventoryState.i)
        {
            StartCoroutine(GotoItemState());
        }
        else if (prevState == BattleState.i)
        {
            var battleState = prevState as BattleState;
            if (SelectedMonster.HP <= 0)
            {
                partyScreen.SetMessageText("You can't send out a fainted monster.");
                return;
            }

            if (SelectedMonster == battleState.BattleSystem.PlayerUnit.Monsters)
            {
                partyScreen.SetMessageText("You can't switch with the same monster.");
                return;
            }

            gc.StateMachine.Pop();
        }
        else
        {
            Debug.Log($"Selected monster at index {selection}");
        }
    }

    private IEnumerator GotoItemState()
    {
        yield return gc.StateMachine.PushAndWait(UseItemState.i);
        gc.StateMachine.Pop();
    }

    private void OnBack()
    {
        SelectedMonster = null;
        var prevState = gc.StateMachine.GetPrevState();

        if (prevState == BattleState.i)
        {
            var battleState = prevState as BattleState;
            if (battleState.BattleSystem.PlayerUnit.Monsters.HP <= 0)
            {
                partyScreen.SetMessageText("You have to choose a monster to continue.");
                return;
            }
        }

        gc.StateMachine.Pop();
    }
}