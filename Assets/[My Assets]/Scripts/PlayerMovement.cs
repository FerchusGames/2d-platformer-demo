/*
 
    Based on the PlayerMovement.cs script by @Dawnosaur on GitHub.
    Game feel concepts learned from @Dawnosaur video: https://www.youtube.com/watch?v=KbtcEVCM7bw.
    Document about concepts learned: https://docs.google.com/document/d/1neL358fLZM3yz3O9kkJKi3RBuq3Bzq6jvvDE-C_UFkM/edit?usp=sharing.
 
*/

using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    #region VARIABLES
    public Rigidbody2D PlayerRigidbody2D { get; private set; }

    // State Control
    public bool IsFacingRight { get; private set; }
    public bool IsJumping { get; private set; }
    public bool IsJumpCut { get; private set; }
    public bool IsJumpFalling { get; private set; }

    // Timers
    public float LastOnGroundTime { get; private set; }
    public float LastPressedJumpTime { get; private set; }

    [Header ("Acceleration")]
    [SerializeField] private float _runMaxSpeed = default;
    [SerializeField] private float _runAccelerationRate = default;
    [SerializeField] private float _runDecelerationRate = default;
    [SerializeField, Range(0, 1f)] private float _airAccelerationMultiplier = default;
    [SerializeField, Range(0, 1f)] private float _airDecelerationMultiplier = default;

    [Header("Jumping")]
    [SerializeField] private float _jumpInputBufferTime = default;
    [SerializeField] private float _jumpHangTimeThreshold = default;
    [SerializeField] private float _jumpHangAccelerationMultiplier = default;
    [SerializeField] private float _jumpHangMaxSpeedMultiplier = default;

    [Header ("Checks")]
    [SerializeField] private Transform _groundCheckPoint = null;
    [SerializeField] private Vector2 _groundCheckSize = new Vector2(0.5f, 0.03f);

    [Header("Layers & Tags")]
    [SerializeField] private LayerMask _groundLayer = default;

    private Vector2 _moveInput = default;
    #endregion

    private void Awake()
    {
        PlayerRigidbody2D = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        IsFacingRight = true;
    }

    private void Update()
    {
        HandleInput();
        UpdateTimers();
    }

    private void FixedUpdate()
    {
        Run();
    }

    #region INPUT HANDLER
    private void HandleInput()
    {
        _moveInput.x = Input.GetAxisRaw("Horizontal");
        _moveInput.y = Input.GetAxisRaw("Vertical");

        if (_moveInput.x != 0)
        {
            CheckDirectionToFace(_moveInput.x > 0);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            OnJumpInput();
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            OnJumpUpInput();
        }
    }
    #endregion

    #region RUN METHODS
    private void Run()
    {
        float targetSpeed = _moveInput.x * _runMaxSpeed;

        #region CALCULATING ACCELERATION RATE
        float accelerationRate;

        // Our acceleration rate will differ depending on if we are trying to accelerate or if we are trying to stop completely.
        // It will also change if we are in the air or if we are grounded.

        if (LastOnGroundTime > 0)
        {
            accelerationRate = (Mathf.Abs(targetSpeed) > 0.01f) ? _runAccelerationRate : _runDecelerationRate;
        }

        else
        {
            accelerationRate = (Mathf.Abs(targetSpeed) > 0.01f) ? _runAccelerationRate * _airAccelerationMultiplier : _runDecelerationRate * _airDecelerationMultiplier;
        }
        #endregion

        #region ADD BONUS JUMP APEX ACCELERATION
        if ((IsJumping || IsJumpFalling) && Mathf.Abs(PlayerRigidbody2D.velocity.y) < _jumpHangTimeThreshold)
        {
            accelerationRate *= _jumpHangAccelerationMultiplier;
            targetSpeed *= _jumpHangMaxSpeedMultiplier;
        }
        #endregion

        float speedDifference = targetSpeed - PlayerRigidbody2D.velocity.x;

        float movement = speedDifference * accelerationRate;

        PlayerRigidbody2D.AddForce(movement * Vector2.right, ForceMode2D.Force);
    }
    private void CheckDirectionToFace(bool isMovingRight)
    {
        if (isMovingRight != IsFacingRight)
        {
            Turn();
        }
    }

    private void Turn()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;

        IsFacingRight = !IsFacingRight;
    }
    #endregion

    #region INPUT CALLBACKS
    private void OnJumpInput()
    {
        LastPressedJumpTime = _jumpInputBufferTime;
    }

    private void OnJumpUpInput()
    {
        if (CanJumpCut())
        {
            IsJumpCut = true;
        }
    }
    #endregion

    #region CHECK METHODS
    private bool CanJump()
    {
        return LastOnGroundTime > 0 && !IsJumping;
    }

    private bool CanJumpCut()
    {
        return IsJumping && PlayerRigidbody2D.velocity.y > 0;
    }
    #endregion

    private void UpdateTimers()
    {
        LastOnGroundTime -= Time.deltaTime;
        LastPressedJumpTime -= Time.deltaTime;
    }
}
