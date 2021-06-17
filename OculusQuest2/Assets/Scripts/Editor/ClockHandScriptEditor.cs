using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ClockHandMovement))]
public class ClockHandScriptEditor : Editor
{
    SerializedProperty interactables;
    SerializedProperty numberOfPoints;
    SerializedProperty rotationAxis;
    SerializedProperty correctionStrength;
    SerializedProperty counterTorqueStrength;
    SerializedProperty smallHand;
    SerializedProperty puzzleHour;
    SerializedProperty puzzleMinute;
    SerializedProperty puzzleTimmer;
    SerializedProperty puzzleThreshold;
    SerializedProperty onPuzzleComplete;
    SerializedProperty doorTransform;
    SerializedProperty doorTime;
    SerializedProperty doorAngle;

    SerializedProperty lastRot;
    SerializedProperty smallHandRot;


    void OnEnable()
    {
        numberOfPoints = serializedObject.FindProperty("numberOfPoints");
        rotationAxis = serializedObject.FindProperty("rotationAxis");
        correctionStrength = serializedObject.FindProperty("correctionStrength");
        counterTorqueStrength = serializedObject.FindProperty("counterTorqueStrength");
        smallHand = serializedObject.FindProperty("smallHand");
        puzzleHour = serializedObject.FindProperty("puzzleHour");
        puzzleMinute = serializedObject.FindProperty("puzzleMinute");
        puzzleTimmer = serializedObject.FindProperty("puzzleTimmer");
        puzzleThreshold = serializedObject.FindProperty("puzzleThreshold");
        onPuzzleComplete = serializedObject.FindProperty("onPuzzleComplete");
        doorTransform = serializedObject.FindProperty("doorTransform");
        doorTime = serializedObject.FindProperty("doorTime");
        doorAngle = serializedObject.FindProperty("doorAngle");
        interactables = serializedObject.FindProperty("interactables");

        lastRot = serializedObject.FindProperty("lastRot");
        smallHandRot = serializedObject.FindProperty("smallHandRot");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(interactables);
        EditorGUILayout.PropertyField(numberOfPoints);
        EditorGUILayout.PropertyField(rotationAxis);
        EditorGUILayout.PropertyField(correctionStrength);
        EditorGUILayout.PropertyField(counterTorqueStrength);
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(smallHand);
        EditorGUILayout.PropertyField(puzzleMinute);
        EditorGUILayout.PropertyField(puzzleHour);
        EditorGUILayout.PropertyField(puzzleTimmer);
        EditorGUILayout.PropertyField(puzzleThreshold);
        EditorGUILayout.PropertyField(onPuzzleComplete);
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(doorTransform);
        EditorGUILayout.PropertyField(doorTime);
        EditorGUILayout.PropertyField(doorAngle);

        EditorGUILayout.LabelField("Current Big Hand Rotation: " + lastRot.floatValue);
        EditorGUILayout.LabelField("Current Small Hand Rotation: " + smallHandRot.floatValue);

        serializedObject.ApplyModifiedProperties();
    }
}
