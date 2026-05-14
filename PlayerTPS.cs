using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerTPS : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float crouchSpeed = 2.5f;
    [SerializeField] private float rotationSpeed = 10f;
    
    [Header("Jump")]
    [SerializeField] private float jumpForce = 5f;

    [Header("Gravity")]
    [SerializeField] private float gravity = -9.81f;
    private float velocityY;

    [Header("Crouch")]
    [SerializeField] private float standingHeight = 2f;
    [SerializeField] private float crouchHeight = 1f;

    [SerializeField] private Vector3 standingCenter = new Vector3(0, 1f, 0);
    [SerializeField] private Vector3 crouchCenter = new Vector3(0, 0.5f, 0);

    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Animator animator;

    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.3f;
    [SerializeField] private LayerMask groundMask;

    private bool isGrounded;
    private bool isCrouching;

    private CharacterController controller;
    private TPS inputActions;

    private Vector2 moveInput;
    private bool jumpPressed;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        inputActions = new TPS();
    }

    private void OnEnable()
    {
        inputActions.Enable();

        inputActions.Player.Move.performed += OnMove;
        inputActions.Player.Move.canceled += OnMove;

        inputActions.Player.Jump.performed += OnJump;

        // CROUCH
        inputActions.Player.Crouch.performed += OnCrouch;
    }

    private void OnDisable()
    {
        inputActions.Player.Move.performed -= OnMove;
        inputActions.Player.Move.canceled -= OnMove;

        inputActions.Player.Jump.performed -= OnJump;

        // CROUCH
        inputActions.Player.Crouch.performed -= OnCrouch;

        inputActions.Disable();
    }

    private void Update()
    {
        CheckGround();
        HandleMovement();
        HandleJump();
        ApplyGravity();
        UpdateAnimator();
    }

    private void CheckGround()
    {
        isGrounded = Physics.CheckSphere(
            groundCheck.position,
            groundDistance,
            groundMask
        );
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (!isCrouching)
        {
            jumpPressed = true;
        }
    }

    // CROUCH
    private void OnCrouch(InputAction.CallbackContext context)
    {
        isCrouching = !isCrouching;

        if (isCrouching)
        {
            controller.height = crouchHeight;
            controller.center = crouchCenter;
        }
        else
        {
            controller.height = standingHeight;
            controller.center = standingCenter;
        }
    }

    private void HandleMovement()
    {
        float currentSpeed = isCrouching ? crouchSpeed : moveSpeed;

        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);

        if (move.magnitude > 0.1f)
        {
            Vector3 camForward = cameraTransform.forward;
            Vector3 camRight = cameraTransform.right;

            camForward.y = 0;
            camRight.y = 0;

            Vector3 moveDirection = camForward * move.z + camRight * move.x;

            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );

            controller.Move(moveDirection.normalized * currentSpeed * Time.deltaTime);

            animator.SetBool("isWalk", true);
        }
        else
        {
            animator.SetBool("isWalk", false);
        }
    }

    private void HandleJump()
    {
        if (jumpPressed && isGrounded)
        {
            velocityY = Mathf.Sqrt(jumpForce * -2f * gravity);
            animator.SetTrigger("jump");
        }

        jumpPressed = false;
    }

    private void ApplyGravity()
    {
        if (isGrounded && velocityY < 0)
        {
            velocityY = -2f;
        }

        velocityY += gravity * Time.deltaTime;

        Vector3 gravityMove = new Vector3(0, velocityY, 0);
        controller.Move(gravityMove * Time.deltaTime);
    }

    private void UpdateAnimator()
    {
        animator.SetBool("isGrounded", isGrounded);
        animator.SetFloat("yVelocity", velocityY);

        // CROUCH ANIMATION
        animator.SetBool("isCrouch", isCrouching);
    }
}