using UnityEngine;
using UnitySampleAssets.CrossPlatformInput;

namespace CompleteProject
{
    public class PlayerShooting : MonoBehaviour
    {
        public int damagePerShot = 20;                  
        public float timeBetweenBullets = 0.15f;        
        public float range = 100f;                      
        public float shotgunRange = 20f;

        bool exploding = false;

        public GameObject ExplossionEffect;
        public GameObject fire;
        public GameObject plasma;
        public GameObject sparks;
        bool hasExploded = false;
        public float explosionForce = 5500;
        float timer;                                    
        float grenadeTimer;
        public float accuracy = 1f;


        Ray shootRay = new Ray();                      
        RaycastHit shootHit;                            
        int shootableMask;                              
        ParticleSystem gunParticles;                    
        LineRenderer gunLine;                           
        AudioSource gunAudio;                           
        public AudioSource Explossion;
        public AudioSource notready;
        Light gunLight;                                
        public Light faceLight;							
        float effectsDisplayTime = 0.2f;                


        void Awake()
        {
            // Create a layer mask for the Shootable layer.
            shootableMask = LayerMask.GetMask("Shootable");

            // Set up the references.
            gunParticles = GetComponent<ParticleSystem>();
            gunLine = GetComponent<LineRenderer>();
            gunAudio = GetComponent<AudioSource>();
            gunLight = GetComponent<Light>();
            faceLight = GetComponentInChildren<Light>();
        }


        void Update()
        {
            
            timer += Time.deltaTime;
            if (hasExploded)
                grenadeTimer += Time.deltaTime;
            if (grenadeTimer > 10f)
            {
                hasExploded = false;
                grenadeTimer = 0f;
            }

            if (Input.GetButtonDown("tab"))
            {
                //switch weapon//
                return;
            }
            if (Input.GetButton("Fire1") && timer >= timeBetweenBullets && Time.timeScale != 0)
            {
                defaultShoot();
            }
            if (Input.GetButton("Fire2") && hasExploded == false){
                bomb();
                exploding = false; //sometimes this doesnt get set and i have no idea why
            } else if (Input.GetButton("Fire2") && hasExploded == true && exploding == false && grenadeTimer > 1f && notready.isPlaying == false)
            {
                notready.Play();
            }

            if (timer >= timeBetweenBullets * effectsDisplayTime)
            {
                DisableEffects();
            }
        }
        
            
        


        public void DisableEffects()
        {
            // Disable the line renderer and the light.
            gunLine.enabled = false;
            faceLight.GetComponent<Light>().enabled = false;
            gunLight.enabled = false;
        }

        void bomb()
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
                    }else
                    {
                        GameObject boom = Instantiate(plasma, nearbyObject.transform.position, nearbyObject.transform.rotation);
                        GameObject spark = Instantiate(sparks, nearbyObject.transform.position, nearbyObject.transform.rotation);
                        Destroy(spark, 7);
                        Destroy(boom, 7);
                    }
                    var EnemyHealth = nearbyObject.GetComponent<EnemyHealth>();
                    if (EnemyHealth != null)
                    {
                        EnemyHealth.TakeDamage(Random.Range(50, 100));
                    }
                    
                }
            }
            exploding = false;
        }

        void shotgunShot()
        {
            //wo there sunny jim, this function aint done. calm your horses m8
            timer = 0f;
            gunAudio.Play();

            // Enable the lights and particles
            gunLight.enabled = true;
            faceLight.GetComponent<Light>().enabled = true;
            gunParticles.Stop();
            gunParticles.Play();
            gunLine.enabled = true;
            gunLine.SetPosition(0, transform.position);            

            for (int i = 0; i < 3; i++)
            {
                shootRay.origin = transform.position;


                float randomOffset_x = UnityEngine.Random.Range(-(1 - accuracy), 1 - 0.7f);
                Vector3 direction = transform.forward;
                direction.x += randomOffset_x;
                shootRay.direction = direction;


                if (Physics.Raycast(shootRay, out shootHit, range, shootableMask))
                {
                    EnemyHealth enemyHealth = shootHit.collider.GetComponent<EnemyHealth>();

                    if (enemyHealth != null)
                    {
                        enemyHealth.TakeDamage(damagePerShot, shootHit.point);
                    }
                    gunLine.SetPosition(1, shootHit.point);
                }
                else
                {
                    gunLine.SetPosition(1, shootRay.origin + shootRay.direction * range);
                }
            }


        }

        void defaultShoot()
        {
            timer = 0f;
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
            if (Physics.Raycast(shootRay, out shootHit, range, shootableMask))
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
                gunLine.SetPosition(1, shootRay.origin + shootRay.direction * range);
            }
        }

    }
}