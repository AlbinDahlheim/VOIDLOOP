using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]

public class PlayerState
{
    [HideInInspector] public PlayerBehavior player;

    public virtual void OnValidate(PlayerBehavior player)
    {
        this.player = player;
    }

    public virtual void Enter() { }

    public virtual void Exit() { }

    public virtual void Update() { }

    public virtual void FixedUpdate() { }

    public virtual void SwordInput(InputAction.CallbackContext context) { }
}
