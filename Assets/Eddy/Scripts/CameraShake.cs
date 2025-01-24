using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;
    private float shakeDuration;
    private float shakeMagnitude;

    void Awake() => Instance = this;

    public void TriggerShake(float duration, float magnitude)
    {
        shakeDuration = duration;
        shakeMagnitude = magnitude;
    }

    void Update()
    {
        if (shakeDuration > 0)
        {
            transform.localPosition += Random.insideUnitSphere * shakeMagnitude;
            shakeDuration -= Time.deltaTime;
        }
    }
}