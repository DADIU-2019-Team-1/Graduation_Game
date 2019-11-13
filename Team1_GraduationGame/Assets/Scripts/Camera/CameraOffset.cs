// Code owner: Jannik Neerdal
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CameraOffset
{
    [Tooltip("Added to existing lookAtPos")]
    [SerializeField] private Vector3 offsetLook;
    [Tooltip("Added to existing Camera Position")]
    [SerializeField] private Vector3 offsetPos;
    [Tooltip("Added to existing Camera FOV")]
    [SerializeField] private float offsetFOV;

    /// <summary>
    /// Set the camera offset and FOV for each point
    /// </summary>
    /// <param name="offsetPos"></param>
    /// <param name="offsetFOV"></param>
    public CameraOffset(Vector3 offsetLook, Vector3 offsetPos, float offsetFOV)
    {
        this.offsetLook = offsetLook;
        this.offsetPos = offsetPos;
        this.offsetFOV = offsetFOV;
    }
    public Vector3 GetLook()
    {
        return offsetLook;
    }

    public Vector3 GetPos()
    {
        return offsetPos;
    }

    public float GetFOV()
    {
        return offsetFOV;
    }
}