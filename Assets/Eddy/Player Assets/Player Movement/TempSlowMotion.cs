using UnityEngine;
using System.Collections;

public class SlowMotionOnCapsLock : MonoBehaviour
{
    // ----------- KEYBINDS -----------
    [Header("Keybinds")]
    [Tooltip("Keybind for activating SlowMotion")]
    public KeyCode slowMotionKey = KeyCode.CapsLock;

    // ----------- ENABLEABLES -----------
    [Header("Enableables")]
    [Tooltip("Controls if slow motion can be used")]
    public bool canSlowMotion = true;
    [Tooltip("Set to true to use Toggle Mode, false for Duration Mode")]
    public bool slowMotionIsToggle = false;

    // ----------- SLOW MOTION SETTINGS -----------
    [Header("Slow Motion Settings")]
    [Tooltip("The time scale during slow motion (e.g., 0.3 for 30% speed)")]
    public float slowTimeScale = 0.3f;
    [Tooltip("Duration of the slow motion effect in seconds (used in Duration Mode)")]
    public float slowDuration = 2f;
    [Tooltip("Duration to ease back to normal time scale in seconds")]
    public float easeBackDuration = 1f;

    // ----------- PRIVATE VARIABLES -----------
    [Header("Private Variables")]
    // Internal state
    private bool isSlowMotionActive = false;
    // Coroutine reference for duration-based slow motion
    private Coroutine slowMotionCoroutine = null;

    void Start()
    {
        // Ensure Time.timeScale starts at 1
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }

    void Update()
    {
        // Early exit if slow motion is disabled
        if (!canSlowMotion)
        {
            if (isSlowMotionActive)
            {
                ToggleSlowMotion(false);
                Debug.Log("SlowMotionOnCapsLock: Slow motion disabled by canSlowMotion.");
            }
            return; // Skip the rest of the Update since slow motion is disabled
        }

        // Detect slow motion key press to toggle or trigger slow motion based on mode
        if (Input.GetKeyDown(slowMotionKey))
        {
            if (slowMotionIsToggle)
            {
                // Toggle-Based Slow Motion
                ToggleSlowMotion(!isSlowMotionActive);
            }
            else
            {
                // Duration-Based Slow Motion
                if (slowMotionCoroutine == null)
                {
                    slowMotionCoroutine = StartCoroutine(HandleSlowMotionDuration());
                }
            }
        }
    }

    /// <param name="activate">If true, activates slow motion; otherwise, deactivates it.</param>
    private void ToggleSlowMotion(bool activate)
    {
        if (activate)
        {
            // Activate slow motion
            Time.timeScale = slowTimeScale;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;

            isSlowMotionActive = true;
            Debug.Log("<color=cyan>Slow motion activated.</color>");
        }
        else
        {
            // Deactivate slow motion by easing back to normal
            StartCoroutine(EaseBackToNormalTime());
        }
    }

    private IEnumerator HandleSlowMotionDuration()
    {
        // Activate slow motion
        Time.timeScale = slowTimeScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        isSlowMotionActive = true;
        Debug.Log("<color=cyan>Slow motion activated.</color>");

        // Wait for the slow duration (unscaled time)
        float elapsed = 0f;
        while (elapsed < slowDuration)
        {
            if (!canSlowMotion)
            {
                // If slow motion is disabled during slow motion, exit coroutine
                yield break;
            }

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        // Start easing back to normal time scale
        yield return StartCoroutine(EaseBackToNormalTime());

        // Reset coroutine reference
        slowMotionCoroutine = null;
    }

    private IEnumerator EaseBackToNormalTime()
    {
        float elapsed = 0f;
        float initialTimeScale = Time.timeScale;

        while (elapsed < easeBackDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / easeBackDuration);
            Time.timeScale = Mathf.Lerp(initialTimeScale, 1f, t);
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
            yield return null;
        }

        // Ensure timeScale is exactly 1
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        isSlowMotionActive = false;
        Debug.Log("<color=cyan>Slow motion deactivated.</color>");
    }

    void OnDisable()
    {
        if (isSlowMotionActive || slowMotionCoroutine != null)
        {
            StopSlowMotion();
            Debug.LogWarning("SlowMotionOnCapsLock was disabled. Resetting Time.timeScale.");
        }
    }

    private void StopSlowMotion()
    {
        if (slowMotionCoroutine != null)
        {
            StopCoroutine(slowMotionCoroutine);
            slowMotionCoroutine = null;
        }

        // Reset time scale to normal
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        isSlowMotionActive = false;

        Debug.LogWarning("SlowMotionOnCapsLock: Slow motion was interrupted and reset.");
    }
}
