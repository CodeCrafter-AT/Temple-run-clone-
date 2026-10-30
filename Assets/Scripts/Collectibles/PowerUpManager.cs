using System;
using System.Collections;
using UnityEngine;
using TempleRunner.Core;
using TempleRunner.Player;
using TempleRunner.Scoring;

namespace TempleRunner.Collectibles
{
    /// <summary>
    /// Manages active power-ups, countdown timers, boost velocity overrides,
    /// and double-tap resurrection wing priming and consumption.
    /// </summary>
    public class PowerUpManager : MonoBehaviour
    {
        public static PowerUpManager Instance { get; private set; }

        [Header("Active States")]
        public bool IsMagnetActive { get; private set; }
        public bool IsBoostActive { get; private set; }
        public bool IsInvisibilityActive { get; private set; }

        [Header("Resurrection Wings")]
        [SerializeField] private int ownedWings = 2;
        private bool isWingPrimed = false;
        private float wingCooldownTimer = 0f;
        public bool IsWingPrimed => isWingPrimed;
        public int OwnedWings => ownedWings;

        public event Action<PowerUpType, float> OnPowerUpStarted;
        public event Action<PowerUpType> OnPowerUpEnded;
        public event Action OnWingPrimed;
        public event Action OnWingConsumed;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            if (SwipeInputHandler.Instance != null)
            {
                SwipeInputHandler.Instance.OnDoubleTapDetected += TryPrimeWing;
            }
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameRestarted += ResetPowerUps;
            }
        }

        private void OnDestroy()
        {
            if (SwipeInputHandler.Instance != null)
            {
                SwipeInputHandler.Instance.OnDoubleTapDetected -= TryPrimeWing;
            }
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameRestarted -= ResetPowerUps;
            }
        }

        public void ResetPowerUps()
        {
            StopAllCoroutines();
            IsMagnetActive = false;
            IsBoostActive = false;
            IsInvisibilityActive = false;
            isWingPrimed = false;
            wingCooldownTimer = 0f;
        }

        public void ActivatePowerUp(PowerUpType type, float duration, int amount = 0)
        {
            switch (type)
            {
                case PowerUpType.MegaCoin:
                    ScoreManager.Instance?.AddCoins(amount > 0 ? amount : 50);
                    break;
                case PowerUpType.CoinMagnet:
                    StartCoroutine(MagnetRoutine(duration));
                    break;
                case PowerUpType.Boost:
                    StartCoroutine(BoostRoutine(duration));
                    break;
                case PowerUpType.Invisibility:
                    StartCoroutine(InvisibilityRoutine(duration));
                    break;
                case PowerUpType.ResurrectionWing:
                    ownedWings++;
                    break;
            }

            OnPowerUpStarted?.Invoke(type, duration);
        }

        private IEnumerator MagnetRoutine(float duration)
        {
            IsMagnetActive = true;
            yield return new WaitForSeconds(duration);
            IsMagnetActive = false;
            OnPowerUpEnded?.Invoke(PowerUpType.CoinMagnet);
        }

        private IEnumerator BoostRoutine(float duration)
        {
            IsBoostActive = true;
            yield return new WaitForSeconds(duration);
            IsBoostActive = false;
            OnPowerUpEnded?.Invoke(PowerUpType.Boost);
        }

        private IEnumerator InvisibilityRoutine(float duration)
        {
            IsInvisibilityActive = true;
            yield return new WaitForSeconds(duration);
            IsInvisibilityActive = false;
            OnPowerUpEnded?.Invoke(PowerUpType.Invisibility);
        }

        public void TryPrimeWing()
        {
            if (ownedWings > 0 && !isWingPrimed && Time.time > wingCooldownTimer)
            {
                isWingPrimed = true;
                ownedWings--;
                OnWingPrimed?.Invoke();
                StartCoroutine(WingDurationRoutine(30.0f));
            }
        }

        private IEnumerator WingDurationRoutine(float duration)
        {
            yield return new WaitForSeconds(duration);
            if (isWingPrimed)
            {
                isWingPrimed = false;
                wingCooldownTimer = Time.time + 20f; // 20s recharge cooldown
            }
        }

        public bool ConsumeResurrectionWing()
        {
            if (isWingPrimed)
            {
                isWingPrimed = false;
                wingCooldownTimer = Time.time + 20f;
                OnWingConsumed?.Invoke();
                return true; // Successfully cheated death!
            }
            return false;
        }
    }
}
