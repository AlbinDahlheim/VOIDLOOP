using UnityEngine;
using UnityEngine.Rendering;

public class SwingHitbox : MonoBehaviour
{
    public PlayerBehavior player;
    public CircleCollider2D swingCollider;

    private Collider2D target = null;

    private void Update()
    {
        if (target != null)
            SwingHit(target);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Sliceable>() != null && collision.GetComponent<CircleCollider2D>() != null)
            SwingHit(collision);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision == target)
            target = null;
    }

    private void SwingHit(Collider2D collision)
    {
        if (collision.GetComponent<Sliceable>() == null || collision.GetComponent<CircleCollider2D>() == null)
        {
            target = null;
            return;
        }

        Sliceable sliceable = collision.GetComponent<Sliceable>();
        Vector2 targetPosition = collision.transform.position;
        float colliderRadius = collision.GetComponent<CircleCollider2D>().radius;

        Vector2 angleVector = targetPosition - new Vector2(player.transform.position.x, player.transform.position.y);

        int angle = Mathf.RoundToInt(Vector2.SignedAngle(Vector2.up, angleVector.normalized));
        angle = angle <= 0 ? angle * -1 : 360 - angle;

        float deltaAngle = Mathf.Abs(Mathf.DeltaAngle(GetAngleOfCardinalFacingDirection(), angle));
        float magnitude = angleVector.magnitude - colliderRadius;

        if (magnitude > swingCollider.radius * (1.0f - deltaAngle / 270))
        {
            target = collision;
            return;
        }

        sliceable.Slice(player.transform.position, GetAngleOfCardinalFacingDirection());
        target = null;
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
