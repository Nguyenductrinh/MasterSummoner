using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapArena))]
public class MapArenaEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        int totalChance = serializedObject.FindProperty("totalChance").intValue;

        var style = new GUIStyle();
        style.fontStyle = FontStyle.Bold;
       
        GUILayout.Label($"Test Label = {totalChance}", style);
        if(totalChance != 100)
        {
            EditorGUILayout.HelpBox("The totl chance percentage is not 100", MessageType.Error);
        } 
    }
}
