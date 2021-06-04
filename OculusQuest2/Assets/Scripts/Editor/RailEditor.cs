using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RailSystem))]
public class RailEditor : Editor
{
    SerializedProperty rails;
    // Used for foldouts
    bool railsFoldout;
    private bool[] showElement = new bool[100];

    SerializedProperty onStartOfRail;
    SerializedProperty onEndOfRail;
    SerializedProperty color;



    void OnEnable()
    {
        rails = serializedObject.FindProperty("rails");
        onStartOfRail = serializedObject.FindProperty("onStartOfRail");
        onEndOfRail = serializedObject.FindProperty("onEndOfRail");
        color = serializedObject.FindProperty("color");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();


        railsFoldout = EditorGUILayout.Foldout(railsFoldout, "Rails");
        EditorGUI.indentLevel++;
        if (railsFoldout)
        {
            // Display the size of the array
            EditorGUILayout.PropertyField(rails.FindPropertyRelative("Array.size"));

            // Iterate over the elements
            for (int i = 0; i < rails.arraySize; i++)
            {
                showElement[i] = EditorGUILayout.Foldout(showElement[i], "Element " + i);
                if (!showElement[i])
                {
                    continue;
                }
                EditorGUI.indentLevel++;

                SerializedProperty element = rails.GetArrayElementAtIndex(i);
                EditorGUILayout.PropertyField(element.FindPropertyRelative("start"));
                EditorGUILayout.PropertyField(element.FindPropertyRelative("end"));

                SerializedProperty straight = element.FindPropertyRelative("isStraight");
                EditorGUILayout.PropertyField(straight);
                // Only display curve point if using a curved rail
                if (!straight.boolValue)
                {
                    EditorGUILayout.PropertyField(element.FindPropertyRelative("curvePoint"));
                }

                EditorGUI.indentLevel--;
            }
        }

        EditorGUI.indentLevel--;

        // Display the remaining properties
        EditorGUILayout.PropertyField(onStartOfRail);
        EditorGUILayout.PropertyField(onEndOfRail);
        EditorGUILayout.PropertyField(color);

        serializedObject.ApplyModifiedProperties();
    }



    void OnSceneGUI()
    {
        RailSystem railSystem = (RailSystem)target;

        // Display object data
        Handles.Label(railSystem.transform.position,
            railSystem.transform.position.ToString() + 
            "\nTotal Distance: " + railSystem.totalDist.ToString() +
            "\nCurrent Rail: " + railSystem.currentRail.ToString() +
            "\nCurrent Distance: " + railSystem.currentDist.ToString());


        Handles.color = railSystem.color;

        // Draw each rail segment
        for (int i = 0; i < railSystem.rails.Length; i++)
        {
            RailSystem.RailSegment railSegment = railSystem.rails[i];

            if (railSegment.isStraight)
            {
                Handles.DrawLine(railSegment.start, railSegment.end);
            }
            else
            {
                Handles.DrawBezier(railSegment.start, railSegment.end, railSegment.start, railSegment.curvePoint, railSystem.color, null, 1);
            }
        }

        // Draw discs at the start and end of the current line
        RailSystem.RailSegment currentRail = railSystem.rails[railSystem.currentRail];
        Handles.DrawWireDisc(currentRail.start, currentRail.start - currentRail.end, 0.1f);
        Handles.DrawWireDisc(currentRail.end, currentRail.start - currentRail.end, 0.1f);
    }
}
