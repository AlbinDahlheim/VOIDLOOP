using UnityEngine;
using UnityEngine.Rendering;

public class SwingHitbox : MonoBehaviour
{
    public PlayerBehavior player;
    public CircleCollider2D swingCollider;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Sliceable>() != null && collision.GetComponent<CircleCollider2D>() != null)
        {
            SwingHit(collision.GetComponent<Sliceable>(), collision.transform.position, collision.GetComponent<CircleCollider2D>().radius);
        }
    }

    private void SwingHit(Sliceable slicable, Vector2 targetPosition, float colliderRadius)
    {
        Vector2 angleVector = targetPosition - new Vector2(player.transform.position.x, player.transform.position.y);

        int angle = Mathf.RoundToInt(Vector2.SignedAngle(Vector2.up, angleVector.normalized));
        angle = angle <= 0 ? angle * -1 : 360 - angle;

        float deltaAngle = Mathf.Abs(Mathf.DeltaAngle(GetAngleOfCardinalFacingDirection(), angle));
        float magnitude = angleVector.magnitude - colliderRadius;

        if (magnitude > swingCollider.radius * (1.0f - deltaAngle / 270))
            return;

        slicable.Slice(player.transform.position, GetAngleOfCardinalFacingDirection());
    }

    private int GetAngleOfCardinalFacingDirection()
    {
        if (player.facingDirection == PlayerBehavior.Direction.UP)
            return 0;
        else if (player.facingDirection == PlayerBehavior.Direction.DOWN)
            return 180;
        else if (player.facingDirection.ToString().Contains("LEFT"))
            return 270;
        else
            return 90;
    }
}
