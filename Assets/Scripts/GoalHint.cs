using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalHint : MonoBehaviour
{
    public Transform goal, player;
    public float dist = 1;
    public Vector3 offset;

    void Update()
    {
        transform.position = player.position + offset + (goal.position - player.position).normalized * dist;
        transform.LookAt(goal);
        transform.rotation *= Quaternion.Euler(90, 0, 0);
    }
}
