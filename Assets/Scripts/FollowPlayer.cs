using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public Transform player;
    public float distance;
    public float smoothTime;
    public float titleRotateSpeed;

    private Vector3 velocity = Vector3.zero;

    public bool titleScreen;

    void Start()
    {
        // Set default position to player + offset
        transform.position = player.position + new Vector3(0, distance, -distance);
    }

    void Update()
    {
        // Rotate during active title screen
        if (titleScreen)
        {
            float rot = Time.time * titleRotateSpeed - Mathf.PI * 0.5F;
            transform.position = player.position + new Vector3(Mathf.Cos(rot), 0.75F, Mathf.Sin(rot)) * distance;
            transform.LookAt(player.position);
        }
        // Player follow with smoothing
        else
        {
            transform.position = Vector3.SmoothDamp(transform.position, player.position + new Vector3(0, distance, -distance), ref velocity, smoothTime);
            transform.LookAt(new Vector3(transform.position.x, player.position.y, player.position.z));
        }

    }
}
