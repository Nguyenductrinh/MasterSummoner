using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.XR;

public class BattleDialogBox : MonoBehaviour
{
    [SerializeField] int letterPerSecond;

    [SerializeField] GameObject actionSelector;
    [SerializeField] GameObject moveSelector;
    [SerializeField] GameObject moveDetails;
    [SerializeField] GameObject choiceBox;
    [SerializeField] TextMeshProUGUI dialogText;

    [SerializeField] List<TextMeshProUGUI> actionTexts;
    [SerializeField] List<TextMeshProUGUI> moveTexts;

    [SerializeField] TextMeshProUGUI ppText;
    [SerializeField] TextMeshProUGUI typeText;

    [SerializeField] TextMeshProUGUI yesText;
    [SerializeField] TextMeshProUGUI noText;

    Color highlightedcolor;

    private void Start()
    {
        highlightedcolor = GobalSetting.i.Highlightcolor;
    }
    public void SetDialog(string dialog)
    {
        dialogText.text = dialog;
    }

    public IEnumerator TypeDialog(string dialog)
    {
        dialogText.text = "";
        foreach (var letter in dialog.ToCharArray())
        {
            dialogText.text += letter;
            yield return new WaitForSeconds(1f / letterPerSecond);
        }

        yield return new WaitForSeconds(1f);
    }

    public void EnableDialogText(bool enabled)
    {
        dialogText.enabled = enabled;
    }

    public void EnableActionSelector(bool enabled)
    {
        actionSelector.SetActive(enabled);
    }

    public void EnableMoveSelector(bool enabled)
    {
        moveSelector.SetActive(enabled);
        moveDetails.SetActive(enabled);
    }

    public void EnableChoiceBox(bool enabled) 
    {
        choiceBox.SetActive(enabled);
    }

    public void UpdateActionSelector(int selectAction)
    {
        for (int i = 0; i < actionTexts.Count; i++) 
        {
            if(i == selectAction)
            {
                actionTexts[i].color = highlightedcolor;
            }
            else
            {
                actionTexts[i].color = Color.black;
            }
        }
    }

    public void UpdateMoveSelection(int selectMove, Move move)
    {
        for (int i = 0;i < moveTexts.Count; i++)
        {
            if (i == selectMove)
            {
                moveTexts[i].color = highlightedcolor;
            }
            else
            {
                moveTexts[i].color = Color.black;
            }
        }

        ppText.text = $"PP {move.PP}/{move.Base.PP}";
        typeText.text = move.Base.Type.ToString();

        if (move.PP == 0)
        {
            ppText.color = Color.red;
        }
        else
        {
            ppText.color = Color.black;
        }
    }

    public void SetMoveNames(List<Move> moves)
    {
        for (int i = 0; i < moveTexts.Count; i++)
        {
            if (i < moves.Count)
            {
                moveTexts[i].text = moves[i].Base.Name; // Access the Move object
            }
            else
            {
                moveTexts[i].text = "_";
            }
        }
    }

    public void UpdateChoiceBox(bool yesSlected)
    {
        if (yesSlected)
        {
            yesText.color = highlightedcolor;
            noText.color = Color.black;
        }
        else
        {
            yesText.color = Color.black;
            noText.color = highlightedcolor;
        }
    }
}
