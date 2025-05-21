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
    
    [Tooltip("Refers to the name of the localization table the translation for this node will be taken from. Can be left blank, if not using localization.")]
    [SerializeField] private string tableReference;
    public string TableReference => tableReference;
}
