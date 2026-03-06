using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class HierarchyPoseBaker : MonoBehaviour
{
    [System.Serializable]
    public struct PoseData
    {
        public string objectName;
        public Vector3 localPosition;
        public Quaternion localRotation;
        public Vector3 localScale;
    }

    [System.Serializable]
    public class PoseContainer
    {
        public List<PoseData> poses = new List<PoseData>();
    }

    [Header("Data Storage (Hidden)")]
    [SerializeField] private PoseContainer lastCapturedPose = new PoseContainer();

    void Update()
    {
        // 1. Capture during Play Mode
        if (Application.isPlaying && Input.GetKeyDown(KeyCode.Space))
        {
            CaptureHierarchyToClipboard();
        }
    }

    [ContextMenu("1. Capture Current Pose")]
    public void CaptureHierarchyToClipboard()
    {
        lastCapturedPose.poses.Clear();
        Transform[] allChildren = GetComponentsInChildren<Transform>();

        foreach (Transform t in allChildren)
        {
            lastCapturedPose.poses.Add(new PoseData
            {
                objectName = t.name,
                localPosition = t.localPosition,
                localRotation = t.localRotation,
                localScale = t.localScale
            });
        }

        // Convert to JSON and save to System Clipboard
        string json = JsonUtility.ToJson(lastCapturedPose);
        GUIUtility.systemCopyBuffer = json;

        Debug.Log($"<color=green>Pose Captured to Clipboard!</color> {lastCapturedPose.poses.Count} objects saved. You can now stop Play Mode.");
    }

    [ContextMenu("2. Bake Saved Pose (Edit Mode)")]
    public void BakeFromClipboard()
    {
        string json = GUIUtility.systemCopyBuffer;
        
        if (string.IsNullOrEmpty(json) || !json.Contains("poses"))
        {
            Debug.LogError("No valid pose data found in clipboard! Capture a pose with [Space] in Play Mode first.");
            return;
        }

        // Load data from clipboard string back into the container
        lastCapturedPose = JsonUtility.FromJson<PoseContainer>(json);

        #if UNITY_EDITOR
        Undo.RecordObjects(GetComponentsInChildren<Transform>(), "Bake Hierarchy Pose");
        #endif

        Transform[] allChildren = GetComponentsInChildren<Transform>();

        foreach (PoseData data in lastCapturedPose.poses)
        {
            foreach (Transform t in allChildren)
            {
                if (t.name == data.objectName)
                {
                    t.localPosition = data.localPosition;
                    t.localRotation = data.localRotation;
                    t.localScale = data.localScale;

                    // Sync Physics for HingeJoint2D chains
                    Rigidbody2D rb = t.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        rb.position = t.position;
                        rb.rotation = t.eulerAngles.z;
                    }
                    break;
                }
            }
        }

        Debug.Log("<color=cyan>Pose Restored from Clipboard and Baked successfully!</color>");
    }
}