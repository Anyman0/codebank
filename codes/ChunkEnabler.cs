using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkEnabler : MonoBehaviour
{
    [SerializeField]
    private List<Transform> objectsToDisableWithChunk;
    
    private void OnEnable()
    {
        foreach (Transform child in objectsToDisableWithChunk)
        {            
            child.gameObject.SetActive(true);
            try
            {
                child.GetComponent<ObjectSpawner>().enabled = true;
            }
            catch
            {

            }
            
        }
    }

    private void OnDisable()
    {
        foreach (Transform child in objectsToDisableWithChunk)
        {           
            child.gameObject.SetActive(false);     
            try
            {
                child.GetComponent<ObjectSpawner>().enabled = false;
            }
            catch
            {

            }
            
        }
    }
}
