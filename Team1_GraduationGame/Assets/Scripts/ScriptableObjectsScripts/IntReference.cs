using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class IntReference : MonoBehaviour
{
    public bool UseConstant = true;
    public int ConstantValue;

    public IntVariable variable;

    public int value 
    {
        get{ return UseConstant ? ConstantValue :
                                    variable.value; }
    }
}
