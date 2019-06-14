using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public Transform player;
    public float distance;
    public float smoothTime;
    private Vector3 velocity = Vector3.zero;

    void Start()
    {
        transform.position = player.position;
    }

    void Update()
    {
        Vector3 targetPosition = player.position + new Vector3(0, distance, -distance);
        transform.position = targetPosition;
        // transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        transform.LookAt(player.position);
        // transform.LookAt(new Vector3(transform.position.x, player.position.y, transform.position.z));
    }
}
