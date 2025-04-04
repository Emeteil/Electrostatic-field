using UnityEngine;

[System.Serializable]
public class Electrode
{
    public bool IsFlatElectrode = false;
    public bool IsHorizontal = false;
    public Vector2Int Position;
    public float Value;
}