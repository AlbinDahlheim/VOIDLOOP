using UnityEngine;

[System.Serializable]

public class PlayerStance : PlayerState
{
    public override void OnValidate(PlayerBehavior player)
    {
        base.OnValidate(player);
    }

    public override void Enter()
    {
        // do stuff
    }
}
