using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Sanicball/Music Profile")]
public class MusicProfile : ScriptableObject {
    public AudioClip backTheme;   // Course screen
    public AudioClip intro;       // Countdown
    public AudioClip firstDrop;   // Race Start (Lap 1)
    public AudioClip lapPhrase;   // Random lap transition
    public AudioClip lapDrop;     // Normal lap loop
    public AudioClip finalDrop;   // Final Lap
    public AudioClip outro;       // Race Finished
}