using UnityEngine;
using UnityEngine.InputSystem;

namespace SpaceExplore.Coursework
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class CoursePlatformerController : MonoBehaviour
    {
        [SerializeField] private float runSpeed = 5f;
        [SerializeField] private float jumpImpulse = 7f;
        [SerializeField] private LayerMask groundMask = ~0;

        private Rigidbody2D body;
        private Collider2D collision;
        private Animator animator;
        private Vector2 moveInput;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            collision = GetComponent<Collider2D>();
            animator = GetComponent<Animator>();
            body.gravityScale = 3f;
            body.freezeRotation = true;
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            moveInput = keyboard == null
                ? Vector2.zero
                : new Vector2(
                    (keyboard.dKey.isPressed ? 1f : 0f) - (keyboard.aKey.isPressed ? 1f : 0f),
                    0f);

            Mouse mouse = Mouse.current;
            if (animator != null && mouse != null)
            {
                if (mouse.leftButton.wasPressedThisFrame) animator.SetTrigger("Attack1");
                if (mouse.rightButton.wasPressedThisFrame) animator.SetTrigger("Block");
            }

            if (keyboard != null && keyboard.spaceKey.wasPressedThisFrame && IsGrounded())
            {
                body.AddForce(Vector2.up * jumpImpulse, ForceMode2D.Impulse);
            }
        }

        private void FixedUpdate()
        {
            body.linearVelocity = new Vector2(moveInput.x * runSpeed, body.linearVelocity.y);
            if (Mathf.Abs(moveInput.x) > 0.01f)
            {
                Vector3 scale = transform.localScale;
                scale.x = Mathf.Abs(scale.x) * Mathf.Sign(moveInput.x);
                transform.localScale = scale;
            }
        }

        private bool IsGrounded()
        {
            if (collision == null) return false;
            Vector2 origin = (Vector2)transform.position + Vector2.down * (collision.bounds.extents.y + 0.03f);
            return Physics2D.Raycast(origin, Vector2.down, 0.08f, groundMask).collider != null;
        }
    }
}
