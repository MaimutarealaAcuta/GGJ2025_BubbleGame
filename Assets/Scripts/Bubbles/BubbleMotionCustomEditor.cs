#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BubbleMotionScript))]
public class BubbleMotionEditor : Editor
{
    public override void OnInspectorGUI()
    {
        BubbleMotionScript bubbleMotionScript = (BubbleMotionScript)target;
        bubbleMotionScript.movementType = (BubbleMotionScript.BubbleMovementType)EditorGUILayout.EnumPopup("Movement Type", bubbleMotionScript.movementType);
        switch (bubbleMotionScript.movementType)
        {
            case BubbleMotionScript.BubbleMovementType.Stationary:
                bubbleMotionScript.PositionRangeX = EditorGUILayout.Slider("Position Range X", bubbleMotionScript.PositionRangeX, 0.1f, 5.0f);
                bubbleMotionScript.PositionRangeY = EditorGUILayout.Slider("Position Range Y", bubbleMotionScript.PositionRangeY, 0.1f, 5.0f);
                bubbleMotionScript.PositionRangeZ = EditorGUILayout.Slider("Position Range Z", bubbleMotionScript.PositionRangeZ, 0.1f, 5.0f);
                bubbleMotionScript.StationarySpeed = EditorGUILayout.Slider("Speed", bubbleMotionScript.StationarySpeed, 0.01f, 1.0f);
                break;
            case BubbleMotionScript.BubbleMovementType.Floating:
                bubbleMotionScript.MaxX = EditorGUILayout.Slider("Max X", bubbleMotionScript.MaxX, 0f, 5.0f);
                bubbleMotionScript.MaxY = EditorGUILayout.Slider("Max Y", bubbleMotionScript.MaxY, 0f, 5.0f);
                bubbleMotionScript.MaxZ = EditorGUILayout.Slider("Max Z", bubbleMotionScript.MaxZ, 0f, 5.0f);
                bubbleMotionScript.FloatingSpeed = EditorGUILayout.Slider("Speed", bubbleMotionScript.FloatingSpeed, 0f, 5.0f);
                break;
            case BubbleMotionScript.BubbleMovementType.Moving:
                break;
        }
        if (GUI.changed)
        {
            EditorUtility.SetDirty(bubbleMotionScript);
        }
    }

}
#endif