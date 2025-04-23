using UnityEngine;

public class Physics2DEvents : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"OnTriggerEnter {gameObject.name} colisiono con {collision.gameObject.name}");
        
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        Debug.Log($"OnTriggerEnter {gameObject.name} colisiono con {collision.gameObject.name}");
        GetComponent<SpriteRenderer>().color = Color.white;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"OnCollisionEnter {gameObject.name} colisiono con {collision.gameObject.name}");
        GetComponent<SpriteRenderer>().color = Color.red;
    }
}
