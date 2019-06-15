using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SheepController : MonoBehaviour
{
    public Transform indicator;

    public bool isDead;
    public float minSpeed, maxSpeed;
    public float minPathTime, maxPathTime;
    public float pathTargetRange;
    public int minFollowChance, maxFollowChance;

    private NavMeshAgent agent;
    private Animator anim;
    private SheepController follow;
    private float pathTime;
    private float speed;
    private int followChance;

    private float panic;
    private float floatOffset, rotOffset;
    private Material indicatorMat;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        indicatorMat = indicator.GetComponent<MeshRenderer>().material;

        speed = Random.Range(minSpeed, maxSpeed);
        followChance = Random.Range(minFollowChance, maxFollowChance);
        float scale = Random.Range(0.8F, 1.5F);
        transform.localScale *= scale;
        indicator.localScale /= scale;
        follow = this;
        floatOffset = Random.Range(0, Mathf.PI * 2);
        rotOffset = Random.Range(0, 360);
    }

    void Update()
    {
        indicator.transform.localPosition = Vector3.up * (0.6F + 0.1F * Mathf.Cos(Time.time * 3 + floatOffset));
        indicator.transform.rotation = Quaternion.Euler(45, Time.time * 90 + rotOffset, 45);

        if (isDead)
            return;

        if (panic > 5)
        {
            Kill();
            return;
        }

        indicatorMat.color = new Color(panic, 1 - (panic - 1), 0);
        panic = Mathf.Max(0, panic - Time.deltaTime);

        if (Time.time > pathTime)
            SetTarget();

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

    void SetTarget()
    {
        if (Random.Range(0, followChance) != 0 || (follow = SheepManager.instance.GetSheepToFollow(this)) == this)
        {
            follow = this;
            agent.destination = transform.position + new Vector3(Random.Range(-pathTargetRange, pathTargetRange), 0, Random.Range(-pathTargetRange, pathTargetRange));
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
        agent.speed = Mathf.Max(agent.speed, speed * Random.Range(1.25F + panic, 1.5F + panic));
        pathTime = Time.time + time;
        follow = this;
    }

    public void Kill()
    {
        SheepManager.instance.OnKillSheep(this);
        isDead = true;
        agent.isStopped = true;
        Destroy(gameObject, 0.5F);
        indicatorMat.color = new Color(0.2F, 0, 0);
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
        if (isDead || agent == null)
            return;

        Gizmos.color = new Color(panic, 1 - (panic - 1), 0);
        Gizmos.DrawLine(transform.position, agent.destination);
        Gizmos.DrawWireSphere(agent.destination, 0.1F);
    }
}
