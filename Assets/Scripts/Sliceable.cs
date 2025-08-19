using System.Collections;
using UnityEngine;

public class Sliceable : MonoBehaviour
{
    public int hp;

    public Color bloodColor;

    public float upperLaunchedDuration;
    public float upperLaunchedIntensity;
    public float upperLaunchedHeight;

    public float lowerLaunchedDuration;
    public float lowerLaunchedIntensity;
    public float lowerLaunchedHeight;

    public float hitstopDuration;

    public SpriteRenderer spriteRenderer;
    public GameObject mainParent;

    public GameObject remainsObject;
    public Sprite wholeRemains, lowerRemains, upperRemains; // Ordered to match what feels natural when making sprites

    // TODO: Keep track of if the object has been sliced this cycle

    public void Slice(Vector2 slicerPosition, int? facingAngle = null) // Damage type?
    {
        Vector2 angleVector = new Vector2(transform.position.x, transform.position.y) - slicerPosition;

        // Play some sort of sound, maybe spawn something empty that does it? idk

        StartCoroutine(Hitstop(hitstopDuration));
        StartCoroutine(HitShake(hitstopDuration, angleVector));
        StartCoroutine(Damage(hitstopDuration, angleVector));
    }

    private IEnumerator Hitstop(float duration)
    {
        // send hitstop to some sort of hitstop manager to make it work with multiple sources (DON'T JUST +=, BUT REPLACE IF HIGHER
        Time.timeScale = 0.0f; // What happens if multiple things got hit?
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1.0f;
    }

    private IEnumerator HitShake(float duration, Vector3 direction)
    {
        Vector3 originalPos = spriteRenderer.gameObject.transform.localPosition; // Maybe this should be set in start?
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
                float directionMultiplier = currentShakes % 2 == 0 ? 1.0f : -1.0f;
                Vector3 offset = direction.normalized * intensity * directionMultiplier;

                spriteRenderer.gameObject.transform.localPosition = originalPos + offset;
                currentShakes++;
            }
        }

        spriteRenderer.gameObject.transform.localPosition = originalPos;
    }

    private IEnumerator Damage(float duration, Vector2 direction) // add a damage type enum I guess
    {
        hp -= 1;
        yield return new WaitForSecondsRealtime(duration);

        if (hp <= 0)
            Die(direction);
    }

    private void Die(Vector2 direction)
    {
        // use blood color in some way
        bool flipX = direction.x < 0.0f;

        GameObject upper = Instantiate(remainsObject, transform.position, transform.rotation);
        upper.GetComponent<SlicedRemains>().SetSprite(upperRemains, flipX);
        upper.GetComponent<SlicedRemains>().Launch(AngleAdjusted(direction, 0, 135), upperLaunchedDuration, upperLaunchedIntensity, upperLaunchedHeight, 2, false);

        GameObject lower = Instantiate(remainsObject, transform.position, transform.rotation);
        lower.GetComponent<SlicedRemains>().SetSprite(lowerRemains, flipX);
        lower.GetComponent<SlicedRemains>().Launch(AngleAdjusted(direction, 0, 135), lowerLaunchedDuration, lowerLaunchedIntensity, lowerLaunchedHeight, 1, false);

        //Destroy(mainParent);
        StartCoroutine(TEMP_TESTING());
    }

    private Vector2 AngleAdjusted(Vector2 direction, int angleOffset, int randomizedRange)
    {
        int angle = Mathf.RoundToInt(Vector2.SignedAngle(Vector2.up, direction.normalized));
        angle = angle <= 0 ? angle * -1 : 360 - angle;

        angleOffset *= angle > 180 ? -1 : 1;

        randomizedRange += randomizedRange % 2 == 1 ? 1 : 0;
        randomizedRange /= 2;

        angle += Random.Range(-randomizedRange, randomizedRange + 1);
        angle += angleOffset;

        return new Vector2(Mathf.Sin(angle * Mathf.Deg2Rad), Mathf.Cos(angle * Mathf.Deg2Rad)).normalized;
    }

    private IEnumerator TEMP_TESTING()
    {
        spriteRenderer.enabled = false;
        yield return new WaitForSeconds(2.0f);
        spriteRenderer.enabled = true;
    }
}
