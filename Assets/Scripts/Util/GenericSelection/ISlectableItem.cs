using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISlectableItem
{
    void Init();
    void Clear();
    void OnSelectionChanged(bool selected);
}
