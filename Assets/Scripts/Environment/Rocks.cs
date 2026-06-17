using UnityEngine;

public class Rocks : MonoBehaviour
{
    public float fallSpeed = 5f;

    void Update()
    {
     transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);
        
   
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }
}
