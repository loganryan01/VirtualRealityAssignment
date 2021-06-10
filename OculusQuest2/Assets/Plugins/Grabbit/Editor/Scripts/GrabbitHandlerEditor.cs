#if UNITY_EDITOR
using UnityEditor;

namespace Grabbit
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(GrabbitHandler))]
    public class GrabbitHandlerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox(
                "Grabbit automatically added a rigidbody and colliders to your objects to handle the physics." +
                "\nBut don't worry! It will get rid of them (if they weren't present before) once it's done!",
                MessageType.Info, true);
        }
    }
}
#endif