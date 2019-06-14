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

    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        pileText.text = "" + piles;
    }

    void Update()
    {
        if (Input.GetButtonDown("DropPile"))
        {
            if (piles > 0)
            {
                Instantiate(pilePrefab, transform.position, transform.rotation, null);
                SheepManager.instance.AddPile(transform.position);

                piles--;
                pileText.text = "" + piles;
            }
        }
        if (Input.GetButtonDown("Bark"))
        {
            barkParticles.Play();
            SheepManager.instance.OnBark(transform.position);
        }

        agent.destination = transform.position + new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
    }

    private void OnDrawGizmos()
    {
        if (SheepManager.instance == null)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, SheepManager.instance.barkDistance);
    }
}
