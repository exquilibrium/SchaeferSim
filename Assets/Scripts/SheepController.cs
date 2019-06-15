using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SheepController : MonoBehaviour
{
    public bool isDead;
    public float minSpeed, maxSpeed;
    public float minPathTime, maxPathTime;
    public int minFollowChance, maxFollowChance;
    
    private NavMeshAgent agent;
    private SheepController follow;
    private float pathTime;
    private float speed;
    private int followChance;

    private float panic;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        speed = Random.Range(minSpeed, maxSpeed);
        followChance = Random.Range(minFollowChance, maxFollowChance);
        transform.localScale = transform.localScale * Random.Range(0.8F, 1.5F);
        follow = this;

    }

    void Update()
    {
        if (isDead)
            return;

        panic = Mathf.Max(0, panic - Time.deltaTime);

        if (Time.time > pathTime)
            SetTargetAvgPos();
            // SetTarget();

        if (follow != this)
        {
            if (follow.isDead)
                follow = this;
            else
                agent.destination = follow.transform.position;
        }

        Vector3 avoidVec;
        if (panic < 2F && SheepManager.instance.AvoidPiles(transform.position, out avoidVec))
        {
            agent.destination = transform.position + avoidVec.normalized * SheepManager.instance.pileAvoidDist;
            agent.speed = speed * Random.Range(1.5F, 2F);
            pathTime = Time.time + 2;
        }
    }

    void SetTargetAvgPos()
    {
        if (Random.Range(0, followChance) > followChance / 4)
        {
            agent.destination = SheepManager.instance.CalcAvgPos() + new Vector3(Random.Range(-3F, 3F), 0, Random.Range(-3F, 3F));
        } else
        {
            agent.destination = transform.position + new Vector3(Random.Range(-10F, 10F), 0, Random.Range(-10F, 10F));
        }
        agent.speed = speed;
        pathTime = Time.time + Random.Range(minPathTime, maxPathTime);
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
        panic += 1;

        agent.destination = transform.position + (transform.position - pos).normalized * dist;
        agent.speed = speed * Random.Range(1.25F + panic, 1.5F + panic);
        pathTime = Time.time + time;
        follow = this;
    }

    public void Kill()
    {
        isDead = true;
        agent.isStopped = true;
        Destroy(gameObject, 0.5F);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isDead && other.CompareTag("Finish"))
        {
            isDead = true;
            follow = this;
            pathTime = Time.time + 1000;
            agent.destination = other.transform.position;
            Destroy(gameObject, 1F);
            SheepManager.instance.FinishSheep(this);
        }
    }

    private void OnDrawGizmos()
    {
        if (isDead)
            return;

        Gizmos.color = new Color(panic, 1 - (panic - 1), 0);
        Gizmos.DrawLine(transform.position, agent.destination);
        Gizmos.DrawWireSphere(agent.destination, 0.1F);
    }
}
