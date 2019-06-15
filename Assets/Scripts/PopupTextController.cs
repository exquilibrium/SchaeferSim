using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupTextController : MonoBehaviour
{
    public float velY;
    public float maxTime;

    private float time = 0;
    private TextMesh tMesh;
    private Color col1, col2;

    private void Start()
    {
        tMesh = GetComponent<TextMesh>();
        col1 = tMesh.color;
        col2 = new Color(col1.r, col1.g, col1.b, 0.0F);
        transform.localScale *= Random.Range(0.5F, 1.2F);
    }

    void Update()
    {
        time += Time.deltaTime;

        float t = time / maxTime;

        transform.Translate(Vector3.up * velY * (1 - t * t) * Time.deltaTime, Space.World);
        tMesh.color = Color.Lerp(col1, col2, t * t);

        if (time > maxTime)
            Destroy(gameObject);
	}
}
