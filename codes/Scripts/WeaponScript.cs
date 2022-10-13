using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using MEC;
using valueGrabs;
using VD;


// Defining the weapon <<<<------------------------------------------------
public enum WeaponTypes
{
    PrimaryWeapon = 0,
    SecondaryWeapon = 1,
    SpecialWeapon = 2, 
    ChargeWeapon = 3
}

public enum FireModes
{
    Single = 0,
    Burst = 1,
    Auto = 2, 
    Charge = 3
}


public class WeaponScript : MonoBehaviour
{
    // Show the enums in Editor 
    public WeaponTypes weaponTypes;
    public FireModes fireModes;
      
    // Defining the weapons attributes <<<<<-------------------------------------

    [SerializeField, Tooltip("Self-explanatory. This is the time between shots.")]
    internal float timeBetweenShots;
    [SerializeField]
    protected float weaponDamage;
    [SerializeField, Tooltip("Range determines how far we can shoot with this weapon.")]
    protected float shootRange;
    [SerializeField, Tooltip("Speed of the projectiles launched from this weapon.")]
    protected float projectileSpeed;
    [SerializeField, Tooltip("Amount of projectiles launched from this weapon at once. You can define burst here or use this for shotgun-like weapons.")]
    protected int shotsFiredAtOnce;
    // Below attributes really only affect ChargeWeapons. TODO: Show these attributes in editor ONLY IF weapontype is ChargeWeapon?
    /*[SerializeField, Tooltip("If weapon uses a charge-timer, define that value here.")]
    protected float weaponChargeTimer;*/
    [SerializeField, Tooltip("Heat of this weapon. Ranges from 0 to 100, incremented by shots, reduced passively and by cooling animation when reaching 100. When cooling down weapon cannot shoot.")]
    internal float weaponHeat = 30f;
    [SerializeField, Tooltip("Amount of heat this weapon produces per shot.")]
    internal float heatPerShot = 3f;
    [SerializeField, Tooltip("This value increments the size of chargeprojectile when holding down trigger. Default at 0.003f.")]
    private float chargeProjectileSizeIncrement = 0.003f;
    [SerializeField, Tooltip("Passive heat reduction per second.")]
    internal float passiveCoolingPerSecond = 0.1f;
    [SerializeField, Tooltip("Time taken by cooling down action (animation)")]
    internal float timeToCoolActive = 2f;
    internal bool coolingFromHeated = false;
    [SerializeField, Tooltip("Animation that runs while weapon is cooling down from overheating")]
    protected ParticleSystem coolingDownParticleSystem;
    [SerializeField, Tooltip("How big is the weapons magazine.")]
    protected int magSize;

    // FIGURE OUT HOW TO SHOW/HIDE THESE CONDITIONALLY. (IE. Show "Primary Weapon Options" only if chosen WeaponType is PrimaryWeapon)
    // Do we even use primary/secondary/special weapons? 
    /*
    [Header("Primary Weapon Options")]
    // TODO: Add options for primary weapons
    [SerializeField, Tooltip("Self-explanatory. Durability of the weapon.")]
    internal int PrimaryWeaponDurability;
    [Header("Secondary Weapon Options")]
    // TODO: Add options for secondary weapons
    [SerializeField, Tooltip("Self-explanatory. Durability of the weapon.")]
    internal int SecondaryWeaponDurability;
    [Header("Special Weapon Options")]
    // TODO: Add options for special weapons
    [SerializeField, Tooltip("Self-explanatory. Durability of the weapon.")]
    internal int SpecialWeaponDurability;*/

    // Weapon prefabs
    [Header("Prefabs used by the weapon")]
    [SerializeField, Tooltip("Place to spawn actual physical projectiles. (ie. ChargeWeapon)")]
    internal List<Transform> projectileHolder; 
    [SerializeField, Tooltip("Name of the AudioClip used by this weapons shoot.")]
    protected string shootAudioName;
    [SerializeField, Tooltip("Name of the AudioClip used by this weapons reload.")]
    protected string reloadAudioName;
    [SerializeField, Tooltip("Projectile prefab used by this weapon. (ie. ChargeWeapon projectile")]
    protected GameObject projectilePrefab;
    [SerializeField, Tooltip("Full size of the charge-projectile.")]
    protected float projectileSizeX;    
    [SerializeField, Tooltip("Muzzle flash prefab used by this weapon.")]
    protected GameObject muzzleEffect;
    [SerializeField, Tooltip("Hit effect prefab used by this weapon.")]
    protected GameObject hitEffect;
    [SerializeField, Tooltip("Muzzle prefab used to cast the rayfire from.")]
    protected GameObject muzzle;
    [SerializeField, Tooltip("Tick this if you want to see a visible line from muzzle to hitpoint.")]
    private bool UseAiming;
    [SerializeField, Tooltip("Tick this if you want to ease aiming by guiding projectile towards the closest enemy.")]
    private bool ProjectileGuiding;
    [SerializeField, Tooltip("This value moves to projectile towards nearest enemy if ProjectileGuiding is ticked. Increase to make hitting enemies easier.")]
    private float projectileAimAssist = 3.0f;
    [SerializeField, Tooltip("String ID of the projectile from ItemManager that we want to spawn.")]
    private string projectileID;


