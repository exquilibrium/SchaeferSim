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
        transform.position = player.position;
    }

    void Update()
    {
        if (titleScreen)
        {
            float rot = Time.time * titleRotateSpeed - Mathf.PI * 0.5F;
            Vector3 targetPosition = player.position + new Vector3(Mathf.Cos(rot), 0.75F, Mathf.Sin(rot)) * distance;
            transform.position = targetPosition;

            transform.LookAt(player.position);
        }
        else
        {
            Vector3 targetPosition = player.position + new Vector3(0, distance, -distance);
            // transform.position = targetPosition;
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
            // transform.LookAt(player.position);
            transform.LookAt(new Vector3(transform.position.x, player.position.y, player.position.z));
        }

    }
}
