using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackPool : MonoBehaviour
{
    [SerializeField] private GameObject attackPrefab;
    private Queue<GameObject> attackPool = new Queue<GameObject>();
    private int poolSize = 10;

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
        if (attackPool.Count > 0)
        {
            GameObject obj = attackPool.Dequeue();
            obj.SetActive(true);
            return obj;
        }
        else
        {
            return Instantiate(attackPrefab);
        }
    }

    public void ReturnToPool(GameObject obj)
    {
        obj.SetActive(false);
        attackPool.Enqueue(obj);
    }

}
