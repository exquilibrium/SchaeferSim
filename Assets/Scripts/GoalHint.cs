using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalHint : MonoBehaviour
{
    public Transform goal, player;

    void Update()
    {
        transform.position = player.position + (goal.position - player.position).normalized;
        transform.LookAt(goal);
        transform.rotation *= Quaternion.Euler(90, 0, 0);
    }
}
