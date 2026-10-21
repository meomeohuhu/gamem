using UnityEngine;
using UnityEngine.InputSystem;

namespace SpaceExplore.Coursework
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class CoursePhysicsShowcase : MonoBehaviour
    {
        [SerializeField] private float impulse = 7f;
        [SerializeField] private float torque = 2f;
        private Rigidbody2D body;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            body.gravityScale = 1f;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) return;
            if (keyboard.spaceKey.wasPressedThisFrame)
            {
                body.AddForce(Vector2.up * impulse, ForceMode2D.Impulse);
                body.AddTorque(torque, ForceMode2D.Impulse);
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            Debug.Log($"Physics practice collision: {collision.gameObject.name}", this);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Debug.Log($"Physics practice trigger: {other.gameObject.name}", this);
        }
    }
}
