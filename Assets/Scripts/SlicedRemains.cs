using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class SlicedRemains : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;

    public int layerIncrease;
    public int layerDecrease;

    private int orderInLayer;

    private void Start()
    {
        orderInLayer = spriteRenderer.sortingOrder;
    }

    private float RandomModifier(float value, float range = 0.2f)
    {
        value *= Random.Range(1.0f - range, 1.0f + range);
        return value;
    }

    public void SetSprite(Sprite sprite)
    {
        spriteRenderer.sprite = sprite;
    }

    public void UpperLaunch(Vector2 direction, float duration, float intensity, float height)
    {
        // The direction maybe should be random?

        duration = RandomModifier(duration);
        intensity = RandomModifier(intensity);
        height = RandomModifier(height);

        StartCoroutine(Launch(direction, duration, intensity, height, layerIncrease));
    }

    public void LowerLaunch(Vector2 direction, float duration, float intensity, float height)
    {
        duration *= 0.5f;
        intensity *= 0.5f;
        height *= 0.2f;

        duration = RandomModifier(duration);
        intensity = RandomModifier(intensity);
        height = RandomModifier(height);


        StartCoroutine(Launch(direction, duration, intensity, height, layerDecrease * -1));
    }

    private IEnumerator Launch(Vector2 direction, float duration, float intensity, float height, int layerChange)
    {
        // REMEMBER SOME SORT OF COLLISION CHECK
        // Maybe some raycast logic, calculate the length of the idk

        // BOUNCING SHOULD BE POSSIBLE, DO SOMETHING WITH DOT PRODUCT OR SOMETHING IDK


        spriteRenderer.sortingOrder = orderInLayer + layerChange;

        float timePassed = 0.0f;
        while (timePassed < duration)
        {
            yield return null;

            timePassed += Time.deltaTime;
        }

        spriteRenderer.sortingOrder = orderInLayer;

        StartCoroutine(DarkenRemains());
    }

    private IEnumerator DarkenRemains()
    {
        float darknessLevel = 0.75f;

        Color baseColor = spriteRenderer.color;
        Color targetColor = new Color(baseColor.r * darknessLevel, baseColor.g * darknessLevel, baseColor.b * darknessLevel, baseColor.a);

        float duration = 0.5f; // Should this be custom? maybe larger foes should darken slower?
        float timePassed = 0.0f;
        while (timePassed < duration)
        {
            yield return null;

            timePassed += Time.deltaTime;
            spriteRenderer.color = Color.Lerp(baseColor, targetColor, timePassed / duration);
        }
    }
}