    private float timer;
    private float coolingSecond = 1f;
    private float setWeaponHeat;
    private float chargeDamageMultiplier;
    private LineRenderer line;
    private GameObject cProjectileClone;
    
    public List<GameObject> enemiesToShoot;
    private GameObject closestEnemy;
    [HideInInspector]
    public IEnumerator destroyRoutine;
    private List<GameObject> grenades;
    private GameObject GrenadeHolder;
    [SerializeField]
    private GameObject ObjectValueGrabber;

    // KEYCODE FOR NOW TO TEST
    public KeyCode GrenadeKey;
         
    // Booleans
    [HideInInspector]
    public bool canDestroy;
    private bool isCloned = false;
    [HideInInspector]
    public bool nadeActive;
    private bool paused;

    // Input source
    [HideInInspector]
    public SteamVR_Input_Sources handToUse;

    private void OnGUI()
    {
        
    }


    private void Awake()
    {
        
        if(UseAiming)
        {
            line = transform.GetComponent<LineRenderer>();
            line.enabled = false;
        }

        try
        {
            // Populate grenade list
            grenades = new List<GameObject>();
            GrenadeHolder = GameObject.Find("GrenadeHolder");           
            foreach (Transform child in GrenadeHolder.transform)
            {
                grenades.Add(child.gameObject);
            }
        }
        catch
        {
            Debug.Log("No GrenadeHolder in game!");
        }

        try
        {
            paused = GameObject.FindGameObjectWithTag("Player").GetComponent<MovementScript1>().paused;
        }
        catch
        {
            
        }
        

        setWeaponHeat = weaponHeat;

       
        enemiesToShoot = new List<GameObject>();

        // Define the hand to use. Below IF using with RadialInventorys Interactable
        /*if (GameObject.FindGameObjectWithTag("Player").GetComponent<Interactable>().weaponHand == Interactable.handToUse.LeftHand) handToUse = SteamVR_Input_Sources.LeftHand;
        else if (GameObject.FindGameObjectWithTag("Player").GetComponent<Interactable>().weaponHand == Interactable.handToUse.RightHand) handToUse = SteamVR_Input_Sources.RightHand;*/

        // Else IF not. Hardcoded hand here.
        handToUse = SteamVR_Input_Sources.RightHand; 


        //muzzleEffect.GetComponent<ParticleSystem>().Stop();

    }

