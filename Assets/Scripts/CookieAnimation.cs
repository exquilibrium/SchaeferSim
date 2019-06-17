using UnityEngine;

public class CookieAnimation : MonoBehaviour
{
    public Vector3 direction;
    public float scrollSpeed = 5f;
    public float cookieSize = 100f;
    
    void Update()
    {        
        transform.position = Mod(transform.position + direction * scrollSpeed * Time.deltaTime, cookieSize);
    }

    private static Vector3 Mod(Vector3 i, float f)
    {
        return new Vector3(Mathf.Repeat(i.x, f), Mathf.Repeat(i.y, f), Mathf.Repeat(i.z, f));
    }

    private void OnValidate()
    {
        direction.Normalize();
    }
}
