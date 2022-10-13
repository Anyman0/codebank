using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum pickUpType
{
    healingItem,
    throwable,
    gun
}
public interface IGrabbable 
{    
    pickUpType type { get; set; }
    void incrementValue(float amount);
}
