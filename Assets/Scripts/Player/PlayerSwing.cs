using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]

public class PlayerSwing : PlayerState
{
    public float duration;
    private float timePassed;

    public float movementForce;
    public float decreasePerSecond;
    private Vector2 velocityDirection;

    private PlayerBehavior.Direction storedDirection;
    private bool returnToStance;

    public override void OnValidate(PlayerBehavior player)
    {
        base.OnValidate(player);
    }

    public override void Enter()
    {
        timePassed = 0.0f;
        storedDirection = player.facingDirection;
        returnToStance = false;

        if (player.LeftStickInput != Vector2.zero)
        {
            SetDirection(player.LeftStickInput);
            velocityDirection = player.LeftStickInput.normalized;
        }
        else
            velocityDirection = player.GetVectorOfDirection(player.facingDirection);

        SetVelocity();
        SetAnimation();

        // Not in enter, not even in this script, just want to make notes:
        // Logic for enemies that die in one hit:
        // They all get split in two (probably)
        // During the player swing, the lower remains have a lower order in layer than the player (to make the sword appear over the enemy)
        // The upper remains have a higher order in layer than the player (to appear over the sword)
        // The lower remains slide along the ground a short bit, it keeps the shadow
        // The upper remains fly in an arc (sprite has a Y offset) while having a new shadow display its actual position and slides along the ground (but further than lower)
        // After the swing is finished, both remains follow the regular order in layer sorting rule

        // COOL THING: All entities should have the same order in layer, and then use Y sorting to do the niceies
    }

    public override void Exit()
    {
        player.facingDirection = storedDirection;
    }

    public override void Update()
    {
        timePassed += Time.deltaTime;
        if (timePassed >= duration)
        {
            if (returnToStance)
                player.ChangeState(player.stanceState);
            else
                player.ChangeState(player.unsheathedState);
        }
    }

    private void SetDirection(Vector2 leftStickInput)
    {
        Vector2 direction = leftStickInput.normalized;

        if (direction.x > 0.0f)
            player.spriteRenderer.flipX = true;
        else if (direction.x < 0.0f)
            player.spriteRenderer.flipX = false;

        int angle = Mathf.RoundToInt(Vector2.SignedAngle(Vector2.up, direction));
        angle = angle <= 0 ? angle * -1 : 360 - angle;
        int angleValue = direction.x > 0.0f ? 2 : 6; // If x = 0, then the angleValue is incorrect, but it should be replaced below

        int verticalRange = 60 / 2; // Desired range gets split in half

        if (angle < verticalRange || angle > (360 - verticalRange))
            angleValue = 0;
        else if (angle < (180 + verticalRange) && angle > (180 - verticalRange))
            angleValue = 4;

        if (direction.y > 0.0f)
        {
            if (angleValue == 2)
                angleValue = 1;
            else if (angleValue == 6)
                angleValue = 7;
        }

        player.facingDirection = (PlayerBehavior.Direction)angleValue;
    }

    private void SetAnimation()
    {
        string direction = player.GetDirectionName();
        direction = direction.Replace("DOWN_", "");

        if (direction == "UP" || direction == "SIDE")
            player.unsheathedState.swapHandednesLogic = true;

        player.animator.Play($"SWORD_SWING_{direction}");
    }

    private void SetVelocity()
    {
        player.movementForce = Vector2.zero;
        player.UpdateInternalForce(velocityDirection.normalized * movementForce, decreasePerSecond);
    }

    public override void SwordInput(InputAction.CallbackContext context)
    {
        if (context.performed)
            returnToStance = true;

        if (context.canceled)
            returnToStance = false;
    }
}
