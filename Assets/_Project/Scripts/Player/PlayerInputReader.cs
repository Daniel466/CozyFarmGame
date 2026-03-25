using UnityEngine;

/// <summary>
/// Input only — reads WASD and mouse, exposes clean values.
/// No gameplay logic. No movement. No farming.
/// All other systems read from here rather than calling Input directly.
/// </summary>
public class PlayerInputReader : MonoBehaviour
{
    /// <summary>Raw WASD/arrow axis input this frame. Not camera-relative.</summary>
    public Vector2 MoveInput    { get; private set; }
    public bool    HasMoveInput => MoveInput.sqrMagnitude > 0.01f;

    public bool LeftClickDown  { get; private set; }
    public bool LeftClickHeld  { get; private set; }
    public bool LeftClickUp    { get; private set; }
    public bool RightClickDown { get; private set; }

    private void Update()
    {
        MoveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        LeftClickDown  = Input.GetMouseButtonDown(0);
        LeftClickHeld  = Input.GetMouseButton(0);
        LeftClickUp    = Input.GetMouseButtonUp(0);
        RightClickDown = Input.GetMouseButtonDown(1);
    }
}
