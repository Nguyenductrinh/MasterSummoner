using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
public class MonsterParty : MonoBehaviour
{
    [SerializeField] List<Monsters> monsters;

    public event Action OnUpdated;

    public List<Monsters> Monster
    {
        get 
        { 
            return monsters; 
        }
        set 
        { 
            monsters = value;
            OnUpdated?.Invoke();
        }
    }
    private void Awake()
    {
        foreach (var monster in monsters)
        {
            monster.Init();
        }
    }
    public Monsters GetHealthyMonster() 
    {
        return monsters.Where(x => x.HP > 0).FirstOrDefault();
    }
    public void AddMonster(Monsters newMonster)
    {
        if (monsters.Count < 6)
        {
            monsters.Add(newMonster);
            OnUpdated?.Invoke();
        }
        else
        {
            // TOO: add to the PC once that's implemented

        }
    }
    public bool CheckForEvolutions()
    {
        return monsters.Any(p => p.CheckForEvolution() != null);
    }
    public IEnumerator RunEvolutions()
    {
        foreach (var monster in monsters)
        {
            var evolution = monster.CheckForEvolution();
            if (evolution != null)
            {
               yield return EvolutionState.i.Evolve(monster, evolution);
            }
        }
    }
    public static MonsterParty GetPlayerParty()
    {
        return FindObjectOfType<PlayerController>().GetComponent<MonsterParty>();
    }
}
