using UnityEngine;
using TempleRunner.Player;
using TempleRunner.Core;

namespace TempleRunner.Camera
{
    /// <summary>
    /// Third-person follow camera with critically damped 90-degree corner rotation,
    /// jump vertical smoothing, and stumble camera shake.
    /// </summary>
    public class CameraFollowController : MonoBehaviour
    {
        [Header("Target & Offsets")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0, 2.4f, -4.2f);
        [SerializeField] private float lookAheadDistance = 3.5f;

        [Header("Smoothing")]
        [SerializeField] private float followSpeed = 15f;
        [SerializeField] private float rotationSpeed = 8f;
        [SerializeField] private float jumpDampening = 0.5f;

        [Header("Camera Shake")]
        [SerializeField] private float shakeMagnitude = 0.25f;
        [SerializeField] private float shakeDuration = 0.35f;
        private float shakeTimer = 0f;

        private Vector3 currentVelocity;
        private Vector3 lastTargetPos;

        private void Start()
        {
            if (target == null && PlayerController.Instance != null)
            {
                target = PlayerController.Instance.transform;
            }

            if (PlayerController.Instance != null)
            {
                PlayerController.Instance.OnStumble += TriggerShake;
            }

            ResetCamera();
        }

        private void OnDestroy()
        {
            if (PlayerController.Instance != null)
            {
                PlayerController.Instance.OnStumble -= TriggerShake;
            }
        }

        public void ResetCamera()
        {
            if (target == null) return;
            lastTargetPos = target.position;
            transform.position = target.position + target.rotation * offset;
            transform.LookAt(target.position + target.forward * lookAheadDistance);
        }

        private void LateUpdate()
        {
            if (target == null) return;
            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Paused)
                return;

            Vector3 targetForward = PlayerController.Instance != null ? PlayerController.Instance.CurrentForward : target.forward;

            // Compute ideal position based on player's current track forward orientation
            Quaternion desiredRotation = Quaternion.LookRotation(targetForward);
            Vector3 rotatedOffset = desiredRotation * offset;

            // Dampen jump vertical bobbing on camera
            float smoothedY = Mathf.Lerp(transform.position.y, target.position.y + offset.y, followSpeed * jumpDampening * Time.deltaTime);
            Vector3 desiredPosition = target.position + rotatedOffset;
            desiredPosition.y = smoothedY;

            // Apply camera shake if active
            if (shakeTimer > 0f)
            {
                desiredPosition += (Vector3)Random.insideUnitCircle * shakeMagnitude;
                shakeTimer -= Time.deltaTime;
            }

            // Smooth position and rotation
            transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);

            Vector3 lookTarget = target.position + (targetForward * lookAheadDistance) + (Vector3.up * 1.0f);
            Quaternion targetLookRot = Quaternion.LookRotation(lookTarget - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetLookRot, rotationSpeed * Time.deltaTime);

            lastTargetPos = target.position;
        }

        public void TriggerShake()
        {
            shakeTimer = shakeDuration;
        }
    }
}
