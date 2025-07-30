using System.Collections;
using UnityEngine;

public class Sliceable : MonoBehaviour
{
    public float hitstopDuration;
    // TODO: Add HP if it should be reused for big boys
    // TODO: Keep track of if the object has been sliced this cycle

    public void Slice(Vector2 slicerPosition, int? facingAngle = null)
    {
        Vector2 angleVector = new Vector2(transform.position.x, transform.position.y) - slicerPosition;

        int angle = Mathf.RoundToInt(Vector2.SignedAngle(Vector2.up, angleVector.normalized));
        angle = angle <= 0 ? angle * -1 : 360 - angle;

        if (facingAngle.HasValue)
        {
            //float deltaAngle = Mathf.Abs(Mathf.DeltaAngle(facingAngle.Value, angle));
            //float magnitude = angleVector.magnitude;

            //Debug.Log(deltaAngle + ", " + magnitude);

            //if (magnitude >= 2.0f * (1.0f - deltaAngle / 225.0f)) // use actual collider radius
            //    return;
        }

        Sliced();
    }

    private void Sliced()
    {
        // Play some sort of sound, maybe spawn something empty that does it? idk
        StartCoroutine(Hitstop(hitstopDuration));
    }

    private IEnumerator Hitstop(float duration)
    {
        Time.timeScale = 0.0f; // What happens if multiple things got hit?
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1.0f;
    }
}
