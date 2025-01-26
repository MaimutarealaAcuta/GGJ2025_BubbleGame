using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum CollectableTypes
{
    DoubleJump,
    Dash,
    Gun
}

public class Collectable : MonoBehaviour
{
    [SerializeField] private MovementPreset preset;
    [SerializeField] private CollectableTypes collectableName;
    [SerializeField] private GameObject gun;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            switch (collectableName)
            {
                case CollectableTypes.DoubleJump:
                    preset.canDoubleJump = true;
                    break;
                case CollectableTypes.Dash:
                    preset.canDash = true;
                    break;
                case CollectableTypes.Gun:
                    gun.SetActive(true);
                    break;
                default:
                    break;
            }
            Destroy(gameObject);
        }
    }
}
