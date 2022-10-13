using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss_1 : MonoBehaviour
{

    // Inspector fields
    [SerializeField, Tooltip("Interval between bosses abilities.")] private float interval;

        // Not sure if this boss uses fatigue and window tho
    [SerializeField] private float fatigue;
    [SerializeField] private float fatigueDrainedPerAbility;
    [SerializeField, Tooltip("Determines how much time does the player have to damage the boss when its fatigue reaches 0.")] private float windowOfOpportunity;


    // Non-inspector fields

}
