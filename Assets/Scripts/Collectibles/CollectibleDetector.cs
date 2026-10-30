using System.Collections.Generic;
using UnityEngine;

namespace Train.Collectibles
{
    [RequireComponent(typeof(Collider))]
    public class CollectibleDetector : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Animator m_teethAnimator;
        [SerializeField] private ParticleSystem m_goreParticleSystemPrefab;
        [SerializeField] private AudioSource m_eatingAudioSource;

        [Header("Collectibles")]
        [SerializeField, Min(0f)] private float m_collectDelay;

        [Header("Gore Particles")]
        [SerializeField, Min(1)] private int m_maxGoreParticleSystems;

        private ParticleSystem[] m_pooledGoreParticles;
        private int m_nextIndex;

        private void Start()
        {
            m_pooledGoreParticles = new ParticleSystem[m_maxGoreParticleSystems];
            for (int i = 0; i < m_maxGoreParticleSystems; i++)
            {
                ParticleSystem newGoreParticle = Instantiate(m_goreParticleSystemPrefab, transform);
                newGoreParticle.name = "Gore Particle " + i;
                m_pooledGoreParticles[i] = newGoreParticle;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out Collectible collectible))
            {
                // begin eating
                collectible.BeginCollect(m_collectDelay, m_teethAnimator.transform);
                m_eatingAudioSource.Play();
                m_teethAnimator.Play("Eat");
            }
        }

        public void SpawnGoreParticle()
        {
            ParticleSystem toPlay = m_pooledGoreParticles[m_nextIndex];

            if (toPlay.isPlaying)
            {
                Debug.LogWarning("Interrupting an existing Gore Particle to play again. Consider increasing the max number of Gore Particles.", this);
                toPlay.Stop();
            }

            toPlay.Play();

            m_nextIndex++;
            if (m_nextIndex == m_maxGoreParticleSystems)
            {
                m_nextIndex = 0;
            }
        }
    }
}
