using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]

public class PlayerStance : PlayerState
{
    private enum BufferEntry
    {
        UP,
        UP_RIGHT,
        RIGHT,
        DOWN_RIGHT,
        DOWN,
        DOWN_LEFT,
        LEFT,
        UP_LEFT,
        NEUTRAL,
        SHEATHE,
        UNSHEATHE
    }
    private List<BufferEntry> bufferList;

    private const int MAX_BUFFER_SIZE = 32;
    private const float NEUTRAL_MAGNITUDE_WINDOW = 0.1f;

    private const int SHAKE_AMOUNT = 2;
    private const float SHAKE_INTENSITY = 1.0f / 32.0f;

    public float releaseDuration;
    public float finalFrameDuration;

    private float maxMagnitude;
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
        maxMagnitude = 0.0f;
        timeSpentReleasing = 0.0f;
        holding = true;

        initialDirection = player.facingDirection;
        previousSpriteFlipX = player.spriteRenderer.flipX;
        player.unsheathedState.swapHandednesLogic = false;
        player.Shake(SHAKE_AMOUNT, SHAKE_INTENSITY);

        bufferList = new();
        AddEntryToBuffer(BufferEntry.SHEATHE);
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


        StringBuilder sb = new();
        foreach(BufferEntry entry in ParsedBuffer())
        {
            sb.Append($"{entry.ToString()}, ");
        }

        //Debug.Log(sb);
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
        if (player.LeftStickInput == Vector2.zero)
        {
            maxMagnitude = 0.0f;
            player.animator.Play("STANCE_NEUTRAL");
            if (!CurrentEntryMatches(BufferEntry.NEUTRAL))
                AddEntryToBuffer(BufferEntry.NEUTRAL);
            return;
        }

        // If a direction is held, the requirements for reaching NEUTRAL become less strict
        // depending on the current magnitude of the stick, allowing for easier same-direction flicking.
        // An example: The current held direction is RIGHT, and the magnitude is 0.9f.
        // If the direction never changes from RIGHT and the magnitude is lowered to 0.8f, the current direction is changed to neutral.
        // If any direction other than RIGHT is held afterwards, the regular rules apply.
        // However, if RIGHT is the next target direction, and the magnitude never goes below the deadzone (meaning it reaches zero),
        // you would need to have a magnitude above 0.8f to change the direction back to RIGHT.
        float currentMagnitude = player.LeftStickInput.magnitude;
        maxMagnitude = maxMagnitude < currentMagnitude ? currentMagnitude : maxMagnitude;

        PlayerBehavior.Direction currentDirection = player.GetDirectionOfVector(player.LeftStickInput);
        if (currentDirection != player.facingDirection)
            maxMagnitude = 0.0f;

        player.facingDirection = currentDirection;

        if (currentMagnitude < maxMagnitude - NEUTRAL_MAGNITUDE_WINDOW)
        {
            player.animator.Play("STANCE_NEUTRAL");
            if (!CurrentEntryMatches(BufferEntry.NEUTRAL))
                AddEntryToBuffer(BufferEntry.NEUTRAL);
            return;
        }

        if (!CurrentEntryMatches((BufferEntry)currentDirection))
        {
            AddCardinalEntryBetweenDiagonals((BufferEntry)currentDirection); // pls no race condition
            AddEntryToBuffer((BufferEntry)currentDirection);
        }

