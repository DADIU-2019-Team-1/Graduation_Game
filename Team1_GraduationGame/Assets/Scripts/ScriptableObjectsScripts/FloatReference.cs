using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FloatReference
{
    public bool UseUnique = true;
    public float UniqueValue;

    public FloatVariable Variable;

    public float value => UseUnique ? UniqueValue : Variable.value;

}
