using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
public class NpcController : MonoBehaviour
{
    private UnityEnvironment environment;
    public float speed = 3.0f;
    public float jumpHeight = 2.0f;
    private InputAction moveAction;
    private Rigidbody rb;
    private Inventory inventory;
    private ThirstControll thirstControll;
    public float drinkingRate = 1f;
    private Item EquippedItem { get; set; }
    public float itemInteractionRange = 2f;
    public EnviromentItemControll itemEnvironmentControll;
    private void Awake()
    {
        environment = new UnityEnvironment();
        rb = GetComponent<Rigidbody>();
        inventory = GetComponent<Inventory>();
        thirstControll = GetComponent<ThirstControll>();
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (IsGrounded())
        {
            rb.AddForce(new Vector3(0, jumpHeight, 0), ForceMode.Impulse);
        }
    }
    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, -Vector3.up, 0.1f);
    }

    public void DrinkItem(String itemName)
    {
        var item = inventory.GetItemByName(itemName);
        if (!(item is LiquidStorage liquid))
        {
            return;
        }

        thirstControll.DrinkWater(liquid.RemoveLiquid(drinkingRate));
    }
    private void FixedUpdate()
    {
        var moveDirection = environment.Player.Move.ReadValue<Vector2>();
        var speedDeltaTime = speed * Time.fixedDeltaTime;
        var movement = new Vector3(moveDirection.x, 0, moveDirection.y) * speedDeltaTime;
        rb.MovePosition(rb.position + movement);
    }

    private void OnEnable()
    {
        environment.Player.Move.Enable();
        environment.Player.Jump.performed += OnJump;
        environment.Player.Jump.Enable();
    }

    private void OnDisable()
    {
        environment.Player.Move.Disable();
        environment.Player.Jump.performed -= OnJump;
        environment.Player.Jump.Disable();
    }
}
