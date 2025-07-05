using UnityEngine;

public class Destroy : MonoBehaviour
{
    public float lifeTime = 1.0f;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }
}
