using UnityEngine;

namespace Terrain
{
    public class ParallaxItem : MonoBehaviour
    {
        public Transform m_camera;
        public float relativeMove = .3f;

        private Vector3 m_initialPosition;
        private Vector3 m_initialCamPosition;

        private void Awake()
        {
            m_initialPosition = transform.position;
            m_initialCamPosition = m_camera.position;
        }

        void Update()
        {
            Vector3 l_delta = m_initialCamPosition - m_camera.position;
            transform.position = new Vector3(m_initialPosition.x + l_delta.x * relativeMove, m_initialPosition.y, m_initialPosition.z);
        }
    }
}