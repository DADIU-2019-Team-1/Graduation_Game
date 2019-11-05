using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using System.Linq;
using System;

//[ExecuteInEditMode]
public class Debugger : MonoBehaviour
{
    // references:
    private MotionMatching mm;

    // private:
    private List<Vector3> joints;
    private List<FeatureVector> featureVectors;
    private bool runOnce = true;
    private string[] clipsNames;
    private selectedJoints selectedJHolder, selectedCHolder;

    // public:
    public bool drawSelectedPos = false;
    [Tooltip("Not implemented yet")]public bool drawForwardVEctors = false; // TODO: make this
    public enum selectedJoints
    {
        All,
        Root,
        LeftFoot,
        RightFoot
    }
    public selectedJoints selectJoints;

    public enum selectedClips
    {

    }
    public selectedClips selectClips;

    [Range(0.005f, 0.3f)] public float pointSize = 0.03f;
    public Color gizmosColor = Color.white;

    [ExecuteInEditMode]
    private void Awake()
    {
        if (GetComponent<MotionMatching>() != null)
        {
            mm = GetComponent<MotionMatching>();
            if (mm.animContainer != null)
            {
                clipsNames = new string[mm.animContainer.animationClips.Length];
                for (int i = 0; i < mm.animContainer.animationClips.Length; i++)
                {
                    clipsNames[i] = mm.animContainer.animationClips[i].name;
                    selectClips = (selectedClips)Enum.Parse(typeof(selectedClips), clipsNames[i]);
                }
            }        
        }
    }

    void Start()
    {
        if (GetComponent<MotionMatching>() != null)
        {
            joints = new List<Vector3>();
            featureVectors = mm.GetFeatureVectors();
        }
        else
        {
            Debug.LogError("Debugger Error: MotionMatching script not found. Please have this script attached to an object with a MotionMatching script attached");
            runOnce = false;
        }
            
    }

    void Update()
    {
        if (drawSelectedPos && runOnce)
            DisplayAllPos();
        else if (!drawSelectedPos && !runOnce)
            runOnce = true;
        if (selectJoints != selectedJHolder)
        {
            runOnce = true;
        }
    }

    private void DisplayAllPos()
    {
        runOnce = false;

        switch ((int)selectJoints)
        {
            case 0:
                //joints = ;    // TODO: Make this
                break;
            case 1:
                for (int i = 0; i < featureVectors.Count; i++)
                {
                    joints.Add(featureVectors[i].GetPose().GetRootPos());
                }
                break;
            case 2:
                for (int i = 0; i < featureVectors.Count; i++)
                {
                    joints.Add(featureVectors[i].GetPose().GetLeftFootPos());
                }
                break;
            case 3:
                for (int i = 0; i < featureVectors.Count; i++)
                {
                    joints.Add(featureVectors[i].GetPose().GetRightFootPos());
                }
                break;
            default:
                break;
        }
        selectedJHolder = selectJoints;
    }

    private void OnDrawGizmos()
    {
        if (drawSelectedPos)
        {
            Gizmos.color = gizmosColor;
            if (joints.Count != 0)
                for (int i = 0; i < joints.Count; i++)
                {
                    Gizmos.DrawSphere(joints[i], pointSize);
                }
        }
    }

    public int GetJointsAmount()
    {
        return joints.Count;
    }
}

[CustomEditor(typeof(Debugger))]
public class VersionDisplay_Editor : Editor
{
    public override void OnInspectorGUI()   // TODO: Finish the custom editor
    {
        DrawDefaultInspector();

        var script = target as Debugger;


    }

    public void OnSceneGUI()
    {
        GUIStyle handleStyle = new GUIStyle();
        handleStyle.fontSize = 13;
        handleStyle.normal.textColor = Color.red;

        var script = target as Debugger;
        if (script.drawSelectedPos)
        {
            Handles.BeginGUI();
            {
                GUI.Label(new Rect(0, 0, 600, 30), "Debugger active showing "
                                                   + script.selectJoints + " joints over " + script.GetJointsAmount() + " frames", handleStyle);
            }
        }
    }
}
#endif