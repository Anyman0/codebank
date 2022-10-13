using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;

public class FrontRunnerScript : MonoBehaviour
{
    // Editor fields
    [SerializeField, Tooltip("Determines the radius of runners dropped objects.")]
    private float randomDropRadius;
    [SerializeField, Tooltip("Determines how often does the runner drop objects.")]
    private float dropInterval;
    [SerializeField, Tooltip("Prefab of the runners dropped object.")]
    private GameObject objectToDrop;
    [SerializeField, Tooltip("Effect when 'dropping' an object. (Spawning)")]
    private GameObject dropEffect;
    [SerializeField, Tooltip("How often do strafe to randomized direction. (Left / Right)")]
    private float strafeInterval;
    [SerializeField, Tooltip("Determines how long does the runner strafe for.")]
    private float strafeTime;
    [SerializeField, Tooltip("How much does the runner strafe from its original x-axis.")]
    private float strafeRadius; 

    // Non-editor fields   
    private float counter = 0f;
    private float x = 0f;
    private float y = 0f;
    private Animator animator;
    private GameObject droppedObject;
    private CoroutineHandle objectDropHandle;
    private CoroutineHandle strafeHandle;

    private void Awake()
    {
        animator = transform.GetComponent<Animator>();        
    }

    private void Update()
    {        
        counter += Time.deltaTime;
        if(counter >= strafeInterval)
        {            
            StrafeDirection();
            //Debug.Log(x + "And the Y: " + y);
            strafeHandle = Timing.RunCoroutine(Strafing().CancelWith(transform.gameObject));
            counter = 0f; // Zero counter
        }

        // Animations         
        animator.SetFloat("X", Mathf.Lerp(animator.GetFloat("X"), x, Time.deltaTime * 1f));
        animator.SetFloat("Y", y);
        // Movement
        if(x < 0 && x != 0)
        {
            var strafeX = transform.position.x - strafeRadius;          
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(strafeX, transform.position.y, transform.position.z), Time.deltaTime * 0.5f);
        }
        else if(x > 0 && x != 0)
        {
            var strafeX = transform.position.x + strafeRadius;
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(strafeX, transform.position.y, transform.position.z), Time.deltaTime * 0.5f);
        }

        if(!objectDropHandle.IsRunning)
        objectDropHandle = Timing.RunCoroutine(Drop().CancelWith(transform.gameObject));
        

    }

    // Changes transforms movement direction (left / right). 
    private void StrafeDirection()
    {
        x = Random.Range(-1, 2);
        if(x != 0)
        y = 1f;               
    }

    IEnumerator<float> Strafing()
    {
        yield return Timing.WaitForSeconds(strafeTime);
        // Return runners direction to forward
        animator.SetFloat("X", Mathf.Lerp(animator.GetFloat("X"), 0f, Time.deltaTime * 10f));
        animator.SetFloat("Y", 0f);
        x = 0;
        y = 0;
    }


    IEnumerator<float> Drop()
    {
        yield return Timing.WaitForSeconds(dropInterval);
        var rndRadius = Random.Range((0 - randomDropRadius), randomDropRadius);
        //Debug.Log(rndRadius);       
        var dropPosition = transform.position + new Vector3(rndRadius, 0f, 0f);
        GameObject effect = Instantiate(dropEffect, transform.position, Quaternion.identity);
        Destroy(effect, 2f);
        droppedObject = Instantiate(objectToDrop, dropPosition, Quaternion.identity);
        Physics.IgnoreCollision(transform.GetComponent<BoxCollider>(), droppedObject.GetComponent<CapsuleCollider>());        
    }
    

}
