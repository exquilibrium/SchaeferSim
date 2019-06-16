using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    public Vector3 euler;

    void Start()
    {
        
    }

    void Update()
    {
        transform.Rotate(euler * Time.deltaTime);
    }
}
