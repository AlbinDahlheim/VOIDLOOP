using UnityEngine;

[System.Serializable]

public class PlayerSheathed : PlayerState
{
    public float test;

    public override void OnValidate(PlayerBehavior player)
    {
        base.OnValidate(player);
    }

    public override void Enter()
    {
        // do stuff
    }
}
