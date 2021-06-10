#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;

namespace Grabbit
{
    public class GrabbitStartingWindow : EditorWindow
    {
        public enum StartingStep
        {
            INTRO,
            SETUP_INTRO,
            CONVEX_COLLIDERS,
            BIG_SCENE,
            PRECISION_VS_PERF,
            GETTING_FURTHER,
            TUTORIAL,
        }

        [InitializeOnLoadMethod]
        public static void PlugToUpdate()
        {
            EditorApplication.update += CheckStartup;
        }

        private static void CheckStartup()
        {
            if (!CurrentSettings)
                GetOrFetchSettings();

            if (!CurrentSettings)
            {
                return;
            }

            EditorApplication.update -= CheckStartup;

            if (CurrentSettings.ShowStartupWindow)
            {
                ShowStartupWindow();
            }
        }

        public static GrabbitSettings CurrentSettings;

        public static void GetOrFetchSettings()
        {
            if (!CurrentSettings)
            {
                var ids = AssetDatabase.FindAssets("t:GrabbitSettings");
                if (ids.Length == 0)
                {
                    return;
                }

                CurrentSettings = AssetDatabase.LoadAssetAtPath<GrabbitSettings>(AssetDatabase.GUIDToAssetPath(ids[0]));

                if (!CurrentSettings)
                {
                    return;
                }
            }

            return;
        }

        [MenuItem("Tools/Grabbit/Open Startup Menu")]
        public static void ShowStartupWindow()
        {
            var startup = GetWindow<GrabbitStartingWindow>();
            startup.titleContent = new GUIContent("Get Started With Grabbit!");
            startup.Focus();
            startup.CurrentStep = StartingStep.INTRO;
            var rect = startup.position;
            rect.height = 600;
            rect.width = 800;
            startup.position = rect;
        }

        public StartingStep CurrentStep;

