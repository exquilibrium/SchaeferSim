using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SheepHint : MonoBehaviour
{
    public Transform player;
    public float minDist;
    public float dist = 1;
    public Vector3 offset;

    private MeshRenderer ren;

    private void Start()
    {
        ren = GetComponentInChildren<MeshRenderer>();
    }

    void LateUpdate()
    {
        SheepController goal = SheepManager.instance.FindClosest(player.position, float.MaxValue);
        ren.enabled = goal != null && (goal.transform.position - player.position).sqrMagnitude > minDist * minDist;

        if (goal == null)
            return;

        transform.position = player.position + offset + dist * (goal.transform.position - player.position).normalized;
        transform.LookAt(goal.transform);
        transform.rotation *= Quaternion.Euler(90, 0, 0);
    }
}
