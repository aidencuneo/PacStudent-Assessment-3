using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip introMusic;
    public AudioClip ghostNormalMusic;

    IEnumerator Start()
    {
        // Play intro music
        audioSource.clip = introMusic;
        audioSource.Play();

        // Once intro music finishes, play normal music
        yield return new WaitForSeconds(introMusic.length);

        audioSource.clip = ghostNormalMusic;
        audioSource.Play();
    }
}
