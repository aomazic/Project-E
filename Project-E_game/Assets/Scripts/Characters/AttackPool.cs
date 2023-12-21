using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class AttackPool : MonoBehaviour
{
    [SerializeField] private GameObject attackPrefab;
    private Queue<GameObject> attackPool = new Queue<GameObject>();
    [SerializeField] private int poolSize = 10;
    [SerializeField] private int maxPoolSize = 20;
    public string attackPrefabName;

    void Start()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(attackPrefab);
            obj.SetActive(false);
            attackPool.Enqueue(obj);
        }
    }

    public GameObject GetAttackObject()
    {
        if (attackPool.Count > 0 && attackPool.Peek().activeInHierarchy == false)
        {
            GameObject obj = attackPool.Dequeue();
            obj.SetActive(true);

            return obj;
        }
        else
        {
            // Only expand the pool if the maximum size has not been reached
            if (attackPool.Count < maxPoolSize)
            {
                ExpandPool();
            }

            return Instantiate(attackPrefab);
        }
    }

    public void ReturnToPool(GameObject obj)
    {
        if (attackPool.Count < maxPoolSize)
        {
            obj.transform.position = new Vector3(0, 0, 0);
            obj.SetActive(false);
            attackPool.Enqueue(obj);
        }
        else
        {
            // Optionally destroy the object if the pool size has exceeded the maximum
            Destroy(obj);
        }
    }

    // Method to expand the pool by adding a new object
    private void ExpandPool()
    {
        GameObject newObj = Instantiate(attackPrefab);
        newObj.SetActive(false);
        attackPool.Enqueue(newObj);
    }
}