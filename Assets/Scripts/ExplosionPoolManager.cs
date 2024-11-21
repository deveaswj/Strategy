using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionPoolManager : MonoBehaviour
{
    [SerializeField] PlayExplosion explosionPrefab;
    [SerializeField] int poolSize = 4;
    int itemID = 0;

    private Queue<PlayExplosion> pool = new Queue<PlayExplosion>();

    void Start()
    {
        // Prepopulate the pool
        for (int i = 0; i < poolSize; i++)
        {
            PlayExplosion explosion = NewExplosion(false);
            pool.Enqueue(explosion);
        }
    }

    public PlayExplosion GetExplosion()
    {
        PlayExplosion explosion = null;
        if (pool.Count > 0)
        {
            explosion = pool.Dequeue();
            explosion.gameObject.SetActive(true);
        }
        else
        {
            // Optionally grow the pool if needed
            explosion = NewExplosion(true);
        }
        Debug.Log("Checking out object: " + explosion.name);
        return explosion;
    }

    PlayExplosion NewExplosion(bool setActive = false)
    {
        PlayExplosion explosion = Instantiate(explosionPrefab);
        explosion.name = "Explosion (" + itemID + ")";
        explosion.OnExplosionComplete += ReturnToPool;
        explosion.gameObject.SetActive(setActive);
        itemID++;
        return explosion;
    }

    public void ReturnToPool(PlayExplosion explosion)
    {
        Debug.Log("Checking in object: " + explosion.name);
        explosion.gameObject.SetActive(false);
        pool.Enqueue(explosion);
    }
}
