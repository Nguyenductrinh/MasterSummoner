using DG.Tweening;
using GDEUtils.StateMachine;
using System;
using System.Collections;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public enum BattleAction { Move, SwitchMonster, UseItem, Run }
public enum BattleTrigger { LongGrass, Water }
public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] Image playerImage;
    [SerializeField] Image trainerImage;
    [SerializeField] GameObject monsterballsSprite;
    [SerializeField] MoveToForgetSelectionUI moveSelectionUI;
    [SerializeField] InventoryUI inventoryUI;

    [Header("Adudio")]
    [SerializeField] AudioClip wildBattleMusic;
    [SerializeField] AudioClip trainerBattleMusic;
    [SerializeField] AudioClip battleVictoryMusic;
    [SerializeField] AudioClip wildVictoryMusic;

    public StateMachine<BattleSystem> StateMachine { get; private set; }

    public event Action<bool> OnBattleOver;

    public int SelectedMove {  get; set; }
    public BattleAction SelectedAction { get; set; }

    public Monsters SelectedMonster { get; set; }
    public ItemBase SelectedItem { get; set; }

    public bool IsBattleOver {  get; set; }
    public MonsterParty PlayerParty { get; private set; }
    public MonsterParty TrainerParty { get; private set; }
    public Monsters WildMonster { get; private set; }

    public bool IsTrainerBattle { get; private set; } = false; 
    PlayerController player;
    TrainerController trainer;

    public int EscapeAttempts { get;set; }
    public void StartBattle(MonsterParty playerParty, Monsters wildMonster)
    {
        this.PlayerParty = playerParty;
        this.WildMonster = wildMonster;
        player = playerParty.GetComponent<PlayerController>();
        IsTrainerBattle = false;

        AudioManager.i.PlayMusic(wildBattleMusic);

        StartCoroutine(SetUpBattle());
    }

    public void StartTrainerBattle(MonsterParty playerParty, MonsterParty trainerParty)
    {
        this.PlayerParty = playerParty;
        this.TrainerParty = trainerParty;

        IsTrainerBattle = true;
        player = playerParty.GetComponent<PlayerController>();
        trainer = trainerParty.GetComponent<TrainerController>();

        AudioManager.i.PlayMusic(trainerBattleMusic);

        StartCoroutine(SetUpBattle());
    }

    public IEnumerator SetUpBattle()
    {
        StateMachine = new StateMachine<BattleSystem>(this);
        playerUnit.Clear();
        enemyUnit.Clear();

        if (!IsTrainerBattle)
        {
            // wild Monster Battle
            playerUnit.SetUp(PlayerParty.GetHealthyMonster());
            enemyUnit.SetUp(WildMonster);

            dialogBox.SetMoveNames(playerUnit.Monsters.Moves);
            yield return dialogBox.TypeDialog($"A wild {playerUnit.Monsters.Base.Name} appeared");
        }
        else
        {
            // wild trainer battle

            // Show trainer and player sprites
            playerUnit.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(false);

            playerImage.gameObject.SetActive(true);
            trainerImage.gameObject.SetActive(true);

            playerImage.sprite = player.Sprite;
            trainerImage.sprite = trainer.Sprite;

            yield return dialogBox.TypeDialog($"{trainer.Name} wants to battle");

            // send out first monster trainer of the team
            trainerImage.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(true);

            var enemyMonster = TrainerParty.GetHealthyMonster();
            enemyUnit.SetUp(enemyMonster);
            yield return dialogBox.TypeDialog($"{trainer.Name} send out {enemyMonster.Base.Name}");

            // send out first monster player of the team
            playerImage.gameObject.SetActive(false);
            playerUnit.gameObject.SetActive(true);
            var playerMonster = PlayerParty.GetHealthyMonster();
            playerUnit.SetUp(playerMonster);
            yield return dialogBox.TypeDialog($"Go {playerMonster.Base.Name}!");
            dialogBox.SetMoveNames(playerUnit.Monsters.Moves);
        }

        IsBattleOver = false;
        EscapeAttempts = 0;
        partyScreen.Init();

        StateMachine.ChangeState(ActionSelectionState.i);
    }
    public void BattleOver(bool won)
    {
        IsBattleOver = true;
        PlayerParty.Monster.ForEach(p => p.OnBattleOver());
        playerUnit.Hud.ClearData();
        enemyUnit.Hud.ClearData();

        OnBattleOver(won);
    }
    public void HandleUpdate()
    {
        StateMachine.Execute();       
    }
    public IEnumerator SwitchMonster(Monsters newMonster)
    {
        if (playerUnit.Monsters.HP > 0)
        {
            yield return dialogBox.TypeDialog($"Come back {playerUnit.Monsters.Base.Name}");
            playerUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);
        }

        playerUnit.SetUp(newMonster);

        dialogBox.SetMoveNames(newMonster.Moves);

        yield return dialogBox.TypeDialog($"Go {newMonster.Base.Name}!");
    }
    public IEnumerator SendNextTrainerMonster()
    {
        var nextMonster = TrainerParty.GetHealthyMonster();

        enemyUnit.SetUp(nextMonster);
        yield return dialogBox.TypeDialog($"{trainer.Name} send out {nextMonster.Base.Name}");
    }
    public IEnumerator ThrowMonsterball(MonsterballItem monsterballItem)
    {

        if (IsTrainerBattle)
        {
            yield return dialogBox.TypeDialog($"You can't steal the trainer monsster!");
            yield break;
        }

        yield return dialogBox.TypeDialog($"{player.Name} used {monsterballItem.Name.ToUpper()}!");

        var monsterballObj = Instantiate(monsterballsSprite, playerUnit.transform.position - new Vector3(2, 0), Quaternion.identity);
        var monsterball = monsterballObj.GetComponent<SpriteRenderer>();
        monsterball.sprite = monsterballItem.Icon;


        // Animation Ball
        yield return monsterball.transform.DOJump(enemyUnit.transform.position + new Vector3(0, 2), 2f, 1, 1f).WaitForCompletion();
        yield return enemyUnit.PlayCaptureAniamtion();
        yield return monsterball.transform.DOMoveY(enemyUnit.transform.position.y - 1.3f, 0.5f).WaitForCompletion();

        int shakeCount = TryToCatchMonster(enemyUnit.Monsters, monsterballItem);

        for (int i = 0; i < Mathf.Min(shakeCount, 3); ++i)
        {
            yield return new WaitForSeconds(0.5f);
            yield return monsterball.transform.DOPunchRotation(new Vector3(0, 0, 10f), 0.8f).WaitForCompletion();
        }

        if (shakeCount == 4)
        {
            // monster is caught
            yield return dialogBox.TypeDialog($"{enemyUnit.Monsters.Base.Name} was caught");
            yield return monsterball.DOFade(0, 1.5f).WaitForCompletion();

            PlayerParty.AddMonster(enemyUnit.Monsters);
            yield return dialogBox.TypeDialog($"{enemyUnit.Monsters.Base.Name} has been added to your party");

            Destroy(monsterball);
            BattleOver(true);
        }
        else
        {
            // monster broke out
            yield return new WaitForSeconds(1f);
            monsterball.DOFade(0, 0.2f);
            yield return enemyUnit.PlayBreakOutAniamtion();

            if (shakeCount < 2)
            {
                yield return dialogBox.TypeDialog($"{enemyUnit.Monsters.Base.Name} broke free");
            }
            else
            {
                yield return dialogBox.TypeDialog($"Almost caught it");
            }

            Destroy(monsterball);
        }
    }

    int TryToCatchMonster(Monsters monster, MonsterballItem monsterballItem)
    {
        float a = (3 * monster.MaxHp - 2 * monster.HP) * monster.Base.CatchRate * monsterballItem.CatchRateModifier * ConditionsDB.GetStatusBonus(monster.Status) / (3 * monster.MaxHp);

        if (a > 255)
        {
            return 4;
        }

        float b = 1048560 / Mathf.Sqrt(Mathf.Sqrt(16711680 / a));

        int shakeCount = 0;
        while (shakeCount < 4)
        {
            if (UnityEngine.Random.Range(0, 65535) >= b)
            {
                break;
            }

            ++shakeCount;
        }

        return shakeCount;
    }

    public BattleDialogBox DialogBox => dialogBox;
    public BattleUnit PlayerUnit => playerUnit;
    public BattleUnit EnemyUnit => enemyUnit;
    public PartyScreen PartyScreen => partyScreen;
    public AudioClip BattleVictoryMusic => battleVictoryMusic;
}