    private void Update()
    {
        if (!paused)
        {

            //Debug.DrawRay(muzzle.transform.position, muzzle.transform.forward * shootRange, Color.green); // Where are we aiming at?     
            if (this.transform.parent != null)
            {
                // TODO: SteamVR input now. Add OVR input too?
                if (this.transform.tag == "RightHand" || this.transform.parent.tag == "RightHand")
                {
                    // IF firemode is single or burst, allow the player to only fire once per trigger pull.
                    if (fireModes == FireModes.Single || fireModes == FireModes.Burst)
                    {
                        if (SteamVR_Actions.default_GrabPinch.GetStateDown(handToUse))
                        {

                            if (fireModes == FireModes.Burst)
                            {
                                for (int i = 0; i < shotsFiredAtOnce; i++)
                                {
                                    Shoot();
                                }
                            }

                            else if (fireModes == FireModes.Single)
                            {
                                Shoot();
                            }

                        }
                    }
                    // IF firemode is auto, allow the user to continue firing while pulling the trigger.
                    else if (fireModes == FireModes.Auto)
                    {
                        if (SteamVR_Actions.default_GrabPinch.GetState(handToUse))
                        {
                            Shoot();
                        }
                    }

                    // Charge mode
                    else if (fireModes == FireModes.Charge)
                    {
                        if (SteamVR_Actions.default_GrabPinch.GetState(handToUse))
                        {
                            Shoot();
                        }

                        // Fire the ChargeWeapon when trigger is released. 
                        //GameObject chargedProjectile = GameObject.FindGameObjectWithTag("ChargeProjectile");

                        // For shooting only full size projectiles!
                        /*if (SteamVR_Actions.default_GrabPinch.GetStateUp(handToUse) && chargedProjectile.transform.localScale.x >= projectileSizeX && canDestroy == false) // Check if projectile is ready to be launched
                        {
                            ChargeWeaponShoot();
                        }

                        else if(SteamVR_Actions.default_GrabPinch.GetStateUp(handToUse) && chargedProjectile.transform.localScale.x < projectileSizeX)
                        {
                            DestroyChargedProjectile();
                        }*/

                        if (SteamVR_Actions.default_GrabPinch.GetStateUp(handToUse)) // For shooting projectiles of all sizes
                        {
                            //ChargeWeaponShoot();
                            ShootChargeWeapon(cProjectileClone);
                        }

                        timer += Time.deltaTime;
                        if ((timer % 60) > coolingSecond && weaponHeat <= setWeaponHeat)
                        {
                            timer = 0f;
                            weaponHeat += passiveCoolingPerSecond;
                        }


                        if (ProjectileGuiding && cProjectileClone != null)
                        {
                            var closeEnemy = getNearestShootableEnemy();

                            if (closeEnemy != null)
                            {
                                try
                                {
                                    if ((cProjectileClone.transform.position - closeEnemy.transform.position).sqrMagnitude < 50f)
                                        cProjectileClone.transform.position = Vector3.Lerp(cProjectileClone.transform.position, closeEnemy.transform.position, Time.deltaTime * projectileAimAssist);
                                }
                                catch
                                {
                                    Debug.Log("No enemies available!");
                                }
                            }

                        }

                        if (weaponHeat < 2 && !coolingFromHeated)
                        {
                                                 
                            Timing.RunCoroutine(coolingDown());
                            coolingFromHeated = true;
                        }

                    }


                    // Do something when mag is empty.
                    if (weaponTypes != WeaponTypes.ChargeWeapon)
                    {
                        if (magSize <= 0)
                        {
                            Debug.Log("Mag is empty!");
                            transform.GetComponent<WeaponScript>().ObjectValueGrabber.GetComponent<valueGrabber>().weaponPickUp = false;
                            this.transform.parent.GetComponent<WeaponScript>().enabled = true;
                            Destroy(this.gameObject);
                        }

                    }

                }
               

            }
            

            try
            {
                if (Input.GetKeyDown(GrenadeKey) || SteamVR_Actions.default_Grenade.GetStateDown(SteamVR_Input_Sources.RightHand) && !ObjectValueGrabber.GetComponent<valueGrabber>().weaponPickUp) holdGrenade();


                foreach (var nade in grenades)
                {
                    if (nade.transform.localPosition != Vector3.zero)
                    {
                        nade.transform.localPosition = Vector3.zero;
                    }
                }
            }

            catch
            {

            }
        }
    }


