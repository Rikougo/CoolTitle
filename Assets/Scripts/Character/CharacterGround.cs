using UnityEngine;

namespace Character
{
    public class CharacterGround : MonoBehaviour
    {
        private bool m_onGround;
        
        public bool OnGround => m_onGround;
       
        [Header("Collider Settings")]
        [SerializeField][Tooltip("Length of the ground-checking collider")] private float m_groundLength = 0.95f;
        [SerializeField][Tooltip("Distance between the ground-checking colliders")] private Vector3 m_colliderOffset;

        [Header("Layer Masks")]
        [SerializeField][Tooltip("Which layers are read as the ground")] private LayerMask m_groundLayer;
 

        private void Update()
        {
            //Determine if the player is stood on objects on the ground layer, using a pair of raycasts
            m_onGround = Physics2D.Raycast(transform.position + m_colliderOffset, Vector2.down, m_groundLength, m_groundLayer);
        }

        private void OnDrawGizmos()
        {
            //Draw the ground colliders on screen for debug purposes
            if (m_onGround) { Gizmos.color = Color.green; } else { Gizmos.color = Color.red; }

            var l_position = transform.position;
            Gizmos.DrawLine(l_position + m_colliderOffset, l_position + m_colliderOffset + Vector3.down * m_groundLength);
        }
    }
}