using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Touchlocation
{
    public int touchID;
    public Vector3 touchPos;

    public Touchlocation(int newTouchId, Vector3 newTouchPos) {
        touchID = newTouchId;
        touchPos = newTouchPos;
    }
}
