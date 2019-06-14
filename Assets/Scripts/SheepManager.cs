using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SheepManager : MonoBehaviour
{
    public static SheepManager instance;

    public GameObject sheepPrefab;
    public int spawnCount;

    public float barkDistance;
    public float minBarkFleeTime, maxBarkFleeTime;
    public float minBarkFleeDist, maxBarkFleeDist;

    public float pileAvoidDist;

    private List<SheepController> sheep = new List<SheepController>();
    private List<Vector3> piles = new List<Vector3>();

    void Start()
    {
        for (int i = 0; i < spawnCount; ++i)
            sheep.Add(Instantiate(sheepPrefab).GetComponent<SheepController>());

        if (instance != null)
            Destroy(instance);
        instance = this;
    }

    public bool AvoidPiles(Vector3 pos, out Vector3 avoidVec)
    {
        bool avoid = false;
        avoidVec = Vector3.zero;
        foreach (Vector3 p in piles)
            if ((p - pos).sqrMagnitude < pileAvoidDist * pileAvoidDist)
            {
                avoid = true;
                avoidVec += (pos - p).normalized;
            }

        return avoid;
    }

    public void AddPile(Vector3 pos)
    {
        piles.Add(pos);
    }

    public void OnBark(Vector3 pos)
    {
        foreach (SheepController s in sheep)
            if ((s.transform.position - pos).sqrMagnitude < barkDistance * barkDistance)
                s.Flee(pos, Random.Range(minBarkFleeDist, maxBarkFleeDist), Random.Range(minBarkFleeTime, maxBarkFleeTime));
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        foreach (Vector3 p in piles)
            Gizmos.DrawWireSphere(p, pileAvoidDist);
    }
}
