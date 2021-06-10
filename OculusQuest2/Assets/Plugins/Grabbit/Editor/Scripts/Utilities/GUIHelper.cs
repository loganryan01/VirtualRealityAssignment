#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Grabbit
{
    public static class GUIHelper
    {
        private static readonly Dictionary<string, bool> foldoutStatuses = new Dictionary<string, bool>();

        public static void DisplayMessage(string message, bool big = false)
        {
            ParamStartIndex();

            GUILayout.Label(message,
                big
                    ? GrabbitEditor.Instance.CurrentSettings.FontMarginStyleBig
                    : GrabbitEditor.Instance.CurrentSettings.Rich);

            GUILayout.EndHorizontal();
        }

        private static void ParamStartIndex()
        {
            GUILayout.BeginHorizontal();
        }

        public static bool DisplayBoolOption(string message, ref bool condition, bool useToggleButton = true,string tooltip="")
        {
            var prev = condition;
            GUILayout.BeginHorizontal(GrabbitEditor.Instance.CurrentSettings.MarginStyle);
            // GUILayout.Label(message, marginStyle);
            var prevWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = prevWidth * 1.5f;
            condition = !useToggleButton ? EditorGUILayout.Toggle(new GUIContent(message,tooltip), condition) : GUILayout.Toggle(condition, new GUIContent(message,tooltip), "Button");
            EditorGUIUtility.labelWidth = prevWidth;
            GUILayout.EndHorizontal();
            return prev != condition;
        }

        public static int DisplayObjectListOption<T>(string name, List<T> list) where T : Object
        {
            var changedID = -1;
            var type = typeof(T);
            ParamStartIndex();

            GUILayout.BeginVertical();
            if (!foldoutStatuses.ContainsKey(name))
                foldoutStatuses.Add(name, false);

            var foldStatus = foldoutStatuses[name];


            foldStatus = EditorGUILayout.Foldout(foldStatus, name, true);


            foldoutStatuses[name] = foldStatus;

            if (foldStatus)
            {
                var array = list.ToArray();
                for (var i = 0; i < array.Length; i++)
                {
                    GUILayout.BeginHorizontal();
                    var member = array[i];

                    EditorGUILayout.LabelField("   ");

                    list[i] = (T) EditorGUILayout.ObjectField("", array[i], type, true);

                    if (GUILayout.Button("S"))
                        if (Selection.activeObject is T)
                            list[i] = (T) Selection.activeObject;

                    if (member != list[i])
                        changedID = i;
                    GUILayout.EndHorizontal();
                }

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                GUILayout.Label("Add");


                T newT = null;

                if (GUILayout.Button("S"))
                    if (Selection.activeObject is T)
                        newT = (T) Selection.activeObject;

                var wasNull = !newT;

                newT = (T) EditorGUILayout.ObjectField("", newT, type, true);


                GUILayout.EndHorizontal();

                if (newT)
                {
                    if (wasNull)
                    {
                        if (Selection.objects.Contains(newT))
                            list.AddRange(Selection.objects.Where(_ => _ is T && !list.Contains(_))
                                .Convert(_ => (T) _));
                        else
                            list.Add(newT);
                    }

                    changedID = list.Count - 1;
                }


                list.RemoveAll(_ => !_);
            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            return changedID;
        }


        public static int DisplayVector2ArrayOption(string name, ref Vector2[] array)
        {
            var changedID = -1;

            ParamStartIndex();


            GUILayout.Label(name);


            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();

            for (var i = array.Length - 1; i >= 0; i--)
            {
                GUILayout.BeginHorizontal();
                var member = array[i];

                array[i] = EditorGUILayout.Vector2Field("", array[i]);


                if (member != array[i])
                    changedID = i;
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            return changedID;
        }

        public static void DisplayObjectOption<T>(string name, ref T obj) where T : Object
        {
            var type = typeof(T);
            ParamStartIndex();


            GUILayout.Label(name);


            GUILayout.FlexibleSpace();

            obj = (T) EditorGUILayout.ObjectField("", obj, type, true);

            if (GUILayout.Button("S"))
                if (Selection.activeObject is T)
                    obj = (T) Selection.activeObject;

            GUILayout.EndHorizontal();
        }

        public static void DisplayEnumOption(string name, ref Enum currentEnumValue)
        {
            ParamStartIndex();

            GUILayout.Label(name);

            GUILayout.FlexibleSpace();
            currentEnumValue = EditorGUILayout.EnumPopup(currentEnumValue);
            GUILayout.EndHorizontal();
        }

        public static bool DisplayIntOption(string name, ref int value, int minValue = int.MinValue,
            int maxValue = int.MaxValue)
        {
            ParamStartIndex();

            var prevWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = prevWidth * 1.5f;

            var prev = value;
            value = EditorGUILayout.IntField(name, value, GrabbitEditor.Instance.CurrentSettings.NumberMarginStyle);

            value = Mathf.Clamp(value, minValue, maxValue);

            GUILayout.EndHorizontal();

            EditorGUIUtility.labelWidth = prevWidth;

            return prev != value;
        }

        public static void DisplayDoubleOption(string name, ref double value)
        {
            ParamStartIndex();

            var style = new GUIStyle(EditorStyles.numberField)
            {
                margin = new RectOffset(5, 0, 0, 0),
                richText = true
            };

            value = EditorGUILayout.DoubleField(name, value);


            GUILayout.EndHorizontal();
        }

        /// <summary>
        ///     Displays a Float parameter on the scene interface
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns> true if teh value was changed</returns>
        public static bool DisplayFloatOption(string name, ref float value, float min = float.MinValue,
            float max = float.MaxValue)
        {
            var prevValue = value;
            ParamStartIndex();

            var prevWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = prevWidth * 1.5f;

            value = EditorGUILayout.FloatField(name, value, GrabbitEditor.Instance.CurrentSettings.NumberMarginStyle);

            value = Mathf.Clamp(value, min, max);

            EditorGUIUtility.labelWidth = prevWidth;

            GUILayout.EndHorizontal();
            return !Mathf.Approximately(prevValue, value);
        }

        public static void DisplayFloatPercentOption(string name, ref float value)
        {
            ParamStartIndex();


            value = EditorGUILayout.Slider(name, value, 0, 1);
            GUILayout.EndHorizontal();
        }

        public static void DisplayVectorOption(string name, ref Vector3 vector)
        {
            ParamStartIndex();


            vector = EditorGUILayout.Vector3Field(name, vector);
            GUILayout.EndHorizontal();
        }

        public static void DisplayVector2Option(string name, ref Vector2 vector)
        {
            ParamStartIndex();

            vector = EditorGUILayout.Vector2Field(name, vector);
            GUILayout.EndHorizontal();
        }

        public static bool DisplayButton(string name, Action action)
        {
            if (GUILayout.Button(name))
            {
                action();
                return true;
            }

            return false;
        }
    }
}
#endif