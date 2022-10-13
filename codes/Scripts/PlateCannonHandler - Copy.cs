namespace VD
{
    using DarkTonic.MasterAudio;
    using MEC;
    using System.Collections.Generic;
    using UnityEngine;

    public class PlateCannonHandler : MonoBehaviour
    {

        private List<Transform> plates;
        private Animator ShootAnimator;
        private Animator LoadAnimator;
        private int plateCounter = 0;
       
        private GameObject plate;
        private bool isFired;
        private List<Transform> plateList;
        private List<Transform> firedPlateList;

        [Header("Plates MUST have a rigidbody.")]
        [Space(10)]
        [SerializeField, Tooltip("Increase to make plates fly faster.")]
        private float shootForce = 5f;
        [SerializeField, Tooltip("Check this box if you want the plate to be destroyed after a given time.")]
        private bool DestroyAfterGivenTime;
        [SerializeField, Tooltip("Automatically return plates to origin after this time if theres no collision that does it. Default at 2.")]
        private float destroyTimer = 2f;
        [SerializeField, Tooltip("End of the 'barrel'. Where to fire the plates from.")]
        private Transform muzzle;
        [SerializeField, Tooltip("PlateStack children component here. (plateStackFill)")]
        private GameObject plateStack;
        [SerializeField, Tooltip("String ID from ItemManager of the plate we want to fire. (IE. 'Plate')")]
        private string plateItemID;

        private void Awake()
        {           
            ShootAnimator = transform.GetComponent<Animator>();
            LoadAnimator = plateStack.GetComponent<Animator>();
            plateList = new List<Transform>();
            firedPlateList = new List<Transform>();

            foreach(Transform plate in GameObject.Find("platereal").transform)
            {
                plateList.Add(plate);
                plate.gameObject.SetActive(false);
            }

            
            // Activate cannon at awake
            if (!ShootAnimator.GetBool("activate"))
                ShootAnimator.SetBool("activate", true);
        }

        private void Update()
        {

            // Keyboard inputs here just for testing
            if (OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger))
            {
                getPlateCount();

                if (plateCounter > 0)
                {
                    ShootPlates();
                }
                    
                else 
                {
                    // What to do when theres no ammo
                    Debug.Log("Out of plates! Reload!");
                }

            }

            // ---------TÄHÄN TILALLE OIKEA CONDITIONI ----------------
            else if (OVRInput.GetDown(OVRInput.RawButton.A))
            {
                
                getPlateCount();
                if (plateCounter < 8)
                {
                    LoadPlates();                    
                }                   
                else
                {
                    // What to do when mag is full
                    Debug.Log("Mag is full!");
                }
               
            }

            
            
        }

        private void LoadPlates()
        {
            try
            {                              
              LoadAnimator.SetTrigger("addPlate");            
                // Load plate sound? 
            }
            catch
            {

            }
            
            
        }

        private void ShootPlates()
        {
                          
            ShootAnimator.SetTrigger("shoot");
            LoadAnimator.SetTrigger("shoot");                                
            GameObject projectile = ItemManager.Instance.Spawn(""+plateItemID, muzzle.position, Quaternion.identity);            
            projectile.GetComponent<Rigidbody>().AddForce(transform.forward * (shootForce*1000f)); // *1000 just to make it smarter in editor.
            if(DestroyAfterGivenTime)
            Timing.RunCoroutine(destroyPlate(projectile), projectile);
            MasterAudio.PlaySoundAndForget("PlateLaunch");                                                
        }

        // Destroy plate with timer if it doesnt collide with anything
        IEnumerator<float> destroyPlate(GameObject plate)
        {
            yield return Timing.WaitForSeconds(destroyTimer);            
            Destroy(plate);
        }

        private void getPlateCount()
        {
            plateCounter = 0;
            foreach (var plate in plateList)
            {
                if (plate.gameObject.activeSelf)
                {
                    plateCounter++;
                }
            }
        }


    }
}
