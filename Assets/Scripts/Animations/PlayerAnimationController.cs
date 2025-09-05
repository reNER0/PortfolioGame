using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField]
    private Player player;
    [SerializeField]
    private Animator animator;

    private void Update()
    {
        animator.SetFloat("Speed", player.Rigidbody.velocity.magnitude / player.GetMaxSpeed());
    }
}
