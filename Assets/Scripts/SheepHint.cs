using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SheepHint : MonoBehaviour
{
    public Transform player;
    public float minDist;

    private MeshRenderer ren;

    private void Start()
    {
        ren = GetComponent<MeshRenderer>();
    }

    void Update()
    {
        SheepController goal = SheepManager.instance.FindClosest(player.position, float.MaxValue);
        ren.enabled = goal != null && (goal.transform.position - player.position).sqrMagnitude > minDist * minDist;

        if (goal == null)
            return;

        transform.position = player.position + (goal.transform.position - player.position).normalized;
        transform.LookAt(goal.transform);
        transform.rotation *= Quaternion.Euler(90, 0, 0);
    }
}
