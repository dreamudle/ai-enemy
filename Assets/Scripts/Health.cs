using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] int maxHealth = 3;
    [SerializeField] float invulnerableSeconds = 0.8f;

    public static int Tries = 1;  

    int current;
    float invulnerableUntil;
    Vector3 startPos;

    void Awake()
    {
        current = maxHealth;
        startPos = transform.position;
    }

    public void Damage(int amount)
    {
        if (Time.time < invulnerableUntil) return;
        current -= amount;
        invulnerableUntil = Time.time + invulnerableSeconds;
        Debug.Log("HP " + current);
        if (current <= 0)
        {
            Tries++;
            current = maxHealth;
            transform.position = startPos;
            Debug.Log("Try " + Tries);
        }
    }
}