
using System.Collections.Generic;
using UnityEngine;

public class SheepManager : MonoBehaviour
{
    public static SheepManager instance;

    public GameObject sheepPrefab;
    public GameObject popupPrefab;

    public int spawnCount;

    public float infectDistance;
    public float killDistance;
    public float barkDistance;
    public float minBarkFleeTime, maxBarkFleeTime;
    public float minBarkFleeDist, maxBarkFleeDist;

    public float pileAvoidDist;
    public float maxFollowDist;

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
        {
            float dist = (s.transform.position - pos).sqrMagnitude;
            if (dist < barkDistance * barkDistance)
                s.Flee(pos, Random.Range(minBarkFleeDist, maxBarkFleeDist) - 0.5F * Mathf.Sqrt(dist), Random.Range(minBarkFleeTime, maxBarkFleeTime));

        }
    }

    public void OnKillSheep(SheepController s)
    {
        sheep.Remove(s);
    }

    public bool InfectClosest(Vector3 pos, SheepController ignore)
    {
        // Find closest sheep to infect
        SheepController closest = FindClosest(pos, 10, ignore);

        // Kill closes sheep
        if (closest != null)
        {
            closest.maxPanicCounter += 1;
            closest.state = SheepController.State.SICK;
            return true;
        }
        return false;
    }

    public SheepController FindClosest(Vector3 pos, float maxDist, SheepController ignore = null)
    {
        // Max distance of closest sheep
        float closestDst = maxDist * maxDist;
        SheepController closest = null;

        // Find closest sheep to kill
        foreach (SheepController s in sheep)
        {
            float dist = (s.transform.position - pos).sqrMagnitude;
            if (s != ignore && dist <= closestDst)
            {
                closestDst = dist;
                closest = s;
            }
        }
        return closest;
    }

    public bool KillClosest(Vector3 pos)
    {
        SheepController closest = FindClosest(pos, infectDistance);

        // Kill closes sheep
        if (closest != null)
        {
            closest.Kill();

            // Endgame when all sheeps are gone
            if (sheep.Count == 0)
                EndGame();

            return true;
        }

        return false;
    }

    public void FinishSheep(SheepController s)
    {
        OnKillSheep(s);
        finishedSheep++;
        SpawnPopup(s.transform.position, "+1");

        // Endgame when all sheeps are gone
        if (sheep.Count == 0)
            EndGame();
    }

    private void EndGame()
    {
        Debug.Log("End, Finished sheep: " + finishedSheep);
    }

    public void SpawnPopup(Vector3 pos, string text)
    {
        Instantiate(popupPrefab, pos + Vector3.up, popupPrefab.transform.rotation).GetComponent<TextMesh>().text = text;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        foreach (Vector3 p in piles)
           Gizmos.DrawWireSphere(p, pileAvoidDist);
    }
}
