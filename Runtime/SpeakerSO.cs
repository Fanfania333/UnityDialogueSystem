using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Speaker", menuName = "Dialogue Manager/Speaker")]
public class SpeakerSO : ScriptableObject
{
    [SerializeField] private Sprite sPortrait;
    public Sprite Portrait => sPortrait;
    
    [SerializeField] private string sName;
    public string Name => sName;
}
