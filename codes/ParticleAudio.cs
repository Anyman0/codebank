using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ParticleAudio : playMasterAudioClip
{

    // Inspector fields
    [SerializeField] private string audioClip;


    // Non-inspector fields
    private ParticleSystem ps;


    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();

    }
    private void OnParticleCollision(GameObject other)
    {
        if (audioClip != "")
        {
            Debug.Log("Collision happened with " + other.transform.name + ". Playing AudioClip " + audioClip);
            PlayMAClip(audioClip);
        }
        else Debug.Log("Collision happened with " + other.transform.name + ". AudioClip is null.");

    }
}
