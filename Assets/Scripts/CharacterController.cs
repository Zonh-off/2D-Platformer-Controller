using System;
using UnityEngine;

public class CharacterController : MonoBehaviour {
    public static CharacterController Instance { get; private set; }

    #region Speed
    [SerializeField] private float runMaxSpeed = 10f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float runAcceleration;
    [SerializeField] private float runDecceleration;
    [SerializeField] private float velPower;
    #endregion

    #region Jump
    private bool isJumping;
    private float _lastOnGroundTime;
    #endregion

    #region WallJump
    private bool isWallJumping;
    private float wallJumpingDirection;
    private float wallJumpingTime = 0.2f;
    private float wallJumpingCounter;
    private float wallJumpingDuration = 0.2f;
    [SerializeField] private Vector2 wallJumpingPower = new Vector2(8f, 22f);
    #endregion

    #region Gravity
    [Range(0f, 0.99f)][SerializeField] private float _jumpCutMultiplier;
    [SerializeField] private float _gravityScale;
    [SerializeField] private float _fallGravityMultiplier;
    #endregion

    #region WallSlide
    private bool isWallSliding;
    [SerializeField] private float wallSlidingSpeed = 2f;
    #endregion

    #region Checks
    [SerializeField] private Transform wallCheck;
    [SerializeField] private Transform _groundCheckPoint;
    [SerializeField] private Vector2 _groundCheckSize = new Vector2(0.49f, 0.03f);
    [SerializeField] private LayerMask _groundLayer;
    #endregion

    private bool isFacingRight;
    private float movement;
    private Rigidbody2D RB;
    private Vector2 moveInput;

    void Awake() {
        Instance = this;
        
        RB = GetComponent<Rigidbody2D>();
        isFacingRight = true;
    }
    
    private void Update() {
        _lastOnGroundTime -= Time.deltaTime;

        if(moveInput.x != 0 && !isWallJumping)
            CheckDirectionToFace(moveInput.x > 0);

        if(Physics2D.OverlapBox(_groundCheckPoint.position, _groundCheckSize, 0, _groundLayer)) {
            _lastOnGroundTime = 0.1f;
            isJumping = false;
        }

        OnJump();
        WallSlide();
        WallJump();

        if(Input.GetKeyDown(KeyCode.Space) && CanJump()) {
            Jump();
        }
    }

    private void FixedUpdate() {
        if(!isWallJumping)
            Run();
    }

    private void Run() {
        moveInput.x = Input.GetAxisRaw("Horizontal");

        float targetSpeed = moveInput.x * runMaxSpeed;
        float speedDif = targetSpeed - RB.velocity.x;
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? runAcceleration : runDecceleration;
        movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, velPower) * Mathf.Sign(speedDif);
                
        RB.AddForce(movement * Vector2.right);
    }

    private void Jump() {
        RB.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        _lastOnGroundTime = 0;
    }

    private void OnJump() {
        if(RB.velocity.y > 0)
            isJumping = true;

        if(isJumping && RB.velocity.y > 0) {
            RB.AddForce(Vector2.down * RB.velocity.y * (1 - _jumpCutMultiplier), ForceMode2D.Impulse);
        }

        if(RB.velocity.y < 0) {
            RB.gravityScale = _gravityScale * _fallGravityMultiplier;
        } else {
            RB.gravityScale = _gravityScale;
        }
    }
    
    private bool IsWalled() {
        return Physics2D.OverlapCircle(wallCheck.position, 0.2f, _groundLayer);
    }

    public bool CanJump() {
        if(_lastOnGroundTime > 0 && !isJumping) {
            return true;
        } else {
            return false;
        }
    }

    public bool IsWallSlide() {
        return isWallSliding && !isWallJumping;
    }
    public bool IsJumping() {
        return isJumping;
    }

    private void WallSlide() {
        if(IsWalled() && isJumping && moveInput.x != 0) {
            isWallSliding = true;
            RB.velocity = new Vector2(RB.velocity.x, Math.Clamp(RB.velocity.y, -wallSlidingSpeed, float.MaxValue));
        } else {
            isWallSliding = false;
        }
    }

    private void WallJump() {
        if(isWallSliding) {
            isWallJumping = false;
            wallJumpingDirection = -transform.localScale.x;
            wallJumpingCounter = wallJumpingTime;

            CancelInvoke(nameof(StopWallJumping));
        } else {
            wallJumpingCounter -= Time.deltaTime;
        }

        if(Input.GetKeyDown(KeyCode.Space) && wallJumpingCounter > 0) {
            isWallJumping = true;
            isWallSliding = false;
            RB.velocity = new Vector2(wallJumpingDirection * wallJumpingPower.x, wallJumpingPower.y);
            wallJumpingCounter = 0f;

            if(transform.localScale.x != wallJumpingDirection) {
                Turn();
            }

            Invoke(nameof(StopWallJumping), wallJumpingDuration);
        }
    }

    private void StopWallJumping() {
        isWallJumping = false; 
    }

    public void CheckDirectionToFace(bool isMovingRight) {
        if(isMovingRight != isFacingRight)
            Turn();
    }

    public float GetMovement() {
        return moveInput.x;
    }

    private void Turn() {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;

        isFacingRight = !isFacingRight;
    }
}
