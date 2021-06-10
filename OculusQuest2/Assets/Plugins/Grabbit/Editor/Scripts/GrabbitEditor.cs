#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Grabbit
{
    public class GrabbitEditor : EditorWindow
    {
        private static GrabbitEditor instance;
        public ColliderMeshContainer ColliderMeshContainer;

        public bool IsStylesInitialized;

        public GrabbitSettings CurrentSettings;
        public GrabbitTool CurrentTool;

        private Vector2 scrollPos;


        public static GrabbitEditor Instance
        {
            get
            {
                CreateInstanceIfNeeded();
                //      instance.FetchSettings();
                return instance;
            }
        }

        public GUIContent GrabbitLogo =>
            CurrentSettings ? new GUIContent(CurrentSettings.GrabbitBigLogo) : new GUIContent("Grabbit");

        [MenuItem("Tools/Grabbit/Contact/Join Our Discord!")]
        public static void ContactUs()
        {
            Application.OpenURL("https://discord.gg/XEET6D6vN9");
        }

        [MenuItem("Tools/Grabbit/Contact/Contact Us!")]
        public static void ContactUsMail()
        {
            Application.OpenURL("mailto:besnard.william@gmail.com");
        }

        [InitializeOnLoadMethod]
        public static void PlugToSceneOnLoad()
        {
            EditorApplication.update -= CheckSceneOnceOnLoad;
            EditorApplication.update += CheckSceneOnceOnLoad;
        }

        private static void CheckSceneOnceOnLoad()
        {
            EditorApplication.update -= CheckSceneOnceOnLoad;
            var ids = AssetDatabase.FindAssets("t:GrabbitSettings");
            if (ids.Length == 0)
            {
                Debug.LogWarning("No Grabbit settings found, please contact us on Discord!");
                return;
            }

            var setting = AssetDatabase.LoadAssetAtPath<GrabbitSettings>(AssetDatabase.GUIDToAssetPath(ids[0]));

            if (setting.ColliderBakingMode == ColliderBakingMode.ON_SCENE_OPEN)
            {
                PlugSceneCheckMode();
            }
        }

        private static void PlugSceneCheckMode()
        {
            EditorSceneManager.sceneOpened -= CheckSceneMeshes;
            EditorSceneManager.sceneOpened += CheckSceneMeshes;
            CheckSceneMeshes(new Scene(), OpenSceneMode.Single);
        }

        private static void RemoveSceneCheckMode()
        {
            EditorSceneManager.sceneOpened -= CheckSceneMeshes;
        }

        private static void CheckSceneMeshes(Scene scene, OpenSceneMode mode)
        {
            CreateInstanceIfNeeded();

            Instance.RegisterSceneMeshes();
        }

        private void RegisterSceneMeshes()
        {
            var filters = FindObjectsOfType<MeshFilter>();

            ColliderMeshContainer.RegisterCollidersFromMeshFiltersInScene(CurrentSettings, filters);
        }

        public GrabbitSettings GetOrFetchSettings(bool noWarning = false)
        {
            if (!CurrentSettings)
            {
                var ids = AssetDatabase.FindAssets("t:GrabbitSettings");
                if (ids.Length == 0)
                {
                    if (!noWarning)
                        Debug.LogWarning("No Grabbit settings found, please contact us on Discord!");
                    return null;
                }

                CurrentSettings = AssetDatabase.LoadAssetAtPath<GrabbitSettings>(AssetDatabase.GUIDToAssetPath(ids[0]));

                if (!CurrentSettings)
                {
                    if (!noWarning)
                        Debug.LogWarning("Grabbit could not locate the settings: please contact us on Discord!");
                    return null;
                }

                //  CurrentSettings.InitializeStyle(true);
            }

            return CurrentSettings;
        }

        private void FetchSettings()
        {
            if (!CurrentSettings)
            {
                var ids = AssetDatabase.FindAssets("t:GrabbitSettings");
                if (ids.Length == 0)
                {
                    Debug.LogWarning("No Grabbit settings found, please contact us on Discord!");
                    return;
                }

                CurrentSettings = AssetDatabase.LoadAssetAtPath<GrabbitSettings>(AssetDatabase.GUIDToAssetPath(ids[0]));

                if (!CurrentSettings)
                {
                    Debug.LogWarning("Grabbit could not locate the settings: please contact us on Discord!");
                }
                else
                {
                    CurrentSettings.InitializeStyle(true);
                }
            }

            if (!ColliderMeshContainer)
            {
                var ids = AssetDatabase.FindAssets("t:ColliderMeshContainer");
                if (ids.Length == 0)
                {
                    //   GUIHelper.DisplayMessage("Grabbit Error: No Collider Mesh Container, please create one!");
                    return;
                }

                ColliderMeshContainer =
                    AssetDatabase.LoadAssetAtPath<ColliderMeshContainer>(AssetDatabase.GUIDToAssetPath(ids[0]));
            }

            titleContent = new GUIContent(" Grabbit", CurrentSettings.GrabbitLogo, "It's a Rabbit that Grabs!");
        }

        public static bool IsInstanceCreated => instance;

        public static void CreateInstanceIfNeeded()
        {
            if (!instance)
            {
                var tempInsts = Resources.FindObjectsOfTypeAll<GrabbitEditor>();
                if (tempInsts.Length > 0) instance = tempInsts[0];

                EditorTools.activeToolChanged -= CheckForGrabbit;
                EditorTools.activeToolChanged += CheckForGrabbit;

                if (instance)
                    return;

                instance = GetWindow<GrabbitEditor>("Grabbit");
                instance.titleContent = new GUIContent(" Grabbit", "It's a Rabbit that Grabs!");
            }
        }

        public static void DisableGrabbitToolChangeChecks()
        {
            EditorTools.activeToolChanged -= CheckForGrabbit;
        }

        private static void CheckForGrabbit()
        {
            if (EditorTools.activeToolType == typeof(GrabbitTool))
            {
                if (instance.CurrentTool)
                {
                    instance.CurrentTool.OnEnable();
                    instance.Repaint();
                }
                else
                {
                    ShowSceneCommandWindowAndMove();
                }
            }
            else
            {
                //  if (instance.CurrentTool) instance.CurrentTool.OnDisable();

                instance.Repaint();
            }
        }


        [MenuItem("Tools/Grabbit/Grabbit Move _u")]
        public static void ShowSceneCommandWindowAndMove()
        {
            StartGrabbitOnKeyDown();
        }

        [MenuItem("Tools/Grabbit/Grabbit Rotate _i")]
        public static void ShowSceneCommandWindowAndRotate()
        {
            StartGrabbitOnKeyDown(GrabbitMode.ROTATE);
        }

        [MenuItem("Tools/Grabbit/Grabbit Align _o")]
        public static void ShowSceneCommandWindowAndAlign()
        {
            StartGrabbitOnKeyDown(GrabbitMode.ALIGN);
        }

        [MenuItem("Tools/Grabbit/Grabbit Fall _p")]
        public static void ShowSceneCommandWindowAndFall()
        {
            StartGrabbitOnKeyDown(GrabbitMode.FALL);
        }

        [MenuItem("Tools/Grabbit/Grabbit Point _[")]
        public static void ShowSceneCommandWindowAndPoint()
        {
            StartGrabbitOnKeyDown(GrabbitMode.POINT);
        }


        private static void StartGrabbitOnKeyDown(GrabbitMode mode = GrabbitMode.PLACE)
        {
            CreateInstanceIfNeeded();

            if (!instance.PrepareSettings())
                return;

            if (EditorApplication.isPlayingOrWillChangePlaymode && !instance.CurrentSettings.UseHotKeyInPlayMode)
            {
                return;
            }

            instance.CurrentSettings.CurrentMode = mode;
            instance.Focus();
            StartGrabbit(mode);
        }

        private static void StartGrabbit(GrabbitMode mode = GrabbitMode.PLACE, bool ignoreChangeOfTool = false)
        {
            if (Instance.CurrentTool)
            {
                instance.CurrentTool.OnEnable();
            }
            else
            {
                FindOrCreateTool();
                instance.CurrentTool.OnEnable();
            }

            if (!ignoreChangeOfTool)
            {
                if (instance.CurrentSettings.CurrentMode == GrabbitMode.SETTINGS)
                {
                    instance.CurrentSettings.CurrentMode = GrabbitMode.PLACE;
                }

                CheckPauseMode();
            }

            if (EditorTools.activeToolType == typeof(GrabbitTool))
            {
                SceneView.lastActiveSceneView.Focus();
            }
            else
            {
                EditorTools.SetActiveTool(instance.CurrentTool);
            }

            instance.Repaint();
            if (SceneView.lastActiveSceneView)
                SceneView.lastActiveSceneView.Focus();
            instance.prefChanged = false;
        }

        private static void FindOrCreateTool()
        {
            var grab = FindObjectOfType<GrabbitTool>();

            if (!grab)
            {
                var tools = Resources.FindObjectsOfTypeAll<GrabbitTool>();
                if (tools.Length > 0)
                    grab = tools[0];
                else
                    grab = CreateInstance<GrabbitTool>();
            }

            instance.CurrentTool = grab;
        }

        private static void CheckPauseMode()
        {
            if (instance.CurrentSettings.IsCurrentModeDynamicPause)
            {
                instance.CurrentSettings.Paused = true;
                instance.CurrentSettings.IsDynamicPauseMode = true;
            }
            else
            {
                instance.CurrentSettings.Paused = false;
                instance.CurrentSettings.IsDynamicPauseMode = false;
            }
        }

        private bool PrepareSettings()
        {
            if (CurrentSettings)
                CurrentSettings.InitializeStyle();
            else
            {
                FetchSettings();

                if (CurrentSettings)
                    CurrentSettings.InitializeStyle();
                else
                    return false;
            }

            return true;
        }

        public static void RestartGrabbit()
        {
            EditorTools.RestorePreviousPersistentTool();
            Selection.objects = new Object[0];
            // CurrentTool.OnDisable();
            StartGrabbit(GrabbitMode.PLACE, false);
        }

        private void PauseUnPause()
        {
            CurrentSettings.Paused = !CurrentSettings.Paused;
        }

        private void GenerateAllColliders()
        {
            ColliderMeshContainer.GenerateAllColliders(CurrentSettings);
        }


        #region GUI

        public void OnDestroy()
        {
            if (CurrentTool)
                CurrentTool.ExitTool();
        }

        private void OnGUI()
        {
            CreateInstanceIfNeeded();
        
            if (!PrepareSettings()) return;

            if (!CurrentSettings)
            {
                GUIHelper.DisplayObjectOption("Grabbit Settings", ref CurrentSettings);
                return;
            }

            if (!CurrentTool)
                FindOrCreateTool();

            if (!IsStylesInitialized)
            {
                CurrentSettings.InitializeStyle(true);
            }


            if (EditorTools.activeToolType != typeof(GrabbitTool))
            {
                InactiveGrabbitGUI();
                return;
            }

            HeaderGUI();

            var previous = CurrentSettings.CurrentMode;
            CurrentSettings.CurrentMode = (GrabbitMode) GUILayout.Toolbar((int) CurrentSettings.CurrentMode,
                Enum.GetNames(typeof(GrabbitMode)));

            var modif = false;

            if (previous != CurrentSettings.CurrentMode)
                CheckPauseMode();

            if (previous != CurrentSettings.CurrentMode) CurrentTool.CanUndo = true;

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            EditorGUILayout.BeginVertical(CurrentSettings.TabStyle);

            switch (CurrentSettings.CurrentMode)
            {
                case GrabbitMode.PLACE:
                    modif = DisplayPlacementParams();
                    break;
                case GrabbitMode.ROTATE:
                    modif = DisplayRotateParams();
                    break;
                case GrabbitMode.ALIGN:
                    modif = DisplayAlignParams();
                    break;
                case GrabbitMode.FALL:
                    modif = DisplayGravityParams();
                    break;
                case GrabbitMode.POINT:
                    modif = DisplayPointParams();
                    break;
                case GrabbitMode.SETTINGS:
                    modif = DisplayPreferenceParams();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndScrollView();

            if (modif || prefChanged)
            {
                EditorUtility.SetDirty(CurrentSettings);
                SceneView.lastActiveSceneView.Focus();
                if (CurrentSettings.CurrentMode == GrabbitMode.SETTINGS)
                {
                    prefChanged = true;
                    GUIHelper.DisplayMessage("<b>Please restart Grabbit for the changes to take effect</b>");
                }
            }

            if (!CurrentSettings.HideGrabbitTextLogo)
            {
                GUILayout.Label(CurrentSettings.GrabbitTextLogo, new GUIStyle(GUI.skin.label)
                    {alignment = TextAnchor.MiddleRight, fixedHeight = Mathf.Min(position.height * 0.06f, 25)});
            }
        }

        private bool prefChanged;

        #region MAINGUI

        private void HeaderGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(CurrentSettings.GrabbitFaceBig, new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter, fixedWidth = position.width * 0.1f,
                fixedHeight = Mathf.Clamp(position.height * 0.15f, 50, 65)
            });
            GUILayout.BeginVertical();
            string status = "<b>Status: </b>" + (CurrentSettings.Paused ? "<color='red'>Paused</color>" : "Active");
            GUIHelper.DisplayMessage(status);
            GUIHelper.DisplayButton(CurrentSettings.Paused ? "Resume Grabbit" : "Pause Grabbit", PauseUnPause);
            GUIHelper.DisplayButton("Exit Grabbit", ExitTool);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUIHelper.DisplayMessage("<b>Grabbit Mode</b>");
        }

        public void ExitTool()
        {
            if (CurrentTool)
                CurrentTool.ExitTool();
        }

        private void InactiveGrabbitGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();

            GUILayout.Label(GrabbitLogo,
                new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter, fixedWidth = position.width,
                    fixedHeight = position.height * 0.5f
                });

            var big = new GUIStyle(CurrentSettings.Rich)
            {
                alignment = TextAnchor.MiddleCenter,
                fixedWidth = position.width * 0.95f,
                fontSize = Mathf.Max((int) (Mathf.Min(position.height, position.width) * 0.04f), 12),
                margin = new RectOffset(0, 0, 10, 0)
            };

            var small = new GUIStyle(CurrentSettings.Rich)
            {
                alignment = TextAnchor.MiddleCenter,
                fixedWidth = position.width * 0.95f,
                fontSize = Mathf.Max((int) (Mathf.Min(position.height, position.width) * 0.03f), 11),
                margin = new RectOffset(0, 0, 2, 0)
            };


            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("<b>Click Here To Activate Grabbit</b>", big);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("<b>Or press the 'U' Key (by default)</b>", small);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            if (Event.current.type == EventType.MouseDown)
                StartGrabbit();
        }

        #endregion

        #region UTILS

        private void DisplayAdvancedOption()
        {
            GUILayout.BeginVertical();
            GUIHelper.DisplayMessage("<b>General Settings</b>");
            GUIHelper.DisplayBoolOption("Show Advanced Options",
                ref CurrentSettings.UseAdvancedOption, false, "shows more advanced options that give you more control");
            GUILayout.EndVertical();
        }

        private void ChangeFlag(bool add, ref RigidbodyConstraints combined, RigidbodyConstraints flagToCheck)
        {
            if (!add)
                // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
                combined &= ~flagToCheck;
            else
                combined |= flagToCheck;
        }

        private void DisplayConstraintParam(ref RigidbodyConstraints combined, bool displayRotation = true)
        {
            GUIHelper.DisplayMessage("<b> Constraints </b>");
            GUILayout.BeginHorizontal(CurrentSettings.LabelMarginStyle);
            EditorGUILayout.LabelField("Position", CurrentSettings.LabelMarginStyle);

            var prevWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = prevWidth * 0.1f;

            var x = GUILayout.Toggle(combined.HasFlag(RigidbodyConstraints.FreezePositionX), new GUIContent("X"),
                "Button");
            ChangeFlag(x, ref combined, RigidbodyConstraints.FreezePositionX);

            var y = GUILayout.Toggle(combined.HasFlag(RigidbodyConstraints.FreezePositionY), "Y", "Button");
            ChangeFlag(y, ref combined, RigidbodyConstraints.FreezePositionY);

            var z = GUILayout.Toggle(combined.HasFlag(RigidbodyConstraints.FreezePositionZ), "Z", "Button");
            ChangeFlag(z, ref combined, RigidbodyConstraints.FreezePositionZ);

            EditorGUIUtility.labelWidth = prevWidth;

            GUILayout.EndHorizontal();

            if (!displayRotation)
                return;

            GUILayout.BeginHorizontal(CurrentSettings.LabelMarginStyle);
            EditorGUILayout.LabelField("Rotation", CurrentSettings.LabelMarginStyle);

            prevWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = prevWidth * 0.1f;

            var rotX = GUILayout.Toggle(combined.HasFlag(RigidbodyConstraints.FreezeRotationX), "X", "Button");
            ChangeFlag(rotX, ref combined, RigidbodyConstraints.FreezeRotationX);

            var rotY = GUILayout.Toggle(combined.HasFlag(RigidbodyConstraints.FreezeRotationY), "Y", "Button");
            ChangeFlag(rotY, ref combined, RigidbodyConstraints.FreezeRotationY);

            var rotZ = GUILayout.Toggle(combined.HasFlag(RigidbodyConstraints.FreezeRotationZ), "Z", "Button");
            ChangeFlag(rotZ, ref combined, RigidbodyConstraints.FreezeRotationZ);

            EditorGUIUtility.labelWidth = prevWidth;

            GUILayout.EndHorizontal();
        }

        private bool RigidBodyLimitOptions(bool modif)
        {
            if (!CurrentSettings.UseAdvancedOption)
                return false;

            GUIHelper.DisplayMessage("<b> Physics Settings </b>");


            modif = modif || GUIHelper.DisplayBoolOption("Use Soft Collisions", ref CurrentSettings.UseSoftCollision,
                false,
                "if true, objects will jitter less, but slow down a bit when colliding with other objects");

            if (CurrentSettings.UseSoftCollision)
                modif = modif || GUIHelper.DisplayFloatOption("Collision Max Velocity Factor",
                    ref CurrentSettings.CollisionMaxVelocity);
            modif = modif || GUIHelper.DisplayFloatOption("Max Velocity", ref CurrentSettings.MaxVelocity);
            modif = modif || GUIHelper.DisplayFloatOption("Max Angular Velocity",
                ref CurrentSettings.MaxAngularVelocity);
            modif = modif || GUIHelper.DisplayFloatOption("Max Depenetration Velocity",
                ref CurrentSettings.MaxDepenetrationVelocity);
            modif = modif || GUIHelper.DisplayFloatOption("Drag", ref CurrentSettings.Drag);
            modif = modif || GUIHelper.DisplayFloatOption("Angular Drag", ref CurrentSettings.AngularDrag);
            return modif;
        }

        private static void GetLayerNames(List<string> layerNames)
        {
            for (int i = 0; i <= 31; i++)
            {
                var name = LayerMask.LayerToName(i);
                if (name.Length > 0)
                    layerNames.Add(name);
            }
        }

        #endregion

        #region MODE SETTINGS

        private bool DisplayPointParams()
        {
            DisplayAdvancedOption();

            if (CurrentSettings.ControlRotation)
            {
                CurrentSettings.Constraints &= ~RigidbodyConstraints.FreezeRotationX;
                CurrentSettings.Constraints &= ~RigidbodyConstraints.FreezeRotationY;
                CurrentSettings.Constraints &= ~RigidbodyConstraints.FreezeRotationZ;
            }

            DisplayConstraintParam(ref CurrentSettings.Constraints, !CurrentSettings.ControlRotation);

            var modif = false;
            GUIHelper.DisplayMessage("<b> Point Settings </b>");

            GUIHelper.DisplayBoolOption("Teleport When Too Far", ref CurrentSettings.PointModeIgnoreCollision, false,
                "Moves objects directly to the wanted position, ignoring the collisions");
            GUIHelper.DisplayFloatOption("Distance To Ignore Collision (x Bounds size)",
                ref CurrentSettings.MinDistanceToIgnoreCollision);

            GUIHelper.DisplayMessage("<b> Selection Constraints Settings </b>");

            modif = modif || GUIHelper.DisplayBoolOption("Preserve Selection's Initial Transforms",
                ref CurrentSettings.useCentroidRelativeTransform);

            if (CurrentSettings.useCentroidRelativeTransform && CurrentSettings.UseAdvancedOption)
            {
                modif = modif || GUIHelper.DisplayFloatOption("Relative Position Factor",
                    ref CurrentSettings.PullToRelativePositionFactor);

                modif = modif || GUIHelper.DisplayFloatOption("Relative Rotation Factor",
                    ref CurrentSettings.PullToOriginalRotationFactor);

                modif = modif || GUIHelper.DisplayBoolOption("Reset Initial Transform On Mouse Up",
                    ref CurrentSettings.resetCentroidRelativeTransformOnMouseUp, false,
                    "the relative rotation and position between objects will be recalculated on release");
            }


            modif = modif || RigidBodyLimitOptions(false);

            return false;
        }

        private bool DisplayRotateParams()
        {
            DisplayAdvancedOption();
            CurrentSettings.Constraints &= ~RigidbodyConstraints.FreezeRotationX;
            CurrentSettings.Constraints &= ~RigidbodyConstraints.FreezeRotationY;
            CurrentSettings.Constraints &= ~RigidbodyConstraints.FreezeRotationZ;

            DisplayConstraintParam(ref CurrentSettings.Constraints, false);

            var modif = false;
            GUIHelper.DisplayMessage("<b> Rotation Settings </b>");

            modif = modif || GUIHelper.DisplayBoolOption("Reajust Rotation On Mouse Up",
                ref CurrentSettings.PausePositionReajustment, false,
                "Make the rotation handle align to the current rotation on mouse up");

            if (CurrentSettings.UseAdvancedOption)
            {
                modif = modif || GUIHelper.DisplayFloatOption("Extra Drag For Rotation Mode",
                    ref CurrentSettings.ExtraDragForRotationMode, 0);

                modif = modif || RigidBodyLimitOptions(false);
            }

            return modif;
        }


        private bool DisplayAlignParams()
        {
            DisplayAdvancedOption();
            if (CurrentSettings.ControlRotation)
            {
                CurrentSettings.Constraints &= ~RigidbodyConstraints.FreezeRotationX;
                CurrentSettings.Constraints &= ~RigidbodyConstraints.FreezeRotationY;
                CurrentSettings.Constraints &= ~RigidbodyConstraints.FreezeRotationZ;
            }

            DisplayConstraintParam(ref CurrentSettings.Constraints, !CurrentSettings.ControlRotation);

            GUIHelper.DisplayMessage("<b> Alignment Settings </b>");

            bool modif = false;

            if (CurrentSettings.UseAdvancedOption)
            {
                modif = GUIHelper.DisplayFloatOption("Align Strength", ref CurrentSettings.AlignStrength);
            }

            modif = modif || GUIHelper.DisplayBoolOption("Control Rotation", ref CurrentSettings.ControlRotation);

            if (CurrentSettings.UseAdvancedOption)
            {
                modif = modif || RigidBodyLimitOptions(false);
            }

            return modif;
        }

        private bool DisplayPlacementParams()
        {
            DisplayAdvancedOption();

            if (CurrentSettings.ControlRotation)
            {
                CurrentSettings.Constraints &= ~RigidbodyConstraints.FreezeRotationX;
                CurrentSettings.Constraints &= ~RigidbodyConstraints.FreezeRotationY;
                CurrentSettings.Constraints &= ~RigidbodyConstraints.FreezeRotationZ;
            }

            DisplayConstraintParam(ref CurrentSettings.Constraints, !CurrentSettings.ControlRotation);

            var modif = false;
            GUIHelper.DisplayMessage("<b> Placement Settings </b>");

            modif = modif || GUIHelper.DisplayBoolOption("Control Rotation", ref CurrentSettings.ControlRotation, true,
                "controls the rotation on the top of the position. Can lead to jitter");


            modif = modif || GUIHelper.DisplayBoolOption("Reajust Handle On Mouse Up",
                ref CurrentSettings.PausePositionReajustment, false,
                "Puts the handle back to the center when released");
            modif = modif || GUIHelper.DisplayFloatOption("Distance For Instant Teleport",
                ref CurrentSettings.DistanceForTeleportation);

            modif = modif || GUIHelper.DisplayBoolOption("Display Collision Indicators",
                ref CurrentSettings.showCollisionIndicators, false,
                "Shows where the selected objects will collide");

            GUIHelper.DisplayMessage("<b> Selection Constraints Settings </b>");

            modif = modif || GUIHelper.DisplayBoolOption("Preserve Selection's Initial Transforms",
                ref CurrentSettings.useCentroidRelativeTransform);

            if (CurrentSettings.useCentroidRelativeTransform && CurrentSettings.UseAdvancedOption)
            {
                modif = modif || GUIHelper.DisplayFloatOption("Relative Position Factor",
                    ref CurrentSettings.PullToRelativePositionFactor);

                modif = modif || GUIHelper.DisplayFloatOption("Relative Rotation Factor",
                    ref CurrentSettings.PullToOriginalRotationFactor);

                modif = modif || GUIHelper.DisplayBoolOption("Reset Initial Transform On Mouse Up",
                    ref CurrentSettings.resetCentroidRelativeTransformOnMouseUp, false,
                    "the relative rotation and position between objects will be recalculated on release");
            }


            modif = modif || RigidBodyLimitOptions(false);

            return modif;
        }

        private bool DisplayGravityParams()
        {
            bool modif = false;
            DisplayAdvancedOption();
            DisplayConstraintParam(ref CurrentSettings.Constraints);
            GUIHelper.DisplayMessage("<b> Gravity Settings </b>");
            GUILayout.BeginVertical(CurrentSettings.MarginStyle);
            modif = modif || GUIHelper.DisplayButton("Reset Gravity", ResetGrav);
            GUIHelper.DisplayVectorOption("Gravity Strength", ref CurrentSettings.GravityStrength);
            GUILayout.EndVertical();

            var prevmouse = CurrentSettings.SetGravityInDirectionOfMouse;
            modif = modif || GUIHelper.DisplayBoolOption("Gravity Towards Mouse",
                ref CurrentSettings.SetGravityInDirectionOfMouse, true,
                "If true, the direction of the gravity will always points towards the mouse");

            if (!CurrentSettings.SetGravityInDirectionOfMouse && prevmouse)
            {
                ResetGrav();
            }

            modif = modif || GUIHelper.DisplayBoolOption("Limit Velocity",
                ref CurrentSettings.LimitBodyVelocityInGravityMode);

            if (CurrentSettings.LimitBodyVelocityInGravityMode) modif = modif || RigidBodyLimitOptions(false);

            return modif;
        }

        private void ResetGrav()
        {
            CurrentSettings.GravityStrength = new Vector3(0, -9.81f, 0);
        }

        private bool DisplayPreferenceParams()
        {
            GUIHelper.DisplayMessage("Current Version: " + GrabbitSettings.GrabbitVersion);
            GUIHelper.DisplayObjectOption("Grabbit Settings", ref CurrentSettings);

            DisplayAdvancedOption();

            int prevLayer = CurrentSettings.LayersToIgnore;

            var layerNames = new List<string>();
            GetLayerNames(layerNames);

            EditorGUILayout.BeginVertical(CurrentSettings.MarginStyle);
            //TODO show all layer options
            CurrentSettings.LayersToIgnore =
                EditorGUILayout.MaskField("Layers To Ignore", prevLayer, layerNames.ToArray());
            var modif = (prevLayer != CurrentSettings.LayersToIgnore);

            var tagEn = (Enum) CurrentSettings.TagLimitation;
            GUIHelper.DisplayEnumOption("Tags To Ignore", ref tagEn);


            CurrentSettings.TagLimitation = (GrabbitTagLimitationMode) tagEn;

            if (CurrentSettings.TagLimitation != GrabbitTagLimitationMode.NONE)
            {
                EditorGUILayout.BeginVertical(CurrentSettings.MarginStyle);
                CurrentSettings.tagSelected = EditorGUILayout.TagField("Tag Name", CurrentSettings.tagSelected);
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndVertical();
            modif = modif || GUIHelper.DisplayBoolOption(
                "Use Hotkeys In PlayMode",
                ref CurrentSettings.UseHotKeyInPlayMode, false,
                "Allows Grabbit's Hotkeys in play mode");

            GUIHelper.DisplayMessage("<b> Performance Settings</b>");

            modif = modif || GUIHelper.DisplayFloatOption("Simulation Speed", ref CurrentSettings.Speed, 0.0001f);

            var prevWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = prevWidth * 2f;

            modif = modif || GUIHelper.DisplayBoolOption(
                "Use Low Quality Colliders On Selected (faster but less precise)",
                ref CurrentSettings.useLowQualityConvexCollidersOnSelection, false,
                "Lets Unity generate faster, but less precise colliders. Makes collision much faster, but imprecise");


            modif = modif || GUIHelper.DisplayBoolOption(
                "Limit Scan Zone",
                ref CurrentSettings.UseLimitationZone, false,
                "Limits the size that Grabbit considers when starting, limiting the number of objects it has to scan");

            modif = modif || GUIHelper.DisplayBoolOption(
                "Change Layer Of Grabbed Objects",
                ref CurrentSettings.ChangeLayerOfDynamicObjects, false,
                "Changes the layer of objects that Grabbit is currently grabbing");

            if (CurrentSettings.ChangeLayerOfDynamicObjects)
            {
                prevLayer = CurrentSettings.DynamicObjectsLayer;
                CurrentSettings.DynamicObjectsLayer = EditorGUILayout.LayerField("Grabbed Objects Layer", prevLayer);
                modif = modif || (prevLayer != CurrentSettings.DynamicObjectsLayer);
            }
            
            modif = modif || GUIHelper.DisplayBoolOption(
                "Change Layer Of Static Objects",
                ref CurrentSettings.ChangeLayerOfStaticObjects, false,
                "Changes the layer of objects that Grabbit uses as background collisions");

            if (CurrentSettings.ChangeLayerOfStaticObjects)
            {
                prevLayer = CurrentSettings.StaticObjectsLayer;
                CurrentSettings.StaticObjectsLayer = EditorGUILayout.LayerField("Static Objects Layer", prevLayer);
                modif = modif || (prevLayer != CurrentSettings.StaticObjectsLayer);
            }

            EditorGUIUtility.labelWidth = prevWidth;


            if (CurrentSettings.UseAdvancedOption)
            {
                modif = modif || GUIHelper.DisplayIntOption("Max PhysX Iterations Per Update",
                    ref CurrentSettings.MaxPhysXIterationPerUpdate);
                modif = modif || GUIHelper.DisplayIntOption("Velocity Iterations",
                    ref CurrentSettings.velocityIterations);
                modif = modif || GUIHelper.DisplayIntOption("Solver Iterations",
                    ref CurrentSettings.solverIterations);
            }

            if (CurrentSettings.UseAdvancedOption)
            {
                GUIHelper.DisplayMessage("<b> Collision Settings</b>");

                modif = modif || GUIHelper.DisplayBoolOption("Set Selection Kinematic When Inactive",
                    ref CurrentSettings.SetKinematicWhenNotActive, false,
                    "Makes the selected objects kinematic when the tool is not clicked");

                modif = modif || GUIHelper.DisplayBoolOption("Use Bounds Relative Values",
                    ref CurrentSettings.UseBoundDependantValues, false,
                    "Use the size of bounds to defined the strength of the forces");
                if (CurrentSettings.UseBoundDependantValues)
                {
                    EditorGUILayout.BeginVertical(CurrentSettings.MarginStyle);


                    modif = modif || GUIHelper.DisplayFloatOption("Min Bounds Size Values",
                        ref CurrentSettings.BoundVolumeForMinSpeed);
                    modif = modif || GUIHelper.DisplayFloatOption("Max Bounds Size Values",
                        ref CurrentSettings.BoundVolumeForMaxSpeed);

                    modif = modif || GUIHelper.DisplayFloatOption("Factor For Min Bounds",
                        ref CurrentSettings.CollisionMinBoundFactor);
                    modif = modif || GUIHelper.DisplayFloatOption("Factor For Max Bounds",
                        ref CurrentSettings.CollisionMaxBoundFactor);
                    EditorGUILayout.EndVertical();
                }


                modif = modif || GUIHelper.DisplayBoolOption("Limit Max Selectable Bound Size",
                    ref CurrentSettings.LimitBoundSize, false,
                    "If true, Grabbit will not affect object that are big enough");
                if (CurrentSettings.LimitBoundSize)
                {
                    EditorGUILayout.BeginVertical(CurrentSettings.MarginStyle);


                    Object refSize = null;
                    GUIHelper.DisplayObjectOption("Use Object To Calculate Bound Size:", ref refSize);
                    if (refSize)
                    {
                        var prevSelection = Selection.objects;
                        Selection.objects = new[] {refSize};
                        Bounds b = UnityEditorInternal.InternalEditorUtility.CalculateSelectionBounds(true, true);
                        Selection.objects = prevSelection;
                        CurrentSettings.maxBoundAxisLength = Mathf.Max(b.size.x, b.size.y, b.size.z);
                    }

                    modif = modif || GUIHelper.DisplayFloatOption("Max Bounds Axis",
                        ref CurrentSettings.maxBoundAxisLength);
                    EditorGUILayout.EndVertical();
                }

                modif = modif || GUIHelper.DisplayBoolOption("Use Collision Normal Factor",
                    ref CurrentSettings.useNormalFactor, false,
                    "make collision forces depend on the direction of the collision (smoother, but has a performance cost)");

                if (CurrentSettings.useNormalFactor)
                    modif = modif || GUIHelper.DisplayFloatOption("Max Normal Speed Factor",
                        ref CurrentSettings.MaxNormalSpeedFactor);
            }

            GUIHelper.DisplayMessage("<b> Collider Settings</b>");

            var collisionEnum = (Enum) CurrentSettings.ColliderGenerationMode;
            EditorGUILayout.BeginVertical(CurrentSettings.MarginStyle);
            GUIHelper.DisplayEnumOption("Collider Generation Mode", ref collisionEnum);
            EditorGUILayout.EndVertical();

            modif = modif || !Equals(collisionEnum, CurrentSettings.ColliderGenerationMode);

            CurrentSettings.ColliderGenerationMode = (ColliderGenerationMode) collisionEnum;

            /*    modif = modif || GUIHelper.DisplayBoolOption("Use Existing Predefined Colliders",
                    ref CurrentSettings.UsePredefinedColliders, false,
                    "use the colliders that you set up on your objects");*/

            prevWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = prevWidth * 2f;
            modif = modif || GUIHelper.DisplayBoolOption("Generate Dynamic Concave Colliders (more precise but slower)",
                ref CurrentSettings.UseDynamicNonConvexColliders, false,
                "generates colliders so that non convex geometries can behave normally");
            EditorGUIUtility.labelWidth = prevWidth;

            if (CurrentSettings.UseDynamicNonConvexColliders)
            {
                if (CurrentSettings.UseAdvancedOption)
                {
                    modif = modif || GUIHelper.DisplayIntOption("Collider Resolution",
                        ref CurrentSettings.ColliderResolution);
                    modif = modif || GUIHelper.DisplayIntOption("Max Collider Count Per Mesh",
                        ref CurrentSettings.MaxMeshCollidersCreated);
                }

                var en = (Enum) CurrentSettings.ColliderBakingMode;
                EditorGUILayout.BeginVertical(CurrentSettings.MarginStyle);
                GUIHelper.DisplayEnumOption("Collider Baking Mode", ref en);
                EditorGUILayout.EndVertical();

                bool endModif = !Equals(en, CurrentSettings.ColliderBakingMode);

                if (endModif)
                {
                    if (CurrentSettings.ColliderBakingMode == ColliderBakingMode.ON_SCENE_OPEN)
                        PlugSceneCheckMode();
                    else
                    {
                        RemoveSceneCheckMode();
                    }
                }

                modif = modif || endModif;

                CurrentSettings.ColliderBakingMode = (ColliderBakingMode) en;

                GUIHelper.DisplayButton("Bake Colliders From The Scene", RegisterSceneMeshes);

                GUIHelper.DisplayButton("Bake Colliders From All Assets", GenerateAllColliders);

                if (CurrentSettings.UseAdvancedOption)
                    GUIHelper.DisplayButton("Clear Colliders From All Assets", ConfirmDeletion);
            }


            GUIHelper.DisplayMessage("<b> Display Settings</b>");

            modif = modif || GUIHelper.DisplayBoolOption(
                "Show Mode Info",
                ref CurrentSettings.showSceneInfo, false,
                "Shows information about the current mode on the top left corner of the scene view");
            modif = modif || GUIHelper.DisplayBoolOption("Debug Collision",
                ref CurrentSettings.debugCollisionDirection);

            return modif;
        }

        private void ConfirmDeletion()
        {
            if (EditorUtility.DisplayDialog("Clearing the colliders",
                "Are you sure you want to delete the generated colliders? \n They will need to be generated again for Grabbit's Dynamic Concave mode to be used",
                "Ok", "Cancel"))
                ColliderMeshContainer.ClearColliders();
        }

        #endregion

        #endregion
    }
}
#endif