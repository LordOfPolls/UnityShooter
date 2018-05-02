using UnityEngine;
using UnitySampleAssets.CrossPlatformInput;
using System.Collections;

namespace CompleteProject
{
    public class PlayerShooting : MonoBehaviour
    {
        public int damagePerShot = 20;
        int shootableMask;
        int weapon = 0;

        public float weapon1FireRate = 0.15f;
        public float weapon2FireRate = 0.5f;
        public float weapon3FireRate = 1f;
        public float weapon1Range = 100f;
        public float weapon2Range = 6f;
        public float explosionForce = 5500;
        public float effectsDisplayTime = 0.2f;
        public float accuracy = 1f;
        public float throwforce = 40f;
        float weapon1Timer;
        float weapon2Timer;
        float weapon3Timer;
        float grenadeTimer;

        bool hasExploded = false;
        bool exploding = false;

        Ray shootRay = new Ray();
        RaycastHit shootHit;

        Light gunLight;
        public Light faceLight;

        public GameObject ExplossionEffect;
        public GameObject fire;
        public GameObject plasma;
        public GameObject sparks;
        public GameObject LineRendererObject1;
        public GameObject LineRendererObject2;

        LineRenderer gunLine;
        LineRenderer LineRenderer1;
        LineRenderer LineRenderer2;

        ParticleSystem gunParticles;


        AudioSource gunAudio;
        public AudioSource Explossion;
        public AudioSource notready;


        void Awake()
        {
            // Create a layer mask for the Shootable layer.
            shootableMask = LayerMask.GetMask("Shootable");

            // Set up the references.
            LineRenderer1 = LineRendererObject1.GetComponent<LineRenderer>();
            LineRenderer2 = LineRendererObject2.GetComponent<LineRenderer>();


            gunParticles = GetComponent<ParticleSystem>();
            gunLine = GetComponent<LineRenderer>();
            gunAudio = GetComponent<AudioSource>();
            gunLight = GetComponent<Light>();
            faceLight = GetComponentInChildren<Light>();
        }


        void Update()
        {

            weapon1Timer += Time.deltaTime;
            weapon2Timer += Time.deltaTime;
            weapon3Timer += Time.deltaTime;
            if (hasExploded)
                grenadeTimer += Time.deltaTime;
            if (grenadeTimer > 10f)
            {
                hasExploded = false;
                grenadeTimer = 0f;
            }

            if (Input.GetKeyDown("tab"))
            {
                weapon += 1;
                if (weapon >= 3)
                    weapon = 0;
                return;
            }

            if (Input.GetButton("Fire1"))
            {
                if (weapon == 0 && weapon1Timer >= weapon1FireRate && Time.timeScale != 0)
                    weapon1Shoot();
                else if (weapon == 1 && weapon2Timer >= weapon2FireRate && Time.timeScale != 0)
                    weapon2Shoot();
                else if (weapon == 2 && weapon3Timer >= weapon3FireRate && Time.timeScale != 0)
                    weapon3Shoot();
            }

            if (Input.GetButton("Fire2") && hasExploded == false)
            {
                explosion();
                exploding = false; //sometimes this doesnt get set and i have no idea why
            }
            else if (Input.GetButton("Fire2") && hasExploded == true && exploding == false && grenadeTimer > 1f && notready.isPlaying == false)
            {
                notready.Play();
            }
        }

        public void DisableEffects()
        {
            gunLine.enabled = false;
            faceLight.GetComponent<Light>().enabled = false;
            gunLight.enabled = false;
            LineRenderer1.enabled = false;
            LineRenderer2.enabled = false;
        }

        public void EnableEffects()
        {
            gunLight.enabled = true;
            faceLight.GetComponent<Light>().enabled = true;
            gunParticles.Stop();
            gunParticles.Play();
            gunLine.enabled = true;
            
        }

        void explosion()
        {
            exploding = true;
            hasExploded = true;
            Explossion.Play();
            var effect = Instantiate(ExplossionEffect, transform.position, transform.rotation);
            Destroy(effect, 7);
            Collider[] colliders = Physics.OverlapSphere(transform.position, 10f);
            foreach (Collider nearbyObject in colliders)
            {
                Rigidbody rb = nearbyObject.GetComponent<Rigidbody>();
                if (rb != null && nearbyObject.tag != "Player" && nearbyObject.tag != "playercomp")
                {
                    rb.drag = 10;
                    rb.angularDrag = 10;
                    rb.isKinematic = false;
                    rb.AddExplosionForce(explosionForce, transform.position, 10f);
                }
                if (nearbyObject != gameObject && nearbyObject.tag != "Player" && nearbyObject.tag != "playercomp")
                {
                    if (nearbyObject.tag != "robot")
                    {
                        GameObject fires = Instantiate(fire, nearbyObject.transform.position, nearbyObject.transform.rotation);
                        Destroy(fires, 7);
                    }
                    else
                    {
                        GameObject boom = Instantiate(plasma, nearbyObject.transform.position, nearbyObject.transform.rotation);
                        GameObject spark = Instantiate(sparks, nearbyObject.transform.position, nearbyObject.transform.rotation);
                        Destroy(spark, 7);
                        Destroy(boom, 7);
                    }
                    var EnemyHealth = nearbyObject.GetComponent<EnemyHealth>();
                    if (EnemyHealth != null)
                    {
                        EnemyHealth.TakeDamage(Random.Range(72, 180));
                    }

                }
            }
            exploding = false;
        }

