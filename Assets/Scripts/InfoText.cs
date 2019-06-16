using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfoText : MonoBehaviour
{

    public FollowPlayer cam;
    public GameObject player;

    private TextMesh text;

    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<TextMesh>();
        transform.position = player.transform.position + new Vector3(1, 0, 1);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void LateUpdate()
    {
        transform.position = player.transform.position + new Vector3(1, 0, 1);
        transform.rotation = Quaternion.Euler(0, cam.transform.eulerAngles.y, 0);
    }
}
