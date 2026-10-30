using UnityEngine;
using TempleRunner.Player;
using TempleRunner.Scoring;

namespace TempleRunner.Audio
{
    /// <summary>
    /// Audio manager providing procedural synthetic sound effect fallbacks
    /// (coin chime, jump whoosh, slide crouch, stumble thud, death) to guarantee
    /// audio feedback without external proprietary sound assets.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        private AudioSource audioSource;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        private void Start()
        {
            if (PlayerController.Instance != null)
            {
                PlayerController.Instance.OnJump += PlayJumpSound;
                PlayerController.Instance.OnSlide += PlaySlideSound;
                PlayerController.Instance.OnStumble += PlayStumbleSound;
                PlayerController.Instance.OnPlayerDied += (reason) => PlayDeathSound();
            }

            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnCoinsUpdated += (coins) => PlayCoinSound();
            }
        }

        public void PlayCoinSound()
        {
            PlayProceduralTone(987.77f, 0.08f, 0.3f); // B5 chime
        }

        public void PlayJumpSound()
        {
            PlayProceduralTone(350f, 0.15f, 0.4f, true); // Rising sweep
        }

        public void PlaySlideSound()
        {
            PlayProceduralTone(220f, 0.20f, 0.3f, false); // Falling sweep
        }

        public void PlayStumbleSound()
        {
            PlayProceduralTone(110f, 0.25f, 0.6f); // Low thud
        }

        public void PlayDeathSound()
        {
            PlayProceduralTone(80f, 0.5f, 0.8f); // Heavy impact
        }

        private void PlayProceduralTone(float baseFrequency, float duration, float volume, bool? pitchSweep = null)
        {
            int sampleRate = 44100;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float freq = baseFrequency;
                if (pitchSweep.HasValue)
                {
                    freq = pitchSweep.Value ? Mathf.Lerp(baseFrequency, baseFrequency * 1.8f, t / duration)
                                            : Mathf.Lerp(baseFrequency * 1.5f, baseFrequency * 0.5f, t / duration);
                }

                // Envelope: quick attack, exponential decay
                float envelope = Mathf.Exp(-5f * (t / duration));
                samples[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * envelope * volume;
            }

            AudioClip clip = AudioClip.Create("ProceduralSfx", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            audioSource.PlayOneShot(clip);
        }
    }
}
