using System;
using System.Collections;
using UnityEngine;
using TempleRunner.Core;
using TempleRunner.Collectibles;

namespace TempleRunner.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        public static PlayerController Instance { get; private set; }

        [Header("Movement & Speed Curve")]
        [SerializeField] private float baseSpeed = 8.0f;
        [SerializeField] private float maxSpeed = 22.0f;
        [SerializeField] private float accelerationRate = 0.05f;
        private float currentSpeed;
        public float CurrentSpeed => currentSpeed;

        [Header("Lanes & Track Alignment")]
        [SerializeField] private float laneWidth = 1.6f;
        [SerializeField] private float laneSwitchSpeed = 12f;
        private int currentLane = 0; // -1: Left, 0: Center, 1: Right
        private float targetLateralOffset = 0f;
        private float currentLateralOffset = 0f;

        [Header("Jump & Slide")]
        [SerializeField] private float jumpHeight = 1.8f;
        [SerializeField] private float gravity = 25f;
        [SerializeField] private float slideDuration = 0.8f;
        private bool isGrounded = true;
        private float verticalVelocity = 0f;
        private bool isSliding = false;
        public bool IsSliding => isSliding;

        [Header("Collider Dimensions")]
        [SerializeField] private float normalHeight = 2.0f;
        [SerializeField] private Vector3 normalCenter = new Vector3(0, 1.0f, 0);
        [SerializeField] private float slideHeight = 0.8f;
        [SerializeField] private Vector3 slideCenter = new Vector3(0, 0.4f, 0);

        [Header("Track Direction")]
        private Vector3 currentForward = Vector3.forward;
        private Vector3 currentRight = Vector3.right;
        public Vector3 CurrentForward => currentForward;
        public Vector3 CurrentRight => currentRight;

        [Header("Turn Zones")]
        private bool canTurnLeft = false;
        private bool canTurnRight = false;
        private Vector3 pendingTurnAnchor = Vector3.zero;

        [Header("Stumble Mechanics")]
        [SerializeField] private float stumbleCooldown = 6.5f;
        private float lastStumbleTime = -10f;
        private bool isStumbling = false;
        public event Action<bool> OnDemonMonkeysAlerted; // true = close chase, false = normal

        [Header("Components")]
        private CharacterController characterController;

        public event Action OnJump;
        public event Action OnSlide;
        public event Action OnStumble;
        public event Action<string> OnPlayerDied;

        private Vector3 startPosition;
        private Quaternion startRotation;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            characterController = GetComponent<CharacterController>();
            startPosition = transform.position;
            startRotation = transform.rotation;
        }

        private void Start()
        {
            ResetPlayer();

            if (SwipeInputHandler.Instance != null)
            {
                SwipeInputHandler.Instance.OnSwipeDetected += HandleSwipe;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameRestarted += ResetPlayer;
            }
        }

        private void OnDestroy()
        {
            if (SwipeInputHandler.Instance != null)
            {
                SwipeInputHandler.Instance.OnSwipeDetected -= HandleSwipe;
            }
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameRestarted -= ResetPlayer;
            }
        }

        public void ResetPlayer()
        {
            StopAllCoroutines();
            characterController.enabled = false;
            transform.position = startPosition;
            transform.rotation = startRotation;
            characterController.enabled = true;

            currentSpeed = baseSpeed;
            currentForward = Vector3.forward;
            currentRight = Vector3.right;
            currentLane = 0;
            targetLateralOffset = 0f;
            currentLateralOffset = 0f;
            verticalVelocity = 0f;
            isGrounded = true;
            isSliding = false;
            isStumbling = false;
            lastStumbleTime = -10f;

            characterController.height = normalHeight;
            characterController.center = normalCenter;

            OnDemonMonkeysAlerted?.Invoke(false);
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
            {
                return;
            }

            // Continuous speed acceleration
            if (currentSpeed < maxSpeed)
            {
                currentSpeed += accelerationRate * Time.deltaTime;
            }

            // Check stumble cooldown recovery
            if (isStumbling && Time.time - lastStumbleTime > stumbleCooldown)
            {
                isStumbling = false;
                OnDemonMonkeysAlerted?.Invoke(false);
            }

            UpdateMovement();
        }

        private void UpdateMovement()
        {
            // Forward displacement along track-local forward vector
            Vector3 forwardMove = currentForward * (currentSpeed * Time.deltaTime);

            // Lateral steering (combines discrete lane switching + analog tilt)
            float tiltInput = SwipeInputHandler.Instance != null ? SwipeInputHandler.Instance.FilteredTilt : 0f;
            float targetLaneX = (currentLane * laneWidth) + (tiltInput * (laneWidth * 0.45f));
            currentLateralOffset = Mathf.MoveTowards(currentLateralOffset, targetLaneX, laneSwitchSpeed * Time.deltaTime);

            // Compute lateral delta
            Vector3 lateralTargetPos = currentRight * currentLateralOffset;

            // Vertical jump / gravity
            if (characterController.isGrounded)
            {
                isGrounded = true;
                if (verticalVelocity < 0) verticalVelocity = -2f;
            }
            else
            {
                isGrounded = false;
                verticalVelocity -= gravity * Time.deltaTime;
            }

            Vector3 verticalMove = Vector3.up * (verticalVelocity * Time.deltaTime);

            // Apply motion
            Vector3 motion = forwardMove + verticalMove;
            characterController.Move(motion);

            // Apply lateral correction directly to lock character to track center line + offset
            // We align rotation smoothly with currentForward
            if (transform.forward != currentForward)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(currentForward), 15f * Time.deltaTime);
            }
        }

        public void HandleSwipe(SwipeDirection direction)
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
                return;

            switch (direction)
            {
                case SwipeDirection.Left:
                    if (canTurnLeft)
                    {
                        ExecuteTurn(-90f);
                    }
                    else
                    {
                        // Shift lane left if within boundaries, or stumble if at edge
                        if (currentLane > -1)
                        {
                            currentLane--;
                        }
                        else
                        {
                            TriggerStumble();
                        }
                    }
                    break;

                case SwipeDirection.Right:
                    if (canTurnRight)
                    {
                        ExecuteTurn(90f);
                    }
                    else
                    {
                        // Shift lane right if within boundaries, or stumble if at edge
                        if (currentLane < 1)
                        {
                            currentLane++;
                        }
                        else
                        {
                            TriggerStumble();
                        }
                    }
                    break;

                case SwipeDirection.Up:
                    if (isGrounded && !isSliding)
                    {
                        Jump();
                    }
                    break;

                case SwipeDirection.Down:
                    if (!isSliding)
                    {
                        StartCoroutine(SlideRoutine());
                    }
                    break;
            }
        }

        private void Jump()
        {
            verticalVelocity = Mathf.Sqrt(2 * jumpHeight * gravity);
            isGrounded = false;
            OnJump?.Invoke();
        }

        private IEnumerator SlideRoutine()
        {
            isSliding = true;
            characterController.height = slideHeight;
            characterController.center = slideCenter;
            OnSlide?.Invoke();

            // If jumping when sliding, force down
            if (!isGrounded)
            {
                verticalVelocity = -gravity * 0.5f;
            }

            yield return new WaitForSeconds(slideDuration);

            characterController.height = normalHeight;
            characterController.center = normalCenter;
            isSliding = false;
        }

        public void SetTurnZone(bool left, bool right, Vector3 anchor)
        {
            canTurnLeft = left;
            canTurnRight = right;
            pendingTurnAnchor = anchor;
        }

        public void ClearTurnZone()
        {
            canTurnLeft = false;
            canTurnRight = false;
        }

        private void ExecuteTurn(float angleDegrees)
        {
            // Snap position to intersection center to guarantee zero track drift
            if (pendingTurnAnchor != Vector3.zero)
            {
                Vector3 snappedPos = pendingTurnAnchor;
                snappedPos.y = transform.position.y;
                transform.position = snappedPos;
            }

            // Rotate direction vectors by 90 degrees
            Quaternion rot = Quaternion.Euler(0, angleDegrees, 0);
            currentForward = (rot * currentForward).normalized;
            currentRight = (rot * currentRight).normalized;

            currentLane = 0;
            currentLateralOffset = 0f;
            ClearTurnZone();
        }

        public void TriggerStumble()
        {
            if (PowerUpManager.Instance != null && PowerUpManager.Instance.IsBoostActive) return;

            if (isStumbling)
            {
                // Consecutively stumbled within cooldown -> FATAL!
                Die("Caught by Demon Monkeys!");
                return;
            }

            isStumbling = true;
            lastStumbleTime = Time.time;
            currentSpeed = Mathf.Max(baseSpeed, currentSpeed * 0.7f);
            OnStumble?.Invoke();
            OnDemonMonkeysAlerted?.Invoke(true);
        }

        public void Die(string cause)
        {
            // Check Resurrection Wings
            if (PowerUpManager.Instance != null && PowerUpManager.Instance.ConsumeResurrectionWing())
            {
                // Revived!
                verticalVelocity = 0f;
                return;
            }

            currentSpeed = 0f;
            OnPlayerDied?.Invoke(cause);
            if (GameManager.Instance != null)
            {
                GameManager.Instance.GameOver();
            }
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            // Detect falling below track
            if (transform.position.y < -3.0f)
            {
                Die("Fell into the abyss!");
            }
        }
    }
}
