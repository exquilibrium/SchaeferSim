using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public Transform player1;
    public Transform player2;

    public float distance;
    public float heightAngle;
    public float smoothTime;
    public float titleRotateSpeed;
    public float multiDistFact, maxMultiDist;

    private Vector3 velocity = Vector3.zero;

    public bool titleScreen;
    private float dist;

    void Start()
    {
        // Set default position to player + offset
        dist = distance;
        transform.position = player1.position + new Vector3(0, dist * heightAngle, -dist);
    }

    void Update()
    {
        // Rotate during active title screen
        if (titleScreen)
        {
            float rot = Time.time * titleRotateSpeed - Mathf.PI * 0.5F;
            transform.position = player1.position + new Vector3(Mathf.Cos(rot), heightAngle, Mathf.Sin(rot)) * dist;
            transform.LookAt(player1.position);
        }
        // Player follow with smoothing
        else
        {
            Vector3 target = (player1.position + player2.position) / 2F;
            dist = distance + Mathf.Min(maxMultiDist, (player1.position - player2.position).magnitude * multiDistFact);
            transform.position = Vector3.SmoothDamp(transform.position, target + new Vector3(0, dist * heightAngle, -dist), ref velocity, smoothTime);
            transform.LookAt(target);
        }

    }
}
