using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RailSystem))]
public class RailEditor : Editor
{
    SerializedProperty rails;
    // Used for foldouts
    bool railsFoldout = true;
    private bool[] showElement = new bool[100];

    SerializedProperty onStartOfRail;
    SerializedProperty onEndOfRail;
    SerializedProperty color;
    SerializedProperty useLocal;

    
    
    void OnEnable()
    {
        rails = serializedObject.FindProperty("rails");
        onStartOfRail = serializedObject.FindProperty("onStartOfRail");
        onEndOfRail = serializedObject.FindProperty("onEndOfRail");
        color = serializedObject.FindProperty("color");
        useLocal = serializedObject.FindProperty("useLocalSpace");

        // Set default values
        for (int i = 0; i < showElement.Length; i++)
        {
            showElement[i] = true;
        }
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
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(element.FindPropertyRelative("startRotation"));
                EditorGUILayout.PropertyField(element.FindPropertyRelative("endRotation"));
                EditorGUILayout.Space();
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
        EditorGUILayout.PropertyField(useLocal);
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(onStartOfRail);
        EditorGUILayout.PropertyField(onEndOfRail);
        EditorGUILayout.PropertyField(color);

        serializedObject.ApplyModifiedProperties();
    }



    void OnSceneGUI()
    {
        RailSystem railSystem = (RailSystem)target;
        Transform railTrans = railSystem.transform;


        if (EditorApplication.isPlaying)
		{
            // Display object data
            Handles.Label(railTrans.position,
                railTrans.position.ToString() +
                "\nTotal Distance: " + railSystem.totalDist.ToString() +
                "\nCurrent Rail: " + railSystem.currentRail.ToString() +
                "\nCurrent Distance: " + railSystem.currentDist.ToString());
        }

        Handles.color = railSystem.color;

        // Draw each rail segment
        for (int i = 0; i < railSystem.rails.Length; i++)
        {
            RailSystem.RailSegment railSegment = railSystem.rails[i];

            // Convert from local to world if option is selected and in edit mode
            Vector3 start, end, curvePoint;
            if (useLocal.boolValue && !EditorApplication.isPlaying)
			{
                start = railTrans.position + (Vector3)(railTrans.localToWorldMatrix * railSegment.start);
                end = railTrans.position + (Vector3)(railTrans.localToWorldMatrix * railSegment.end);
                curvePoint = railTrans.position + (Vector3)(railTrans.localToWorldMatrix * railSegment.curvePoint);
            }
			else
			{
                start = railSegment.start;
                end = railSegment.end;
                curvePoint = railSegment.curvePoint;
            }

            // Draw the rail line
            if (railSegment.isStraight)
            {
                Handles.DrawLine(start, end);
            }
            else
            {
                Handles.DrawBezier(start, end, start, curvePoint, railSystem.color, null, 1);
            }
        }

        // Draw discs at the start and end of the current line
        RailSystem.RailSegment currentRail = railSystem.rails[railSystem.currentRail];
        if (useLocal.boolValue && !EditorApplication.isPlaying)
        {
            Handles.DrawWireDisc(railTrans.position + (Vector3)(railTrans.localToWorldMatrix * currentRail.start), currentRail.start - currentRail.end, 0.1f);
            Handles.DrawWireDisc(railTrans.position + (Vector3)(railTrans.localToWorldMatrix * currentRail.end), currentRail.start - currentRail.end, 0.1f);
        }
        else
        {
            Handles.DrawWireDisc(currentRail.start, currentRail.start - currentRail.end, 0.1f);
            Handles.DrawWireDisc(currentRail.end, currentRail.start - currentRail.end, 0.1f);
        }
    }
}
