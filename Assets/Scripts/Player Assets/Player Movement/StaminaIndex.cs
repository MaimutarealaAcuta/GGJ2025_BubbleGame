using UnityEngine;
using System;

public class StaminaIndex : MonoBehaviour
{
    public event Action OnStaminaDepleted;
    public event Action OnStaminaRegenStarted;
    public event Action OnStaminaFullyRegenerated;

    [Header("Enableables")]
    [Tooltip("Whether stamina can regenerate.")]
    public bool canRegenStamina = true;

    [Header("Stamina Core Settings")]
    [Tooltip("Maximum stamina.")]
    public float maxStamina = 100f;
    [Tooltip("Current stamina value.")]
    public float currentStamina = 100f;
    [Tooltip("How fast stamina regenerates per second after a delay.")]
    public float staminaRegenRate = 10f;
    [Tooltip("Time after last stamina usage before regen starts.")]
    public float staminaRegenDelay = 2f;
    [HideInInspector] public float lastStaminaUseTime = 0f;

    [Header("Stamina Toggles & Costs")]
    [Tooltip("If true, sprint will consume stamina.")]
    public bool sprintUsesStamina = true;
    [Tooltip("Stamina cost per second of sprinting.")]
    public float sprintCost = 5f;

    [Space]
    [Tooltip("If true, jumping consumes stamina.")]
    public bool jumpUsesStamina = true;
    public float jumpCost = 10f;

    [Space]
    [Tooltip("If true, double-jumping consumes stamina.")]
    public bool doubleJumpUsesStamina = true;
    public float doubleJumpCost = 10f;

    [Space]
    [Tooltip("If true, wall-jumping consumes stamina.")]
    public bool wallJumpUsesStamina = true;
    public float wallJumpCost = 10f;

    [Space]
    [Tooltip("If true, dashing consumes stamina.")]
    public bool dashUsesStamina = true;
    public float dashCost = 20f;

    [Space]
    [Tooltip("If true, sliding consumes stamina.")]
    public bool slideUsesStamina = true;
    public float slideCost = 3f;

    [Space]
    [Tooltip("If true, climbing consumes stamina.")]
    public bool climbUsesStamina = true;
    [Tooltip("How much stamina per second of climbing.")]
    public float climbCostPerSecond = 20f;

    [Space]
    [Tooltip("If true, grabbing/holding items consumes stamina.")]
    public bool grabUsesStamina = true;
    [Tooltip("Base stamina cost for grabbing an object.")]
    public float grabBaseCost = 1f;
    [Tooltip("Multiplier for how object weight adds to stamina cost.")]
    public float grabWeightMultiplier = 1.5f;

    private void Start()
    {
        currentStamina = maxStamina;
    }

    private void Update()
    {
        HandleStaminaRegen();
    }

    public void HandleStaminaRegen()
    {
        if (!canRegenStamina) return;
        if (currentStamina >= maxStamina) return;

        // If enough time has passed since last usage, begin regeneration
        if (Time.time - lastStaminaUseTime >= staminaRegenDelay)
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
            currentStamina = Mathf.Min(currentStamina, maxStamina);

            // Send optional events
            if (currentStamina > 0 && currentStamina < maxStamina)
                OnStaminaRegenStarted?.Invoke();

            if (currentStamina == maxStamina)
                OnStaminaFullyRegenerated?.Invoke();
        }
    }

    public bool UseStamina(float amount)
    {
        if (currentStamina < amount)
            return false; // Not enough stamina

        currentStamina -= amount;
        lastStaminaUseTime = Time.time;

        if (currentStamina <= 0)
            OnStaminaDepleted?.Invoke();

        return true;
    }

    // ------------------------------------------------------------------------
    // Per-action "Regulate" methods, so other scripts only call these:
    // ------------------------------------------------------------------------

    public bool RegulateSprintStamina(float deltaTime)
    {
        if (!sprintUsesStamina) return true;
        float costThisFrame = sprintCost * deltaTime;
        return UseStamina(costThisFrame);
    }

    public bool RegulateJumpStamina()
    {
        if (!jumpUsesStamina) return true;
        return UseStamina(jumpCost);
    }

    public bool RegulateDoubleJumpStamina()
    {
        if (!doubleJumpUsesStamina) return true;
        return UseStamina(doubleJumpCost);
    }

    public bool RegulateWallJumpStamina()
    {
        if (!wallJumpUsesStamina) return true;
        return UseStamina(wallJumpCost);
    }

    public bool RegulateDashStamina()
    {
        if (!dashUsesStamina) return true;
        return UseStamina(dashCost);
    }

    public bool RegulateSlideStamina()
    {
        if (!slideUsesStamina) return true;
        return UseStamina(slideCost);
    }

    public bool RegulateClimbStamina()
    {
        if (!climbUsesStamina) return true;
        float staminaCost = climbCostPerSecond * Time.deltaTime;
        return UseStamina(staminaCost);
    }

    public bool RegulateGrabStamina(float objectWeight)
    {
        if (!grabUsesStamina) return true;
        float staminaCost = grabBaseCost + (objectWeight * grabWeightMultiplier);
        return UseStamina(staminaCost * Time.deltaTime);
    }
}
