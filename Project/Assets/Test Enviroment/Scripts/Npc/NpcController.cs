using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class NpcController : MonoBehaviour
{
    private UnityEnvironment _environment;
    public float speed = 3.0f;
    public float jumpHeight = 2.0f;
    private InputAction _moveAction;
    private Rigidbody _rb;

    private void Awake()
    {
        _environment = new UnityEnvironment();
        _rb = GetComponent<Rigidbody>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (IsGrounded())
        {
            _rb.AddForce(new Vector3(0, jumpHeight, 0), ForceMode.Impulse);
        }
    }
    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, -Vector3.up, 0.1f);
    }

    private void FixedUpdate()
    {
        Vector2 moveDirection = _environment.Player.Move.ReadValue<Vector2>();
        float speedDeltaTime = speed * Time.fixedDeltaTime;
        Vector3 movement = new Vector3(moveDirection.x, 0, moveDirection.y) * speedDeltaTime;
        _rb.MovePosition(_rb.position + movement);
    }

    private void OnEnable()
    {
        _environment.Player.Move.Enable();
        _environment.Player.Jump.performed += OnJump;
        _environment.Player.Jump.Enable();

    }

    private void OnDisable()
    {
        _environment.Player.Move.Disable();
        _environment.Player.Jump.performed -= OnJump;
        _environment.Player.Jump.Disable();
    }
}
