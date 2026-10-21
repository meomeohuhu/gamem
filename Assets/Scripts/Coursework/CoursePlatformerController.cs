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
        private CapsuleCollider2D capsule;
        private Vector2 moveInput;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            capsule = GetComponent<CapsuleCollider2D>();
            body.gravityScale = 3f;
            body.freezeRotation = true;
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                moveInput = Vector2.zero;
                return;
            }

            moveInput = new Vector2(
                (keyboard.dKey.isPressed ? 1f : 0f) - (keyboard.aKey.isPressed ? 1f : 0f),
                0f);

            if (keyboard.spaceKey.wasPressedThisFrame && IsGrounded())
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
            if (capsule == null) return false;
            Vector2 origin = (Vector2)transform.position + Vector2.down * (capsule.bounds.extents.y + 0.03f);
            return Physics2D.Raycast(origin, Vector2.down, 0.08f, groundMask).collider != null;
        }
    }
}
