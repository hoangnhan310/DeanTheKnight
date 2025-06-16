using UnityEngine;

public class PlayerState : MonoBehaviour
{
    public bool IsWallSliding { get; set; } = false;
    public bool IsGrounded { get; set; } = false;
    public bool CanDoubleJump { get; set; } = false;
    public bool IsRolling { get; set; } = false;
    public int FacingDirection { get; set; } = 1;
}
