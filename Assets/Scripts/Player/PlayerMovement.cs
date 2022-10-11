using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using GameScenario;
using GameScenario.Utils;
using Interactions;
using Player;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Components")]
    private Rigidbody2D m_body2D;
    private PlayerGround m_groundChecker;
    private PlayerAttack m_attack;

    [Header("Movement Stats")]
    [SerializeField, Range(0f, 20f)][Tooltip("Maximum movement speed")] public float m_maxSpeed = 10f;
    [SerializeField, Range(0f, 100f)][Tooltip("How fast to reach max speed")] public float m_maxAcceleration = 52f;
    [SerializeField, Range(0f, 100f)][Tooltip("How fast to stop after letting go")] public float m_maxDecceleration = 52f;
    [SerializeField, Range(0f, 100f)][Tooltip("How fast to stop when changing direction")] public float m_maxTurnSpeed = 80f;
    [SerializeField, Range(0f, 100f)][Tooltip("How fast to reach max speed when in mid-air")] public float m_maxAirAcceleration;
    [SerializeField, Range(0f, 100f)][Tooltip("How fast to stop in mid-air when no direction is used")] public float m_maxAirDeceleration;
    [SerializeField, Range(0f, 100f)][Tooltip("How fast to stop when changing direction when in mid-air")] public float m_maxAirTurnSpeed = 80f;
    [SerializeField][Tooltip("Friction to apply against movement on stick")] private float m_friction;

    [Header("Options")]
    [Tooltip("When false, the charcter will skip acceleration and deceleration and instantly move and stop")] public bool m_useAcceleration;

    [Header("Calculations")]
    public float m_directionX;
    private Vector2 m_desiredVelocity;
    public Vector2 m_velocity;
    private float m_maxSpeedChange;
    private float m_acceleration;
    private float m_deceleration;
    private float m_turnSpeed;

    [Header("Current State")]
    public bool m_grounded;
    public bool m_pressingKey;

    private void Awake()
    {
        //Find the character's Rigidbody and ground detection script
        m_body2D = GetComponent<Rigidbody2D>();
        m_groundChecker = GetComponent<PlayerGround>();
        m_attack = GetComponent<PlayerAttack>();
    }

    public void OnMovement(InputAction.CallbackContext p_context)
    {
        //This is called when you input a direction on a valid input type, such as arrow keys or analogue stick
        //The value will read -1 when pressing left, 0 when idle, and 1 when pressing right.
        
        m_directionX = p_context.ReadValue<float>();
    }

    private void Update()
    {
        //Used to flip the character's sprite when she changes direction
        //Also tells us that we are currently pressing a direction button
        if (m_directionX != 0 && !m_attack.Attacking)
        {
            transform.localScale = new Vector3(m_directionX > 0 ? 1 : -1, 1, 1);
            m_pressingKey = true;
        }
        else
        {
            m_pressingKey = false;
        }

        //Calculate's the character's desired velocity - which is the direction you are facing, multiplied by the character's maximum speed
        //Friction is not used in this game
        m_desiredVelocity = m_attack.Attacking ? 
            Vector2.zero : 
            new Vector2(m_directionX, 0f) * Mathf.Max(m_maxSpeed - m_friction, 0f);
    }

    private void FixedUpdate()
    {
        //Fixed update runs in sync with Unity's physics engine
        m_grounded = m_groundChecker.OnGround;

        //Get the Rigidbody's current velocity
        m_velocity = m_body2D.velocity;

        //Calculate movement, depending on whether "Instant Movement" has been checked
        if (m_useAcceleration)
        {
            RunWithAcceleration();
        }
        else
        {
            if (m_grounded)
            {
                RunWithoutAcceleration();
            }
            else
            {
                RunWithAcceleration();
            }
        }
    }

    private void RunWithAcceleration()
    {
        //Set our acceleration, deceleration, and turn speed stats, based on whether we're on the ground on in the air

        m_acceleration = m_grounded ? m_maxAcceleration : m_maxAirAcceleration;
        m_deceleration = m_grounded ? m_maxDecceleration : m_maxAirDeceleration;
        m_turnSpeed = m_grounded ? m_maxTurnSpeed : m_maxAirTurnSpeed;

        if (m_pressingKey)
        {
            //If the sign (i.e. positive or negative) of our input direction doesn't match our movement, it means we're turning around and so should use the turn speed stat.
            if (Mathf.Sign(m_directionX) != Mathf.Sign(m_velocity.x))
            {
                m_maxSpeedChange = m_turnSpeed * Time.deltaTime;
            }
            else
            {
                //If they match, it means we're simply running along and so should use the acceleration stat
                m_maxSpeedChange = m_acceleration * Time.deltaTime;
            }
        }
        else
        {
            //And if we're not pressing a direction at all, use the deceleration stat
            m_maxSpeedChange = m_deceleration * Time.deltaTime;
        }

        //Move our velocity towards the desired velocity, at the rate of the number calculated above
        m_velocity.x = Mathf.MoveTowards(m_velocity.x, m_desiredVelocity.x, m_maxSpeedChange);

        //Update the Rigidbody with this new velocity
        m_body2D.velocity = m_velocity;

    }

    private void RunWithoutAcceleration()
    {
        //If we're not using acceleration and deceleration, just send our desired velocity (direction * max speed) to the Rigidbody
        m_velocity.x = m_desiredVelocity.x;

        m_body2D.velocity = m_velocity;
    }
}