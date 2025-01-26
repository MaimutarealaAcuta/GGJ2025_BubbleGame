using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BubbleTypeScript))]
public class BubbleTypeCustomEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // switch BubbleType
        BubbleTypeScript bubbleTypeScript = (BubbleTypeScript)target;

        bubbleTypeScript.bubbleType = (BubbleTypeScript.BubbleType)EditorGUILayout.EnumPopup("Bubble Type", bubbleTypeScript.bubbleType);

        switch (bubbleTypeScript.bubbleType)
        {
            case BubbleTypeScript.BubbleType.Platform:
                bubbleTypeScript.DecayTime = EditorGUILayout.FloatField("Decay Time", bubbleTypeScript.DecayTime);
                bubbleTypeScript.RespawnTime = EditorGUILayout.FloatField("Respawn Time", bubbleTypeScript.RespawnTime);
                break;
            case BubbleTypeScript.BubbleType.Jump:
                bubbleTypeScript.DecayTime = EditorGUILayout.FloatField("Decay Time", bubbleTypeScript.DecayTime);
                bubbleTypeScript.RespawnTime = EditorGUILayout.FloatField("Respawn Time", bubbleTypeScript.RespawnTime);
                bubbleTypeScript.jumpDirection = (BubbleTypeScript.JumpDirection)EditorGUILayout.EnumPopup("Jump Direction", bubbleTypeScript.jumpDirection);
                bubbleTypeScript.jumpForce = EditorGUILayout.FloatField("Jump Force", bubbleTypeScript.jumpForce);
                break;
            case BubbleTypeScript.BubbleType.Death:
                bubbleTypeScript.DecayTime = EditorGUILayout.FloatField("Decay Time", bubbleTypeScript.DecayTime);
                bubbleTypeScript.RespawnTime = EditorGUILayout.FloatField("Respawn Time", bubbleTypeScript.RespawnTime);
                break;
            case BubbleTypeScript.BubbleType.Portal:
                bubbleTypeScript.NextSceneName = EditorGUILayout.TextField("Next Scene Name", bubbleTypeScript.NextSceneName);
                break;
            case BubbleTypeScript.BubbleType.Collectible:
                bubbleTypeScript.CollectibleName = EditorGUILayout.TextField("Collectible Name", bubbleTypeScript.CollectibleName);
                break;
        }
    }
}
