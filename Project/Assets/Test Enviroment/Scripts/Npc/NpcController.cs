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
    private void Awake()
    {
        environment = new UnityEnvironment();
        rb = GetComponent<Rigidbody>();
        inventory = GetComponent<Inventory>();
    }

    public void OnJump(InputAction.CallbackContext context)
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

    public void OnAddItem(InputAction.CallbackContext context)
    {
        if (inventory == null)
        {
            Debug.LogError("Inventory is null");
            return;
        }

        float pickupRange = 5f; // Set the range for picking up items
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, pickupRange);
        foreach (var hitCollider in hitColliders)
        {
            Item item = hitCollider.GetComponent<Item>();
            if (item != null)
            {
                inventory.AddItem(item, 1); // Add each item in range to the inventory
            }
            else
            {
                Debug.LogError("Item is null");
            }
        }
    }

    public void OnDropItem(InputAction.CallbackContext context)
    {
        foreach (var item in inventory.items.Keys.ToList()) // Loop through all items in the inventory
        {
            while (inventory.items[item] > 0) // Drop each item until its quantity is 0
            {
                inventory.DropItem(item);
            }
        }
    }

    private void FixedUpdate()
    {
        Vector2 moveDirection = environment.Player.Move.ReadValue<Vector2>();
        float speedDeltaTime = speed * Time.fixedDeltaTime;
        Vector3 movement = new Vector3(moveDirection.x, 0, moveDirection.y) * speedDeltaTime;
        rb.MovePosition(rb.position + movement);
    }

    private void OnEnable()
    {
        environment.Player.Move.Enable();
        environment.Player.Jump.performed += OnJump;
        environment.Player.Jump.Enable();
        environment.Player.AddItem.performed += OnAddItem;
        environment.Player.AddItem.Enable();
        environment.Player.DropItem.performed += OnDropItem;
        environment.Player.DropItem.Enable();
    }

    private void OnDisable()
    {
        environment.Player.Move.Disable();
        environment.Player.Jump.performed -= OnJump;
        environment.Player.Jump.Disable();
        environment.Player.AddItem.performed -= OnAddItem;
        environment.Player.AddItem.Disable();
        environment.Player.DropItem.performed -= OnDropItem;
        environment.Player.DropItem.Disable();
    }
}
