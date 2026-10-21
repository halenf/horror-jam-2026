using UnityEngine;

namespace Train.Destroyables
{
    public class Destroyable : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject m_model;
        [SerializeField] private ParticleSystem m_destroyedParticles;

        [Header("Properties")]
        [SerializeField] private Material m_particleMaterial;
        [SerializeField, Min(0f)] private float m_respawnDuration;

        private float m_respawnCountdown;

        private void Update()
        {
            if (m_respawnCountdown > 0)
            {
                m_respawnCountdown -= Time.deltaTime;
                if (m_respawnCountdown <= 0)
                {
                    Respawn();
                }
            }
        }

        public void Destroy()
        {
            // if not already destroyed
            if (m_respawnCountdown <= 0)
            {
                PlayParticles();
                m_model.SetActive(false);
                m_respawnCountdown = m_respawnDuration;
            }
        }

        private void PlayParticles()
        {
            if (m_particleMaterial != null)
                m_destroyedParticles.GetComponent<ParticleSystemRenderer>().sharedMaterial = m_particleMaterial;
            m_destroyedParticles.Play();
        }

        private void Respawn()
        {
            m_model.SetActive(true);
            m_destroyedParticles.Stop();
        }
    }
}
