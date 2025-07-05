using System;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]

public class PlayerSheathed : PlayerState
{
    private const float RUN_ANIMATION_SPEED = 1.0f;

    public float movementSpeed;

    private PlayerBehavior.Direction previousDirection;

    private float timeSincePreviousDirection;
    private bool isIdle;

    private float animationPoint;
    private int stepCount;

    public override void OnValidate(PlayerBehavior player)
    {
        base.OnValidate(player);
    }

    public override void Enter()
    {
        previousDirection = player.facingDirection;
        timeSincePreviousDirection = 0.0f;
        animationPoint = 0.0f;
        stepCount = 0;
    }

    public override void Update()
    {
        UpdateFlip();

        if (player.LeftStickInput != Vector2.zero)
        {
            UpdateRunning();
            isIdle = false;
        }
        else
        {
            if (!isIdle)
                EnteredIdle();

            UpdateIdle();
            isIdle = true;
        }
    }

    private void UpdateFlip()
    {
        if (player.facingDirection == PlayerBehavior.Direction.UP || player.facingDirection == PlayerBehavior.Direction.DOWN)
            return;

        if (player.LeftStickInput.x > 0.0f)
            player.spriteRenderer.flipX = true;
        else if (player.LeftStickInput.x < 0.0f)
            player.spriteRenderer.flipX = false;
    }

    private void UpdateRunning()
    {
        PlayerBehavior.Direction currentDirection = player.GetDirectionOfVector(player.LeftStickInput);
        if (player.facingDirection != currentDirection)
            ChangeDirection(currentDirection);

        animationPoint += Time.deltaTime * RUN_ANIMATION_SPEED * 2.0f; // 6 frames, 12 fps
        timeSincePreviousDirection += Time.deltaTime;

        if (animationPoint >= 1.0f)
            animationPoint -= 1.0f;

        UpdateFootstep(animationPoint);

            Vector2 runVelocity = player.LeftStickInput.normalized * movementSpeed;
        player.movementForce = runVelocity;

        player.animator.Play($"RUN_{player.GetDirectionName()}_SHEATHED", 0, animationPoint);
    }

    private void UpdateFootstep(float animationPoint)
    {
        if (animationPoint > 0.0f && animationPoint < 0.5f && stepCount % 2 == 0)
            Footstep();
        else if (animationPoint > 0.5f && stepCount % 2 == 1)
            Footstep();
    }

    private void Footstep()
    {
        // play a sound

        if (stepCount > 0)
            player.SpawnDustCloud();

        stepCount++;
    }

    private void ChangeDirection(PlayerBehavior.Direction currentDirection)
    {
        previousDirection = player.facingDirection;
        player.facingDirection = currentDirection;

        timeSincePreviousDirection = 0.0f;
    }

    private void UpdateIdle()
    {
        player.movementForce = Vector2.zero;

        player.animator.Play($"IDLE_{player.GetDirectionName()}_SHEATHED");
    }

    private void EnteredIdle()
    {
        animationPoint = 0.0f;
        stepCount = 0;

        CorrectDirection();
    }

    private void CorrectDirection()
    {
        int currentDirectionValue = (int)player.facingDirection;
        int previousDirectionValue = (int)previousDirection;

        if (!IsCurrentNextToPrevious(currentDirectionValue, previousDirectionValue))
            return;

        float maxTimePassedAllowed = 0.03f;

        if (timeSincePreviousDirection > maxTimePassedAllowed)
            return;

        // Odd = diagonal direction
        if ((int)previousDirection % 2 == 1)
            ChangeDirection(previousDirection);
    }

    private bool IsCurrentNextToPrevious(int current, int previous)
    {
        int difference = Math.Abs(current - previous);
        // Since there are 8 directions, we also need to take the difference between min and max direction values into account
        return (difference == 1 || difference == 7);
    }

    public override void SwordInput(InputAction.CallbackContext context)
    {
        if (context.performed)
            player.ChangeState(player.stanceState);
    }
}
