using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VD;

public class GrowableEnemy : MonoBehaviour
{

    // Inspector fields
    [SerializeField]
    private float growSpeed;
    [SerializeField, Tooltip("How high does the object ascend.")]
    private float maxY;

    // Non-inspector fields
    private Vector3 maxAscend;
    private Vector3 position;
    private Vector3 startPosition;
    private GameObject spawnedObject;
    private bool descend;

    private void OnEnable()
    {
        startPosition = transform.position;
        maxAscend = new Vector3(transform.position.x, maxY, transform.position.z);
        position = new Vector3(0, 3f, 0);
               
    }

    private void Update()
    {
        if(!descend && (transform.position - maxAscend).sqrMagnitude > 0.001)
        {            
            transform.position = Vector3.Lerp(transform.position, maxAscend, Time.deltaTime * growSpeed);            
        }
        else if((transform.position - maxAscend).sqrMagnitude < 0.001 || descend)
        {
            descend = true;
            transform.position = Vector3.Lerp(transform.position, startPosition, Time.deltaTime * growSpeed);
        }
        if ((transform.position - startPosition).sqrMagnitude < 0.001) descend = false;
        
    }

}
