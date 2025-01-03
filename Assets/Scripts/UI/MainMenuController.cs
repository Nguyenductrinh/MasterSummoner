using GDE.GenericSelectionUI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class MainMenuController : SelectionUI<TextSlot>
{
    [SerializeField] AudioClip tileMusic;
    private void Start()
    {
        AudioManager.i.PlayMusic(tileMusic);

        var textSlots = GetComponentsInChildren<TextSlot>().ToList();

        if (SavingSystem.i.CheckIfSaveExists("SaveSlot1"))
        {
            SetItems(textSlots);
        }
        else
        {
            SetItems(textSlots.TakeLast(2).ToList());
            textSlots.First().GetComponent<Text>().color = Color.gray;
        }
        OnSelected += OnSelectedItem;
    }

    private void Update()
    {
        HandleUpdate();
    }

    void OnSelectedItem(int selection)
    {
        if (!SavingSystem.i.CheckIfSaveExists("SaveSlot1"))
            ++selection;

        if (selection == 0 )
        {
            // Continue
            GameController.i.StateMachine.ChangeState(FreeRoamState.i);
            DontDestroyOnLoad(gameObject);

            SceneManager.LoadScene(1);
            SavingSystem.i.Load("SaveSlot1");

            Destroy(gameObject);
        }
        else if(selection == 1)
        {
            // New Game

            GameController.i.StateMachine.ChangeState(FreeRoamState.i);
            SavingSystem.i.Delete("SaveSlot1");
            SceneManager.LoadScene(1);
        }
        else if (selection == 2) 
        {
            // Exit
        }
    }
}
