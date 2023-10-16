using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "Audio_Data", menuName = "Scriptable Object/Audio_Data")]
public class Audio_Data : ScriptableObject
{
    public AudioClip attack_base;
    public AudioClip attacked;
    public AudioClip guard;
    public AudioClip parry;
    public AudioClip parried;
}
