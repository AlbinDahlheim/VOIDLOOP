using UnityEngine;

[System.Serializable]

public class PlayerUnsheathed : PlayerState
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
