using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SheepController : MonoBehaviour
{
    public float minSpeed, maxSpeed;
    public float minPathTime, maxPathTime;
    public int minFollowChance, maxFollowChance;

    private NavMeshAgent agent;
    private SheepController follow;
    private float pathTime;
    private float speed;
    private int followChance;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        speed = Random.Range(minSpeed, maxSpeed);
        followChance = Random.Range(minFollowChance, maxFollowChance);
        transform.localScale = transform.localScale * Random.Range(0.8F, 1.2F);
    }

    void Update()
    {
        if (Time.time > pathTime)
            SetTarget();

        if (follow != this)
            agent.destination = follow.transform.position;

        Vector3 avoidVec;
        if (SheepManager.instance.AvoidPiles(transform.position, out avoidVec))
        {
            agent.destination = transform.position + avoidVec.normalized * SheepManager.instance.pileAvoidDist;
            agent.speed = speed * Random.Range(1.5F, 2F);
            pathTime = Time.time + 2;
        }
    }

    void SetTarget()
    {
        if (Random.Range(0, followChance) != 0 || (follow = SheepManager.instance.GetSheepToFollow(this)) == this)
        {
            follow = this;
            agent.destination = transform.position + new Vector3(Random.Range(-10F, 10F), 0, Random.Range(-10F, 10F));
        }

        if (follow.follow == this)
            follow = this;

        agent.speed = speed;
        pathTime = Time.time + Random.Range(minPathTime, maxPathTime);
    }

    public void Flee(Vector3 pos, float dist, float time)
    {
        agent.destination = transform.position + (transform.position - pos).normalized * dist;
        agent.speed = speed * Random.Range(1.25F, 2F);
        pathTime = Time.time + time;
        follow = this;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, agent.destination);
        Gizmos.DrawWireSphere(agent.destination, 0.1F);
    }
}
