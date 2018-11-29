using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{

  public static AudioManager Instance { set; get; }

  public AudioClip BackgroundMusicClip;
  public AudioClip KillSfxClip;
  public AudioClip MoveSfxClip;

  public AudioSource BackgroundMusicSource;
  public AudioSource KillSfxAudioObject;
  public AudioSource MoveSfxAudioObject;

  // Use this for initialization
  void Start()
  {
    Instance = this;
    BackgroundMusicSource.clip = BackgroundMusicClip;
    KillSfxAudioObject.clip = KillSfxClip;
    MoveSfxAudioObject.clip = MoveSfxClip;
  }

  // Update is called once per frame
  void Update()
  {
    // if (!BackgroundMusicSource.isPlaying)
    //   BackgroundMusicSource.Play();
  }

  public void PlayMoveSfx()
  {
    if (!MoveSfxAudioObject.isPlaying) {
      MoveSfxAudioObject.Play();
      // Debug.Log("move audio");
    }
  }

  public void PlayKillSfx()
  {
    if (!KillSfxAudioObject.isPlaying)
      KillSfxAudioObject.Play();
  }

}
