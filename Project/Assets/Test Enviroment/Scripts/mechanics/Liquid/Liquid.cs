using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Liquid", menuName = "Liquid")]
public class Liquid : ScriptableObject
{
    [SerializeField]
    private string type;

    public string Type
    {
        get => type;
        set => type = value;
    }
}
