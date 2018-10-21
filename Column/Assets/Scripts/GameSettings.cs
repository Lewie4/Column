using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings1", menuName = "Settings/Game Settings")]
public class GameSettings : ScriptableObject
{
    public LevelSettings levelSettings;
    public PlayerSettings playerSettings;
}
