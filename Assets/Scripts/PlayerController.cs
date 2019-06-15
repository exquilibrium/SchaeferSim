using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public GameObject pilePrefab;
    public ParticleSystem barkParticles;
    public Text pileText;
    public int piles;

    public int controller;

    private NavMeshAgent agent;
    private Animator anim;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

        pileText.text = "" + piles;
    }

    void Update()
    {
        if (Input.GetButtonDown("DropPile" + controller))
        {
            if (piles > 0)
            {
                Instantiate(pilePrefab, transform.position, transform.rotation, null);
                SheepManager.instance.AddPile(transform.position);

                piles--;
                pileText.text = "" + piles;
            }
            SheepManager.instance.SpawnPopup(transform.position, "Piles x" + piles);
        }
        if (Input.GetButtonDown("Bark" + controller))
        {
            barkParticles.Play();
            SheepManager.instance.OnBark(transform.position);
        }
        if (Input.GetButtonDown("Kill" + controller))
        {
            if (SheepManager.instance.KillClosest(transform.position))
            {
                if (piles < 3)
                {
                    piles++;
                    pileText.text = "" + piles;
                    SheepManager.instance.SpawnPopup(transform.position, "Piles x" + piles);
                }
            }
        }

        agent.destination = transform.position + new Vector3(Input.GetAxis("Horizontal" + controller), 0, Input.GetAxis("Vertical" + controller));
    }

    private void OnDrawGizmos()
    {
        if (SheepManager.instance == null)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, SheepManager.instance.barkDistance);
    }
}
