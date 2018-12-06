using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuAudioManager : MonoBehaviour
{

  public AudioClip BackgroundMusicClip;

  public AudioSource BackgroundMusicSource;

  void Start()
  {
    BackgroundMusicSource.clip = BackgroundMusicClip;
  }

  // Update is called once per frame
  void Update()
  {
    if (!BackgroundMusicSource.isPlaying)
      BackgroundMusicSource.Play();
  }
}
