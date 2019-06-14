using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleController : MonoBehaviour
{
    public FollowPlayer cam;
    private TextMesh text;

    void Start()
    {
        text = GetComponent<TextMesh>();
        cam.titleScreen = true;
    }

    void Update()
    {
        if (Input.anyKeyDown)
        {
            gameObject.SetActive(false);
            cam.titleScreen = false;
        }
    }
}
