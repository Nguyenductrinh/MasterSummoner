using GDEUtils.StateMachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EvolutionState : State<GameController>
{
    [SerializeField] GameObject evolutionUI;
    [SerializeField] Image monsterImge;

    [SerializeField] AudioClip evolutionMusic;

    public static EvolutionState i { get; private set; }
    private void Awake()
    {
        i = this;  
    }

    public IEnumerator Evolve( Monsters monster, Evolution evolution)
    {
        var gc = GameController.i;
        gc.StateMachine.Push(this);

        evolutionUI.SetActive(true);

        AudioManager.i.PlayMusic(evolutionMusic);

        monsterImge.sprite = monster.Base.FrontSprite;
        yield return DialogManager.i.ShowDialogText($"{monster.Base.Name} is evoling");

        var oldMonster = monster.Base;
        monster.Evolve(evolution);

        monsterImge.sprite = monster.Base.FrontSprite;
        yield return DialogManager.i.ShowDialogText($"{oldMonster.Name} evoling into {monster.Base.Name}");

        evolutionUI.SetActive(false);

        gc.PartyScreen.SetPartyData();
        AudioManager.i.PlayMusic(gc.CurrentScene.SceneMusic, fade: true);

        gc.StateMachine.Pop();
    }
}