        player.animator.Play($"STANCE_{player.GetDirectionName()}");
    }

    private void UpdateReleasing()
    {
        timeSpentReleasing += Time.deltaTime;

        player.facingDirection = initialDirection;

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

    private void AddCardinalEntryBetweenDiagonals(BufferEntry entry)
    {
        BufferEntry previousEntry = bufferList[bufferList.Count - 1];

        int currentEntryValue = (int)entry;
        int previousEntryValue = (int)previousEntry;

        if (currentEntryValue > 7 || previousEntryValue > 7) // At least one entry is out of bounds to be a direction
            return;

        if (currentEntryValue % 2 == 0 || previousEntryValue % 2 == 0) // At least one entry is not a diagonal direction
            return;

        int difference = Mathf.Abs(currentEntryValue - previousEntryValue);

        if (difference != 2 && difference != 6) // The directions are adjacent relatively speaking
            return;

        string currentEntryString = entry.ToString();
        string previousEntryString = previousEntry.ToString();

        if (currentEntryString.Contains("UP") && previousEntryString.Contains("UP"))
            AddEntryToBuffer(BufferEntry.UP);
        else if (currentEntryString.Contains("DOWN") && previousEntryString.Contains("DOWN"))
            AddEntryToBuffer(BufferEntry.DOWN);
        else if (currentEntryString.Contains("LEFT") && previousEntryString.Contains("LEFT"))
            AddEntryToBuffer(BufferEntry.LEFT);
        else if (currentEntryString.Contains("RIGHT") && previousEntryString.Contains("RIGHT"))
            AddEntryToBuffer(BufferEntry.RIGHT);
    }

    private void AddEntryToBuffer(BufferEntry entry)
    {
        if (bufferList.Count >= MAX_BUFFER_SIZE)
            bufferList.RemoveAt(0);

        bufferList.Add(entry);
    }

    private List<BufferEntry> ParsedBuffer()
    {
        List<BufferEntry> parsedBuffer = new List<BufferEntry>(bufferList);

        // Clear diagonals not preceeded by NEUTRAL and replace all other diagonals with LEFT/RIGHT
        // Clear cardinal directions preceeded by a diagonal which in itself is preceeded by the previous cardinal direction
        // (or NEUTRAL/SHEATHE if the previous cardinal direction was LEFT/RIGHT)
        for (int i = parsedBuffer.Count - 1; i > 0; i--)
        {
            if ((int)parsedBuffer[i] > 7) // Entry is out of bounds to be a direction
                continue;

            if ((int)parsedBuffer[i] % 2 == 1) // Entry is a diagonal direction
            {
                if (parsedBuffer[i - 1] == BufferEntry.NEUTRAL || parsedBuffer[i - 1] == BufferEntry.SHEATHE)
                {
                    if (parsedBuffer[i] == BufferEntry.UP_RIGHT || parsedBuffer[i] == BufferEntry.DOWN_RIGHT)
                        parsedBuffer[i] = BufferEntry.RIGHT;
                    else
                        parsedBuffer[i] = BufferEntry.LEFT;
                    continue;
                }

                parsedBuffer.RemoveAt(i);
            }
            else // Entry is a cardinal direction
            {
                if (i <= 1)
                    continue;

                if ((int)parsedBuffer[i - 1] % 2 == 0)
                    continue;

                if (parsedBuffer[i - 2] == parsedBuffer[i])
                {
                    parsedBuffer.RemoveAt(i);
                    continue;
                }

                if (parsedBuffer[i - 2] == BufferEntry.NEUTRAL && (parsedBuffer[i] == BufferEntry.LEFT || parsedBuffer[i] == BufferEntry.RIGHT))
                    parsedBuffer.RemoveAt(i);
            }
        }

        // Clear all NEUTRAL
        for (int i = parsedBuffer.Count - 1; i >= 0; i--)
        {
            if (parsedBuffer[i] == BufferEntry.NEUTRAL)
            {
                parsedBuffer.RemoveAt(i);
                continue;
            }
        }

        // Convert diagonal at first position if it exists
        if ((int)parsedBuffer[0] <= 7 && (int)parsedBuffer[0] % 2 == 1)
        {
            if (parsedBuffer[0] == BufferEntry.UP_RIGHT || parsedBuffer[0] == BufferEntry.DOWN_RIGHT)
                parsedBuffer[0] = BufferEntry.RIGHT;
            else
                parsedBuffer[0] = BufferEntry.LEFT;
        }

        return parsedBuffer;
    }

    private bool BufferMatches(List<BufferEntry> technique)
    {
        // Buffer is too short
        if (technique.Count > ParsedBuffer().Count)
            return false;

        int startIndex = ParsedBuffer().Count - technique.Count;
        for (int i = 0; i < technique.Count; i++)
        {
            // Buffer does not match technique
            if (ParsedBuffer()[i + startIndex] != technique[i])
                return false;
        }
        // Buffer matches technique
        return true;
    }

    private bool CurrentEntryMatches(BufferEntry entry)
    {
        return bufferList[bufferList.Count - 1] == entry;
    }

    private bool CurrentEntryIsDirection()
    {
        return (int)bufferList[bufferList.Count - 1] < 8;
    }

    public override void SwordInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            AddEntryToBuffer(BufferEntry.SHEATHE);
            player.Shake(SHAKE_AMOUNT, SHAKE_INTENSITY);

            holding = true;
        }

        if (context.canceled)
        {
            // some big fucked up chain here depending on if a move combo has been performed
            if (BufferMatches(new List<BufferEntry> { BufferEntry.SHEATHE }))
            {
                timeSpentReleasing = 0.0f;
            }
            else if (CurrentEntryIsDirection()) // Sword slashes
            {
                player.ChangeState(player.swingState);
            }
            else
            {
                timeSpentReleasing = releaseDuration - finalFrameDuration;
            }

            AddEntryToBuffer(BufferEntry.UNSHEATHE);

            holding = false;
        }
    }
}
