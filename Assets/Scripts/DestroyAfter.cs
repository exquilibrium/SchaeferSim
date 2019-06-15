using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfter : MonoBehaviour
{
    public float seconds;

    void Start()
    {
        StartCoroutine(Co());
    }

    IEnumerator Co()
    {
        yield return new WaitForSeconds(seconds);
        Destroy(gameObject);
    }
}
