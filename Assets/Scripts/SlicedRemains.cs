using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class SlicedRemains : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Rigidbody2D rb2d;

    private int orderInLayer;

    private void OnEnable()
    {
        orderInLayer = spriteRenderer.sortingOrder;
    }

    private float RandomModifier(float value, float range = 0.2f)
    {
        value *= Random.Range(1.0f - range, 1.0f + range);
        return value;
    }

    public void SetSprite(Sprite sprite, bool flipX)
    {
        spriteRenderer.sprite = sprite;
        spriteRenderer.flipX = flipX;
    }

    public void Launch(Vector2 direction, float duration, float intensity, float height, int layerIncrease, bool spin)
    {
        duration = RandomModifier(duration);
        intensity = RandomModifier(intensity);
        height = RandomModifier(height);

        spriteRenderer.sortingOrder = orderInLayer + layerIncrease;

        StartCoroutine(LaunchAnimation(direction, duration, intensity, height, spin));
    }

    private IEnumerator LaunchAnimation(Vector2 direction, float duration, float intensity, float height, bool spin)
    {
        // REMEMBER SOME SORT OF COLLISION CHECK
        // Maybe some raycast logic, calculate the length of the idk

        // BOUNCING SHOULD BE POSSIBLE, DO SOMETHING WITH DOT PRODUCT OR SOMETHING IDK

        rb2d.linearVelocity = direction.normalized * intensity;
        float rotationValue = direction.x < 0.0f ? -360.0f : 360.0f; 

        float timePassed = 0.0f;
        while (timePassed < duration)
        {
            float lerpValue = Mathf.Sin(timePassed / duration * 180.0f * Mathf.Deg2Rad);
            spriteRenderer.transform.localPosition = Vector2.Lerp(Vector2.zero, new Vector2(0.0f, height), lerpValue);
            
            if (spin)
            {
                float spinPoint = Mathf.Ceil(timePassed * 8.0f / duration);
                spinPoint /= 8.0f;
                spriteRenderer.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, rotationValue * spinPoint);
            }

            yield return null;

            timePassed += Time.deltaTime;
        }

        spriteRenderer.sortingOrder = orderInLayer;
        rb2d.linearVelocity = Vector2.zero;

        StartCoroutine(DarkenRemains());

        // Simulates the effect of being pushed into the ground before bouncing back up to normal
        float shakeTime = 0.075f;
        spriteRenderer.transform.localPosition = new Vector2(0.0f, 0.0f - 1.0f / 16.0f);
        spriteRenderer.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
        yield return new WaitForSeconds(shakeTime);
        spriteRenderer.transform.localPosition = Vector2.zero;
    }

    private IEnumerator DarkenRemains()
    {
        float darknessLevel = RandomModifier(0.75f, 0.15f);

        Color baseColor = spriteRenderer.color;
        Color targetColor = new Color(baseColor.r * darknessLevel, baseColor.g * darknessLevel, baseColor.b * darknessLevel, baseColor.a);

        float duration = 2.0f; // Should this be custom? maybe larger foes should darken slower?
        float timePassed = 0.0f;
        while (timePassed < duration)
        {
            yield return null;

            timePassed += Time.deltaTime;
            spriteRenderer.color = Color.Lerp(baseColor, targetColor, timePassed / duration);
        }
    }
}
