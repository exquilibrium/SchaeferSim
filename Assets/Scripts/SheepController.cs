using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SheepController : MonoBehaviour
{
    public enum State
    {
        DEAD,
        ALIVE,
        SLEEPING,
        SICK,
        LOVE,
    };

    public Transform indicator;

    public State state = State.ALIVE;
    public float minSpeed, maxSpeed;
    public float minPathTime, maxPathTime;
    public float minSickTime;
    public float minSleepWaitTime, maxSleepWaitTime, maxSleepTime;
    public float pathTargetRange;
    public int minFollowChance, maxFollowChance;
    public int wanderOffChance;
    public int loveChance;

    public string[] mehs;
    public string[] panicMehs;

    private NavMeshAgent agent;
    private Animator anim;
    private SheepController follow;
    private float pathTime;
    private float speed;
    private float sickTimer;
    private float sleepWaitTime, sleepTimer;
    private int followChance;
    public int maxPanicCounter;

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
        sleepWaitTime = Random.Range(minSleepWaitTime, maxSleepWaitTime);
    }

    void Update()
    {
        indicator.transform.localPosition = Vector3.up * (0.6F + 0.1F * Mathf.Cos(Time.time * 3 + floatOffset));
        indicator.transform.rotation = Quaternion.Euler(45, Time.time * 90 + rotOffset, 45);

        if (state == State.DEAD)
            return;

        if (panic > 5 || (maxPanicCounter > 3 && panic > 3) || (maxPanicCounter > 9))
        {
            Kill();
            return;
        }

        if (panic > 1)
        {
            sickTimer += Time.deltaTime;
            if (sickTimer > minSickTime)
            {
                sickTimer = 0;
                maxPanicCounter += 1;
                if (maxPanicCounter > 3)
                {
                    state = State.SICK;
                }
                return;
            }
        }

        if (state == State.SLEEPING)
        {
            if (Random.Range(0, 200) == 0)
                SheepManager.instance.SpawnPopup(transform.position, "Zzz");
            sleepTimer -= Time.deltaTime;
            if (sleepTimer <= 0)
            {
                panic = 0;
                sleepTimer = 0;
                state = State.ALIVE;
            }
            else
                return;
        }

        if (panic < -5 && state == State.ALIVE)
        {
            sleepTimer += Time.deltaTime;
            if (sleepTimer > sleepWaitTime)
            {
                SheepManager.instance.SpawnPopup(transform.position, "Zzz");

                sleepTimer = Random.Range(maxSleepTime * 0.5F, maxSleepTime);
                state = State.SLEEPING;
                indicatorMat.color = new Color(0, 0, 1);
                follow = this;
                pathTime = 0;
                agent.destination = transform.position;
                sleepWaitTime = Random.Range(minSleepWaitTime, maxSleepWaitTime);
                if (maxPanicCounter > 0)
                {
                    maxPanicCounter -= 1;
                }
                return;
            }
        }

        if (state == State.ALIVE)
        {
            indicatorMat.color = new Color(Mathf.Clamp01(panic), Mathf.Clamp01(1 - (panic - 1)), 0);
            panic = panic - Time.deltaTime;
        }
        else if (state == State.SICK)
        {
            indicatorMat.color = new Color(0.5F, 0, 0.5F);
        }
        else if (state == State.LOVE)
        {
            indicatorMat.color = new Color(1.0F, 0.4F, 0.6F);

            if (panic > 0)
                state = State.ALIVE;
        }

        if (Time.time > pathTime || panic > 0 && (agent.destination - transform.position).sqrMagnitude < 1)
            SetTarget();

        // Only follow alive sheeps, else follow self
        if (follow != this)
        {
            if (follow.state != State.ALIVE)
            {
                if (follow.state == State.DEAD && state == State.LOVE)
                {
                    Kill();
                    return;
                }
                follow = this;
            }
            else
                agent.destination = follow.transform.position;
        }

        // Avoid piles
        Vector3 avoidVec;
        if (panic < 2F && SheepManager.instance.AvoidPiles(transform.position, out avoidVec))
        {
            agent.destination = transform.position + avoidVec.normalized * SheepManager.instance.pileAvoidDist;
            agent.speed = speed * Random.Range(1.5F, 2F);
            pathTime = Time.time + 2;
            panic += 0.5F * Time.deltaTime;
        }

        if (state == State.LOVE)
        {
             if (Random.Range(0, 500) == 0)
                SheepManager.instance.SpawnPopup(transform.position, mehs[Random.Range(0, mehs.Length)] + " ❤️");
        }
        else if (Random.Range(0, 1000) == 0)
            SheepManager.instance.SpawnPopup(transform.position, mehs[Random.Range(0, mehs.Length)]);
    }

    void SetTarget()
    {
        if (state == State.LOVE)
            state = State.ALIVE;

        // Sets sheep target as rand
        if (panic > 1 || Random.Range(0, followChance) != 0 || (follow = SheepManager.instance.GetSheepToFollow(this)) == this)
        {
            follow = this;
            if (Random.Range(0, wanderOffChance) == 0)
                agent.destination = transform.position + new Vector3(Random.Range(pathTargetRange * 2, pathTargetRange * 5) * Random.Range(-1, 1), 0, Random.Range(pathTargetRange * 2, pathTargetRange * 5) * Random.Range(-1, 1));

            else
                agent.destination = transform.position + new Vector3(Random.Range(-pathTargetRange, pathTargetRange), 0, Random.Range(-pathTargetRange, pathTargetRange));
        }

        if (follow != this && panic < -2 && Random.Range(0, loveChance) == 0)
            state = State.LOVE;

        if (follow.follow == this)
            follow = this;

        agent.speed = speed + Mathf.Max(0, panic);
        pathTime = Time.time + (state == State.LOVE ? 5 : 1) * Random.Range(minPathTime, maxPathTime) / Mathf.Max(1, panic);
    }

    public void Flee(Vector3 pos, float dist, float time)
    {
        if (state == State.DEAD)
            return;

        if (state == State.SLEEPING)
        {
            state = State.ALIVE;
            panic = Mathf.Max(0, panic) + 2;
            if (Random.Range(0, 2) == 0)
                SheepManager.instance.SpawnPopup(transform.position, panicMehs[Random.Range(0, mehs.Length)]);
        }
        else
        {
            if (panic > 1 && Random.Range(0, 3) == 0)
                SheepManager.instance.SpawnPopup(transform.position, panicMehs[Random.Range(0, mehs.Length)]);
            panic = Mathf.Max(0, panic) + 1;
        }

        agent.destination = transform.position + (transform.position - pos).normalized * dist;
        agent.speed = Mathf.Max(agent.speed, speed * Random.Range(1.25F + Mathf.Max(0, panic), 1.5F + panic + Mathf.Max(0, panic)));
        pathTime = Time.time + time;
        sleepTimer = 0;
        follow = this;
    }

    public void Kill()
    {
        SheepManager.instance.OnKillSheep(this);
        state = State.DEAD;
        agent.isStopped = true;
        Destroy(gameObject, 0.5F);
        indicatorMat.color = new Color(0.2F, 0, 0);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (state != State.DEAD && other.CompareTag("Finish"))
        {
            state = State.DEAD;
            follow = this;
            pathTime = Time.time + 1000;
            agent.destination = other.transform.position;
            Destroy(gameObject, 1F);
            SheepManager.instance.FinishSheep(this);
        }
    }

    private void OnDrawGizmos()
    {
        if (state == State.DEAD || agent == null)
            return;

        Gizmos.color = new Color(panic, 1 - (panic - 1), 0);
        Gizmos.DrawLine(transform.position, agent.destination);
        Gizmos.DrawWireSphere(agent.destination, 0.1F);
    }
}
