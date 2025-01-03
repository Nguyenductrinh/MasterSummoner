using GDEUtils.StateMachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState { FreeRoam, Battle, Dialog, Menu, PartyScreen, Bag, Cutscene, Paused, Evolution }

public class GameController : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera worldCamera;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] InventoryUI inventoryUI;

    GameState state;
    GameState prevState;
    GameState stateBeforeEvolution;

    public StateMachine<GameController> StateMachine { get; private set; }

    public SceneDetails CurrentScene {  get; private set; } 
    public SceneDetails PrevScene {  get; private set; }
    public static GameController i {get; private set;}


    private void Awake()
    {
        i = this;

        MonsterDB.Init();
        MoveDB.Init();
        ConditionsDB.Init();
        ItemDB.Init();
    }
    private void Start()
    {
        StateMachine = new StateMachine<GameController>(this);
        StateMachine.ChangeState(FreeRoamState.i);

        // Đảm bảo sự kiện được đăng ký
        battleSystem.OnBattleOver += EndBattle;

        partyScreen.Init();

        DialogManager.i.OnShowDialog += () =>
        {
            prevState = state;
            state = GameState.Dialog;  // Chuyển trạng thái thành Dialog khi dialog được hiển thị
        };

        DialogManager.i.OnDialogFinished += () =>
        {
            if (state == GameState.Dialog)
            {
                state = prevState;
            }
        };
    }
    public void PauseGame(bool pause)
    {
        if (pause)
        {
            prevState = state;
            state = GameState.Paused;
        }
        else
        {
            state = prevState;
        }
    }
    public void StartBattle( BattleTrigger trigger)
    {
        BattleState.i.trigger = trigger;
        StateMachine.Push(BattleState.i);
    }
    TrainerController trainer;
    public void StartTrainerBattle(TrainerController trainer)
    {
        BattleState.i.trainer = trainer;
        StateMachine.Push(BattleState.i);
    }
    public void OnEnterTrainerView(TrainerController trainer) 
    {
        state = GameState.Cutscene;
        StartCoroutine(trainer.TriggerTrainerBattle(playerController));
    }

    void EndBattle(bool won)
    {
        if(trainer != null && won == true) 
        {
            trainer.BattleLost();
            trainer = null;
        }

        partyScreen.SetPartyData();

        state = GameState.FreeRoam;
        battleSystem.gameObject.SetActive(false);
        worldCamera.gameObject.SetActive(true);


        var playerParty = playerController.GetComponent<MonsterParty>();
        bool hasEvolutions = playerParty.CheckForEvolutions();

        if (hasEvolutions)
        {
            StartCoroutine(playerParty.RunEvolutions());
        }
        else
        {
            AudioManager.i.PlayMusic(CurrentScene.SceneMusic, fade: true);
        }
    }
    private void Update()
    {
        StateMachine.Execute();

        if(state == GameState.Dialog)
        {
            DialogManager.i.HandleUpdate();
        }
    }
    public void SetCurrentScene(SceneDetails currScene)
    {
        PrevScene = CurrentScene;
        CurrentScene = currScene;
    }
    private void OnGUI()
    {
        var style = new GUIStyle();
        style.fontSize = 24;

        GUILayout.Label("STATE STACK", style);
        foreach(var state in StateMachine.StateStack)
        {
            GUILayout.Label(state.GetType().ToString(), style);
        }
    }

    public GameState State => state;

    public PlayerController PlayerController => playerController;
    public Camera WorldCamera => worldCamera;

    public PartyScreen PartyScreen => partyScreen;
}
