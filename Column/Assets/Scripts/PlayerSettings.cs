using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSettings1", menuName = "Settings/Player Settings")]
public class PlayerSettings : ScriptableObject
{
    public GameObject player;
    public float jumpTime;
    public AnimationCurve jumpCurve;
}
