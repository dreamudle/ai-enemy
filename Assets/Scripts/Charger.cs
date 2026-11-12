using UnityEngine;

public class Charger : MonoBehaviour
{
    enum State { Idle, WindUp, Dash, Recover }

    [SerializeField] Vector2 baseFacing = Vector2.left;
    [SerializeField] float sweepAngle = 80f;
    [SerializeField] float sweepSpeed = 0.8f;
    [SerializeField] float visionRange = 7f;
    [SerializeField] float visionAngle = 120f;
    [SerializeField] float windUp = 0.7f;
    [SerializeField] float dashSpeed = 9f;
    [SerializeField] float dashTime = 0.6f;
    [SerializeField] float recoverTime = 1.1f;
    [SerializeField] int damage = 1;

    State state;
    float stateStart;
    Transform player;
    Rigidbody2D rb;
    SpriteRenderer sr;
    Color baseColor;
    LineRenderer line;
    Vector2 facing;
    Vector2 dashDir;

    float T { get { return Time.time - stateStart; } }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        baseColor = sr.color;
        Physics2D.queriesStartInColliders = false;
        facing = baseFacing.normalized;

        line = gameObject.AddComponent<LineRenderer>();
        line.positionCount = 2;
        line.startWidth = line.endWidth = 0.5f;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = line.endColor = new Color(1f, 0.4f, 0.1f, 0.45f);
        line.sortingOrder = 4;
        line.enabled = false;
    }

    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
        SetState(State.Idle);
    }

    void SetState(State s)
    {
        state = s;
        stateStart = Time.time;
        sr.color = (s == State.Recover) ? Color.Lerp(baseColor, Color.gray, 0.5f) : baseColor;   
        line.enabled = false;
    }

    void Update()
    {
        switch (state)
        {
            case State.Idle:
                rb.linearVelocity = Vector2.zero;
                facing = Quaternion.Euler(0, 0, Mathf.Sin(Time.time * sweepSpeed) * sweepAngle) * baseFacing.normalized;
                if (CanSee())
                {
                    dashDir = ((Vector2)player.position - (Vector2)transform.position).normalized;
                    SetState(State.WindUp);
                }
                break;

            case State.WindUp:
                rb.linearVelocity = Vector2.zero;
                if (T < windUp * 0.7f)   
                    dashDir = ((Vector2)player.position - (Vector2)transform.position).normalized;
                facing = dashDir;
                sr.color = Color.Lerp(baseColor, Color.red, T / windUp);
                line.enabled = true;
                line.SetPosition(0, transform.position);
                line.SetPosition(1, (Vector2)transform.position + dashDir * dashSpeed * dashTime);
                if (T >= windUp) SetState(State.Dash);
                break;

            case State.Dash:
                rb.linearVelocity = dashDir * dashSpeed;
                if (T >= dashTime) SetState(State.Recover);
                break;

            case State.Recover:
                rb.linearVelocity = Vector2.zero;
                if (T >= recoverTime) SetState(State.Idle);
                break;
        }
    }

    void OnCollisionEnter2D(Collision2D c)
    {
        if (state != State.Dash) return;
        Health h = c.collider.GetComponent<Health>();
        if (h != null) h.Damage(damage);
        SetState(State.Recover);  
    }

    bool CanSee()
    {
        Vector2 to = player.position - transform.position;
        float dist = to.magnitude;
        if (dist > visionRange) return false;
        if (Vector2.Angle(facing, to) > visionAngle * 0.5f) return false;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, to.normalized, dist);
        return hit.collider != null && hit.collider.transform == player;
    }
}