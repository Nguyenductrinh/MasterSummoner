using GDEUtils.StateMachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class RunTurnState : State<BattleSystem>
{
    public static RunTurnState i {  get; private set; }
    private void Awake()
    {
        i = this;
    }

    BattleSystem bs;

    BattleUnit playerUnit;
    BattleUnit enemyUnit;
    BattleDialogBox dialogBox;
    PartyScreen partyScreen;
    bool isTrainerBattle;
    MonsterParty playerParty;
    MonsterParty trainerParty;

    public override void Enter(BattleSystem owner)
    {
        bs = owner;

        playerUnit = bs.PlayerUnit;
        enemyUnit = bs.EnemyUnit;
        dialogBox = bs.DialogBox;
        partyScreen = bs.PartyScreen;
        isTrainerBattle = bs.IsTrainerBattle;
        playerParty = bs.PlayerParty;
        trainerParty = bs.TrainerParty;

        StartCoroutine(RunTurns(bs.SelectedAction));
    }

    IEnumerator RunTurns(BattleAction playerAction)
    {
        if (playerAction == BattleAction.Move)
        {
            playerUnit.Monsters.CurrentMove = playerUnit.Monsters.Moves[bs.SelectedMove];
            enemyUnit.Monsters.CurrentMove = enemyUnit.Monsters.GetRandomMove();

            int playerMovePriorty = playerUnit.Monsters.CurrentMove.Base.Priority;
            int enemyMovePriorty = enemyUnit.Monsters.CurrentMove.Base.Priority;

            // Check who goes first
            bool playerGoesFirst = true;
            if (enemyMovePriorty > playerMovePriorty)
            {
                playerGoesFirst = false;
            }
            else if (enemyMovePriorty == playerMovePriorty)
            {
                playerGoesFirst = playerUnit.Monsters.Speed >= enemyUnit.Monsters.Speed;
            }

            var firstUnit = (playerGoesFirst) ? playerUnit : enemyUnit;
            var secondUnit = (playerGoesFirst) ? enemyUnit : playerUnit;

            var secondMonster = secondUnit.Monsters;

            // First Turn
            yield return RunMove(firstUnit, secondUnit, firstUnit.Monsters.CurrentMove);
            yield return RunAfterTurn(firstUnit);
            if (bs.IsBattleOver) yield break;

            if (secondMonster.HP > 0)
            {
                // Second Turn
                yield return RunMove(secondUnit, firstUnit, secondUnit.Monsters.CurrentMove);
                yield return RunAfterTurn(secondUnit);
                if (bs.IsBattleOver) yield break;
            }
        }
        else
        {
            if (playerAction == BattleAction.SwitchMonster)
            {
                yield return bs.SwitchMonster(bs.SelectedMonster);
            }
            else if (playerAction == BattleAction.UseItem)
            {
                if(bs.SelectedItem is MonsterballItem)
                {
                    yield return bs.ThrowMonsterball(bs.SelectedItem as MonsterballItem);
                    if (bs.IsBattleOver) yield break;
                }
                else
                {
                    // This handle from item screen, so do nothing end skip to enemy move 
                }
            }
            else if (playerAction == BattleAction.Run)
            {
                yield return TryToEscape();
            }

            // Enemy Turn
            var enemyMove = enemyUnit.Monsters.GetRandomMove();
            yield return RunMove(enemyUnit, playerUnit, enemyMove);
            yield return RunAfterTurn(enemyUnit);
            if (bs.IsBattleOver) yield break;
        }

        if (!bs.IsBattleOver)
        {
            bs.StateMachine.ChangeState(ActionSelectionState.i);
        }
    }
    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetunit, Move move)
    {
        bool canRunMove = sourceUnit.Monsters.OnBeforeMove();
        if (!canRunMove)
        {
            yield return ShowStatusChanges(sourceUnit.Monsters);
            yield return sourceUnit.Hud.WaitForHPUpdate();
            yield break;
        }
        yield return ShowStatusChanges(sourceUnit.Monsters);

        move.PP--;
        yield return dialogBox.TypeDialog($"{sourceUnit.Monsters.Base.Name} used {move.Base.Name}");

        if (CheckIfMoveHits(move, sourceUnit.Monsters, targetunit.Monsters))
        {
            sourceUnit.PlayerAttackAnimation();

            AudioManager.i.PlaySfx(move.Base.Sound);

            yield return new WaitForSeconds(1f);

            targetunit.playHitAnimation();
            AudioManager.i.PlaySfx(AudioId.Hit);

            if (move.Base.Category == MoveCategory.Status)
            {
                yield return RunMoveEffects(move.Base.Effects, sourceUnit.Monsters, targetunit.Monsters, move.Base.Target);
            }
            else
            {
                var damageDetails = targetunit.Monsters.TakeDamage(move, sourceUnit.Monsters);
                yield return targetunit.Hud.WaitForHPUpdate();
                yield return ShowDamageDetails(damageDetails);
            }

            if (move.Base.Secondaries != null && move.Base.Secondaries.Count > 0 && targetunit.Monsters.HP > 0)
            {
                foreach (var secondary in move.Base.Secondaries)
                {
                    var rnd = UnityEngine.Random.Range(1, 101);
                    if (rnd <= secondary.Chance)
                    {
                        yield return RunMoveEffects(secondary, sourceUnit.Monsters, targetunit.Monsters, secondary.Target);
                    }
                }
            }

            if (targetunit.Monsters.HP <= 0)
            {
                yield return HandleMonsterFainted(targetunit);
            }
        }
        else
        {
            yield return dialogBox.TypeDialog($"{sourceUnit.Monsters.Base.Name}'s attack missed ");
        }
    }
    IEnumerator RunMoveEffects(MoveEffects effects, Monsters source, Monsters target, MoverTarget moverTarget)
    {

        // Start Boosting
        if (effects.Boosts != null)
        {
            if (moverTarget == MoverTarget.Self)
            {
                source.ApplyBoosts(effects.Boosts);
            }
            else
            {
                target.ApplyBoosts(effects.Boosts);
            }
        }

        // Status Condition
        if (effects.Status != ConditionID.none)
        {
            target.SetStatus(effects.Status);
        }

        // volatile Status Condition
        if (effects.VolatileStatus != ConditionID.none)
        {
            target.SetVolatileStatus(effects.VolatileStatus);
        }

        yield return ShowStatusChanges(source);
        yield return ShowStatusChanges(target);
    }
    IEnumerator RunAfterTurn(BattleUnit sourceUnit)
    {
        if (bs.IsBattleOver)
        {
            yield break;
        }

        // Status like burn or psn will hurt the monster after the turn
        sourceUnit.Monsters.OnAfterTurn();
        yield return ShowStatusChanges(sourceUnit.Monsters);
        yield return sourceUnit.Hud.WaitForHPUpdate();

        if (sourceUnit.Monsters.HP <= 0)
        {
            yield return HandleMonsterFainted(sourceUnit);
        }
    }
    bool CheckIfMoveHits(Move move, Monsters source, Monsters target)
    {
        if (move.Base.AlwaysHits)
        {
            return true;
        }

        float moveAccuracy = move.Base.Accuracy;

        int accuracy = source.StatBoosts[Stat.Accuracy];
        int evasion = target.StatBoosts[Stat.Evasion];

        var boostValues = new float[] { 1f, 4f / 3f, 5f / 3f, 2f, 7f / 3f, 8f / 3f, 3f };

        if (accuracy > 0)
        {
            moveAccuracy *= boostValues[accuracy];
        }
        else
        {
            moveAccuracy /= boostValues[-accuracy];
        }

        if (evasion > 0)
        {
            moveAccuracy /= boostValues[evasion];
        }
        else
        {
            moveAccuracy *= boostValues[-evasion];
        }

        return UnityEngine.Random.Range(1, 101) <= moveAccuracy;
    }

    IEnumerator ShowStatusChanges(Monsters monster)
    {
        while (playerUnit.Monsters.StatusChanges.Count > 0)
        {
            var message = monster.StatusChanges.Dequeue();
            yield return dialogBox.TypeDialog(message);
        }
    }
    IEnumerator HandleMonsterFainted(BattleUnit fainteUnit)
    {
        yield return dialogBox.TypeDialog($"{fainteUnit.Monsters.Base.Name} Fainted");
        fainteUnit.PlayFaintAnimation();
        yield return new WaitForSeconds(2f);

        if (!fainteUnit.IsPlayerUnit)
        {
            bool battleWon = true;
            if (isTrainerBattle)
            {
                battleWon = trainerParty.GetHealthyMonster() == null;
            }

            if (battleWon)
            {
                AudioManager.i.PlayMusic(bs.BattleVictoryMusic);
            }

            // Exp Gain
            int expYield = fainteUnit.Monsters.Base.ExpYield;
            int enemyLevel = fainteUnit.Monsters.Level;
            float trainerBonus = (isTrainerBattle) ? 1.5f : 1f;

            int expGain = Mathf.FloorToInt((expYield * enemyLevel * trainerBonus) / 7);
            playerUnit.Monsters.Exp += expGain;
            yield return dialogBox.TypeDialog($"{playerUnit.Monsters.Base.Name} gained {expGain} exp");
            yield return playerUnit.Hud.SetExpSmooth();

            // Check lv Up
            while (playerUnit.Monsters.CheckForLevelUp())
            {
                playerUnit.Hud.SetLevel();
                yield return dialogBox.TypeDialog($"{playerUnit.Monsters.Base.Name} grew to level {playerUnit.Monsters.Level}");

                // Try to learn a new Move
                var newMove = playerUnit.Monsters.GetLearnableMoveAtCurrentLevel();
                if (newMove != null)
                {
                    if (playerUnit.Monsters.Moves.Count < MonsterBase.MaxNumOfMoves)
                    {
                        playerUnit.Monsters.LearnMove(newMove.Base);
                        yield return dialogBox.TypeDialog($"{playerUnit.Monsters.Base.Name} learned {newMove.Base.Name}");
                        dialogBox.SetMoveNames(playerUnit.Monsters.Moves);
                    }
                    else
                    {
                        yield return dialogBox.TypeDialog($"{playerUnit.Monsters.Base.Name} trying to learn {newMove.Base.Name}");
                        yield return dialogBox.TypeDialog($"But it cannot learn more than {MonsterBase.MaxNumOfMoves} moves");
                        yield return dialogBox.TypeDialog($"Choose a move to forget");

                        MoveToForgetState.i.NewMove = newMove.Base;
                        MoveToForgetState.i.CurrentMoves = playerUnit.Monsters.Moves.Select(m => m.Base).ToList();
                        yield return GameController.i.StateMachine.PushAndWait(MoveToForgetState.i);

                        var moveIndex = MoveToForgetState.i.Selection;
                        if (moveIndex == MonsterBase.MaxNumOfMoves || moveIndex == -1)
                        {
                            // Don't learn the new move
                            yield return dialogBox.TypeDialog($"{playerUnit.Monsters.Base.Name} did not learn {newMove.Base.Name}");
                        }
                        else
                        {
                            // Forget the selected move and learn new move
                            var selectedMove = playerUnit.Monsters.Moves[moveIndex].Base;
                            yield return dialogBox.TypeDialog($"{playerUnit.Monsters.Base.Name} forgot {selectedMove.Name} and learned {newMove.Base.Name}");

                            playerUnit.Monsters.Moves[moveIndex] = new Move(newMove.Base);
                        }
                    }
                }
                yield return playerUnit.Hud.SetExpSmooth(true);
            }

            yield return new WaitForSeconds(1f);

        }

        yield return CheckForBattleOver(fainteUnit);
    }
    IEnumerator CheckForBattleOver(BattleUnit faintedUnit)
    {
        if (faintedUnit.IsPlayerUnit)
        {
            var nextMonster = playerParty.GetHealthyMonster();
            if (nextMonster != null)
            {
                yield return GameController.i.StateMachine.PushAndWait(PartyState.i);
                yield return bs.SwitchMonster(PartyState.i.SelectedMonster);
            }
            else
            {
                bs.BattleOver(false);
            }
        }
        else
        {
            if (!isTrainerBattle)
            {
                bs.BattleOver(true);
            }
            else
            {
                var nextMonster = trainerParty.GetHealthyMonster();
                if (nextMonster != null)
                {
                    yield break;
                }
                else
                {
                    bs.BattleOver(true);
                }
            }
        }
    }
    IEnumerator ShowDamageDetails(DamageDatails damageDetails)
    {
        if (damageDetails.Critical > 1f)
            yield return dialogBox.TypeDialog("A critical hit!");

        if (damageDetails.TypeEffectiveness > 1f)
            yield return dialogBox.TypeDialog("It's super effectivel");

        else if (damageDetails.TypeEffectiveness < 1f)
            yield return dialogBox.TypeDialog("It's not very effectivel");

    }
    IEnumerator TryToEscape()
    {
        if (isTrainerBattle)
        {
            yield return dialogBox.TypeDialog($"You can't run from trainer battle!");
            yield break;
        }

        ++bs.EscapeAttempts;

        int playerSpeed = playerUnit.Monsters.Speed;
        int enemeSpeed = enemyUnit.Monsters.Speed;

        if (enemeSpeed < playerSpeed)
        {
            yield return dialogBox.TypeDialog($"Ran away safely!");
            bs.BattleOver(true);
        }
        else
        {
            float f = (playerSpeed * 128) / enemeSpeed + 30 * bs.EscapeAttempts;
            f = f % 256;

            if (UnityEngine.Random.Range(0, 256) < f)
            {
                yield return dialogBox.TypeDialog($"Ran away safely!");
                bs.BattleOver(true);
            }
            else
            {
                yield return dialogBox.TypeDialog($"Can't escape!");
            }
        }
    }
}
