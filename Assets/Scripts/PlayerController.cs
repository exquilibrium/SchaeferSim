using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public GameObject pilePrefab;
    public ParticleSystem barkParticles, barkLineParticles;
    public Text pileText;
    public int piles;
    public int pilesPerKill;
    public int controller;
    public bool control; // Deactivate for MainScreen
    public int tutorialState;
    public TextMesh playerText;

    public string[] wuffs;
    public AudioClip[] wuffSounds;

    public NavMeshAgent agent;
    private Animator anim;
    private AudioSource sound;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        sound = GetComponent<AudioSource>();

        if (controller == 0)
            playerText.text = "Press 'E' to start.";
        else
            playerText.text = "Press 'P' to join.";
    }

    /*
     * Input:
     * DropPile - Space
     * Bark - E
     * Kill - Q
     * 
     * Movement - WASD / ArrowKeys
     */
    void Update()
    {

        if (SheepManager.instance.GetSheepCount() == 0)
        {
            playerText.transform.position += new Vector3(0, 5, 0);
            playerText.text = "GOOD DOGGIE !";
        }
        if (control)
        {
            if (tutorialState > -1)
            {
                if (tutorialState == 0)
                {
                    tutorialState = 1;
                    if (controller == 0)
                        playerText.text = "Press 'W/A/S/D' to move.";
                    else
                        playerText.text = "Press 'ARROW -KEYS' to move.";
                } else if (tutorialState == 1 && (Input.GetAxis("Horizontal" + controller) != 0 || Input.GetAxis("Vertical" + controller) != 0))
                {
                    tutorialState = 2;
                    if (controller == 0)
                        playerText.text = "Press 'E' to bark.";    
                    else
                        playerText.text = "Press 'P' to bark.";
                }
                if (tutorialState == 5 && Input.anyKeyDown)
                {
                    tutorialState = -1;
                    playerText.text = "";
                
                }
            }
            if (Input.GetButtonDown("DropPile" + controller))
            {
                if (tutorialState == 3)
                { 
                    tutorialState = 4;
                    if (controller == 0)
                        playerText.text = "Press 'Q' to eat a sheep for a pile.";
                    else
                        playerText.text = "Press 'O' to eat a sheep for a pile.";
                }
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
                if (tutorialState == 2)
                {
                    tutorialState = 3;
                    if (controller == 0)
                        playerText.text = "Press 'space' to drop a pile.";
                    else
                        playerText.text = "Press 'L' to drop a pile.";
                }

                barkParticles.Play();
                barkLineParticles.Play();
                SheepManager.instance.OnBark(transform.position);
                SheepManager.instance.SpawnPopup(transform.position, wuffs[Random.Range(0, wuffs.Length)]);
                sound.pitch = Random.Range(0.9F, 1.1F);
                sound.PlayOneShot(wuffSounds[Random.Range(0, wuffSounds.Length)]);
            }

            // Kill closest sheep if possible
            if (Input.GetButtonDown("Kill" + controller))
            {
                if (tutorialState == 4)
                {
                    tutorialState = 5;
                    playerText.text = "Good luck...";
                }

                // Kill closest sheep if possible
                if (SheepManager.instance.KillClosest(transform.position))
                {
                    if (piles < 3)
                    {
                        piles++;
                        pileText.text = "" + piles;
                        SheepManager.instance.SpawnPopup(transform.position, "Piles x" + piles);
                    }
                    piles = Mathf.Min(3, piles + pilesPerKill);
                    pileText.text = "" + piles;
                }
            }

            Vector3 mov = new Vector3(Input.GetAxis("Horizontal" + controller), 0, Input.GetAxis("Vertical" + controller));
            agent.destination = transform.position + 0.5F * mov.normalized * Mathf.Min(mov.magnitude, 1.0F);
            //agent.Move(agent.speed * Time.deltaTime * mov.normalized * Mathf.Min(mov.magnitude, 1.0F));
            anim.SetFloat("velocity", agent.velocity.magnitude);
        }

    }

    public void set(Vector3 pos)
    {
        agent.destination = pos;
    }

    // Debug
    private void OnDrawGizmos()
    {
        if (SheepManager.instance == null)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, SheepManager.instance.barkDistance);
    }
}
