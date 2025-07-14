using System;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]

public class PlayerUnsheathed : PlayerState
{
    private const float RUN_ANIMATION_SPEED = 1.2f;

    public float movementSpeed;

    private PlayerBehavior.Direction previousDirection;

    private float timeSincePreviousDirection;
    private bool isIdle;

    private float animationPoint;
    private bool usingLeftHand;
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
        usingLeftHand = player.spriteRenderer.flipX;
        stepCount = 0;
    }

    public override void Update()
    {
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

        // Unsheathed running sprites should truly be 8 directional,
        // and the arm holding the sword is determined by which direction the player is facing when entering the unsheathed state
        player.animator.Play($"RUN_{GetHandCorrectedDirection(player.facingDirection.ToString())}_UNSHEATHED", 0, animationPoint);
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

        player.animator.Play($"IDLE_{GetHandCorrectedDirection(player.facingDirection.ToString())}_UNSHEATHED");
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

    private string GetHandCorrectedDirection(string direction)
    {
        if (!usingLeftHand)
            return direction;

        string correctedDirection = direction.Replace("RIGHT", "TEMP");
        correctedDirection = correctedDirection.Replace("LEFT", "RIGHT");
        correctedDirection = correctedDirection.Replace("TEMP", "LEFT");

        return correctedDirection;
    }

    public override void SwordInput(InputAction.CallbackContext context)
    {
        if (context.performed)
            player.ChangeState(player.stanceState);
    }
}
