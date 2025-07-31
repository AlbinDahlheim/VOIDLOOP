using System.Collections;
using UnityEngine;

public class Sliceable : MonoBehaviour
{
    public float hitstopDuration;

    public SpriteRenderer spriteRenderer;

    // TODO: Add HP if it should be reused for big boys
    // TODO: Keep track of if the object has been sliced this cycle

    public void Slice(Vector2 slicerPosition, int? facingAngle = null)
    {
        Vector2 angleVector = new Vector2(transform.position.x, transform.position.y) - slicerPosition;

        int angle = Mathf.RoundToInt(Vector2.SignedAngle(Vector2.up, angleVector.normalized));
        angle = angle <= 0 ? angle * -1 : 360 - angle;

        // Play some sort of sound, maybe spawn something empty that does it? idk
        float directionMultiplier = angle > 180 ? -1.0f : 1.0f; 
        StartCoroutine(Hitstop(hitstopDuration));
        StartCoroutine(HitShake(hitstopDuration, angleVector));
    }

    private IEnumerator Hitstop(float duration)
    {
        Time.timeScale = 0.0f; // What happens if multiple things got hit?
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1.0f;
    }

    private IEnumerator HitShake(float duration, Vector3 direction)
    {
        Vector3 originalPos = spriteRenderer.gameObject.transform.localPosition;
        float intensity = 1.0f / 16.0f;

        spriteRenderer.gameObject.transform.localPosition = originalPos + direction.normalized * intensity;

        int currentShakes = 1;
        float timePassed = 0.0f;
        float durationPerShake = 0.025f;
        while (timePassed < duration)
        {
            yield return null;
            timePassed += Time.unscaledDeltaTime;

            if (timePassed >= durationPerShake * currentShakes)
            {
                float directionMultiplier = currentShakes % 2 == 0 ? 1.0f : -1.0f; // rename to direction multiplier
                Vector3 offset = direction.normalized * intensity * directionMultiplier;

                spriteRenderer.gameObject.transform.localPosition = originalPos + offset;
                currentShakes++;
            }
        }

        spriteRenderer.gameObject.transform.localPosition = originalPos;
    }
}