        public void OnGUI()
        {
            if (!CurrentSettings)
                GetOrFetchSettings();
            
            CurrentSettings.InitializeStyle();

            switch (CurrentStep)
            {
                case StartingStep.INTRO:
                    ShowIntro();
                    break;
                case StartingStep.SETUP_INTRO:
                    SetupIntro();
                    break;
                case StartingStep.CONVEX_COLLIDERS:
                    ConvexColliders();
                    break;
                case StartingStep.BIG_SCENE:
                    BigSceneSetup();
                    break;
                case StartingStep.PRECISION_VS_PERF:
                    PrecisionPerfSetup();
                    break;
                case StartingStep.GETTING_FURTHER:
                    GettingFurther();
                    break;
                case StartingStep.TUTORIAL:
                    Tutorial();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        #region INTRO

        private void ShowIntro()
        {
            EditorGUILayout.BeginVertical(CurrentSettings.TabStyle);
            GUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            GUILayout.Label("<b>Getting Started With</b>",
                new GUIStyle(CurrentSettings.RichTitle) {margin = new RectOffset(15, 5, 5, 5)});
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            GUILayout.Label(CurrentSettings.GrabbitTextLogo, new GUIStyle(GUI.skin.label)
                {alignment = TextAnchor.MiddleLeft, fixedHeight = 46, margin = new RectOffset(5, 5, 5, 5)});
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndVertical();


            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            GetStartedButtonArea();

            AboutGrabbitSection();

            LikeUsSection();

            GUILayout.BeginHorizontal();

            OtherPlugins();

            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            GUILayout.Label(CurrentSettings.JungleLogo, new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter, fixedHeight = 55, fixedWidth = 120,
                margin = new RectOffset(5, 15, 0, 0)
            });
            GUILayout.FlexibleSpace();

            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
        }

        private static void LikeUsSection()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(CurrentSettings.Heart, new GUIStyle(GUI.skin.label)
                {alignment = TextAnchor.MiddleCenter, stretchHeight = true, margin = new RectOffset(15, 5, 0, 0)});
            EditorGUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            GUILayout.Label($"<b>Like What We're Doing?</b>", CurrentSettings.RichCenter);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            EditorGUILayout.BeginVertical(CurrentSettings.TabStyle);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Review Grabbit to support more updates :)", new GUIStyle(GUI.skin.button)
                {
                    margin = new RectOffset(15, 15, 15, 0), padding = new RectOffset(5, 5, 5, 5), fontSize = 15
                }
            ))
            {
                Application.OpenURL(
                    "https://assetstore.unity.com/packages/tools/utilities/grabbit-editor-physics-transforms-182328#reviews");
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            for (int i = 0; i < 5; i++)
            {
                GUILayout.Label(CurrentSettings.Star, new GUIStyle(GUI.skin.label)
                    {alignment = TextAnchor.MiddleCenter, fixedWidth = 20});
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private static void OtherPlugins()
        {
            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();

            var prev = CurrentSettings.ShowStartupWindow;
            CurrentSettings.ShowStartupWindow = !GUILayout.Toggle(!CurrentSettings.ShowStartupWindow,
                "Don't show this window again", new GUIStyle(GUI.skin.toggle) {margin = new RectOffset(15, 5, 0, 0)});

            if (prev != CurrentSettings.ShowStartupWindow)
            {
                EditorUtility.SetDirty(CurrentSettings);
            }

            GUILayout.FlexibleSpace();

            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();

            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(new GUIContent(CurrentSettings.MonKey)))
            {
                Application.OpenURL(
                    "https://assetstore.unity.com/packages/tools/utilities/monkey-productivity-commands-119938");
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(new GUIContent(CurrentSettings.Parrot)))
            {
                Application.OpenURL(
                    "https://assetstore.unity.com/packages/tools/utilities/selection-parrot-group-select-organize-130053");
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        private void AboutGrabbitSection()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(CurrentSettings.GetStartedArrow, new GUIStyle(GUI.skin.label)
                {alignment = TextAnchor.MiddleCenter, stretchHeight = true, margin = new RectOffset(15, 5, 0, 0)});
            EditorGUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            GUILayout.Label($"<b>About Grabbit</b>", CurrentSettings.RichCenter);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndVertical();

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            EditorGUILayout.BeginVertical(CurrentSettings.TabStyle);

            GUILayout.BeginHorizontal();

            GUILayout.Label($"Current Version: {GrabbitSettings.GrabbitVersion}", CurrentSettings.Rich);

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal();

            if (GUILayout.Button($"Check What's New!", CurrentSettings.RegularButton))
            {
                ShowChangeLog();
            }

            if (GUILayout.Button("Come chat with us on Discord!", CurrentSettings.RegularButton))
            {
                Application.OpenURL("https://discord.gg/XEET6D6vN9");
            }

            if (GUILayout.Button("Follow us on Twitter!", CurrentSettings.RegularButton))
            {
                Application.OpenURL("https://twitter.com/BillSansky");
            }

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();


            GUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void GetStartedButtonArea()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(CurrentSettings.Exclamation, new GUIStyle(GUI.skin.label)
                {alignment = TextAnchor.MiddleCenter, stretchHeight = true});
            EditorGUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            GUILayout.Label($"<b>New to Grabbit?</b>", CurrentSettings.RichCenter);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal(CurrentSettings.TabStyle);

            EditorGUILayout.BeginVertical();
            GUILayout.BeginHorizontal();


            if (GUILayout.Button("Set It Up!", CurrentSettings.BigButton))
            {
                CurrentStep = StartingStep.SETUP_INTRO;
                var pos = position;
                pos.height *= 0.5f;
                position = pos;
            }

            GUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            GUILayout.EndHorizontal();
        }

        #endregion

        #region SETUP

        private void SetupIntro()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(CurrentSettings.Exclamation, new GUIStyle(GUI.skin.label)
                {alignment = TextAnchor.MiddleCenter, fixedHeight = 60});

            EditorGUILayout.BeginVertical();

            GUILayout.Label($"<b>Let's Set Grabbit Up! (1/6) </b>", CurrentSettings.RichTitle);

            EditorGUILayout.EndVertical();

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            EditorGUILayout.BeginVertical(CurrentSettings.TabStyle);
            GUILayout.FlexibleSpace();
            GUILayout.Label($"<b>Grabbit has a lot of options, which can be scary when you start.</b>",
                new GUIStyle(CurrentSettings.RichCenter) {margin = new RectOffset(15, 0, 0, 0)});
            GUILayout.Label(
                $"This setup will adapt Grabbit to your needs. You can change everything later in the options too :)",
                new GUIStyle(CurrentSettings.RichParagraph) {margin = new RectOffset(15, 0, 0, 0)});

            GUILayout.BeginHorizontal();

            if (GUILayout.Button($"Let's Do This!", CurrentSettings.BigButton))
            {
                CurrentStep = StartingStep.CONVEX_COLLIDERS;
            }

            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndVertical();
        }


        public void ConvexColliders()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(CurrentSettings.Exclamation, new GUIStyle(GUI.skin.label)
                {alignment = TextAnchor.MiddleCenter, fixedHeight = 60});

            EditorGUILayout.BeginVertical();

            GUILayout.Label($"<b>Collider Type Setup (2/6)</b>", CurrentSettings.RichTitle);

            EditorGUILayout.EndVertical();

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            EditorGUILayout.BeginVertical(CurrentSettings.TabStyle);
            GUILayout.FlexibleSpace();

            GUILayout.Label(
                $"<b>Grabbit can generate concave colliders, but will need to scan your meshes</b>",
                new GUIStyle(CurrentSettings.RichCenter) {margin = new RectOffset(15, 0, 0, 0)});
            GUILayout.Label(
                $"It's required <b>once only</b> the first time you move a mesh, but can take a few seconds.",
                new GUIStyle(CurrentSettings.RichParagraph) {margin = new RectOffset(15, 0, 0, 0)});


            GUILayout.BeginHorizontal();

            if (GUILayout.Button($"Use Concave Colliders (precise & accurate)", CurrentSettings.RegularButton))
            {
                CurrentSettings.UseDynamicNonConvexColliders = true;
                EditorUtility.SetDirty(CurrentSettings);
                CurrentStep = StartingStep.BIG_SCENE;
            }

            if (GUILayout.Button($"Use Default Colliders (no scan, but limited)", CurrentSettings.RegularButton))
            {
                CurrentSettings.UseDynamicNonConvexColliders = false;
                EditorUtility.SetDirty(CurrentSettings);
                CurrentStep = StartingStep.BIG_SCENE;
            }

            GUILayout.EndHorizontal();
            GUILayout.Label(
                $"<i>You can scan all your scene objects (or even all your assets) at once in the options too!</i>",
                new GUIStyle(CurrentSettings.RichParagraph) {margin = new RectOffset(15, 0, 0, 0)});
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndVertical();
        }

        public void BigSceneSetup()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(CurrentSettings.Exclamation, new GUIStyle(GUI.skin.label)
                {alignment = TextAnchor.MiddleCenter, fixedHeight = 60});

            EditorGUILayout.BeginVertical();

            GUILayout.Label($"<b>Scene Scan Setup (3/6)</b>", CurrentSettings.RichTitle);

            EditorGUILayout.EndVertical();

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            EditorGUILayout.BeginVertical(CurrentSettings.TabStyle);
            GUILayout.FlexibleSpace();

            GUILayout.Label(
                $"<b>Grabbit must scan your scene when it starts</b>",
                new GUIStyle(CurrentSettings.RichCenter) {margin = new RectOffset(15, 0, 0, 0)});
            GUILayout.Label(
                $"If you plan to use Grabbit on big scenes (20k+ objects), then it would load in around 0.8s (depending on your CPU)." +
                $" If this feels too long then you may want to use the limitation zone feature to make it start faster",
                new GUIStyle(CurrentSettings.RichParagraph) {margin = new RectOffset(15, 0, 0, 0)});

            GUILayout.BeginHorizontal();

            if (GUILayout.Button($"Use The Normal Behaviour (better UX)",
                CurrentSettings.RegularButton))
            {
                CurrentSettings.UseLimitationZone = false;
                EditorUtility.SetDirty(CurrentSettings);
                CurrentStep = StartingStep.PRECISION_VS_PERF;
            }

            if (GUILayout.Button($"Use The Limitation Zone (faster loading on big scenes)",
                CurrentSettings.RegularButton))
            {
                CurrentSettings.UseLimitationZone = true;
                EditorUtility.SetDirty(CurrentSettings);
                CurrentStep = StartingStep.PRECISION_VS_PERF;
            }

            GUILayout.EndHorizontal();
            GUILayout.Label($"<i>The limitation zone lets you define where grabbit should limit its scan to," +
                            $" so it doesn't have to scan your entire scene!</i>",
                new GUIStyle(CurrentSettings.RichParagraph) {margin = new RectOffset(15, 0, 0, 0)});
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndVertical();
        }

        public void PrecisionPerfSetup()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(CurrentSettings.Exclamation, new GUIStyle(GUI.skin.label)
                {alignment = TextAnchor.MiddleCenter, fixedHeight = 60});

            EditorGUILayout.BeginVertical();

            GUILayout.Label($"<b>Collision Setup (4/6)</b>", CurrentSettings.RichTitle);

            EditorGUILayout.EndVertical();

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            EditorGUILayout.BeginVertical(CurrentSettings.TabStyle);
            GUILayout.FlexibleSpace();

            GUILayout.Label(
                $"<b>Grabbit can simulate high precision collisions</b>",
                new GUIStyle(CurrentSettings.RichCenter) {margin = new RectOffset(15, 0, 0, 0)});
            GUILayout.Label(
                $"This allows smooth collisions that make it easy for you to move things around. " +
                $"If you plan on moving a lot of complex objects at once, then the collisions may sometimes slow down. " +
                $"If your framerate is low already, you may want to turn it off",
                new GUIStyle(CurrentSettings.RichParagraph) {margin = new RectOffset(15, 0, 0, 0)});


            GUILayout.BeginHorizontal();

            if (GUILayout.Button($"Use High Precision Collisions (better UX)",
                CurrentSettings.RegularButton))
            {
                CurrentSettings.useLowQualityConvexCollidersOnSelection = false;
                CurrentSettings.useNormalFactor = true;
                CurrentSettings.UseBoundDependantValues = true;

                EditorUtility.SetDirty(CurrentSettings);
                CurrentStep = StartingStep.GETTING_FURTHER;
            }

            if (GUILayout.Button($"Use Low Precision Collisions (higher FPS)",
                CurrentSettings.RegularButton))
            {
                CurrentSettings.useLowQualityConvexCollidersOnSelection = true;
                CurrentSettings.useNormalFactor = false;
                CurrentSettings.UseBoundDependantValues = false;

                EditorUtility.SetDirty(CurrentSettings);
                CurrentStep = StartingStep.GETTING_FURTHER;
            }

            GUILayout.EndHorizontal();
            GUILayout.Label(
                $"<i>A few parameters define several aspects of the collisions: check the options for more advanced tweaking!</i>",
                new GUIStyle(CurrentSettings.RichParagraph) {margin = new RectOffset(15, 0, 0, 0)});
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndVertical();
        }

        public void GettingFurther()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(CurrentSettings.Exclamation, new GUIStyle(GUI.skin.label)
                {alignment = TextAnchor.MiddleCenter, fixedHeight = 60});

            EditorGUILayout.BeginVertical();

            GUILayout.Label($"<b>Going Further (5/6)</b>", CurrentSettings.RichTitle);

            EditorGUILayout.EndVertical();

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            EditorGUILayout.BeginVertical(CurrentSettings.TabStyle);
            GUILayout.FlexibleSpace();

            GUILayout.Label(
                $"<b>Grabbit has many options to fit any workflows!</b>",
                new GUIStyle(CurrentSettings.RichCenter) {margin = new RectOffset(15, 0, 0, 0)});
            GUILayout.Label(
                $"In the options, you can:" +
                $"\n-Check Layers or Tags that interact with Grabbit" +
                $"\n-Prevent Grabbit from selecting objects that are too large (such as terrains)" +
                $"\n-Change the precision of generated colliders" +
                $"\n-And much more!",
                new GUIStyle(CurrentSettings.RichParagraph) {margin = new RectOffset(15, 0, 0, 0)});


            GUILayout.BeginHorizontal();

            if (GUILayout.Button($"Got It!",
                CurrentSettings.RegularButton))
            {
                CurrentStep = StartingStep.TUTORIAL;
            }

            GUILayout.EndHorizontal();
            GUILayout.Label(
                $"<i>The documentation can help you understand better the options, and you can always contact us on Discord for extra help!</i>",
                new GUIStyle(CurrentSettings.RichParagraph) {margin = new RectOffset(15, 0, 0, 0)});
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndVertical();
        }

        public void Tutorial()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(CurrentSettings.Exclamation, new GUIStyle(GUI.skin.label)
                {alignment = TextAnchor.MiddleCenter, fixedHeight = 60});

            EditorGUILayout.BeginVertical();

            GUILayout.Label($"<b>Quick Tutorial! (6/6)</b>", CurrentSettings.RichTitle);

            EditorGUILayout.EndVertical();

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            EditorGUILayout.BeginVertical(CurrentSettings.TabStyle);
            GUILayout.FlexibleSpace();

            GUILayout.Label(
                $"<b>You are almost ready to start!</b>",
                new GUIStyle(CurrentSettings.RichCenter) {margin = new RectOffset(15, 0, 0, 0)});
            GUILayout.Label(
                $"Start Grabbit by pressing the <b>U, I, O, P, or [ Keys</b> for each mode.",
                new GUIStyle(CurrentSettings.RichParagraph) {margin = new RectOffset(15, 0, 0, 0)});
            GUILayout.Label(
                $"<b>But watch out!</b> Some keys may be in conflict with some Unity's shortcuts. If that's the case you can configure this in the shortcut manager (Edit>Shortcuts..)",
                new GUIStyle(CurrentSettings.RichParagraph) {margin = new RectOffset(15, 0, 0, 0)});

            GUILayout.BeginHorizontal();

            if (GUILayout.Button($"Let's Start To Grab!",
                CurrentSettings.RegularButton))
            {
                CurrentStep = StartingStep.INTRO;
                CurrentSettings.ShowStartupWindow = false;
                EditorUtility.SetDirty(CurrentSettings);
                Close();
            }

            GUILayout.EndHorizontal();
            GUILayout.Label(
                $"<i>That's it for now! We hope you'll enjoy Grabbit as much as we do!</i>",
                new GUIStyle(CurrentSettings.RichParagraph) {margin = new RectOffset(15, 0, 0, 0)});
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndVertical();
        }

        #endregion

        public void ShowChangeLog()
        {
            //TODO changelog popup
        }
    }
}

#endif