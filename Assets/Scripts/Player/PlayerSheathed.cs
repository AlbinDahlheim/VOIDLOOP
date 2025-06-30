using UnityEngine;

[System.Serializable]

public class PlayerSheathed : PlayerState
{
    public float movementSpeed;

    public override void OnValidate(PlayerBehavior player)
    {
        base.OnValidate(player);
    }

    public override void Enter()
    {
        // do stuff
    }

    public override void Update()
    {
        FlipDirection();

        if (player.LeftStickInput != Vector2.zero)
            UpdateRunning();
        else
            UpdateIdle();
    }

    private void FlipDirection()
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
        player.facingDirection = player.GetDirectionOfVector(player.LeftStickInput);

        Vector2 runVelocity = player.LeftStickInput.normalized * movementSpeed;
        player.movementForce = runVelocity;

        player.animator.Play($"RUN_{player.GetDirectionName()}_SHEATHED");
    }

    private void UpdateIdle()
    {
        player.movementForce = Vector2.zero;

        player.animator.Play($"IDLE_{player.GetDirectionName()}_SHEATHED");
    }
}
