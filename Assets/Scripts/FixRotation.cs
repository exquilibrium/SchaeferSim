using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixRotation : MonoBehaviour
{
    Quaternion initial;
    void Start()
    {
        initial = transform.rotation;
    }

    void LateUpdate()
    {
        transform.rotation = initial;
    }
}
