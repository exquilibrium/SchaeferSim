using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SheepManager : MonoBehaviour
{
    public static SheepManager instance;

    public GameObject sheepPrefab;
    public int spawnCount;

    public float killDistance;
    public float barkDistance;
    public float minBarkFleeTime, maxBarkFleeTime;
    public float minBarkFleeDist, maxBarkFleeDist;

    public float pileAvoidDist;
    public float maxFollowDist;
    public float maxGroupDist = 0;

    private List<SheepController> sheep = new List<SheepController>();
    private List<Vector3> piles = new List<Vector3>();
    private int finishedSheep = 0;

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
        // Avoid piles if within piles range
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

    public Vector3 CalcAvgPos()
    {
        // Calculate middlepoint between all existing sheeps
        Vector3 avgPos = Vector3.zero;
        for (int i = 0; i < sheep.Count; i++)
        {
            avgPos += sheep[i].transform.position;
        }
        return avgPos / sheep.Count;
    }

    public SheepController GetSheepToFollow(SheepController me)
    {
        // Sheeps follow candidate sheep if within following distance
        for (int i = 0; i < 5; ++i)
        {
            SheepController candidate = sheep[Random.Range(0, sheep.Count)];
            if ((candidate.transform.position - me.transform.position).sqrMagnitude < maxFollowDist * maxFollowDist)
                return candidate;
        }
        return me;
    }

    public void AddPile(Vector3 pos)
    {
        piles.Add(pos);
    }

    public void OnBark(Vector3 pos)
    {
        // Sheeps flee from player
        foreach (SheepController s in sheep)
            if ((s.transform.position - pos).sqrMagnitude < barkDistance * barkDistance)
                s.Flee(pos, Random.Range(minBarkFleeDist, maxBarkFleeDist), Random.Range(minBarkFleeTime, maxBarkFleeTime));
    }

    public bool KillClosest(Vector3 pos)
    {
        // Max distance of closest sheep
        float closestDst = 100;
        SheepController closest = null;

        // Find closest sheep
        foreach (SheepController s in sheep)
        {
            float dist = (s.transform.position - pos).sqrMagnitude;
            if (dist < killDistance * killDistance && (closest == null || dist < closestDst))
            {
                closestDst = dist;
                closest = s;
            }
        }

        // Kill closes sheep
        if (closest != null)
        {
            sheep.Remove(closest);
            closest.Kill();
            // Endgame when all sheeps are gone
            if (sheep.Count == 0)
            {
                EndGame();
            }
            return true;
        }

        return false;
    }

    public void FinishSheep(SheepController s)
    {
        sheep.Remove(s);
        finishedSheep++;
        Debug.Log("A sheep reached the goal.");
        
        // Endgame when all sheeps are gone
        if (sheep.Count == 0)
        {
            EndGame();
        }
    }

    private void EndGame()
    {
        Debug.Log("End, Finished sheep: " + finishedSheep);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        foreach (Vector3 p in piles)
           Gizmos.DrawWireSphere(p, pileAvoidDist);
    }
}
