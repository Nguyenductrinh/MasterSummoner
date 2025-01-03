using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SufableWater : MonoBehaviour, Interactable
{
    public IEnumerator Interact(Transform initiator)
    {
        yield return DialogManager.i.ShowDialogText("The water is deep blue");
        var monsterWithSurf = initiator.GetComponent<MonsterParty>().Monster.FirstOrDefault(p => p.Moves.Any(m => m.Base.Name == "Surf"));

        if (monsterWithSurf != null)
        {
            int selectedChoice = 0;
            yield return DialogManager.i.ShowDialogText($"Should {monsterWithSurf.Base.Name} use surf");

        }
    }
}
