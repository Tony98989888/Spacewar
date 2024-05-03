using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using VSX.Pooling;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Unity event for running functions when a projectile is launched by a projectile launcher
    /// </summary>
    [System.Serializable]
    public class OnProjectileLauncherProjectileLaunchedEventHandler : UnityEvent<Projectile> { }
    
    /// <summary>
    /// This class spawns a projectile prefab at a specified interval and with a specified launch velocity.
    /// </summary>
    public class ProjectileWeaponUnit : WeaponUnit, IRootTransformUser
    {

        [Header("Settings")]

        [SerializeField]
        protected Transform spawnPoint;

        public override void Aim(Vector3 aimPosition) 
        { 
            if (aimAssistEnabled) spawnPoint.LookAt(aimPosition, transform.up);
        }
        public override void ClearAim() { spawnPoint.localRotation = Quaternion.identity; }

        [SerializeField]
        protected Projectile projectilePrefab;
        public Projectile ProjectilePrefab 
        {
            get { return projectilePrefab; }
            set { projectilePrefab = value; } 
        }

        [SerializeField]
        protected bool usePoolManager;

        [SerializeField]
        protected bool addLauncherVelocityToProjectile;

        [Tooltip("Additional velocity to add to the projectile (relative to the launcher). Can be used e.g. to make a missile move out to the side to prevent exhaust blocking the view.")]
        [SerializeField]
        protected Vector3 projectileRelativeImpulseVelocity = Vector3.zero;

        [SerializeField]
        protected float maxInaccuracyAngle = 2;
        public float MaxInaccuracyAngle
        {
            get { return maxInaccuracyAngle; }
            set { maxInaccuracyAngle = value; }
        }

        [Range(0,1)]
        [SerializeField]
        protected float accuracy = 1;
        public float Accuracy
        {
            get { return accuracy; }
            set { accuracy = value; }
        }

        [Header("Events")]

        // Projectile launched event
        public OnProjectileLauncherProjectileLaunchedEventHandler onProjectileLaunched;

        protected Transform rootTransform;
        public Transform RootTransform
        {
            set
            {
                rootTransform = value;

                if (rootTransform != null)
                {
                    rBody = rootTransform.GetComponent<Rigidbody>();
                }
                else
                {
                    rBody = null;
                }
            }
        }

        protected Rigidbody rBody;

        public override float Speed
        {
            get { return projectilePrefab != null ? projectilePrefab.Speed : 0; }
        }

        public override float Range
        {
            get { return projectilePrefab != null ? projectilePrefab.Range : 0; }
        }

        public override float Damage(HealthType healthType)
        {
            if (projectilePrefab != null)
            {
                return projectilePrefab.Damage(healthType);
            }
            else
            {
                return 0;
            }
        }


        protected override void Reset()
        {
            base.Reset();

            spawnPoint = transform;

            Projectile defaultProjectilePrefab = Resources.Load<Projectile>("SCK/Projectile");
            if (defaultProjectilePrefab != null)
            {
                projectilePrefab = defaultProjectilePrefab;
            }
        }


        protected virtual void Awake()
        {

            if (rootTransform == null) rootTransform = transform.root;

            if (rootTransform != null)
            {
                rBody = rootTransform.GetComponent<Rigidbody>();
            }
        }

        protected virtual void Start()
        {
            if (usePoolManager && PoolManager.Instance == null)
            {
                usePoolManager = false;
                Debug.LogWarning("No PoolManager component found in scene, please add one to pool projectiles.");
            }
        }


        // Launch a projectile
        public override void TriggerOnce()
        {
          
            if (projectilePrefab != null)
            {
                float nextMaxInaccuracyAngle = maxInaccuracyAngle * (1 - accuracy);
                spawnPoint.Rotate(new Vector3(Random.Range(-nextMaxInaccuracyAngle, nextMaxInaccuracyAngle),
                                                Random.Range(-nextMaxInaccuracyAngle, nextMaxInaccuracyAngle),
                                                Random.Range(-nextMaxInaccuracyAngle, nextMaxInaccuracyAngle)));

                // Get/instantiate the projectile
                Projectile projectileController;

                if (usePoolManager)
                {
                    projectileController = PoolManager.Instance.Get(projectilePrefab.gameObject, spawnPoint.position, spawnPoint.rotation).GetComponent<Projectile>();
                }
                else
                {
                    projectileController = GameObject.Instantiate(projectilePrefab, spawnPoint.position, spawnPoint.rotation);
                }

                projectileController.SetSenderRootTransform(rootTransform);

                if (addLauncherVelocityToProjectile && rBody != null)
                {
                    projectileController.AddVelocity(rBody.velocity);
                    projectileController.AddVelocity(transform.TransformDirection(projectileRelativeImpulseVelocity));
                }

                // Call the event
                onProjectileLaunched.Invoke(projectileController);
            }

            ClearAim();
        }
    }
}