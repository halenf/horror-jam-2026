using UnityEngine;

namespace Train.Collectable
{
    using Manager;

    public class Collectable : MonoBehaviour
    {

        [SerializeField] protected float m_value;

        public virtual void Intialize(float value)
        {
            m_value = value;
        }

        public virtual void OnTriggerEnter(Collider other)
        {
            Collect();
        }

        public virtual void Collect()
        {
            TimerScript.Instance.AddTime(m_value);

            //Die
            Destroy(gameObject);
        }
    }
}


