using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameScenario;
using Interactions;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] private float m_speed;
    [SerializeField] private float m_jumpForce;
    [SerializeField] private float m_rotationSpeed;
    [SerializeField] private int m_bonusJump;
    private int m_jumpLeft;
    private bool m_grounded;

    [SerializeField] private Vector3 m_splashScale;
    private float m_lastVerticalVelocity;
    private float m_splashTimer = 0.0f;

    private GameDirector m_gameDirector;
    private Rigidbody2D m_rBody2D;
    [SerializeField] private Transform m_groundChecker;
    [SerializeField] private Transform m_visualTransform;
    [SerializeField] private SpriteRenderer m_spriteRenderer;
    [SerializeField] private LayerMask m_groundMask;
    [SerializeField] private ParticleSystem m_deathParticle;

    private List<AreaActionable> m_actionables;

    private void Awake()
    {
        m_rBody2D = GetComponent<Rigidbody2D>();

        m_actionables = new List<AreaActionable>();

        m_grounded = false;
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
        if (m_gameDirector.Input.actions["Move"].IsPressed())
        {
            float l_rawSpeed = m_gameDirector.Input.actions["Move"].ReadValue<float>();

            l_currentVelocity.x = l_rawSpeed * m_speed;

            Vector3 l_eulerAngles = m_spriteRenderer.transform.eulerAngles;
            l_eulerAngles.z -= Mathf.Sign(l_rawSpeed) * m_rotationSpeed * Time.deltaTime;
            m_spriteRenderer.transform.eulerAngles = l_eulerAngles;
        }
        else
            l_currentVelocity.x = 0;

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

    public void Jump()
    {
        if (m_grounded || m_jumpLeft > 0)
        {
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
        enabled = false;
    }

    public void Resurrect()
    {
        m_visualTransform.gameObject.SetActive(true);
        enabled = true;
    }
}