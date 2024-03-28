using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestArea : MonoBehaviour
{
    public GameObject[] props;
    public GameObject[] spawnAreas;
    public float range;
    public GameObject goal;

    public void ResetArea()
    {
        CleanArena();
        SpawnProps();
        goal.transform.position = new Vector3(Random.Range(-range, range), 0f, Random.Range(-range, range));
    }

    void SpawnProps()
    {
        foreach (var spawnArea in spawnAreas)
        {
            for (var i = 0; i < props.Length; i++)
            {
                var prop = props[i];
                var position = spawnArea.transform.position + new Vector3(Random.Range(-range, range), 0f,
                                       Random.Range(-range, range));
                var rotation = Quaternion.Euler(0f, Random.Range(0, 360), 0f);
                Instantiate(prop, position, rotation, transform);
            }
        }
    }


    void CleanArena()
    {
        foreach (Transform child in transform)
            if (child.CompareTag("Prop") || child.CompareTag("MovableProp"))
            {
                Destroy(child.gameObject);
            }
    }
}