        void weapon1Shoot()
        {
            weapon1Timer = 0f;
            gunAudio.Play();

            // Enable the lights and particles
            gunLight.enabled = true;
            faceLight.GetComponent<Light>().enabled = true;
            gunParticles.Stop();
            gunParticles.Play();
            gunLine.enabled = true;
            gunLine.SetPosition(0, transform.position);

            // Set the shootRay so that it starts at the end of the gun and points forward from the barrel.
            shootRay.origin = transform.position;

            // Accuracy code, so the shots arent always perfect
            float randomOffset_x = UnityEngine.Random.Range(-(1 - accuracy), 1 - accuracy);
            Vector3 direction = transform.forward;
            direction.x += randomOffset_x;
            shootRay.direction = direction; // decides what direction the "bullet goes"


            // Perform the raycast against gameobjects on the shootable layer and if it hits something...
            if (Physics.Raycast(shootRay, out shootHit, weapon1Range, shootableMask))
            {
                // Try and find an EnemyHealth script on the gameobject hit.
                EnemyHealth enemyHealth = shootHit.collider.GetComponent<EnemyHealth>();

                // If the EnemyHealth component exist...
                if (enemyHealth != null)
                {
                    // ... the enemy should take damage.
                    enemyHealth.TakeDamage(damagePerShot, shootHit.point);
                }

                // Set the second position of the line renderer to the point the raycast hit.
                gunLine.SetPosition(1, shootHit.point);
            }
            // If the raycast didn't hit anything on the shootable layer...
            else
            {
                // ... set the second position of the line renderer to the fullest extent of the gun's range.
                gunLine.SetPosition(1, shootRay.origin + shootRay.direction * weapon1Range);
            }
            StartCoroutine(killEffects());
        }

        void weapon2Shoot()
        {
            /*
                Shotgun weapon
            */
            weapon2Timer = 0f;
            gunAudio.Play();
            // Enable the lights and effects
            EnableEffects();
            LineRenderer1.enabled = true;
            LineRenderer2.enabled = true;
            for (int i = 0; i < 3; i++) // Fire 3 shots
            {
                shootRay.origin = transform.position; //Where is the gun right now?
                float randomOffset_x = UnityEngine.Random.Range(-(1 - accuracy), 1 - 0.5f); //Impliment some spray to the weapon
                Vector3 direction = transform.forward;
                direction.x += randomOffset_x; // add the offset generated above
                shootRay.direction = direction;
                if (Physics.Raycast(shootRay, out shootHit, 20, shootableMask))
                {
                    EnemyHealth enemyHealth = shootHit.collider.GetComponent<EnemyHealth>();

                    if (enemyHealth != null)
                    {
                        enemyHealth.TakeDamage(Random.Range(70, 100), shootHit.point);
                    }
                    switch (i)
                    {
                        case 0:
                            gunLine.SetPosition(0, shootRay.origin);
                            gunLine.SetPosition(1, shootHit.point);
                            break;
                        case 1:
                            LineRenderer1.SetPosition(0, shootRay.origin);
                            LineRenderer1.SetPosition(1, shootHit.point);
                            break;
                        case 2:
                            LineRenderer2.SetPosition(0, shootRay.origin);
                            LineRenderer2.SetPosition(1, shootHit.point);
                            break;
                    }
                }
                else
                {
                    switch (i)
                    {
                        case 0:
                            gunLine.SetPosition(1, shootRay.origin + shootRay.direction * weapon2Range);
                            break;
                        case 1:
                            LineRenderer1.SetPosition(0, shootRay.origin);
                            LineRenderer1.SetPosition(1, shootRay.origin + shootRay.direction * weapon2Range);
                            break;
                        case 2:
                            LineRenderer2.SetPosition(0, shootRay.origin);
                            LineRenderer2.SetPosition(1, shootRay.origin + shootRay.direction * weapon2Range);
                            break;
                    }

                }

            }
            StartCoroutine(killEffects());


        }

        void weapon3Shoot()
        {
            weapon3Timer = 0f;
            gunAudio.Play();
            gunLight.enabled = true;
            faceLight.GetComponent<Light>().enabled = true;
            gunParticles.Stop();
            gunParticles.Play();
            gunLine.enabled = true;
            gunLine.SetPosition(0, transform.position);
            shootRay.origin = transform.position;
            shootRay.direction = transform.forward;


            if (Physics.Raycast(shootRay, out shootHit, weapon1Range, shootableMask))
            {
                EnemyHealth enemyHealth = shootHit.collider.GetComponent<EnemyHealth>();

                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(100, shootHit.point);
                }

                gunLine.SetPosition(1, shootHit.point);
            }
            else
            {
                gunLine.SetPosition(1, shootRay.origin + shootRay.direction * weapon1Range);
            }
            StartCoroutine(killEffects());
        }

        IEnumerator killEffects()
        {
            yield return new WaitForSeconds(0.1f);
            gunLine.enabled = false;
            faceLight.GetComponent<Light>().enabled = false;
            gunLight.enabled = false;
            LineRenderer1.enabled = false;
            LineRenderer2.enabled = false;
        }

        IEnumerator sleep()
        {
            yield return new WaitForSeconds(0.6f);
        }

    }
}
