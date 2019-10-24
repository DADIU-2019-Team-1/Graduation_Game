using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class IntReference
{
    public bool UseUnique = true;
    public int UniqueValue;

    public IntVariable variable;

    public int value 
    {
        get{ return UseUnique ? UniqueValue :
                                    variable.value; }
    }
}
