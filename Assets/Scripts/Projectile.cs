using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] float speed = 7f;
    [SerializeField] int damage = 1;
    [SerializeField] float life = 4f;

    Rigidbody2D rb;
    Vector2 dir;

    public void Launch(Vector2 d) { dir = d.normalized; }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = dir * speed;
        Destroy(gameObject, life);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Health h = other.GetComponent<Health>();
        if (h != null) { h.Damage(damage); Destroy(gameObject); return; }
        if (other.GetComponent<Patroller>() != null || other.GetComponent<Shooter>() != null) return;
        if (!other.isTrigger) Destroy(gameObject);   
    }
}