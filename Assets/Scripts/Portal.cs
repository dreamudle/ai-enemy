using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    [SerializeField] float appearAfter = 10f;

    SpriteRenderer sr;
    Collider2D col;
    float startTime;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        sr.enabled = false;    
        col.enabled = false;   
    }

    void Start() { startTime = Time.time; }

    void Update()
    {
        if (!sr.enabled && Time.time - startTime >= appearAfter)
        {
            sr.enabled = true;
            col.enabled = true;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<Health>() != null)
        {
            Debug.Log("Beaten in " + Health.Tries + " tries");
            Health.Tries = 1;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); 
        }
    }
}