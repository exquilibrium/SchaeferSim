using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleController : MonoBehaviour
{
    public FollowPlayer cam;
    public GameObject player1;
    public GameObject player2;
    public GameObject goalHint2;
    public GameObject sheepHint2;
    public GameObject pileDisplay2;
    public GameObject spotLight;
    public GameObject spotLight2;
    public int controller;

    private TextMesh text;

    void Start()
    {
        // Activate title on start
        text = GetComponent<TextMesh>();
        cam.titleScreen = true;
        controller = 0;
        player1.GetComponent<PlayerController>().enabled = false;
        player2.GetComponent<PlayerController>().enabled = false;
        spotLight2.GetComponent<Light>().intensity = 1;
    }

    void Update()
    {
        if (Input.GetButtonDown("Bark" + 1))
        {
            // Switch Mode
            controller = (controller + 1) % 2;
            if (controller != 0)
            {
                spotLight2.GetComponent<Light>().intensity = 10;
            }
            else
            {
                spotLight2.GetComponent<Light>().intensity = 1;
            }

        }
        // Deactivate title on input
        if (Input.GetButtonDown("Bark" + 0))
        {
            cam.titleScreen = false;
            player1.GetComponent<PlayerController>().enabled = true;
            player2.GetComponent<PlayerController>().enabled = true;
            cam.player2 = player2.transform;
            if (controller == 0)
            {
                player2.SetActive(false);
                goalHint2.SetActive(false);
                sheepHint2.SetActive(false);
                pileDisplay2.SetActive(false);
                cam.player2 = player1.transform;
            }
            spotLight.SetActive(false);
            spotLight2.SetActive(false);
            gameObject.SetActive(false);
        }
    }

    void LateUpdate()
    {
        transform.rotation = Quaternion.Euler(0, cam.transform.eulerAngles.y, 0);
    }
}

