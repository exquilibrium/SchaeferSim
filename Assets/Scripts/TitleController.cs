using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleController : MonoBehaviour
{
    public FollowPlayer cam;
    private TextMesh text;

    void Start()
    {
        // Activate title on start
        text = GetComponent<TextMesh>();
        cam.titleScreen = true;
    }

    void Update()
    {
        // Deactivate title on input
        if (Input.anyKeyDown)
        {
            gameObject.SetActive(false);
            cam.titleScreen = false;
        }
    }
}
