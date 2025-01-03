using GDEUtils.StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BattleState : State<GameController>
{
    [SerializeField] BattleSystem battleSystem;

    public BattleTrigger trigger { get;set; }

    public TrainerController trainer { get; set; }

    public static BattleState i { get; private set; }

    private void Awake()
    {
        i = this;
    }

    GameController gc;
    public override void Enter(GameController owner)
    {
        gc = owner;

        battleSystem.gameObject.SetActive(true);
        gc.WorldCamera.gameObject.SetActive(false);

        // Lấy Party của người chơi và quái vật hoang dã
        var playerParty = gc.PlayerController.GetComponent<MonsterParty>();

        if (playerParty != null)
        {
            var wildMonster = gc.CurrentScene.GetComponent<MapArena>().GetRandomWildMonsters(trigger);
            var wildMonsterCopy = new Monsters(wildMonster.Base, wildMonster.Level);

            battleSystem.StartBattle(playerParty, wildMonsterCopy);
        }
        else
        {
            var trainerParty = trainer.GetComponent<MonsterParty>();
            battleSystem.StartTrainerBattle(playerParty, trainerParty);
        }

        battleSystem.OnBattleOver += EndBattle;
    }

    public override void Execute()
    {
        battleSystem.HandleUpdate();
    }

    public override void Exit()
    {
        battleSystem.gameObject.SetActive(false);
        gc.WorldCamera.gameObject.SetActive(true);

        battleSystem.OnBattleOver -= EndBattle;
    }

    void EndBattle(bool won)
    {
        if (trainer != null && won == true)
        {
            trainer.BattleLost();
            trainer = null;
        }

        gc.StateMachine.Pop();
    }

    public BattleSystem BattleSystem => battleSystem;
}
