using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    // Singleton
    public static AudioPlayer me;

    // References
    public AudioSource audioSource;
    public AudioClip introMusic;
    public AudioClip ghostNormalMusic;
    public AudioClip ghostScaredMusic;

    IEnumerator Start()
    {
        me = this;

        // Play intro music
        audioSource.clip = introMusic;
        audioSource.Play();

        // Once intro music finishes, play normal music
        yield return new WaitForSeconds(introMusic.length);

        PlayRegularMusic();
    }

    public void PlayScaredMusic()
    {
        audioSource.clip = ghostScaredMusic;
        audioSource.Play();
    }

    public void PlayRegularMusic()
    {
        audioSource.clip = ghostNormalMusic;
        audioSource.Play();
    }
}
