using UnityEngine;

[RequireComponent(typeof(CharacterMovement))]
public class MonkBrain : MonoBehaviour
{
    [Header("AI settings")] [SerializeField]
    private float m_targetDistance = 1.5f;
    
    private Transform m_playerTransform;

    private CharacterMovement m_movement;
    private void Start()
    {
        m_playerTransform = GameObject.FindWithTag("Player").transform;
        m_movement = GetComponent<CharacterMovement>();
    }

    private void FixedUpdate()
    {
        float l_rawDist = m_playerTransform.position.x - transform.position.x;
        if (Mathf.Abs(l_rawDist) > m_targetDistance)
        {
            Debug.Log(Mathf.Sign(l_rawDist));
            m_movement.SetXDirection(Mathf.Sign(l_rawDist));
        }
        else
        {
            m_movement.SetXDirection(0);
        }
    }
}
