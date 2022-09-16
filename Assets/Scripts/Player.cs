using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using GameScenario;
using GameScenario.Utils;
using Interactions;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] private LayerMask m_terrainMask;
    [SerializeField] private float m_speed;
    [SerializeField] private float m_dashDistance;
    [SerializeField] private float m_dashDuration;
    [SerializeField] private float m_jumpForce;
    [SerializeField] private float m_rotationSpeed;
    [SerializeField] private int m_bonusJump;
    private int m_jumpLeft;
    private bool m_grounded;
    private bool m_canMove;

    [SerializeField] private Vector3 m_splashScale;
    private float m_lastVerticalVelocity;
    private float m_splashTimer = 0.0f;
    
    private Rigidbody2D m_rBody2D;
    private GameDirector m_gameDirector;
    [SerializeField] private Transform m_groundChecker;
    [SerializeField] private Transform m_visualTransform;
    [SerializeField] private SpriteRenderer m_spriteRenderer;
    [SerializeField] private LayerMask m_groundMask;
    [SerializeField] private ParticleSystem m_deathParticle;

    private List<AreaActionable> m_actionables;

    void Awake()
    {
        m_rBody2D = GetComponent<Rigidbody2D>();

        m_actionables = new List<AreaActionable>();

        m_grounded = false;
        m_canMove = true;
    }

    private void Start()
    {
        m_gameDirector = GameObject.FindWithTag("GameDirector").GetComponent<GameDirector>();
    }

    private void Update()
    {
        Collider2D l_groundCheckCollider = Physics2D.OverlapCircle(m_groundChecker.position, 0.2f, m_groundMask);
        m_grounded = !(l_groundCheckCollider is null);

        if (m_grounded)
        {
            m_jumpLeft = m_bonusJump;
        }

        Vector2 l_currentVelocity = m_rBody2D.velocity;
        float l_inputDirection = m_gameDirector.Input.actions["Move"].ReadValue<float>();

        if (m_canMove)
        {
            this.Move(l_inputDirection, ref l_currentVelocity);

            const float MaxVerticalSpeed = 10.0f;
            if (Mathf.Abs(l_currentVelocity.y) > 1.5f)
            {
                float l_ratio = Mathf.Min(Mathf.Abs(l_currentVelocity.y) / MaxVerticalSpeed, 1.0f);

                Vector3 l_spriteScale = m_visualTransform.localScale;
                l_spriteScale.x = 1.0f - l_ratio * 0.2f;
                l_spriteScale.y = 1.0f + l_ratio * 0.2f;

                m_visualTransform.localScale = l_spriteScale;
                m_lastVerticalVelocity = Mathf.Abs(l_currentVelocity.y);
            }
            else
            {
                if (m_lastVerticalVelocity > 3.0f)
                {
                    m_lastVerticalVelocity = 0.0f;
                    m_visualTransform.localScale = m_splashScale;
                    m_splashTimer = 0.25f;
                }

                if (m_splashTimer > 0.0f)
                {
                    float l_cursor = m_splashTimer / 0.25f;
                    m_visualTransform.localScale = Vector3.Lerp(
                        new Vector3(1.0f, 1.0f, 1.0f),
                        m_splashScale,
                        l_cursor);
                    m_splashTimer -= Time.deltaTime;
                }
            }
        }
        else
        {
            // l_currentVelocity = Vector2.zero;
        }

        this.Stretch();

        m_rBody2D.velocity = l_currentVelocity;
    }

    private void OnTriggerEnter2D(Collider2D p_collider2D)
    {
        AreaActionable l_actionable = p_collider2D.GetComponent<AreaActionable>();
        if (l_actionable is not null)
        {
            m_actionables.Add(l_actionable);
        }
    }

    private void OnTriggerExit2D(Collider2D p_collider2D)
    {
        AreaActionable l_actionable = p_collider2D.GetComponent<AreaActionable>();
        if (l_actionable is not null)
        {
            m_actionables.Remove(l_actionable);
        }
    }

    private void Stretch()
    {
        
    }
    
    /// <summary>
    /// Moving player along X axis (horizontally) based on given direction (player input)
    /// Managed player rotation on movement
    /// </summary>
    /// <param name="p_direction">Clamped between [-1;1]</param>
    /// <param name="p_currentVelocity">Modified velocity (on x axis only)</param>
    private void Move(float p_direction, ref Vector2 p_currentVelocity)
    {
        if (p_direction != 0.0f)
        {
            p_currentVelocity.x = p_direction * m_speed;
            
            Vector3 l_eulerAngles = m_spriteRenderer.transform.eulerAngles;
            l_eulerAngles.z -= Mathf.Sign(p_direction) * m_rotationSpeed * Time.deltaTime;
            m_spriteRenderer.transform.eulerAngles = l_eulerAngles;
        }
        else
            p_currentVelocity.x = 0.0f;
    }

    public void Dash()
    {
        if (!m_canMove) return;

        m_rBody2D.velocity = Vector2.zero;
        GameObject l_camObject = GameObject.FindWithTag("MainCamera");
        
        Vector3 l_initialPosition = transform.position;
        Vector2 l_mousePosition = Mouse.current.position.ReadValue();
        /*Vector3 l_worldMousePosition = m_camObject.GetComponent<Camera>()
            .ScreenToWorldPoint(new Vector3(l_mousePosition.x, l_mousePosition.y, m_camObject.GetComponent<Camera>().nearClipPlane));*/
        Vector3 l_worldMousePosition = GetWorldPositionOnPlane(new Vector3(l_mousePosition.x, l_mousePosition.y, 0.0f), 0.0f);
        Vector2 l_direction = (l_worldMousePosition - l_initialPosition);
        
        RaycastHit2D l_hit = Physics2D.Raycast(l_initialPosition, l_direction.normalized, float.PositiveInfinity, m_terrainMask);

        float l_targetDashDistance = Mathf.Min(l_direction.magnitude, m_dashDistance);

        if (l_hit.collider != null) l_targetDashDistance = Mathf.Min(l_targetDashDistance, l_hit.distance - 0.5f);
        
        Debug.Log(l_targetDashDistance);

        l_direction = l_direction.normalized;

        m_canMove = false;
        TimerHolder l_dashTimer = new TimerHolder()
        {
            Duration = m_dashDuration * (l_targetDashDistance / m_dashDistance)
        };

        const float startShakeProgress = 0.65f;
        l_dashTimer.OnUpdate += (p_timer, p_deltaTime) =>
        {
            float l_progress = 1 - Mathf.Pow(1.0f - p_timer.Progress, 4.0f);
            Vector2 l_delta = l_direction * (l_targetDashDistance * l_progress);

            if (l_progress > startShakeProgress)
            {
                var l_brain = l_camObject.GetComponent<CinemachineBrain>().ActiveVirtualCamera as CinemachineVirtualCamera;
                if (l_brain != null)
                {
                    float l_cameraShakeProgress = (l_progress - startShakeProgress) / (1 - startShakeProgress);
                    l_brain.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = 5.0f * l_cameraShakeProgress;
                } 
            }

            transform.position = l_initialPosition + new Vector3(l_delta.x, l_delta.y, 0.0f);
        };

        l_dashTimer.OnEnd += (_) =>
        {
            var l_brain = l_camObject.GetComponent<CinemachineBrain>().ActiveVirtualCamera as CinemachineVirtualCamera;
            if (l_brain != null)
            {
                l_brain.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = 0.0f;
            }
            m_canMove = true;
        };
        
        GameObject.FindWithTag("GameDirector").GetComponent<GameDirector>().AddTimer(l_dashTimer);
    }
    
    public void Jump()
    {
        if (m_grounded || m_jumpLeft > 0)
        {
            Vector2 l_currentVelocity = m_rBody2D.velocity;
            l_currentVelocity.y = 0.0f;
            m_rBody2D.velocity = l_currentVelocity;
            m_rBody2D.AddForce(new Vector2(0, m_jumpForce), ForceMode2D.Impulse);
            m_jumpLeft--;
            m_grounded = false;
        }
    }
    
    public void ProcessInteractions()
    {
        if (m_actionables.Count > 0)
        {
            m_actionables.First().Execute(this);
        }
    }

    public void Die()
    {
        m_deathParticle.Play();
        m_visualTransform.gameObject.SetActive(false);
        m_rBody2D.velocity = Vector2.zero; // reset velocity to prevent inertia movements
        m_rBody2D.isKinematic = true;
        enabled = false;
    }

    public void Resurrect()
    {
        m_visualTransform.gameObject.SetActive(true);
        m_rBody2D.isKinematic = false;
        enabled = true;
    }
    
    public Vector3 GetWorldPositionOnPlane(Vector3 screenPosition, float z) {
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        Plane xy = new Plane(Vector3.forward, new Vector3(0, 0, z));
        float distance;
        xy.Raycast(ray, out distance);
        return ray.GetPoint(distance);
    }
}