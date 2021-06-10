#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Grabbit
{
    [Serializable]
    public struct UndoState
    {
        public Vector3 Position;
        public Quaternion Rotation;

        public UndoState(Rigidbody body)
        {
            var transform = body.transform;
            Position = transform.position;
            Rotation = transform.rotation;
        }

        public void RecordUndoBodyMove(Rigidbody body, bool force = false)
        {
            if (!body)
                return;

            var transform = body.transform;

            if (Position == transform.position && Rotation == transform.rotation && !force)
                return;

            var current = new UndoState {Position = transform.position, Rotation = transform.rotation};
            transform.position = Position;
            transform.rotation = Rotation;

            Undo.RegisterCompleteObjectUndo(body.transform, body.name);

            transform.position = current.Position;
            transform.rotation = current.Rotation;
            Position = transform.position;
            Rotation = transform.rotation;
            Undo.FlushUndoRecordObjects();
        }
    }

    [Serializable]
    public struct RbSaveState
    {
        public bool UseGravity;
        public bool DetectCollision;
        public bool IsKinematic;
        public float MaxAngularVelocity;
        public float MaxDepenetrationVelocity;
        public float AngularDrag;
        public float Drag;
        public CollisionDetectionMode CollisionMode;
        public int Layer;
        public RigidbodyInterpolation Interpolation;
        public RigidbodyConstraints Constraints;

        public void RegisterRigidBody(Rigidbody body)
        {
            Interpolation = body.interpolation;
            UseGravity = body.useGravity;
            IsKinematic = body.isKinematic;
            MaxAngularVelocity = body.maxAngularVelocity;
            MaxDepenetrationVelocity = body.maxDepenetrationVelocity;
            AngularDrag = body.angularDrag;
            Drag = body.drag;
            CollisionMode = body.collisionDetectionMode;
            Layer = body.gameObject.layer;
            DetectCollision = body.detectCollisions;
            Constraints = body.constraints;
        }

        public void RestoreRigidBody(Rigidbody body)
        {
            if (!body)
                return;

            body.useGravity = UseGravity;
            body.collisionDetectionMode = CollisionMode;
            body.isKinematic = IsKinematic;
            body.maxAngularVelocity = MaxAngularVelocity;
            body.maxDepenetrationVelocity = MaxDepenetrationVelocity;
            body.angularDrag = AngularDrag;
            body.drag = Drag;
            body.gameObject.layer = Layer;
            body.detectCollisions = DetectCollision;
            body.interpolation = Interpolation;
            body.constraints = Constraints;
        }
    }


    [ExecuteInEditMode]
    public class GrabbitHandler : MonoBehaviour
    {
        public static bool InstantDestroyFlag = false;
        private static GrabbitSettings lastSettings;

        [SerializeField] private bool SelectionConfigured;
        [SerializeField] private bool StaticConfigured;

        #region COLLISION PARAMS

        private readonly List<ContactPoint> contacts = new List<ContactPoint>(3);

        public bool IsCollidingWithStaticGeo;

        private readonly HashSet<Collider> collidingStaticGeo = new HashSet<Collider>();

        public Vector3 AverageCollisionNormal;

        #endregion

        #region SELECTION MODE

        public bool IsInSelectionMode;
        public Vector3 DistanceToCentroid;
        public Quaternion OriginalRotation;
        [SerializeField] private UndoState UndoState;

        #endregion

        #region DATA

        public Rigidbody Body => Data.Body;


        public GrabbitData Data;

        public Bounds Bounds => Data.bounds;
        public float Volume => Bounds.size.x * Bounds.size.y * Bounds.size.z;

        public float BoundMaxDimension => Mathf.Max(Bounds.size.x, Bounds.size.y, Bounds.size.z);

        #endregion

        private bool DestroyIfNotActive()
        {
            if (lastSettings && !lastSettings.IsGrabbitActive)
            {
                DestroyImmediate(this);
                return true;
            }

            return false;
        }

        public void OnEnable()
        {
            if (DestroyIfNotActive())
                return;

            if (!StaticConfigured)
                ConfigureStaticMode();


            ClearCollisionStatus();
        }

        public void RecordUndo()
        {
            UndoState.RecordUndoBodyMove(Body);
        }

        private void ClearCollisionStatus()
        {
            collidingStaticGeo.Clear();
            IsCollidingWithStaticGeo = false;
        }

        public void DisableAllColliders(GrabbitSettings settings)
        {
            Data.DisableColliders();
        }

        public void EnableColliders(GrabbitSettings settings)
        {
            if (settings.CanActivateExistingColliders)
            {
                Data.ActivateNonGrabbitColliders();
            }

            if (settings.CanActivateGeneratedColliders(this))
            {
                if (IsInSelectionMode)
                {
                    Data.ActivateDynamicColliders();
                }
                else
                {
                    Data.ActivateStaticColliders();
                }
            }
        }

        private void ConfigureStaticMode()
        {
            lastSettings = GrabbitEditor.Instance.CurrentSettings;

            var data = GetComponent<GrabbitData>();
            if (!data)
            {
                data = gameObject.AddComponent<GrabbitData>();
            }

            Data = data;

            IsCollidingWithStaticGeo = false;
            collidingStaticGeo.Clear();


            StaticConfigured = true;

            ActivateBackgroundMode();
        }


        public void ConfigureSelectionMode(GrabbitSettings settings,
            ColliderMeshContainer colliderMeshContainer = null)
        {
            if (!StaticConfigured)
                ConfigureStaticMode();

            Data.PrepareDynamic();

            Data.SetBodiesAsStatic();

            UndoState = new UndoState(Body);

            SelectionConfigured = true;
        }


        public void ActivateBackgroundMode()
        {
            if (!StaticConfigured)
                ConfigureStaticMode();
            else if (!IsInSelectionMode)
                return;

            Data.SetToBackgroundLayer();

            if (GrabbitEditor.Instance.CurrentSettings.CanActivateExistingColliders)
            {
                Data.ActivateNonGrabbitColliders();
            }
            else
            {
                Data.DeActivateNonGrabbitColliders();
            }

            if (SelectionConfigured)
            {
                Data.DeActivateDynamicColliders();
            }

            if (GrabbitEditor.Instance.CurrentSettings.CanActivateGeneratedColliders(this))
            {
                Data.ActivateStaticColliders();
            }
            else
            {
                Data.DeActivateStaticColliders();
            }

            Data.DestroyAllAddedJoints();

            Data.SetBodiesAsStatic();

            AverageCollisionNormal = Vector3.zero;
            IsCollidingWithStaticGeo = false;
            collidingStaticGeo.Clear();
            IsInSelectionMode = false;
            enabled = false;
        }

        public void ActivateSelectionMode(GrabbitSettings settings, ColliderMeshContainer colliderMeshContainer = null)
        {
            if (IsInSelectionMode)
                return;

            lastSettings = settings;


            //raises the overall perfs
            InternalEditorUtility.SetIsInspectorExpanded(this, true);
            InternalEditorUtility.SetIsInspectorExpanded(transform, false);
            InternalEditorUtility.SetIsInspectorExpanded(Body, false);

            if (!SelectionConfigured)
                ConfigureSelectionMode(settings, colliderMeshContainer);

            IsInSelectionMode = true;

            Data.SetToSelectionLayer();

            if (settings.CanActivateExistingColliders)
            {
                Data.ActivateNonGrabbitColliders();
            }
            else
            {
                Data.DeActivateNonGrabbitColliders();
            }

            Data.DeActivateStaticColliders();

            if (settings.CanActivateGeneratedColliders(this))
            {
                Data.ActivateDynamicColliders();
            }
            else
            {
                Data.DeActivateDynamicColliders();
            }

            Data.SetBodiesAsDynamic();

            UndoState.RecordUndoBodyMove(Body);
            enabled = true;
            IsInSelectionMode = true;
        }

        public void OnDestroy()
        {
            EditorUtility.SetDirty(gameObject);
            if (InstantDestroyFlag)
                Cleanup();
            else
                EditorApplication.delayCall += Cleanup;
        }

        public void Cleanup()
        {
            EditorApplication.delayCall -= Cleanup;

            DestroyImmediate(Data);
        }

        public void NotifyHandlerNowSelected(GrabbitHandler otherHandler)
        {
            otherHandler.Data.RemoveCollidersFromSet(collidingStaticGeo);

            /* foreach (var otherCollider in otherHandler.DynamicColliders)
                 collidingStaticGeo.Remove(otherCollider);
 
             foreach (var staticCollider in otherHandler.StaticColliders)
                 collidingStaticGeo.Remove(staticCollider);*/

            if (collidingStaticGeo.Count == 0)
            {
                IsCollidingWithStaticGeo = false;
                AverageCollisionNormal = Vector3.zero;
                contacts.Clear();
            }
        }

//stupid trick because of collision exit bug

//giving it a bunch of frames for safety
        public static int FrameDifferenceConcern = 5;
        public int NumberOfFramesWithoutDifference;
        public bool CollisionStayCalls;

        public void OnCollisionEnter(Collision other)
        {
            CollisionStayCalls = false;
            NumberOfFramesWithoutDifference = 0;

            //TODO make it that when there is no collision, the object just teleport directly back to where they should be
            if (!lastSettings.UseSoftCollision || !enabled)
                return;

            var handler = other.gameObject.GetComponent<GrabbitHandler>();
            if (handler && !handler.IsInSelectionMode && !collidingStaticGeo.Contains(other.collider))
            {
                IsCollidingWithStaticGeo = true;
                collidingStaticGeo.Add(other.collider);
            }
        }

        public void OnCollisionStay(Collision other)
        {
            if (!enabled || !collidingStaticGeo.Contains(other.collider))
                return;

            AverageCollisionNormal = Vector3.zero;
            contacts.Clear();
            other.GetContacts(contacts);

            foreach (var contact in contacts) AverageCollisionNormal += contact.normal;

            AverageCollisionNormal.Normalize();
            CollisionStayCalls = true;
        }

        public void OnCollisionExit(Collision other)
        {
            if (!enabled || !lastSettings.UseSoftCollision)
            {
                return;
            }

            collidingStaticGeo.Remove(other.collider);

            if (collidingStaticGeo.Count == 0)
            {
                NotifyNoMoreCollisions();
            }
        }

        public void NotifyNoMoreCollisions(bool alsoClear = false)
        {
            if (alsoClear)
                collidingStaticGeo.Clear();
            IsCollidingWithStaticGeo = false;
            AverageCollisionNormal = Vector3.zero;
            contacts.Clear();
        }

        public void OnDrawGizmosSelected()
        {
            if (lastSettings && lastSettings.debugCollisionDirection)
            {
                var position = transform.position;
                Gizmos.DrawLine(position, position + AverageCollisionNormal);
                Gizmos.color = Color.yellow;
                foreach (var contact in contacts) Gizmos.DrawLine(contact.point, contact.point + contact.normal);
            }
        }
    }
}
#endif