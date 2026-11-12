using UnityEngine;

public class Patroller : MonoBehaviour
{
    enum State { Patrol, Chase, WindUp, Cooldown }

    [SerializeField] Transform[] waypoints;
    [SerializeField] float patrolSpeed = 2f;
    [SerializeField] float chaseSpeed = 3.2f;
    [SerializeField] float visionRange = 6f;
    [SerializeField] float visionAngle = 100f;
    [SerializeField] float attackRange = 1.1f;
    [SerializeField] float windUp = 0.6f;
    [SerializeField] float cooldown = 1.2f;
    [SerializeField] float loseSightTime = 2f;
    [SerializeField] int damage = 1;

    State state;
    float stateStart;
    int wp;
    Rigidbody2D rb;
    SpriteRenderer sr;
    Color baseColor;
    Transform player;
    Vector2 facing = Vector2.right;
    Vector2 lastKnown;
    float lastSeen;

    float T { get { return Time.time - stateStart; } }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        baseColor = sr.color;
        Physics2D.queriesStartInColliders = false; 
    }

    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;   
        SetState(State.Patrol);
    }

    void SetState(State s)
    {
        state = s;
        stateStart = Time.time;
        sr.color = baseColor;
    }

    void Update()
    {
        bool sees = CanSee();
        if (sees) { lastKnown = player.position; lastSeen = Time.time; }

        switch (state)
        {
            case State.Patrol:
                if (waypoints.Length > 0 && MoveTo(waypoints[wp].position, patrolSpeed))
                    wp = (wp + 1) % waypoints.Length;
                if (sees) SetState(State.Chase);
                break;

            case State.Chase:
                MoveTo(lastKnown, chaseSpeed);
                if (sees && Vector2.Distance(transform.position, player.position) <= attackRange)
                    SetState(State.WindUp);
                else if (!sees && Time.time - lastSeen > loseSightTime)
                    SetState(State.Patrol);
                break;

            case State.WindUp:
                rb.linearVelocity = Vector2.zero;
                sr.color = Color.Lerp(baseColor, Color.red, T / windUp);  
                if (T >= windUp)
                {
                    if (Vector2.Distance(transform.position, player.position) <= attackRange + 0.3f)
                        player.GetComponent<Health>().Damage(damage);
                    SetState(State.Cooldown);
                }
                break;

            case State.Cooldown:
                rb.linearVelocity = Vector2.zero;
                if (T >= cooldown) SetState(sees ? State.Chase : State.Patrol);
                break;
        }
    }

    bool CanSee()
    {
        Vector2 to = player.position - transform.position;
        float dist = to.magnitude;
        if (dist > visionRange) return false;                                
        if (Vector2.Angle(facing, to) > visionAngle * 0.5f) return false;    
        RaycastHit2D hit = Physics2D.Raycast(transform.position, to.normalized, dist);  
        Debug.DrawRay(transform.position, to, Color.yellow);
        return hit.collider != null && hit.collider.transform == player;
    }

    bool MoveTo(Vector2 target, float speed)
    {
        Vector2 to = target - (Vector2)transform.position;
        if (to.magnitude < 0.2f) { rb.linearVelocity = Vector2.zero; return true; }
        facing = to.normalized;
        rb.linearVelocity = facing * speed;
        return false;
    }
}