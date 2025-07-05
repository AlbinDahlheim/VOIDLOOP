using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]

public class PlayerStance : PlayerState
{
    public float releaseDuration;
    private float timeSpentReleasing;
    private bool holding;

    private PlayerBehavior.Direction initialDirection;
    private bool previousSpriteFlipX;

    public override void OnValidate(PlayerBehavior player)
    {
        base.OnValidate(player);
    }

    public override void Enter()
    {
        player.movementForce = Vector2.zero;
        timeSpentReleasing = 0.0f;
        holding = true;

        initialDirection = player.facingDirection;
        previousSpriteFlipX = player.spriteRenderer.flipX;
    }

    public override void Update()
    {
        if (holding)
        {
            UpdateFlip();
            UpdateHolding();
        }
        else
            UpdateReleasing();
        
    }

    private void UpdateFlip()
    {
        if (player.facingDirection == PlayerBehavior.Direction.UP || player.facingDirection == PlayerBehavior.Direction.DOWN)
            return;

        if (player.LeftStickInput.x > 0.0f)
            player.spriteRenderer.flipX = true;
        else if (player.LeftStickInput.x < 0.0f)
            player.spriteRenderer.flipX = false;

        previousSpriteFlipX = player.spriteRenderer.flipX;
    }

    private void UpdateHolding()
    {
        timeSpentReleasing = 0.0f;

        if (player.LeftStickInput == Vector2.zero)
        {
            player.animator.Play("STANCE_NEUTRAL");
            return;
        }

        PlayerBehavior.Direction currentDirection = player.GetDirectionOfVector(player.LeftStickInput);
        if (player.facingDirection != currentDirection)
            ChangeDirection(currentDirection);

        player.animator.Play($"STANCE_{player.GetDirectionName()}");
    }

    private void UpdateReleasing()
    {
        timeSpentReleasing += Time.deltaTime;

        player.facingDirection = initialDirection;
        float finalFrameDuration = 0.05f;

        if (timeSpentReleasing >= releaseDuration - finalFrameDuration)
        {
            // Kinda horrible, but I do want to do something unique in each case
            switch(player.facingDirection)
            {
                case PlayerBehavior.Direction.UP:
                    player.animator.Play("IDLE_UP_SIDE_SHEATHED");
                    break;
                case PlayerBehavior.Direction.UP_RIGHT:
                    player.spriteRenderer.flipX = true;
                    player.animator.Play("IDLE_SIDE_SHEATHED");
                    break;
                case PlayerBehavior.Direction.RIGHT:
                    player.spriteRenderer.flipX = true;
                    player.animator.Play("IDLE_DOWN_SIDE_SHEATHED");
                    break;
                case PlayerBehavior.Direction.DOWN_RIGHT:
                    if (previousSpriteFlipX)
                        player.animator.Play("STANCE_NEUTRAL");
                    else
                    {
                        player.spriteRenderer.flipX = true;
                        player.animator.Play("IDLE_DOWN_SHEATHED");
                    }
                    break;
                case PlayerBehavior.Direction.DOWN:
                    player.animator.Play("IDLE_DOWN_SIDE_SHEATHED");
                    break;
                case PlayerBehavior.Direction.DOWN_LEFT:
                    if (!previousSpriteFlipX)
                        player.animator.Play("STANCE_NEUTRAL");
                    else
                    {
                        player.spriteRenderer.flipX = false;
                        player.animator.Play("IDLE_DOWN_SHEATHED");
                    }
                    break;
                case PlayerBehavior.Direction.LEFT:
                    player.spriteRenderer.flipX = false;
                    player.animator.Play("IDLE_DOWN_SIDE_SHEATHED");
                    break;
                case PlayerBehavior.Direction.UP_LEFT:
                    player.spriteRenderer.flipX = false;
                    player.animator.Play("IDLE_SIDE_SHEATHED");
                    break;
            }
        }
        else
            player.animator.Play("STANCE_LEAVE");

        if (timeSpentReleasing >= releaseDuration)
            player.ChangeState(player.sheathedState);

        if (player.LeftStickInput != Vector2.zero)
            player.ChangeState(player.sheathedState);
    }

    private void ChangeDirection(PlayerBehavior.Direction currentDirection)
    {
        //previousDirection = player.facingDirection;
        player.facingDirection = currentDirection;
    }

    public override void SwordInput(InputAction.CallbackContext context)
    {
        if (context.performed)
            holding = true;

        if (context.canceled)
        {
            // some big fucked up chain here depending on if a move combo has been performed
            holding = false;
        }
    }
}
