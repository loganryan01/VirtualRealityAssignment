#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using Grabbit;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.EditorTools;
using UnityEditor.Experimental.SceneManagement;
using UnityEditor.IMGUI.Controls;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Grabbit
{
    [EditorTool("Grabbit")]
    public class GrabbitTool : EditorTool
    {
        private static double lastTimeSinceStartup;

        private readonly Dictionary<GameObject, GrabbitHandler> handlers =
            new Dictionary<GameObject, GrabbitHandler>();

        public readonly Dictionary<Rigidbody, RbSaveState> savedDisabledRigidbodies =
            new Dictionary<Rigidbody, RbSaveState>();

        private readonly List<GrabbitHandler> handlersToRemove = new List<GrabbitHandler>();

        private readonly List<GrabbitHandler> selectionHandlers = new List<GrabbitHandler>();
        private readonly List<GameObject> selectionLimitedObjects = new List<GameObject>();

        public bool Active;

        public bool CanUndo;
        private Vector3 centroid;
        private ColliderMeshContainer colliderMeshContainer;

        private bool collisionFound;

        private Quaternion gravityRotation = Quaternion.identity;

        private bool mouseDown;
        private bool isToolSelected;
        private bool isToolActive;

        private bool previousAutoSim;

        private Vector3 previousGravity;
        private int prevSolverIteration;
        private int prevVelocityIteration;
        private Vector3 pullToCenter = Vector3.one;

        private GrabbitSettings settings;
        private double timer;
        private GUIContent toolContent;

        private int updateBeforeCompile;

        private Vector3 wantedCenter = Vector3.zero;
        private Quaternion wantedRotation = Quaternion.identity;
        private static double EditorDeltaTime => EditorApplication.timeSinceStartup - lastTimeSinceStartup;

        [NonSerialized] public bool IsPrefabMode;
        private PhysicsScene prefabModePhysicsScene;

        private GameObject prefabRoot;

        private Texture2D Logo
        {
            get
            {
                if (!settings)
                {
                    settings = GrabbitEditor.Instance.GetOrFetchSettings(true);
                }

                return settings ? settings.GrabbitLogo : new Texture2D(1, 1);
            }
        }

        private Texture2D SceneLogo
        {
            get
            {
                if (!settings)
                {
                    settings = GrabbitEditor.Instance.GetOrFetchSettings(true);
                }

                return settings ? settings.GrabbitSceneLogo : new Texture2D(1, 1);
            }
        }

        public override GUIContent toolbarIcon => toolContent ?? (toolContent = new GUIContent(Logo, "Grabbit"));

        private bool isConfiguringLimitationZone;

        public void OnEnable()
        {
            if (Active || !EditorTools.IsActiveTool(this))
                return;

            if (!GrabbitEditor.IsInstanceCreated)
                GrabbitEditor.CreateInstanceIfNeeded();

            if (!GrabbitEditor.Instance.CurrentSettings)
                GrabbitEditor.Instance.GetOrFetchSettings();

            if (GrabbitEditor.Instance.CurrentSettings.UseLimitationZone)
            {
                if (!GrabbitEditor.Instance.CurrentSettings.DidConfigureLimitationRangeAtLeastOnce)
                {
                    GrabbitEditor.Instance.CurrentSettings.LimitationRange.Initialize();

                    SceneView.duringSceneGui -= ConfigureLimitationZoneGUI;
                    SceneView.duringSceneGui += ConfigureLimitationZoneGUI;
                    isConfiguringLimitationZone = true;
                    return;
                }
            }
            else
            {
                SceneView.duringSceneGui -= ConfigureLimitationZoneGUI;
            }

            EnableLogic();
        }


        public void OnDisable()
        {
            isConfiguringLimitationZone = false;

            if (!Active)
                return;

            DisableLogic();
        }

        private bool disabling = false;
        private bool enabling = false;

        private bool IsSettingUp => disabling || enabling;

        private void DisableLogic(bool butDontChangeTools = false)
        {
            EditorUtility.ClearProgressBar();
            SceneView.duringSceneGui -= ConfigureLimitationZoneGUI;

            try
            {
                enabling = false;
                disabling = true;
                DeactivationUnhook();

                if (!butDontChangeTools)
                {
                    EditorTools.RestorePreviousPersistentTool();
                    if (EditorTools.IsActiveTool(this))
                        EditorTools.SetActiveTool((EditorTool) null);
                }

                toolbarIcon.image = settings.GrabbitLogo;

                DeactivateCurrentSelection();
                PreparePhysicsDeactivation();

                Active = false;

                settings.IsGrabbitActive = false;
                EditorUtility.SetDirty(settings);
                disabling = false;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                Debug.LogError("Grabbit encountered an error while deactivating, please contact us on Discord");
            }
        }

        private void EnableLogic()
        {
            try
            {
                enabling = true;

                GrabbitEditor.Instance.CurrentTool = this;
                settings = GrabbitEditor.Instance.CurrentSettings;
                settings.IsGrabbitActive = true;

                colliderMeshContainer = GrabbitEditor.Instance.ColliderMeshContainer;

                lastTimeSinceStartup = EditorApplication.timeSinceStartup;

                toolbarIcon.image = settings.GrabbitSelectedLogo;

                PrepareActivationHookUps();
                ClearSelectionData();
                PreparePhysicsActivation();

                wantedCenter = Vector3.zero;
                wantedRotation = Quaternion.identity;
                pullToCenter = Vector3.one;

                ResolveSelection();

                Active = true;
                disabling = false;
                enabling = false;
            }
            catch (Exception e)
            {
                Active = true;
                disabling = false;
                enabling = false;

                Debug.LogException(e);
                Debug.LogWarning("Grabbit encountered an issue while activating, please contact us on Discord");
                DisableLogic();
            }
        }

        private void PrepareActivationHookUps()
        {
            GrabbitHandler.InstantDestroyFlag = false;
            EditorTools.activeToolChanged += DisableToolOnChange;

            EditorApplication.playModeStateChanged += ResetOnPlay;

            EditorSceneManager.sceneOpened += ResetOnSceneLoaded;

            SceneView.beforeSceneGui -= BeforeSceneViewGUI;
            SceneView.beforeSceneGui += BeforeSceneViewGUI;
            SceneView.duringSceneGui -= DuringSceneViewGUI;
            SceneView.duringSceneGui += DuringSceneViewGUI;

            EditorApplication.update -= UpdatePhysics;
            EditorApplication.update += UpdatePhysics;

            EditorApplication.update -= UpdateDeltaTime;
            EditorApplication.update += UpdateDeltaTime;

            Selection.selectionChanged += ResolveSelection;
            CompilationPipeline.compilationStarted += ResetToolOnCompilation;
            EditorApplication.wantsToQuit += ResetOnQuit;
            EditorSceneManager.sceneSaving += ResetOnSceneSaving;
        }

        private void ResetOnSceneLoaded(Scene scene, OpenSceneMode mode)
        {
            ResetOnSceneSaving(scene, "");
        }


        private void DeactivationUnhook()
        {
#if UNITY_2020_2
#else
            EditorTools.activeToolChanged -= DisableToolOnChange;
#endif
            SceneView.beforeSceneGui -= BeforeSceneViewGUI;
            SceneView.duringSceneGui -= DuringSceneViewGUI;

            EditorSceneManager.sceneOpened -= ResetOnSceneLoaded;

            EditorApplication.playModeStateChanged -= ResetOnPlay;

            EditorApplication.update -= UpdatePhysics;
            EditorApplication.update -= UpdateDeltaTime;

            Selection.selectionChanged -= ResolveSelection;
            CompilationPipeline.compilationStarted -= ResetToolOnCompilation;
            EditorApplication.wantsToQuit -= ResetOnQuit;
            EditorSceneManager.sceneSaving -= ResetOnSceneSaving;
        }

        private void ResetToolOnCompilation(object obj)
        {
            EditorApplication.LockReloadAssemblies();
            GrabbitHandler.InstantDestroyFlag = true;

            ExitTool();

            updateBeforeCompile = 0;
            EditorApplication.update += UnlockCompilationAfterReset;
            CompilationPipeline.compilationStarted -= ResetToolOnCompilation;
        }

        private void UnlockCompilationAfterReset()
        {
            updateBeforeCompile++;
            if (updateBeforeCompile > 5)
            {
                EditorApplication.update -= UnlockCompilationAfterReset;
                GrabbitHandler.InstantDestroyFlag = false;
                Selection.objects = new Object[0] { };

             /*   if (EditorTools.IsActiveTool(this))
                    EditorTools.RestorePreviousPersistentTool();*/
                EditorApplication.UnlockReloadAssemblies();
            }
        }

        private void ResetOnSceneSaving(Scene scene, string path)
        {
            GrabbitHandler.InstantDestroyFlag = true;
            ExitTool();
        }

        private void ResetOnPlay(PlayModeStateChange obj)
        {
            switch (obj)
            {
                case PlayModeStateChange.EnteredEditMode:
                    GrabbitHandler.InstantDestroyFlag = true;
                    ExitTool();
                    break;
                case PlayModeStateChange.ExitingEditMode:
                    GrabbitHandler.InstantDestroyFlag = true;
                    ExitTool();
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    break;
            }
        }

        public void ResetOnPrefabModeSave()
        {
            GrabbitHandler.InstantDestroyFlag = true;
            ExitTool();
            if (prefabRoot)
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(prefabRoot);
                EditorUtility.SetDirty(prefabRoot);
                AssetDatabase.SaveAssets();
            }
        }


        private bool ResetOnQuit()
        {
            //    GrabbitEditor.DisableGrabbitToolChangeChecks();
            GrabbitHandler.InstantDestroyFlag = true;
            ExitTool();
            EditorTools.SetActiveTool((EditorTool) null);
            return true;
        }

        public void ExitTool()
        {
            DisableLogic();
            if (settings.DisplayWarning && Selection.gameObjects.Length > 0)
            {
/*                Debug.LogWarning(
                    "Grabbit Warning: You may receive an Exception when Grabbit is active while your code compiles. " +
                    "You can safely ignore this, everything works fine, it seems to be a bug in Unity :) - You can disable that message in the options");*/
            }
        }

        private void DisableToolOnChange()
        {
            if (EditorTools.activeToolType != typeof(GrabbitTool))
                DisableLogic(true);
        }

        private void PreparePhysicsActivation()
        {
            previousAutoSim = Physics.autoSimulation;
            prevSolverIteration = Physics.defaultSolverIterations;
            prevVelocityIteration = Physics.defaultSolverVelocityIterations;
            previousGravity = Physics.gravity;

            Physics.autoSimulation = false;
            Physics.gravity = settings.GravityStrength;
            Physics.defaultSolverIterations = settings.solverIterations;
            Physics.defaultSolverVelocityIterations = settings.velocityIterations;

            savedDisabledRigidbodies.Clear();
            handlers.Clear();


            //TODO add an option related to the count of objects to know whether to show the message or not
            EditorUtility.DisplayProgressBar("Grabbit is loading, please wait...", "", 0);

            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null)
            {
                IsPrefabMode = true;
                prefabModePhysicsScene = prefabStage.scene.GetPhysicsScene();
                prefabRoot = prefabStage.prefabContentsRoot;
            }
            else
            {
                IsPrefabMode = false;
            }


            var bodies = FindObjectsOfType<Rigidbody>();

            //delete all the bodies that we're not handling
            foreach (var body in bodies)
            {
                RegisterAndDisableRigidBody(body);
            }

            MeshRenderer[] rens;
            if (settings.UseLimitationZone)
            {
                if (!settings.LimitationRange.IsInitialized)
                    settings.LimitationRange.Initialize();

                settings.LimitationRange.SelectObjectsFromScenes();
                rens = settings.LimitationRange.SelectedRenderers.ToArray();
            }
            else
            {
                rens = FindObjectsOfType<MeshRenderer>();
            }

            HandleMeshRenderers(rens);
            EditorUtility.ClearProgressBar();
        }

        private void HandleMeshRenderers(MeshRenderer[] rens, bool displayProgress = true)
        {
            var i = 0;
            foreach (var mr in rens)
            {
                if (!mr)
                    continue;

                if (displayProgress)
                {
                    if (EditorUtility.DisplayCancelableProgressBar("Grabbit is loading, please wait...",
                        $"Registering {mr}",
                        (float) i / rens.Length))
                    {
                        break;
                    }
                }

                var root = PrefabUtility.IsPartOfAnyPrefab(mr)
                    ? PrefabUtility.GetOutermostPrefabInstanceRoot(mr)
                    : mr.gameObject;

                if (handlers.ContainsKey(root))
                    continue;

                RegisterHandler(root);
            }

            if (displayProgress)
                EditorUtility.ClearProgressBar();
        }

        private void RegisterAndDisableRigidBody(Rigidbody body)
        {
            //do not do anything to a body that has a mesh, its already taken care of by a handler
            if (savedDisabledRigidbodies.ContainsKey(body))
                return;

            var save = new RbSaveState();
            save.RegisterRigidBody(body);

            savedDisabledRigidbodies.Add(body, save);
            body.isKinematic = true;
        }

        private void RegisterHandler(GameObject go)
        {
            if ((settings.LayersToIgnore & (1 << go.layer)) > 0)
            {
                return;
            }

            switch (settings.TagLimitation)
            {
                case GrabbitTagLimitationMode.NONE:
                    break;
                case GrabbitTagLimitationMode.All_TAGS_EXCEPT:
                    if (!go.CompareTag(settings.tagSelected))
                        return;
                    break;
                case GrabbitTagLimitationMode.SELECTED_TAG:
                    if (go.CompareTag(settings.tagSelected))
                        return;
                    break;
            }

            var handler = go.gameObject.AddComponent<GrabbitHandler>();
            handlers.Add(go, handler);
        }

        private void ClearSelectionData()
        {
            selectionHandlers.Clear();
        }

        private void PreparePhysicsDeactivation()
        {
            foreach (var pair in handlers)
            {
                if (!pair.Value)
                    continue;
                EditorUtility.SetDirty(pair.Value.gameObject);
                pair.Value.Cleanup();
                DestroyImmediate(pair.Value);
            }

            handlers.Clear();
            selectionHandlers.Clear();

            foreach (var saveState in savedDisabledRigidbodies)
            {
                var rb = saveState.Key;

                //ignore if the rb was deleted, or if a handler already takes care of it
                if (!rb)
                    continue;

                saveState.Value.RestoreRigidBody(rb);
            }

            Physics.autoSimulation = previousAutoSim;
            Physics.defaultSolverIterations = prevSolverIteration;
            Physics.defaultSolverVelocityIterations = prevVelocityIteration;
            Physics.gravity = previousGravity;
        }

        private void DeactivateCurrentSelection()
        {
            foreach (var handler in selectionHandlers)
            {
                if (!handler)
                    continue;
                handler.enabled = false;
            }

            selectionHandlers.Clear();
        }

        private void BeforeSceneViewGUI(SceneView obj)
        {
            if (EditorTools.activeToolType != typeof(GrabbitTool) && Active)
            {
                DisableLogic();
                return;
            }

            HandleInput();
        }

        private void HandleInput()
        {
            if (Event.current.type == EventType.MouseDown)
            {
                mouseDown = true;
            }
            else if (Event.current.type == EventType.MouseUp)
            {
                if (!isToolActive)
                    isToolSelected = false;
                mouseDown = false;
            }

            if (settings.IsDynamicPauseMode)
            {
                if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Space)
                {
                    settings.Paused = false;
                    GrabbitEditor.Instance.Repaint();
                    SceneView.currentDrawingSceneView.Focus();
                    CanUndo = true;
                }
                else if (Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.Space)
                {
                    settings.Paused = true;
                    SceneView.currentDrawingSceneView.Focus();
                    GrabbitEditor.Instance.Repaint();
                    CanUndo = true;
                }
            }
            else
            {
                if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Space)
                {
                    settings.Paused = !settings.Paused;
                    SceneView.currentDrawingSceneView.Focus();
                    GrabbitEditor.Instance.Repaint();
                    CanUndo = true;
                }
            }
        }


        private void DuringSceneViewGUI(SceneView obj)
        {
            if (IsSettingUp)
                return;

            if (!settings) settings = GrabbitEditor.Instance.CurrentSettings;

            if (settings.UseLimitationZone)
                settings.LimitationRange.ShowLimitationGUI();

            if (isConfiguringLimitationZone)
            {
                ConfigureLimitationZoneGUI(obj);
                return;
            }

            GrabbitModeSceneGUI();

            RestrictionGUI();

            var prevPressed = isToolSelected;

            switch (settings.CurrentMode)
            {
                case GrabbitMode.PLACE:
                    PlacementInput();
                    break;
                case GrabbitMode.ROTATE:
                    RotateInput();
                    break;
                case GrabbitMode.ALIGN:
                    AlignInput();
                    break;
                case GrabbitMode.FALL:
                    GravityInput();
                    break;
                case GrabbitMode.POINT:
                    PointerInput();
                    break;
                case GrabbitMode.SETTINGS:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (prevPressed != isToolSelected)
            {
                CanUndo = true;
            }
        }

        private void ConfigureLimitationZoneGUI(SceneView obj)
        {
            if (!settings) settings = GrabbitEditor.Instance.CurrentSettings;

            if (!Active && settings.DidConfigureLimitationRangeAtLeastOnce)
            {
                isConfiguringLimitationZone = false;
                SceneView.duringSceneGui -= ConfigureLimitationZoneGUI;

                return;
            }


            if (!settings.LimitationRange.IsInitialized)
                settings.LimitationRange.Initialize();

            settings.LimitationRange.ConfigurationGUI();

            Handles.BeginGUI();

            string formattedName = "<b><color='white'>Limit Configuration Mode</color></b>";
            float length = formattedName.Length * 10 + 108;
            var rect = new Rect(5, 5, length, 30);
            GUI.Label(new Rect(5, 40, length, 30),
                "<color='white'><i>Select objects to grow the limitation zone!</i></color>", settings.RichCenter);
            GUI.Box(rect, new GUIContent());

            GUILayout.BeginArea(rect);
            GUILayout.BeginHorizontal();

            GUILayout.Label(settings.GrabbitLogoWhiter, new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter, fixedWidth = 30,
                fixedHeight = 28
            });
            GUILayout.Label(formattedName, settings.RichCenter);

            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Restrict To Selection"))
            {
                settings.LimitationRange.RestrictToSelection();
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Center On Camera"))
            {
                settings.LimitationRange.MoveToCameraCenter();
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            bool confirmed = GUILayout.Button("Confirm Range");

            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.EndArea();

            Handles.EndGUI();

            if (confirmed)
            {
                Event.current.Use();

                isConfiguringLimitationZone = false;

                if (!settings.DidConfigureLimitationRangeAtLeastOnce)
                {
                    settings.DidConfigureLimitationRangeAtLeastOnce = true;
                    SceneView.duringSceneGui -= ConfigureLimitationZoneGUI;
                    EnableLogic();
                }
                else
                {
                    settings.LimitationRange.SelectObjectsFromScenes();
                    HandleMeshRenderers(settings.LimitationRange.SelectedRenderers.ToArray(), false);
                }

                EditorUtility.SetDirty(settings);
            }
        }


        private void RestrictionGUI()
        {
            foreach (var gameObject in selectionLimitedObjects)
            {
                switch (IsObjectSelectionPrevented(gameObject))
                {
                    case 1:

                        Handles.Label(gameObject.transform.position,
                            "<b>Grabbit Restriction: Layer Ignored</b>", settings.ErrorStyle);
                        break;
                    case 2:
                        Handles.Label(gameObject.transform.position,
                            "<b>Grabbit Restriction: Bounds Size Too Large</b>", settings.ErrorStyle);
                        break;
                    case 3:
                        Handles.Label(gameObject.transform.position,
                            "<b>Grabbit Restriction: Tag Ignored</b>", settings.ErrorStyle);
                        break;
                }
            }
        }

        private void GrabbitModeSceneGUI()
        {
            if (!settings.showSceneInfo)
                return;

            if (Event.current.type == EventType.KeyDown || Event.current.type == EventType.KeyUp)
                return;

            Handles.BeginGUI();

            string formattedName = settings.CurrentMode.ToString();
            float length = formattedName.Length * 20 + (settings.UseLimitationZone ? 218 : 55);
            var box = new Rect(5, 5, settings.Paused ? length + 350 : length, 30);

            GUI.Box(box, new GUIContent());

            GUILayout.BeginArea(box);

            formattedName = "<b><color='white'>" + formattedName.Substring(0, 1) +
                            formattedName.Substring(1).ToLower() + " Mode" + "</color></b>";

            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();

            GUILayout.Label(settings.GrabbitLogoWhiter, new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter, fixedWidth = 30,
                fixedHeight = 28
            });

            GUILayout.Label(formattedName, settings.RichCenter);

            if (settings.Paused)
            {
                string pauseText = settings.IsDynamicPauseMode
                    ? $"<b><color='white'>: Hold Space To Use The Mode</color></b>"
                    : "<b><color='white'>: Paused, Press Space To Resume</color></b>";


                GUILayout.Label(pauseText, settings.RichCenter);
            }

            if (settings.UseLimitationZone)
            {
                GUILayout.BeginVertical();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Configure Limitation Zone"))
                {
                    isConfiguringLimitationZone = true;
                }

                GUILayout.FlexibleSpace();
                GUILayout.EndVertical();
            }

            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();

            GUILayout.EndArea();

            Handles.EndGUI();
        }

        private void PointerInput()
        {
            isToolActive = isToolSelected = !settings.Paused;

            if (Selection.gameObjects.Length == 0)
                return;

            Handles.DrawWireDisc(wantedCenter, hitNormal, 0.5f);

            var centerOnScreen = HandleUtility.WorldToGUIPoint(wantedCenter);
            var size = Handles.GetMainGameViewSize();
            size.x = -0.05f;
            centerOnScreen -= size * 0.13f;

            ShowHandleGrabbitLogo(centerOnScreen);

            /* if (settings.ControlRotation)
                 wantedRotation = Handles.RotationHandle(wantedRotation, wantedCenter);*/
        }

        private void RotateInput()
        {
            if (Selection.gameObjects.Length == 0)
                return;

            wantedCenter = CalculateCentroid();
            var centerOnScreen = HandleUtility.WorldToGUIPoint(wantedCenter);
            var size = Handles.GetMainGameViewSize();
            size.x = -0.05f;
            centerOnScreen -= size * 0.13f;


            ShowHandleGrabbitLogo(centerOnScreen);

            EditorGUI.BeginChangeCheck();

            wantedRotation = Handles.RotationHandle(wantedRotation, wantedCenter);

            SetToolFlags();
        }

        private void AlignInput()
        {
            if (Selection.gameObjects.Length == 0)
                return;

            wantedCenter = CalculateCentroid();

            var centerOnScreen = HandleUtility.WorldToGUIPoint(wantedCenter);
            var size = Handles.GetMainGameViewSize();
            size.x = -0.05f;
            centerOnScreen -= size * 0.13f;


            ShowHandleGrabbitLogo(centerOnScreen);

            EditorGUI.BeginChangeCheck();

            if (settings.ControlRotation)
                wantedRotation = Handles.RotationHandle(wantedRotation, wantedCenter);

            // if (selectedRigidBodies.Count > 1)
            pullToCenter = Handles.DoScaleHandle(pullToCenter, wantedCenter, Quaternion.identity,
                HandleUtility.GetHandleSize(wantedCenter) * 0.7f);
            pullToCenter = pullToCenter.GetClampedMagnitude(0, 2);

            SetToolFlags();

            if (!isToolActive) pullToCenter = Vector3.one;
        }

        private void SetToolFlags()
        {
            bool wasActive = isToolActive;
            isToolActive = EditorGUI.EndChangeCheck();
            if (isToolActive && mouseDown)
            {
                isToolSelected = true;
            }

            if (wasActive && !isToolActive)
                CanUndo = true;
        }

        private void GravityInput()
        {
            isToolActive = isToolSelected = !settings.Paused;

            var controlId = GUIUtility.GetControlID(42, FocusType.Passive);
            var coord = new Vector2(SceneView.lastActiveSceneView.camera.pixelWidth,
                SceneView.lastActiveSceneView.camera.pixelHeight);

            var pos = HandleUtility.GUIPointToWorldRay(coord * 0.8f).GetPoint(10);

            if (!isToolActive)
                gravityRotation =
                    Quaternion.LookRotation(settings.GravityStrength, Vector3.up);

            gravityRotation = Handles.RotationHandle(gravityRotation, pos);

            Handles.color = Color.green;

            Handles.ArrowHandleCap(controlId, pos, gravityRotation, HandleUtility.GetHandleSize(pos) * 1.1f,
                EventType.Repaint);

            var currentGrav = gravityRotation * Vector3.forward;

            var scale = Handles.ScaleSlider(settings.GravityStrength.magnitude, pos,
                currentGrav,
                Quaternion.identity, HandleUtility.GetHandleSize(pos) * 0.7f, 1f);

            Handles.Label(pos, scale.ToString("0.00"));

            settings.GravityStrength = gravityRotation * Vector3.forward * scale;
        }

        private void PlacementInput()
        {
            if (Selection.gameObjects.Length == 0)
                return;

            EditorGUI.BeginChangeCheck();
            wantedCenter = Handles.PositionHandle(wantedCenter, Quaternion.identity);

            var centerOnScreen = HandleUtility.WorldToGUIPoint(wantedCenter);
            var size = Handles.GetMainGameViewSize();
            size.x = -0.05f;
            centerOnScreen -= size * 0.13f;

            ShowHandleGrabbitLogo(centerOnScreen);

            if (settings.ControlRotation)
                wantedRotation = Handles.RotationHandle(wantedRotation, wantedCenter);

            SetToolFlags();

            if (isToolSelected)
            {
                var limit = centroid + settings.DistanceForTeleportation *
                    (wantedCenter - centroid).normalized;
                Handles.color = Color.yellow;
                Handles.SphereHandleCap(-1, centroid, Quaternion.identity, HandleUtility.GetHandleSize(centroid) * 0.2f,
                    EventType.Repaint);

                Handles.color = Color.white;
                Handles.DrawDottedLine(centroid, wantedCenter, 5f);

                if ((centroid - wantedCenter).magnitude * 2 >
                    settings.DistanceForTeleportation)
                {
                    Handles.DrawDottedLine(limit, wantedCenter, 5f);

                    Handles.color = Color.red;
                    Handles.SphereHandleCap(-1, limit, Quaternion.identity,
                        HandleUtility.GetHandleSize(centroid) * 0.2f, EventType.Repaint);
                }
            }
        }

        private void ShowHandleGrabbitLogo(Vector2 centerOnScreen)
        {
            if (settings.Paused)
            {
                Handles.Label(HandleUtility.GUIPointToWorldRay(centerOnScreen).GetPoint(50), "Paused");
            }
            else
            {
                Handles.Label(HandleUtility.GUIPointToWorldRay(centerOnScreen).GetPoint(50), SceneLogo);
            }
        }

        private Vector3 CalculateCentroid()
        {
            var center = Vector3.zero;
            foreach (var handler in selectionHandlers)
            {
                if (!handler)
                    continue;

                center += handler.Body.position;
            }

            center /= selectionHandlers.Count;
            return center;
        }

        private Quaternion CalculateAverageRotation()
        {
            var average = new Quaternion(0, 0, 0, 0);

            var amount = 0;
            foreach (var handler in selectionHandlers)
            {
                if (!handler)
                    continue;

                amount++;

                average = Quaternion.Slerp(average, handler.Body.rotation, (float) 1 / amount);
            }

            return average;
        }

        private readonly List<GameObject> toRemoveFromLimited = new List<GameObject>();

        public void AddHandlerToSelectionHandlers(GrabbitHandler handler)
        {
            if (selectionHandlers.Contains(handler))
                return;

            if (!handlers.ContainsKey(handler.gameObject))
                handlers.Add(handler.gameObject, handler);
            else
                handlers[handler.gameObject] = handler;

            selectionHandlers.Add(handler);

            newlyAdded.Add(handler);
        }

        public void RemoveHandlerFromSelection(GrabbitHandler handler)
        {
            selectionHandlers.Remove(handler);

            if (!handler) return;

            handler.ActivateBackgroundMode();
        }

        private readonly List<GrabbitHandler> newlyAdded = new List<GrabbitHandler>();

        private void ResolveSelection()
        {
            //making it more intuitive for the user
            if (Selection.gameObjects.Length > 0 && settings.CurrentMode == GrabbitMode.SETTINGS)
                settings.CurrentMode = GrabbitMode.PLACE;

            newlyAdded.Clear();
            foreach (var o in Selection.gameObjects)
            {
                if (IsObjectSelectionPrevented(o) > 0)
                {
                    if (!selectionLimitedObjects.Contains(o))
                        selectionLimitedObjects.Add(o);
                    continue;
                }

                if (!o.scene.IsValid())
                    continue;

                var handler = o.GetComponent<GrabbitHandler>();

                if (!handler)
                {
                    foreach (var rigidbody in o.GetComponentsInChildren<Rigidbody>())
                    {
                        RegisterAndDisableRigidBody(rigidbody);
                    }

                    handler = o.AddComponent<GrabbitHandler>();
                }

                CanUndo = true;
                AddHandlerToSelectionHandlers(handler);
            }


            handlersToRemove.Clear();
            toRemoveFromLimited.Clear();

            foreach (var o in selectionLimitedObjects)
            {
                if (!Selection.objects.Contains(o))
                    toRemoveFromLimited.Add(o);
            }

            foreach (var o in toRemoveFromLimited)
            {
                selectionLimitedObjects.Remove(o);
            }

            foreach (var handler in selectionHandlers)
            {
                if (!handler)
                {
                    handlersToRemove.Add(handler);
                    continue;
                }

                if (Selection.gameObjects.Contains(handler.gameObject))
                    continue;

                handlersToRemove.Add(handler);

                handler.RecordUndo();
            }

            foreach (var handler in handlersToRemove)
            {
                RemoveHandlerFromSelection(handler);
            }

            //needs to be done here to avoid colliders from parent handler to be deactivated after the children handlers try to activate them
            foreach (var handler in newlyAdded)
            {
                if (!handler.IsInSelectionMode) handler.ActivateSelectionMode(settings, colliderMeshContainer);

                foreach (var grabbitHandler in selectionHandlers)
                    if (grabbitHandler != handler)
                        grabbitHandler.NotifyHandlerNowSelected(handler);
            }

            if (selectionHandlers.Count == 0)
            {
                wantedCenter = Vector3.zero;
                wantedRotation = Quaternion.identity;
            }
            else
            {
                wantedCenter = CalculateCentroid();
                centroid = wantedCenter;
                wantedRotation = CalculateAverageRotation();
                pullToCenter = Vector3.one;

                foreach (var selectionHandler in selectionHandlers)
                {
                    selectionHandler.DistanceToCentroid = selectionHandler.Body.position - wantedCenter;
                    selectionHandler.OriginalRotation = selectionHandler.Body.rotation;
                }
            }

            ReccordAllUndos();
        }

        private int IsObjectSelectionPrevented(GameObject o)
        {
            if ((settings.LayersToIgnore & (1 << o.layer)) > 0)
            {
                return 1;
            }

            switch (settings.TagLimitation)
            {
                case GrabbitTagLimitationMode.NONE:
                    break;
                case GrabbitTagLimitationMode.All_TAGS_EXCEPT:
                    if (!o.CompareTag(settings.tagSelected))
                        return 3;
                    break;
                case GrabbitTagLimitationMode.SELECTED_TAG:
                    if (o.CompareTag(settings.tagSelected))
                        return 3;
                    break;
            }

            if (settings.LimitBoundSize)
            {
                var handler = o.GetComponent<GrabbitHandler>();
                if (!handler)
                    return -1;
                Vector3 size = handler.Bounds.size;
                if (Mathf.Max(size.x, size.y, size.z) >= settings.maxBoundAxisLength)
                {
                    return 2;
                }
            }

            return -1;
        }

        private Vector3 mousePosition;
        private Ray mouseRay;

        public override void OnToolGUI(EditorWindow window)
        {
            if (IsSettingUp)
                return;

            if (!settings)
                return;

            EditorGUI.BeginChangeCheck();

            mousePosition = Event.current.mousePosition;

            mouseRay = HandleUtility.GUIPointToWorldRay(mousePosition);

            ReccordAllUndos();

            switch (settings.CurrentMode)
            {
                case GrabbitMode.PLACE:
                    PlacementModeGUI();
                    break;
                case GrabbitMode.ROTATE:
                    RotationModeGUI();
                    break;
                case GrabbitMode.FALL:
                    GravityModeGUI();
                    break;
                case GrabbitMode.ALIGN:
                    AlignModeGUI();
                    break;
                case GrabbitMode.POINT:
                    PointModeGUI();
                    break;
                case GrabbitMode.SETTINGS:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            UpdatePhysics();
        }

        private void ReccordAllUndos()
        {
            if (CanUndo)
            {
                foreach (var handler in selectionHandlers) handler.RecordUndo();

                CanUndo = false;
            }
        }

        private void PointModeGUI()
        {
        }

        private void AlignModeGUI()
        {
        }

        private void GravityModeGUI()
        {
        }

        private void RotationModeGUI()
        {
        }

        private void PlacementModeGUI()
        {
            if (!settings.showCollisionIndicators)
                return;

            if (isToolSelected)
            {
                foreach (var handler in selectionHandlers)
                {
                    if (settings.showCollisionIndicators)
                    {
                        //temporarilly disable the colliders so that the hits are registered properly
                        handler.DisableAllColliders(settings);
                        //TODO adjust to local/ global later
                        if (handler.Body.velocity.sqrMagnitude > 0.8f && Physics.Raycast(handler.Body.position,
                            handler.Body.velocity, out var hit))
                        {
                            ShowRayHandle(Color.yellow, handler.Body.position, hit.point, hit.normal);
                        }

                        handler.EnableColliders(settings);
                    }
                }
            }
            else
            {
                foreach (var handler in selectionHandlers)
                {
                    var body = handler.Body;
                    handler.DisableAllColliders(settings);

                    //TODO adjust to local/ global later
                    if (Physics.Raycast(body.position, Vector3.down, out var hit))
                    {
                        ShowRayHandle(Color.green, body.position, hit.point, hit.normal);
                    }

                    if (Physics.Raycast(body.position, Vector3.up, out hit))
                    {
                        ShowRayHandle(Color.green, body.position, hit.point, hit.normal);
                    }

                    if (Physics.Raycast(body.position, Vector3.forward, out hit))
                    {
                        ShowRayHandle(Color.blue, body.position, hit.point, hit.normal);
                    }

                    if (Physics.Raycast(body.position, Vector3.back, out hit))
                    {
                        ShowRayHandle(Color.blue, body.position, hit.point, hit.normal);
                    }

                    if (Physics.Raycast(body.position, Vector3.right, out hit))
                    {
                        ShowRayHandle(Color.red, body.position, hit.point, hit.normal);
                    }

                    if (Physics.Raycast(body.position, Vector3.left, out hit))
                    {
                        ShowRayHandle(Color.red, body.position, hit.point, hit.normal);
                    }

                    handler.EnableColliders(settings);
                }
            }
        }

        private void UpdatePhysics()
        {
            if (IsSettingUp)
                return;

            if (!settings)
                return;

            if (settings.Paused)
                return;

            if (IsPrefabMode)
            {
                if (PrefabStageUtility.GetCurrentPrefabStage() == null)
                {
                    IsPrefabMode = false;
                    GrabbitHandler.InstantDestroyFlag = true;
                    ExitTool();
                    return;
                }
            }
            else
            {
                if (PrefabStageUtility.GetCurrentPrefabStage() != null)
                {
                    IsPrefabMode = false;
                    GrabbitHandler.InstantDestroyFlag = true;
                    ExitTool();
                    return;
                }
            }

            collisionFound = false;

            if (isToolSelected && settings.SetKinematicWhenNotActive)
            {
                foreach (var handler in selectionHandlers)
                {
                    handler.ActivateSelectionMode(settings, colliderMeshContainer);
                }
            }

            foreach (var handler in selectionHandlers)
            {
                handler.Body.constraints = settings.Constraints;
            }
            
            switch (settings.CurrentMode)
            {
                case GrabbitMode.PLACE:
                    PlacementModeLogic();
                    break;
                case GrabbitMode.ROTATE:
                    RotationModeLogic();
                    break;
                case GrabbitMode.FALL:
                    GravityLogic();
                    break;
                case GrabbitMode.ALIGN:
                    AlignLogic();
                    break;
                case GrabbitMode.POINT:
                    PointLogic();
                    break;
                case GrabbitMode.SETTINGS:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            PhysicsTick();

            if (!isToolSelected && settings.SetKinematicWhenNotActive)
            {
                foreach (var handler in selectionHandlers)
                {
                    handler.ActivateBackgroundMode();
                }
            }

            SceneView.lastActiveSceneView.Repaint();
        }

        private static void UpdateDeltaTime()
        {
            lastTimeSinceStartup = EditorApplication.timeSinceStartup;
        }


        private void PhysicsTick()
        {
            timer += EditorDeltaTime;

            var i = 0;

            Physics.autoSimulation = false;
            while (timer >= Time.fixedDeltaTime)
            {
                timer -= Time.fixedDeltaTime;

                if (IsPrefabMode)
                {
                    prefabModePhysicsScene.Simulate(Time.fixedDeltaTime * settings.Speed);
                }
                else
                {
                    Physics.Simulate(Time.fixedDeltaTime * settings.Speed);
                }

                if (i > settings.MaxPhysXIterationPerUpdate)
                {
                    timer = 0;
                    break;
                }

                i++;
            }
        }

        private void PrepareHandler(GrabbitHandler handler)
        {
            handler.ActivateSelectionMode(settings, colliderMeshContainer);
        }

        private void GravityLogic()
        {
            Physics.gravity = settings.GravityStrength;

            if (settings.SetGravityInDirectionOfMouse)
            {
                settings.GravityStrength = mouseRay.direction;
            }


            //do not update when the player clicks on something
            if (!isToolSelected)
                return;


            foreach (var handler in selectionHandlers)
            {
                if (!handler)
                    continue;

                var body = handler.Body;

                PrepareHandler(handler);
                body.useGravity = true;
                if (settings.LimitBodyVelocityInGravityMode)
                {
                    SetBodyVelocityLimits(handler);
                }
                else
                {
                    body.drag = 0;
                    body.angularDrag = 0;

                    body.maxAngularVelocity = float.MaxValue;
                    body.maxDepenetrationVelocity = float.MaxValue;
                }
            }
        }

        private void RotationModeLogic()
        {
            Physics.gravity = Vector3.zero;

            if (isToolSelected)
            {
                foreach (var handler in selectionHandlers)
                {
                    if (!handler)
                        continue;

                    var body = handler.Body;

                    body.useGravity = false;
                    PrepareHandler(handler);

                    var rotation = wantedRotation * Quaternion.Inverse(body.rotation);
                    var angularVelocity = new Vector3(
                            Mathf.DeltaAngle(0, rotation.eulerAngles.x),
                            Mathf.DeltaAngle(0, rotation.eulerAngles.y),
                            Mathf.DeltaAngle(0, rotation.eulerAngles.z))
                        * Mathf.Deg2Rad / Time.fixedDeltaTime;

                    body.angularVelocity = angularVelocity;

                    SetBodyVelocityLimits(handler);
                    body.drag += settings.ExtraDragForRotationMode;
                }
            }
            else
            {
                if (settings.PausePositionReajustment)
                    wantedRotation = CalculateAverageRotation();
                foreach (var handler in selectionHandlers)
                {
                    if (!handler)
                        return;

                    var body = handler.Body;
                    body.velocity = Vector3.zero;
                    body.angularVelocity = Vector3.zero;
                }
            }
        }

        private readonly RaycastHit[] rayHits = new RaycastHit[20];
        private readonly List<Vector3> hitPos = new List<Vector3>();
        private readonly List<Vector3> hitNormals = new List<Vector3>();
        private Vector3 hitNormal;

        private void PointLogic()
        {
            Physics.gravity = Vector3.zero;

            RaycastMouse();

            centroid = CalculateCentroid();

            var distance = wantedCenter - centroid;


            var mag = distance.magnitude;
            var magOverLimit = settings.DistanceForTeleportation <= mag;

            foreach (var handler in selectionHandlers)
            {
                if (!handler)
                    continue;

                var body = handler.Body;

                body.useGravity = false;
                PrepareHandler(handler);

                if (settings.PointModeIgnoreCollision && distance.magnitude >=
                    settings.MinDistanceToIgnoreCollision * handler.BoundMaxDimension)
                {
                    body.position = wantedCenter + hitNormal * handler.BoundMaxDimension;
                    continue;
                }

                var relativeDistance = Vector3.zero;
                if (settings.useCentroidRelativeTransform)
                {
                    relativeDistance = wantedCenter + handler.DistanceToCentroid - handler.Body.position - distance;
                    relativeDistance *= settings.PullToRelativePositionFactor;

                    var rotation = handler.OriginalRotation * Quaternion.Inverse(body.rotation);
                    var angularVelocity = new Vector3(
                            Mathf.DeltaAngle(0, rotation.eulerAngles.x),
                            Mathf.DeltaAngle(0, rotation.eulerAngles.y),
                            Mathf.DeltaAngle(0, rotation.eulerAngles.z))
                        * Mathf.Deg2Rad / Time.fixedDeltaTime;

                    body.angularVelocity = angularVelocity * settings.PullToOriginalRotationFactor;
                }


                var velocity = (distance + relativeDistance) / Time.fixedDeltaTime;

                body.velocity = velocity;

                SetBodyVelocityLimits(handler);


                //TODO take care of orientation
                if (settings.ControlRotation)
                {
                    var initialOrient = Quaternion.LookRotation(hitNormal);
                    var rotation = initialOrient * Quaternion.Inverse(body.rotation);
                    var angularVelocity = new Vector3(
                            Mathf.DeltaAngle(0, rotation.eulerAngles.x),
                            Mathf.DeltaAngle(0, rotation.eulerAngles.y),
                            Mathf.DeltaAngle(0, rotation.eulerAngles.z))
                        * Mathf.Deg2Rad / Time.fixedDeltaTime;

                    body.angularVelocity = angularVelocity;
                }
            }
        }

        private void RaycastMouse()
        {
            int length = 0;
            if (IsPrefabMode)
            {
                prefabModePhysicsScene.Raycast(mouseRay.origin, mouseRay.direction, rayHits);
                length = rayHits.Length;
            }
            else
            {
                length = Physics.RaycastNonAlloc(mouseRay, rayHits);
                if (length == rayHits.Length)
                {
                    Debug.LogWarning("Grabbit Warning: The raycast position may be compromised:" +
                                     " this is unlikely to happen, please contact us on Discord if you encounter this warning!");
                }
            }

            if (length > 0)
            {
                hitPos.Clear();
                hitNormals.Clear();
                //todo resolve normals too
                for (var index = 0; index < length; index++)
                {
                    var hit = rayHits[index];
                    bool found = false;
                    foreach (var handler in selectionHandlers)
                    {
                        if (handler.Body == hit.rigidbody)
                        {
                            found = true;
                            break;
                        }
                    }

                    if (found)
                        continue;

                    hitPos.Add(hit.point);
                    hitNormals.Add(hit.normal);
                }

                int closestId = -1;
                float closestSoFar = float.MaxValue;
                for (var i = 0; i < hitPos.Count; i++)
                {
                    var po = hitPos[i];
                    var closestDi = (mouseRay.origin - po).sqrMagnitude;
                    if (closestDi < closestSoFar)
                    {
                        closestSoFar = closestDi;
                        closestId = i;
                    }
                }

                if (closestId != -1)
                {
                    wantedCenter = hitPos[closestId];
                    hitNormal = hitNormals[closestId];
                }
            }
        }

        private void PlacementModeLogic()
        {
            Physics.gravity = Vector3.zero;

            centroid = CalculateCentroid();

            if (isToolSelected)
            {
                var distance = wantedCenter - centroid;
                var mag = distance.magnitude;
                var magOverLimit = settings.DistanceForTeleportation <= mag;

                foreach (var handler in selectionHandlers)
                {
                    if (!handler)
                        continue;

                    var body = handler.Body;

                    body.useGravity = false;
                    PrepareHandler(handler);

                    var relativeDistance = Vector3.zero;
                    if (settings.useCentroidRelativeTransform)
                    {
                        relativeDistance = wantedCenter + handler.DistanceToCentroid - handler.Body.position - distance;
                        relativeDistance *= settings.PullToRelativePositionFactor;

                        var rotation = handler.OriginalRotation * Quaternion.Inverse(body.rotation);
                        var angularVelocity = new Vector3(
                                Mathf.DeltaAngle(0, rotation.eulerAngles.x),
                                Mathf.DeltaAngle(0, rotation.eulerAngles.y),
                                Mathf.DeltaAngle(0, rotation.eulerAngles.z))
                            * Mathf.Deg2Rad / Time.fixedDeltaTime;

                        body.angularVelocity = angularVelocity * settings.PullToOriginalRotationFactor;
                    }

                    if (magOverLimit)
                    {
                        body.position += distance;
                        continue;
                    }


                    var velocity = (distance + relativeDistance) / Time.fixedDeltaTime;

                    body.velocity = velocity;

                    SetBodyVelocityLimits(handler);

                    if (settings.ControlRotation)
                    {
                        var rotation = wantedRotation * Quaternion.Inverse(body.rotation);
                        var angularVelocity = new Vector3(
                                Mathf.DeltaAngle(0, rotation.eulerAngles.x),
                                Mathf.DeltaAngle(0, rotation.eulerAngles.y),
                                Mathf.DeltaAngle(0, rotation.eulerAngles.z))
                            * Mathf.Deg2Rad / Time.fixedDeltaTime;

                        body.angularVelocity = angularVelocity;
                    }
                }
            }
            else
            {
                if (settings.PausePositionReajustment)
                {
                    wantedCenter = centroid;
                }

                foreach (var handler in selectionHandlers)
                {
                    if (!handler)
                        return;

                    var body = handler.Body;
                    body.velocity = Vector3.zero;
                    body.angularVelocity = Vector3.zero;

                    if (settings.resetCentroidRelativeTransformOnMouseUp)
                    {
                        handler.DistanceToCentroid = handler.Body.position - wantedCenter;
                        handler.OriginalRotation = handler.Body.rotation;
                    }
                }
            }
        }

        private void ShowRayHandle(Color color, Vector3 origin, Vector3 hitPoint, Vector3 hitNormal, float lineSize = 1,
            float discSize = .1f)
        {
            Handles.color = color;
            Handles.DrawDottedLine(origin, hitPoint, lineSize);
            Handles.DrawWireDisc(hitPoint, hitNormal, discSize);
        }

        private void AlignLogic()
        {
            Physics.gravity = Vector3.zero;

            centroid = CalculateCentroid();

            if (isToolSelected)
            {
                foreach (var handler in selectionHandlers)
                {
                    if (!handler)
                        continue;

                    var body = handler.Body;


                    PrepareHandler(handler);

                    body.useGravity = false;
                    Vector3 towardCenter;

                    if (selectionHandlers.Count <= 1)
                        towardCenter = Vector3.zero;
                    else
                        towardCenter = (wantedCenter - body.position).Mult(Vector3.one - pullToCenter) *
                                       (0.1f * settings.AlignStrength);

                    body.velocity = towardCenter / Time.fixedDeltaTime;

                    SetBodyVelocityLimits(handler);

                    if (settings.ControlRotation)
                    {
                        var rotation = wantedRotation * Quaternion.Inverse(body.rotation);
                        var angularVelocity = new Vector3(
                                Mathf.DeltaAngle(0, rotation.eulerAngles.x),
                                Mathf.DeltaAngle(0, rotation.eulerAngles.y),
                                Mathf.DeltaAngle(0, rotation.eulerAngles.z))
                            * Mathf.Deg2Rad / Time.fixedDeltaTime;

                        body.angularVelocity = angularVelocity;
                    }
                }
            }
            else
            {
                wantedCenter = centroid;

                foreach (var handler in selectionHandlers)
                {
                    if (!handler)
                        continue;

                    var body = handler.Body;
                    body.velocity = Vector3.zero;
                    body.angularVelocity = Vector3.zero;
                }
            }
        }

        private void SetBodyVelocityLimits(GrabbitHandler handler)
        {
            if (handler.IsCollidingWithStaticGeo)
            {
                //then On collisionStay should be called: check if it actually is
                if (!handler.CollisionStayCalls)
                {
                    handler.NumberOfFramesWithoutDifference++;
                    if (handler.NumberOfFramesWithoutDifference > GrabbitHandler.FrameDifferenceConcern)
                    {
                        handler.NotifyNoMoreCollisions(true);
                        handler.NumberOfFramesWithoutDifference = 0;
                    }
                }

                handler.CollisionStayCalls = false;
            }

            var body = handler.Body;
            body.drag = settings.Drag;
            body.angularDrag = settings.AngularDrag;

            body.maxAngularVelocity = settings.MaxAngularVelocity;
            body.maxDepenetrationVelocity = settings.MaxDepenetrationVelocity;
            var velocity = body.velocity;

            var collisionMaxVelocity = settings.CollisionMaxVelocity;

            if (settings.UseBoundDependantValues)
            {
                var percent = MathExt.Percent(settings.BoundVolumeForMinSpeed, settings.BoundVolumeForMaxSpeed,
                    handler.Volume);
                collisionMaxVelocity *= Mathf.Lerp(settings.CollisionMinBoundFactor, settings.CollisionMaxBoundFactor,
                    percent);
            }

            if (settings.useNormalFactor)
            {
                var direction = wantedCenter - centroid;
                var dotProduct = Mathf.Abs(Vector3.Dot(handler.AverageCollisionNormal, direction.normalized));
                var collisionSpeed = Mathf.Lerp(collisionMaxVelocity,
                    settings.MaxVelocity * settings.MaxNormalSpeedFactor,
                    dotProduct);

                collisionMaxVelocity = collisionSpeed;
            }

            if (settings.UseSoftCollision && handler.IsCollidingWithStaticGeo)
                collisionFound = true;

            var maxVelocity = collisionFound ? collisionMaxVelocity : settings.MaxVelocity;

            body.velocity = velocity.normalized * Mathf.Clamp(velocity.magnitude, 0f,
                maxVelocity);
            body.angularVelocity = Vector3.ClampMagnitude(body.angularVelocity, settings.MaxAngularVelocity);
        }
    }
}

#endif