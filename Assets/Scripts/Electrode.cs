using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Electrode
{
    public bool IsFlatElectrode = false;
    public bool IsHorizontal = false;
    public Vector2Int Position;
    public float Value;
}

[System.Serializable]
public class ElectrodeGui
{
    public Toggle IsFlatElectrode;
    public Toggle IsHorizontal;
    public TMP_InputField Position_x;
    public TMP_InputField Position_y;
}