using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ClockHandMovement))]
public class ClockHandScriptEditor : Editor
{
    SerializedProperty numberOfPoints;
    SerializedProperty rotationAxis;
    SerializedProperty correctionStrength;
    SerializedProperty smallHand;
    SerializedProperty puzzleHour;
    SerializedProperty puzzleMinute;
    SerializedProperty puzzleTimmer;
    SerializedProperty puzzleThreshold;
    SerializedProperty onPuzzleComplete;

    SerializedProperty lastRot;
    SerializedProperty smallHandRot;


    void OnEnable()
    {
        numberOfPoints = serializedObject.FindProperty("numberOfPoints");
        rotationAxis = serializedObject.FindProperty("rotationAxis");
        correctionStrength = serializedObject.FindProperty("correctionStrength");
        smallHand = serializedObject.FindProperty("smallHand");
        puzzleHour = serializedObject.FindProperty("puzzleHour");
        puzzleMinute = serializedObject.FindProperty("puzzleMinute");
        puzzleTimmer = serializedObject.FindProperty("puzzleTimmer");
        puzzleThreshold = serializedObject.FindProperty("puzzleThreshold");
        onPuzzleComplete = serializedObject.FindProperty("onPuzzleComplete");

        lastRot = serializedObject.FindProperty("lastRot");
        smallHandRot = serializedObject.FindProperty("smallHandRot");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(numberOfPoints);
        EditorGUILayout.PropertyField(rotationAxis);
        EditorGUILayout.PropertyField(correctionStrength);
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(smallHand);
        EditorGUILayout.PropertyField(puzzleMinute);
        EditorGUILayout.PropertyField(puzzleHour);
        EditorGUILayout.PropertyField(puzzleTimmer);
        EditorGUILayout.PropertyField(puzzleThreshold);
        EditorGUILayout.PropertyField(onPuzzleComplete);

        EditorGUILayout.LabelField("Current Big Hand Rotation: " + lastRot.floatValue);
        EditorGUILayout.LabelField("Current Small Hand Rotation: " + smallHandRot.floatValue);

        serializedObject.ApplyModifiedProperties();
    }
}