    public void Shoot()
    {
        RaycastHit hit;                        
        
        if(Physics.Raycast(muzzle.transform.position, muzzle.transform.forward * shootRange, out hit, shootRange) && weaponTypes != WeaponTypes.ChargeWeapon && magSize > 0) 
        {          
            Debug.Log(hit.transform.name+ ". And the actual point we hit: " + hit.point); // What did we hit?
            //muzzleEffect.GetComponent<ParticleSystem>().Play();            
            // Create and almost immediately destroy the hitEffect particlesystem if weapon is not charge weapon. (Charge weapon instantiates actual projectiles)
            if (weaponTypes != WeaponTypes.ChargeWeapon)
            {
                GameObject HitGameObject = Instantiate(hitEffect, hit.point, Quaternion.identity);                
                Destroy(HitGameObject, 2f);
            }
            
            // What happens when we hit an object with a rigidbody attached
            if(hit.rigidbody != null)
            {
                hit.rigidbody.AddForce(-hit.normal * projectileSpeed);
                bool hasAttributes = false;
                if (hit.transform.GetComponent<AttributeSystemScript>() != null) hasAttributes = true;

                if(hasAttributes && !hit.transform.CompareTag("Player")) // Damage everything with attributes except player
                hit.transform.GetComponent<AttributeSystemScript>().RemoveHealth(weaponDamage);
            }

            // TODO: Add if statements here to run different methods on different hits
            if(hit.transform.tag == "Destructible")
            {
                // TODO: Add method to destroy the destructible object
            }

            
            /*if(fireModes != FireModes.Burst) this.magSize -= shotsFiredAtOnce;

            else if(fireModes == FireModes.Burst)
            {
                this.magSize -= 1;
            }*/

        }
        if(weaponTypes != WeaponTypes.ChargeWeapon && magSize > 0)
        {

            // Play particle-effect
            muzzleEffect.GetComponent<ParticleSystem>().Play();
            // Fire-anim
            if(transform.GetComponent<Animator>() != null) 
            transform.GetComponent<Animator>().SetTrigger("Fire");

            if (fireModes != FireModes.Burst) this.magSize -= shotsFiredAtOnce;

            else if (fireModes == FireModes.Burst)
            {
                this.magSize -= 1;
            }
        }
        

        /*else if(weaponTypes == WeaponTypes.ChargeWeapon && canDestroy != true) // if canDestroy is false, projectile has been fired. Then we dont enter here.
        {
            GameObject chargeProjectile;
            if(GameObject.FindGameObjectWithTag("ChargeProjectile") == null)
            {                
                chargeProjectile = Instantiate(projectilePrefab, muzzle.transform.position, muzzle.transform.rotation);               
                chargeProjectile.GetComponent<SphereCollider>().enabled = false;
                chargeProjectile.GetComponent<Rigidbody>().isKinematic = true;  
                // TODO: Add a chargeweapon cooldown system. Invoke a method with a cooldown particle system? For example
            }

            GameObject.FindGameObjectWithTag("ChargeProjectile").transform.position = GameObject.Find("Muzzle").transform.position;
        }*/

        // Projectiles already loaded in scene in this case
        else if(weaponTypes == WeaponTypes.ChargeWeapon && weaponHeat > heatPerShot)
        {

            if(UseAiming)
            {                
                if (Physics.Raycast(muzzle.transform.position, muzzle.transform.forward, out hit, 500f))
                {
                    line.enabled = true;
                    line.SetPosition(0, transform.position);
                    line.SetPosition(1, hit.point);

                }
                else line.enabled = false;
            }
            
            //muzzleEffect.GetComponent<ParticleSystem>().Play();
            //GameObject cProjectileClone;
            //int lastProjectile = projectileHolder.Count;
            var muzzlePos = muzzle.transform.position;// 
           
                //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<< PREVIOUS METHOD >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
                /*if(isCloned == false)
                {
                    cProjectileClone = Instantiate(projectilePrefab, muzzlePos, muzzle.transform.rotation);
                    Physics.IgnoreCollision(transform.parent.GetComponent<CapsuleCollider>(), cProjectileClone.GetComponent<CapsuleCollider>());
                    cProjectileClone.GetComponent<CapsuleCollider>().enabled = false;                    
                    projectileHolder.Add(cProjectileClone.transform);
                    isCloned = true;
                }
               
                // 
                else if(isCloned && (this.transform.position - projectileHolder[lastProjectile - 1].transform.position).sqrMagnitude < 0.5)
                {                   
                    cProjectileClone = projectileHolder[lastProjectile - 1].gameObject;                    
                    
                    if(cProjectileClone.transform.localScale.x <= projectileSizeX )
                    {                       
                        cProjectileClone.transform.localScale += new Vector3(chargeProjectileSizeIncrement, chargeProjectileSizeIncrement, chargeProjectileSizeIncrement);
                    }
                    //Debug.Log("Keeping the projectile by the muzzle until its fired.");
                    cProjectileClone.transform.position = muzzlePos; // Projectile follows the muzzle until its fired
                }*/
                
                if(!isCloned)
                {
                    cProjectileClone = ItemManager.Instance.Spawn("" + projectileID, muzzlePos, muzzle.transform.rotation);                       
                    isCloned = true;
                }
                
                else if(isCloned)
                {
                    if(cProjectileClone.transform.localScale.x <= projectileSizeX)
                    {
                        cProjectileClone.transform.localScale += new Vector3(chargeProjectileSizeIncrement, chargeProjectileSizeIncrement, chargeProjectileSizeIncrement);
                    }
                    // Keeps the projectile at muzzles position until fired.
                    cProjectileClone.transform.position = muzzlePos;
                }
                                      
        }

    }

