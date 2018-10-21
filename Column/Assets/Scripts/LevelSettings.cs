using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelSettings1", menuName = "Settings/Level Settings")]
public class LevelSettings : ScriptableObject
{
    public GameObject pillar;
    public int visibleRows;
    public int totalRows;
    public Vector3 positionOffset;
    public Vector3 spawnChance;
    public float timeToDespawn = 5f;
}