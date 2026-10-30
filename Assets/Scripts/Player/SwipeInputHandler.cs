using System;
using UnityEngine;

namespace TempleRunner.Player
{
    public enum SwipeDirection
    {
        None,
        Left,
        Right,
        Up,
        Down
    }

    /// <summary>
    /// Handles mobile touchscreen swipes, accelerometer tilt with low-pass filtering,
    /// double-tap gestures, and editor/desktop keyboard fallbacks.
    /// </summary>
    public class SwipeInputHandler : MonoBehaviour
    {
        public static SwipeInputHandler Instance { get; private set; }

        [Header("Swipe Settings")]
        [SerializeField] private float minSwipeDistancePixels = 45f;
        [SerializeField] private float maxSwipeDurationSeconds = 0.5f;

        [Header("Double Tap Settings")]
        [SerializeField] private float doubleTapThresholdSeconds = 0.3f;

        [Header("Tilt / Accelerometer Settings")]
        [SerializeField] private bool enableAccelerometer = true;
        [SerializeField] [Range(0.01f, 1f)] private float tiltFilterAlpha = 0.15f; // Single-pole IIR smoothing
        [SerializeField] private float tiltSensitivity = 2.5f;
        [SerializeField] private float tiltDeadzone = 0.05f;

        public event Action<SwipeDirection> OnSwipeDetected;
        public event Action OnDoubleTapDetected;

        private Vector2 touchStartPosition;
        private float touchStartTime;
        private bool isTrackingTouch;
        private float lastTapReleaseTime;

        private float filteredTilt;
        public float FilteredTilt => filteredTilt;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Update()
        {
            HandleTouchInput();
            HandleAccelerometerTilt();
            HandleDesktopFallback();
        }

        private void HandleTouchInput()
        {
            if (Input.touchCount == 0) return;

            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    touchStartPosition = touch.position;
                    touchStartTime = Time.unscaledTime;
                    isTrackingTouch = true;
                    break;

                case TouchPhase.Ended:
                    if (!isTrackingTouch) return;
                    isTrackingTouch = false;

                    float duration = Time.unscaledTime - touchStartTime;
                    Vector2 delta = touch.position - touchStartPosition;

                    if (delta.magnitude >= minSwipeDistancePixels && duration <= maxSwipeDurationSeconds)
                    {
                        // Recognized as a swipe gesture
                        delta.Normalize();
                        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                        {
                            OnSwipeDetected?.Invoke(delta.x > 0 ? SwipeDirection.Right : SwipeDirection.Left);
                        }
                        else
                        {
                            OnSwipeDetected?.Invoke(delta.y > 0 ? SwipeDirection.Up : SwipeDirection.Down);
                        }
                    }
                    else if (delta.magnitude < minSwipeDistancePixels && duration <= maxSwipeDurationSeconds)
                    {
                        // Check for double-tap
                        if (Time.unscaledTime - lastTapReleaseTime <= doubleTapThresholdSeconds)
                        {
                            OnDoubleTapDetected?.Invoke();
                            lastTapReleaseTime = 0f;
                        }
                        else
                        {
                            lastTapReleaseTime = Time.unscaledTime;
                        }
                    }
                    break;

                case TouchPhase.Canceled:
                    isTrackingTouch = false;
                    break;
            }
        }

        private void HandleAccelerometerTilt()
        {
            if (!enableAccelerometer) return;

            // Raw accelerometer X axis corresponds to tilting phone left/right in portrait
            float rawTilt = Input.acceleration.x;

            if (Mathf.Abs(rawTilt) < tiltDeadzone)
            {
                rawTilt = 0f;
            }

            // Low-pass filter to reject hand tremor
            filteredTilt = Mathf.Lerp(filteredTilt, rawTilt * tiltSensitivity, tiltFilterAlpha);
            filteredTilt = Mathf.Clamp(filteredTilt, -1f, 1f);
        }

        private void HandleDesktopFallback()
        {
            // Keyboard arrows/WASD for desktop testing
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
                OnSwipeDetected?.Invoke(SwipeDirection.Left);
            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
                OnSwipeDetected?.Invoke(SwipeDirection.Right);
            else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space))
                OnSwipeDetected?.Invoke(SwipeDirection.Up);
            else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
                OnSwipeDetected?.Invoke(SwipeDirection.Down);

            if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.R))
                OnDoubleTapDetected?.Invoke();

            // Fallback keyboard tilt
            if (!enableAccelerometer || Mathf.Approximately(filteredTilt, 0f))
            {
                float keyTilt = 0f;
                if (Input.GetKey(KeyCode.Q)) keyTilt = -1f;
                if (Input.GetKey(KeyCode.E)) keyTilt = 1f;
                if (keyTilt != 0f)
                {
                    filteredTilt = Mathf.Lerp(filteredTilt, keyTilt, tiltFilterAlpha);
                }
            }
        }

        public void SetAccelerometerEnabled(bool enabled)
        {
            enableAccelerometer = enabled;
            if (!enabled) filteredTilt = 0f;
        }

        public void SetTiltSensitivity(float sensitivity)
        {
            tiltSensitivity = Mathf.Clamp(sensitivity, 0.5f, 5.0f);
        }
    }
}
