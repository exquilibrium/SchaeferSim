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

    void LateUpdate()
    {
        transform.rotation = Quaternion.Euler(0, cam.transform.eulerAngles.y, 0);

        // Deactivate title on input
        if (Input.anyKeyDown)
        {
            gameObject.SetActive(false);
            cam.titleScreen = false;
        }
    }
}
