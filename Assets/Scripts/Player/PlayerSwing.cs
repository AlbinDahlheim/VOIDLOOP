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

    public override void OnValidate(PlayerBehavior player)
    {
        base.OnValidate(player);
    }

    public override void Enter()
    {
        timePassed = 0.0f;
        storedDirection = player.facingDirection;

        if (player.LeftStickInput != Vector2.zero)
        {
            SetDirection(player.LeftStickInput);
            velocityDirection = player.LeftStickInput.normalized;
        }
        else
            velocityDirection = player.GetVectorOfDirection(player.facingDirection);

        SetVelocity();
        SetAnimation();
    }

    public override void Exit()
    {
        player.facingDirection = storedDirection;
    }

    public override void Update()
    {
        timePassed += Time.deltaTime;
        if (timePassed >= duration)
            player.ChangeState(player.unsheathedState);
    }

    private void SetDirection(Vector2 leftStickInput)
    {
        Vector2 direction = leftStickInput.normalized;

        int angle = Mathf.RoundToInt(Vector2.SignedAngle(Vector2.up, direction));
        angle = angle <= 0 ? angle * -1 : 360 - angle;
        int offset = angle == 45 || angle == 225 ? 46 : 44; // All diagonals on keyboard should result in SIDE
        int angleValue = (angle + offset) / 90 % 4;
        angleValue += angleValue; // Lock to cardinal directions

        Debug.Log($"angle: {angle}, value: {angleValue}");

        player.facingDirection = (PlayerBehavior.Direction)angleValue;
    }

    private void SetAnimation()
    {
        string direction = player.GetDirectionName();
        direction = direction.Replace("UP_", "");
        direction = direction.Replace("DOWN_", "");

        if (direction == "SIDE")
            player.unsheathedState.swapHandednesLogic = true;

        player.animator.Play($"SWORD_SWING_{direction}");
    }

    private void SetVelocity()
    {
        player.movementForce = Vector2.zero;
        player.UpdateInternalForce(velocityDirection.normalized * movementForce, decreasePerSecond);
    }
}