    /*public void ChargeWeaponShoot()
    {       
             
        if(weaponHeat > heatPerShot)
        {
            if (UseAiming)
                line.enabled = false;
            //RaycastHit hit;
            int lastProjectile = projectileHolder.Count;
            GameObject chargeProjectile = projectileHolder[lastProjectile - 1].gameObject;
            chargeProjectile.GetComponent<Rigidbody>().isKinematic = false;
            
            chargeDamageMultiplier = chargeProjectile.transform.localScale.x;
            Debug.Log(chargeProjectile.transform.localScale.x);
            float dmg = chargeProjectile.GetComponent<ProjectileScript>().projectileDamage;
            chargeProjectile.GetComponent<ProjectileScript>().projectileDamage = dmg * chargeDamageMultiplier;
            Debug.Log(chargeProjectile.GetComponent<ProjectileScript>().projectileDamage);
            chargeProjectile.GetComponent<Rigidbody>().AddForce(muzzle.transform.forward * projectileSpeed);

            // Adjust the weapons heat based on attributes given in the editor
            //var valueToDec = chargeProjectile.transform.localScale.x * 2;
            //var valueToDec = heatPerShot + chargeProjectile.transform.localScale.x;
            transform.GetComponent<WeaponScript>().weaponHeat -= heatPerShot;
            
            Timing.RunCoroutine(enableCollider());
            canDestroy = true;
            isCloned = false;
            destroyRoutine = DestroyChargedProjectile(chargeProjectile, chargeProjectile.GetComponent<ProjectileScript>().lifeTime);
            //StartCoroutine(DestroyChargedProjectile(chargeProjectile, chargeProjectile.GetComponent<ProjectileScript>().lifeTime));
            StartCoroutine(destroyRoutine);
        }
        
    }*/

    private void ShootChargeWeapon(GameObject projectile)
    {
        if(weaponHeat > heatPerShot)
        {
            if (UseAiming) line.enabled = false;
            chargeDamageMultiplier = projectile.transform.localScale.x;
            float dmg = projectile.GetComponent<ProjectileScript>().projectileDamage;
            projectile.GetComponent<ProjectileScript>().projectileDamage = dmg * chargeDamageMultiplier;            
            isCloned = false;
            Physics.IgnoreCollision(GameObject.FindGameObjectWithTag("Player").GetComponent<CapsuleCollider>(), projectile.GetComponent<CapsuleCollider>());
            projectile.GetComponent<Rigidbody>().AddForce(muzzle.transform.forward * projectileSpeed);
            weaponHeat -= heatPerShot;            
            Timing.RunCoroutine(projectile.GetComponent<ProjectileScript>().destroyProjectile().CancelWith(projectile));
            projectile.transform.localScale = new Vector3(1, 1, 1);            
        }
        
    }

    /*
    // lifetime here comes from projectiles ProjectileScript
    IEnumerator DestroyChargedProjectile(GameObject projectile, float lifetime)
    {
        yield return new WaitForSeconds(lifetime);
        try
        {
            projectileHolder.Remove(projectile.transform);
            Destroy(projectile);
        }
        catch
        {
            Debug.Log("Heeeyhooouhey, already removed!");
        }      
        canDestroy = false;        
    }*/

    // This enables the projectiles collider after it has been released from weapon.
    IEnumerator<float> enableCollider()
    {
        yield return Timing.WaitForSeconds(0.4f);
        try
        {
            int lastProjectile = projectileHolder.Count;
            GameObject projectile = projectileHolder[lastProjectile - 1].gameObject;
            projectile.GetComponent<CapsuleCollider>().enabled = true;
        }
        catch
        {
            Debug.Log("Projectile is already destroyed!");
        }
        
    }

    private GameObject getNearestShootableEnemy()
    {
        enemiesToShoot.Clear();
        foreach (GameObject enemy in FindObjectsOfType(typeof(GameObject)))
        {
            if (enemy.layer == 30) // ASSUMING THAT ENEMYTARGET-TAG IS 30 AND ENEMIES ALWAYS HAVE IT
            {
                //Debug.Log(enemy);
                enemiesToShoot.Add(enemy);
            }
        }

        float distance = float.MaxValue;
        
        try
        {
            if (enemiesToShoot.Count > 0)
            {
                foreach (var enemy in enemiesToShoot)
                {
                    if ((enemy.transform.position - cProjectileClone.transform.position).sqrMagnitude < distance)
                    {
                        closestEnemy = enemy;
                        distance = (enemy.transform.position - cProjectileClone.transform.position).sqrMagnitude;
                    }
                }
            }
        }
        catch
        {
            Debug.Log("Couldnt get enemy as target!");
        }



        return closestEnemy;

    }

    private void holdGrenade()
    {       
        var rnd = Random.Range(0, grenades.Count-1);
        if(grenades[rnd].GetComponent<Throwable>().count > 0)
        {
            enabled = false;
            grenades[rnd].SetActive(true);
            nadeActive = true;
        }  
        else
        {
            // What to do if nades count is 0. 
            // Notify player somehow or just show nade count in UI?
            Debug.Log("No nades!");
        }
    }

    IEnumerator<float> coolingDown()
    {
        yield return Timing.WaitForSeconds(timeToCoolActive);
        weaponHeat += 20;
        coolingFromHeated = false;
    }



}
