#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Grabbit
{
    public enum GrabbitMode
    {
        PLACE,
        ROTATE,
        ALIGN,
        FALL,
        POINT,
        SETTINGS
    }

    public enum GrabbitTagLimitationMode
    {
        NONE,
        All_TAGS_EXCEPT,
        SELECTED_TAG
    }

    public enum ColliderBakingMode
    {
        ON_SCENE_OPEN,
        ON_SELECTION
    }

    public enum ColliderGenerationMode
    {
        USE_EXISTING_ONLY,
        USE_GENERATED_WHEN_NO_EXISTING,
        USE_GENERATED_AND_IGNORE_EXISTING,
        USE_BOTH_GENERATED_AND_EXISTING,
    }

    [CreateAssetMenu(fileName = "Grabbit Settings", menuName = "Tools/Grabbit/Create New Settings", order = 1)]
    public class GrabbitSettings : ScriptableObject
    {
        public static string GrabbitVersion = "2021.0.4";
       
       
       
        #region UI STYLE

        public GUIStyle Rich;
        public GUIStyle RichParagraph;
        public GUIStyle RichCenter;

        public GUIStyle RichTitle;

        public GUIStyle BigButton;
        public GUIStyle RegularButton;

        public GUIStyle NumberMarginStyle;

        public GUIStyle ToggleMarginStyle;

        public GUIStyle MarginStyle;

        public GUIStyle FontMarginStyleBig;

        public GUIStyle EnumMarginStyle;

        public GUIStyle LabelMarginStyle;

        public GUIStyle TabStyle;
        public GUIStyle ErrorStyle;
        public GUIStyle LayerStyle;

        public void InitializeStyle(bool force = false)
        {
            if (!force && IsStyleInitialized)
                return;

            Rich = new GUIStyle(GUI.skin.label) {richText = true};
            RichParagraph = new GUIStyle(GUI.skin.label) {richText = true, wordWrap = true, fontSize = 15};
            RichTitle = new GUIStyle(GUI.skin.label) {richText = true, fontSize = 40};

            RichCenter = new GUIStyle(GUI.skin.label) {richText = true, fontSize = 20};

            NumberMarginStyle = new GUIStyle(EditorStyles.numberField)
            {
                margin = new RectOffset(20, 10, 0, 0),
                richText = true
            };

            ToggleMarginStyle = new GUIStyle(EditorStyles.toggle)
            {
                margin = new RectOffset(20, 10, 0, 0),
                richText = true
            };

            MarginStyle = new GUIStyle
            {
                margin = new RectOffset(20, 10, 0, 0),
                richText = true
            };


            FontMarginStyleBig = new GUIStyle
            {
                margin = new RectOffset(20, 10, 0, 0),
                richText = true,
                fontSize = 50
            };


            EnumMarginStyle = new GUIStyle
            {
                margin = new RectOffset(20, 10, 0, 0)
            };

            LabelMarginStyle = new GUIStyle(GUI.skin.label)
            {
                margin = new RectOffset(20, 10, 0, 0),
                richText = true
            };

            TabStyle = new GUIStyle(GUI.skin.window)
            {
                margin = new RectOffset(0, 0, 0, 0),
                padding = new RectOffset(0, 0, 10, 0),
                richText = true
            };


            ErrorStyle = new GUIStyle(GUI.skin.textArea)
            {
                richText = true,
                fontSize = 10,
            };

            BigButton = new GUIStyle(GUI.skin.button)
            {
                margin = new RectOffset(15, 15, 15, 15), padding = new RectOffset(5, 5, 5, 5), fontSize = 30
            };

            RegularButton = new GUIStyle(GUI.skin.button)
            {
                margin = new RectOffset(15, 15, 15, 15), padding = new RectOffset(5, 5, 5, 5), fontSize = 15
            };

            IsStyleInitialized = true;
        }

        #endregion

        #region STATUS

        public bool ShowStartupWindow = true;
        public string LastVersion = "0.0";

        [NonSerialized] public bool IsGrabbitActive = false;
        [NonSerialized] public bool Paused = false;
        [NonSerialized] public bool IsDynamicPauseMode = false;

        [NonSerialized] public GrabbitMode CurrentMode = GrabbitMode.PLACE;

        public GrabbitRange LimitationRange;
        public bool DidConfigureLimitationRangeAtLeastOnce = false;

        public bool IsCurrentModeDynamicPause => IsModePausedOnStart(CurrentMode);

        public static bool IsModePausedOnStart(GrabbitMode mode)
        {
            return PausedOnStartModes.Contains(mode);
        }

        [NonSerialized] private bool IsStyleInitialized = false;

        #endregion

        #region GENERAL

        public static readonly GrabbitMode[] PausedOnStartModes = {GrabbitMode.FALL, GrabbitMode.POINT};

        public bool UseAdvancedOption = false;

        public bool UseHotKeyInPlayMode = false;

        public ColliderGenerationMode ColliderGenerationMode = ColliderGenerationMode.USE_GENERATED_WHEN_NO_EXISTING;

        public bool CanActivateExistingColliders =>
            ColliderGenerationMode != ColliderGenerationMode.USE_GENERATED_AND_IGNORE_EXISTING;

        public bool CanActivateGeneratedColliders(GrabbitHandler handler)
        {
            return (ColliderGenerationMode != ColliderGenerationMode.USE_EXISTING_ONLY &&
                    ColliderGenerationMode != ColliderGenerationMode.USE_GENERATED_WHEN_NO_EXISTING) || (
                ColliderGenerationMode == ColliderGenerationMode.USE_GENERATED_WHEN_NO_EXISTING &&
                handler.Data.NonGrabbitColliderCount == 0);
        }

        /* public bool UsePredefinedColliders = false;
        public bool UseOnlyPredefinedColliders = false;
        public bool AddCollidersOnlyWhenNoPredefinedOnesExist = false;*/

        public float Speed = 1;

        public int LayersToIgnore = 0;

        public bool ForceLayer = false;
        public int LayerToSwitchTo = 0;

        public bool UseDynamicNonConvexColliders;
        public ColliderBakingMode ColliderBakingMode = ColliderBakingMode.ON_SCENE_OPEN;
        public int MaxMeshCollidersCreated = 5;
        public int ColliderResolution = 100;

        public int velocityIterations = 1;
        public int solverIterations = 1;
        public bool useLowQualityConvexCollidersOnSelection;

        public bool UseLimitationZone;

        public RigidbodyConstraints Constraints;

        public Texture2D JungleLogo;

        public Texture2D GetStartedArrow;
        public Texture2D Question;
        public Texture2D Heart;
        public Texture2D Star;
        public Texture2D Exclamation;
        public Texture2D MonKey;
        public Texture2D Parrot;

        public Texture2D GrabbitLogoBlack;
        public Texture2D GrabbitLogoWhite;
        public Texture2D GrabbitLogoWhiter;

        public Texture2D GrabbitLogo => EditorGUIUtility.isProSkin ? GrabbitLogoWhite : GrabbitLogoBlack;
        public Texture2D GrabbitSelectedLogo => EditorGUIUtility.isProSkin ? GrabbitLogoWhiter : GrabbitLogoWhite;

        public Texture2D GrabbitBigLogoWhite;
        public Texture2D GrabbitBigLogoBlack;

        public Texture2D GrabbitBigLogo => EditorGUIUtility.isProSkin ? GrabbitBigLogoWhite : GrabbitBigLogoBlack;

        public bool HideGrabbitTextLogo;
        public Texture2D GrabbitTextLogo;

        public Texture2D GrabbitFaceBigWhite;
        public Texture2D GrabbitFaceBigBlack;

        public Texture2D GrabbitFaceBig => EditorGUIUtility.isProSkin ? GrabbitFaceBigWhite : GrabbitFaceBigBlack;

        public Texture2D GrabbitSceneLogo;

        public bool showSceneInfo = true;

        public bool showCollisionIndicators = true;

        public bool SetKinematicWhenNotActive = true;

        public bool UseSoftCollision = true;
        public bool DisplayWarning = true;

        public bool useNormalFactor = true;
        public float MaxNormalSpeedFactor = 0.5f;

        public bool debugCollisionDirection = true;

        public int MaxPhysXIterationPerUpdate = 2;

        public bool LimitBoundSize = true;
        public float maxBoundAxisLength = 0;

        public GrabbitTagLimitationMode TagLimitation;
        public string tagSelected;

        #endregion
        
        #region LAYERING

        public bool ChangeLayerOfDynamicObjects;
        public bool ChangeLayerOfStaticObjects;

        public int DynamicObjectsLayer = 0;
        public int StaticObjectsLayer = 0;
        
        #endregion

        #region ALIGN

        public float AlignStrength = 1f;

        #endregion

        #region ROTATION

        public float ExtraDragForRotationMode = 100;

        #endregion

        #region GRAVITY

        public Vector3 GravityStrength = new Vector3(0, -9.81f, 0);
        public bool LimitBodyVelocityInGravityMode;

        public bool SetGravityInDirectionOfMouse = false;

        #endregion

        #region PLACEMENT

        public bool PausePositionReajustment = true;
        public bool ControlRotation;

        public float MaxVelocity = 20;
        public float CollisionMaxVelocity = 0.1f;
        public float MaxAngularVelocity = 20;
        public float MaxDepenetrationVelocity;

        public bool UseBoundDependantValues = true;

        public float CollisionMinBoundFactor = 0.1f;
        public float CollisionMaxBoundFactor = 1;

        public float BoundVolumeForMinSpeed = 0.01f;
        public float BoundVolumeForMaxSpeed = 2;

        public float DistanceForTeleportation = 5;

        public float AngularDrag = 10;
        public float Drag = 10;

        public bool useCentroidRelativeTransform = true;
        public bool resetCentroidRelativeTransformOnMouseUp = true;
        public float PullToOriginalRotationFactor = 1;
        public float PullToRelativePositionFactor = 1;

        #endregion

        #region POINT

        public bool PointModeIgnoreCollision;
        public float MinDistanceToIgnoreCollision = 1f;

        #endregion
    }
}

#endif