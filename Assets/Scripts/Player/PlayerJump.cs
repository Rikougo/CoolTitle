using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class PlayerJump : MonoBehaviour
    {
        [Header("Components")] [HideInInspector]
        public Rigidbody2D m_body2D;
        private Animator m_animator;

        private PlayerGround m_ground;
        [HideInInspector] public Vector2 m_velocity;

        [Header("Jumping Stats")] [SerializeField, Range(2f, 5.5f)] [Tooltip("Maximum jump height")]
        public float m_jumpHeight = 7.3f;


//If you're using your stats from Platformer Toolkit with this character controller, please note that the number on the Jump Duration handle does not match this stat
//It is re-scaled, from 0.2f - 1.25f, to 1 - 10.
//You can transform the number on screen to the stat here, using the function at the bottom of this script


        [SerializeField, Range(0.2f, 1.25f)] [Tooltip("How long it takes to reach that height before coming back down")]
        public float m_timeToJumpApex;

        [SerializeField, Range(0f, 5f)] [Tooltip("Gravity multiplier to apply when going up")]
        public float m_upwardMovementMultiplier = 1f;

        [SerializeField, Range(1f, 10f)] [Tooltip("Gravity multiplier to apply when coming down")]
        public float m_downwardMovementMultiplier = 6.17f;

        [SerializeField, Range(0, 1)] [Tooltip("How many times can you jump in the air?")]
        public int m_maxAirJumps = 0;

        [Header("Options")] [Tooltip("Should the character drop when you let go of jump?")]
        public bool m_variablejumpHeight;

        [SerializeField, Range(1f, 10f)] [Tooltip("Gravity multiplier when you let go of jump")]
        public float m_jumpCutOff;

        [SerializeField] [Tooltip("The fastest speed the character can fall")]
        public float m_speedLimit;

        [SerializeField, Range(0f, 0.3f)] [Tooltip("How long should coyote time last?")]
        public float m_coyoteTime = 0.15f;

        [SerializeField, Range(0f, 0.3f)] [Tooltip("How far from ground should we cache your jump?")]
        public float jumpBuffer = 0.15f;

        [Header("Calculations")] public float m_jumpSpeed;
        private float m_defaultGravityScale;
        public float m_gravMultiplier;

        [Header("Current State")] public bool m_canJumpAgain = false;
        private bool m_desiredJump;
        private float m_jumpBufferCounter;
        private float m_coyoteTimeCounter = 0;
        private bool m_pressingJump;
        public bool m_onGround;
        private bool m_currentlyJumping;
        
        private static readonly int JumpAnimID = Animator.StringToHash("jumpTrigger");
        private static readonly int GroundedAnimID = Animator.StringToHash("grounded");
        private static readonly int HorizontalVelocityAnimID = Animator.StringToHash("horizontal_velocity");
        private static readonly int VerticalVelocityAnimID = Animator.StringToHash("vertical_velocity");
        private static readonly int Jumping = Animator.StringToHash("jumping");

        void Awake()
        {
            //Find the character's Rigidbody and ground detection and juice scripts

            m_body2D = GetComponent<Rigidbody2D>();
            m_ground = GetComponent<PlayerGround>();
            m_animator = GetComponentInChildren<Animator>();
            // juice = GetComponentInChildren<characterJuice>();
            m_defaultGravityScale = 1f;
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            //This function is called when one of the jump buttons (like space or the A button) is pressed.

            //When we press the jump button, tell the script that we desire a jump.
            //Also, use the started and canceled contexts to know if we're currently holding the button
            if (context.started)
            {
                m_desiredJump = true;
                m_pressingJump = true;
                
                m_animator.SetTrigger(JumpAnimID);
            }

            if (context.canceled)
            {
                m_pressingJump = false;
            }
        }

        void Update()
        {
            setPhysics();

            //Check if we're on ground, using Kit's Ground script
            m_onGround = m_ground.OnGround;
            
            m_animator.SetTrigger(GroundedAnimID);

            //Jump buffer allows us to queue up a jump, which will play when we next hit the ground
            if (jumpBuffer > 0)
            {
                //Instead of immediately turning off "desireJump", start counting up...
                //All the while, the DoAJump function will repeatedly be fired off
                if (m_desiredJump)
                {
                    m_jumpBufferCounter += Time.deltaTime;

                    if (m_jumpBufferCounter > jumpBuffer)
                    {
                        //If time exceeds the jump buffer, turn off "desireJump"
                        m_desiredJump = false;
                        m_jumpBufferCounter = 0;
                    }
                }
            }

            //If we're not on the ground and we're not currently jumping, that means we've stepped off the edge of a platform.
            //So, start the coyote time counter...
            if (!m_currentlyJumping && !m_onGround)
            {
                m_coyoteTimeCounter += Time.deltaTime;
            }
            else
            {
                //Reset it when we touch the ground, or jump
                m_coyoteTimeCounter = 0;
            }
            
            m_animator.SetFloat(HorizontalVelocityAnimID, Mathf.Abs(m_body2D.velocity.x));
            m_animator.SetFloat(VerticalVelocityAnimID, m_body2D.velocity.y);
            m_animator.SetBool(Jumping, m_currentlyJumping);
        }

        private void setPhysics()
        {
            //Determine the character's gravity scale, using the stats provided. Multiply it by a gravMultiplier, used later
            Vector2 newGravity = new Vector2(0, (-2 * m_jumpHeight) / (m_timeToJumpApex * m_timeToJumpApex));
            m_body2D.gravityScale = (newGravity.y / Physics2D.gravity.y) * m_gravMultiplier;
        }

        private void FixedUpdate()
        {
            //Get velocity from Kit's Rigidbody 
            m_velocity = m_body2D.velocity;

            //Keep trying to do a jump, for as long as desiredJump is true
            if (m_desiredJump)
            {
                DoAJump();
                m_body2D.velocity = m_velocity;

                //Skip gravity calculations this frame, so currentlyJumping doesn't turn off
                //This makes sure you can't do the coyote time double jump bug
                return;
            }

            calculateGravity();
        }

        private void calculateGravity()
        {
            //We change the character's gravity based on her Y direction

            //If Kit is going up...
            if (m_body2D.velocity.y > 0.01f)
            {
                if (m_onGround)
                {
                    //Don't change it if Kit is stood on something (such as a moving platform)
                    m_gravMultiplier = m_defaultGravityScale;
                }
                else
                {
                    //If we're using variable jump height...)
                    if (m_variablejumpHeight)
                    {
                        //Apply upward multiplier if player is rising and holding jump
                        if (m_pressingJump && m_currentlyJumping)
                        {
                            m_gravMultiplier = m_upwardMovementMultiplier;
                        }
                        //But apply a special downward multiplier if the player lets go of jump
                        else
                        {
                            m_gravMultiplier = m_jumpCutOff;
                        }
                    }
                    else
                    {
                        m_gravMultiplier = m_upwardMovementMultiplier;
                    }
                }
            }

            //Else if going down...
            else if (m_body2D.velocity.y < -0.01f)
            {
                if (m_onGround)
                    //Don't change it if Kit is stood on something (such as a moving platform)
                {
                    m_gravMultiplier = m_defaultGravityScale;
                }
                else
                {
                    //Otherwise, apply the downward gravity multiplier as Kit comes back to Earth
                    m_gravMultiplier = m_downwardMovementMultiplier;
                }
            }
            //Else not moving vertically at all
            else
            {
                if (m_onGround)
                {
                    m_currentlyJumping = false;
                }

                m_gravMultiplier = m_defaultGravityScale;
            }

            //Set the character's Rigidbody's velocity
            //But clamp the Y variable within the bounds of the speed limit, for the terminal velocity assist option
            m_body2D.velocity = new Vector3(m_velocity.x, Mathf.Clamp(m_velocity.y, -m_speedLimit, 100));
        }

        private void DoAJump()
        {
            //Create the jump, provided we are on the ground, in coyote time, or have a double jump available
            if (m_onGround || (m_coyoteTimeCounter > 0.03f && m_coyoteTimeCounter < m_coyoteTime) || m_canJumpAgain)
            {
                m_animator.SetTrigger(JumpAnimID);
                m_desiredJump = false;
                m_jumpBufferCounter = 0;
                m_coyoteTimeCounter = 0;

                //If we have double jump on, allow us to jump again (but only once)
                m_canJumpAgain = (m_maxAirJumps == 1 && m_canJumpAgain == false);

                //Determine the power of the jump, based on our gravity and stats
                m_jumpSpeed = Mathf.Sqrt(-2f * Physics2D.gravity.y * m_body2D.gravityScale * m_jumpHeight);

                //If Kit is moving up or down when she jumps (such as when doing a double jump), change the jumpSpeed;
                //This will ensure the jump is the exact same strength, no matter your velocity.
                if (m_velocity.y > 0f)
                {
                    m_jumpSpeed = Mathf.Max(m_jumpSpeed - m_velocity.y, 0f);
                }
                else if (m_velocity.y < 0f)
                {
                    m_jumpSpeed += Mathf.Abs(m_body2D.velocity.y);
                }

                //Apply the new jumpSpeed to the velocity. It will be sent to the Rigidbody in FixedUpdate;
                m_velocity.y += m_jumpSpeed;
                m_currentlyJumping = true;
            }

            if (jumpBuffer == 0)
            {
                //If we don't have a jump buffer, then turn off desiredJump immediately after hitting jumping
                m_desiredJump = false;
            }
        }

        public void bounceUp(float bounceAmount)
        {
            //Used by the springy pad
            m_body2D.AddForce(Vector2.up * bounceAmount, ForceMode2D.Impulse);
        }
    }
}