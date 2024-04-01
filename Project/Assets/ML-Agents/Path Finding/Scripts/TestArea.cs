using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Random = UnityEngine.Random;

public class TestArea : MonoBehaviour
{
    [SerializeField] private GameObject[] props;
    [SerializeField] private GameObject[] spawnAreas;
    [SerializeField] private GameObject floor;
    public GameObject goal;

    // Create a dictionary to store the bounds of each spawn area
    private Dictionary<string, Bounds> spawnAreaBounds;
    private Bounds floorSize;

    private void Start()
    {
        floorSize = floor.GetComponent<Renderer>().bounds;

        spawnAreaBounds = new Dictionary<string, Bounds>();

        foreach (var spawnArea in spawnAreas)
        {
            var spawnAreaRenderer = spawnArea.GetComponent<Renderer>();
            if (spawnAreaRenderer != null)
            {
                spawnAreaBounds.Add(spawnArea.name, spawnAreaRenderer.bounds);
            }
            else
            {
                Debug.LogError("Renderer component is not attached to " + spawnArea.name);
            }
        }
    }

    public void ResetArea()
    {
        CleanArena();
        SpawnProps();
        goal.transform.position = new Vector3(
            Random.Range(floorSize.min.x, floorSize.max.x),
            0f,
            Random.Range(floorSize.min.z, floorSize.max.z)
        );
    }

    private void SpawnProps()
    {
        foreach (var spawnArea in spawnAreas)
        {
            // Get the cached bounds of the spawn area
            var bounds = spawnAreaBounds[spawnArea.name];

            foreach (var prop in props)
            {
                // Generate a random position within the bounds of the spawn area
                var position = new Vector3(
                    Random.Range(bounds.min.x, bounds.max.x),
                    0f,
                    Random.Range(bounds.min.z, bounds.max.z)
                );

                var rotation = Quaternion.Euler(0f, Random.Range(0, 360), 0f);
                Instantiate(prop, position, rotation, transform);
            }
        }
    }

    private void CleanArena()
    {
        foreach (Transform child in transform)
            if (child.CompareTag("prop") || child.CompareTag("movableProp"))
            {
                Destroy(child.gameObject);
            }
    }
}
