using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Train.Player
{
    public class FollowerController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerController m_player;
        [SerializeField] private Transform m_follower;

        [Header("Fields")]
        [SerializeField, Min(0f)] private float m_timestep = 0.0133f;

        private readonly List<Vector3> m_waypoints = new();
        private List<Vector3> m_forwards = new();

        private float m_timer;

        private void Update()
        {
            // update timer
            IncrementTimer(m_player.CurrentSpeed / m_player.MoveSpeed);

            // update pos and forward
            float moveSpeed = m_player.CurrentSpeed * Time.deltaTime;
            m_follower.position = Vector3.MoveTowards(m_follower.position, m_waypoints[0], moveSpeed);
            float turnSpeed = m_player.MaxTurnSpeed * Time.deltaTime;
            m_follower.forward = Vector3.RotateTowards(m_follower.forward, m_forwards[0], turnSpeed * Mathf.Deg2Rad * 2f, 0f);
            // m_follower.rotation = Quaternion.FromToRotation( Quaternion.RotateTowards(m_follower.rotation, Quaternion.LookRotation(m_forwards[0]), turnSpeed);
        }

        public void Begin(PlayerController player, Transform follower)
        {
            // enable
            enabled = true;

            // set references
            m_player = player;
            m_follower = follower;

            // add initial waypoints
            Vector3 start = m_follower.position;
            Vector3 end = transform.position;
            Vector3 current = start;
            while (current != end)
            {
                current = Vector3.MoveTowards(current, end, m_player.MoveSpeed * m_timestep);
                m_waypoints.Add(current);
            }

            // in case position matches
            if (m_waypoints.Count == 0)
            {
                m_waypoints.Add(current);
            }

            // set up forwards
            m_forwards = Enumerable.Repeat(transform.forward, m_waypoints.Count).ToList();
        }

        private void IncrementTimer(float multiplier)
        {
            m_timer += Time.deltaTime * multiplier;

            while (m_timer >= m_timestep)
            {
                m_timer = Mathf.Max(0f, m_timer - m_timestep);
                AddWaypoint();
            }
        }

        private void AddWaypoint()
        {
            // add new one, remove old one
            m_waypoints.Add(transform.position);
            m_waypoints.RemoveAt(0);

            // add new one, remove old one
            m_forwards.Add(transform.forward);
            m_forwards.RemoveAt(0);
        }
    }
}
