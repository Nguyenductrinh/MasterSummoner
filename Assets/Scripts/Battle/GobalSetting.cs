using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GobalSetting : MonoBehaviour
{
    [SerializeField] Color highlightedcolor;

    public Color Highlightcolor => highlightedcolor;

    public static GobalSetting i {  get; private set; }
    private void Awake()
    {
        i = this;
    }
}
