using System.Collections;
using System.Collections.Generic;
using TMPro.Examples;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;


public class BattleHud : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] TextMeshProUGUI statusText;
    [SerializeField] HPBar hpBar;
    [SerializeField] GameObject expBar;

    [SerializeField] Color psnColor;
    [SerializeField] Color brnColor;
    [SerializeField] Color slpColor;
    [SerializeField] Color parColor;
    [SerializeField] Color frzColor;

    Monsters _monsters;
    Dictionary<ConditionID,Color> statusColors;

    public void SetData(Monsters monsters)
    {
        if(_monsters != null)
        {
            _monsters.OnHPChanged -= UpdateHP;
            _monsters.OnStatusChanged -= SetStatusText;
        }
        _monsters = monsters;
        nameText.text = monsters.Base.Name;
        SetLevel();
        hpBar.SetHP((float) monsters.HP / monsters.MaxHp);
        SetExp();

        statusColors = new Dictionary<ConditionID, Color>()
        {
            { ConditionID.psn, psnColor},
            { ConditionID.brn, brnColor},
            { ConditionID.slp, slpColor},
            { ConditionID.par, parColor},
            { ConditionID.frz, frzColor},
        };

        SetStatusText();
        _monsters.OnStatusChanged += SetStatusText;
        _monsters.OnHPChanged += UpdateHP;
    }

    void SetStatusText()
    {
        if(_monsters.Status == null)
        {
            Debug.Log("Status is null. Clearing statusText.");
            statusText.text = "";
        }
        else
        {
            Debug.Log($"Updating statusText to {_monsters.Status.Id.ToString().ToUpper()}");
            statusText.text = _monsters.Status.Id.ToString().ToUpper();
            statusText.color = statusColors[_monsters.Status.Id];
        }
    }

    public void SetLevel()
    {
        levelText.text = "Lv" + _monsters.Level;
    }

    public void SetExp()
    {
        if (expBar == null)
        {
            return;
        }
        float normalizedExp = GetNormalizedExp();
        expBar.transform.localScale = new Vector3(normalizedExp, 1,1);
    }

    public IEnumerator SetExpSmooth( bool reset= false)
    {
        if(expBar == null)
        {
            yield break;
        }
        if (reset)
        {
            expBar.transform.localScale = new Vector3(0, 1, 1);
        }

        float normalizedExp = GetNormalizedExp();
        yield return expBar.transform.DOScaleX(normalizedExp,1.5f).WaitForCompletion();
    }

    float GetNormalizedExp()
    {
        int currLevelExp = _monsters.Base.GetExpForLevel(_monsters.Level);
        int nextLevelExp = _monsters.Base.GetExpForLevel(_monsters.Level + 1);

        float normalizedExp = (float)(_monsters.Exp - currLevelExp) / (nextLevelExp - currLevelExp);
        return Mathf.Clamp01(normalizedExp);
    }

    public void UpdateHP()
    {
        StartCoroutine(UpdateHPAsync());
    }

    public IEnumerator UpdateHPAsync()
    {
         yield return hpBar.SetHPSmooth((float)_monsters.HP / _monsters.MaxHp);
    }

    public IEnumerator WaitForHPUpdate()
    {
        yield return new WaitUntil(() => hpBar.IsUpdating == false);
    }

    public void ClearData()
    {
        if (_monsters != null)
        {
            _monsters.OnHPChanged -= UpdateHP;
            _monsters.OnStatusChanged -= SetStatusText;
        }
    }

}
