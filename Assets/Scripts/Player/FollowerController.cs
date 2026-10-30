using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Train.Player
{
    public class FollowerController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerController m_player;
        [SerializeField] private FollowerController m_follower;
        [SerializeField] private Collider m_col;

        [Header("Fields")]
        [SerializeField, Min(0f)] private float m_timestep = 0.0133f;

        private readonly List<Vector3> m_waypoints = new();

        private float m_timer;
        private float m_disabledCountdown;

        private Vector3 m_lastPosition;

        private void Update()
        {
            // update timer
            IncrementTimer(m_player.CurrentSpeed / m_player.MoveSpeed);

            // update pos and forward
            float moveSpeed = m_player.CurrentSpeed * Time.deltaTime;
            Vector3 currentPos = m_follower.transform.position;
            m_follower.transform.position = Vector3.MoveTowards(currentPos, m_waypoints[0], moveSpeed);

            // only update forward is position is different, otherwise it snaps to grid-aligned
            if (m_lastPosition != currentPos)
            {
                m_follower.transform.forward = (currentPos - m_lastPosition).normalized;
                m_lastPosition = currentPos;
            }

            // do collider disabled timer
            if (m_disabledCountdown > 0f)
            {
                m_disabledCountdown -= Time.deltaTime;
                if (m_disabledCountdown <= 0f)
                {
                    m_col.enabled = true;
                }
            }
        }

        public void Begin(PlayerController player, FollowerController follower)
        {
            // enable
            enabled = true;

            // set references
            m_player = player;
            m_follower = follower;

            // add initial waypoints
            Vector3 start = m_follower.transform.position;
            Vector3 end = transform.position;
            Vector3 current = start;
            while (current != end)
            {
                current = Vector3.MoveTowards(current, end, m_player.MoveSpeed * m_timestep);
                m_waypoints.Add(current);
            }

            // set initial position for forward
            m_lastPosition = start;

            // in case position matches
            if (m_waypoints.Count == 0)
            {
                m_waypoints.Add(current);
            }
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
        }

        public void OnCollideWithPlayer(float disableDuration)
        {
            // if its the FollowerController attached to the player, we don't want to disable their collider
            // otherwise they'll be able to go through walls n shit
            if (m_col != null)
            {
                m_col.enabled = false;
                m_disabledCountdown = disableDuration;
            }
        }
    }
}
