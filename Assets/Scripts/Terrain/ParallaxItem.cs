using UnityEngine;

namespace Terrain
{
    [ExecuteInEditMode]
    public class ParallaxItem : MonoBehaviour
    {
        [Header("Layer settings")]
        public float relativeMove = .3f;
        [SerializeField] private Vector3 m_initialPosition;

        private Transform m_camera;

        private void Start()
        {
            m_camera = GameObject.FindWithTag("MainCamera").transform;
        }

        private void Update()
        {
            if (!UnityEditor.EditorApplication.isPlaying) UpdatePosition();
        }

        private void FixedUpdate()
        {
            if (UnityEditor.EditorApplication.isPlaying) UpdatePosition();
        }
        
        void UpdatePosition()
        {
            
            Vector3 l_delta = m_camera.position;
            transform.position = new Vector3(m_initialPosition.x + l_delta.x * relativeMove, m_initialPosition.y, m_initialPosition.z);
        }
    }
}