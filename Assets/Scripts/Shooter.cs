using UnityEngine;

public class Shooter : MonoBehaviour
{
    enum State { Scan, Aim, Fire, Cooldown }

    [SerializeField] Projectile bulletPrefab;
    [SerializeField] Vector2 baseFacing = Vector2.down;
    [SerializeField] float sweepAngle = 50f;
    [SerializeField] float sweepSpeed = 1.2f;
    [SerializeField] float visionRange = 8f;
    [SerializeField] float visionAngle = 90f;
    [SerializeField] float windUp = 0.6f;
    [SerializeField] float lockTime = 0.25f;   // прицел замирает перед выстрелом, чтобы можно было уклониться
    [SerializeField] float cooldown = 1.6f;

    State state;
    float stateStart;
    Transform player;
    SpriteRenderer sr;
    Color baseColor;
    LineRenderer line;
    Vector2 facing;
    Vector2 aimDir;

    float T { get { return Time.time - stateStart; } }

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        baseColor = sr.color;
        Physics2D.queriesStartInColliders = false;
        facing = baseFacing.normalized;

        line = gameObject.AddComponent<LineRenderer>();
        line.positionCount = 2;
        line.startWidth = line.endWidth = 0.07f;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = line.endColor = Color.red;
        line.sortingOrder = 4;
        line.enabled = false;
    }

    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
        SetState(State.Scan);
    }

    void SetState(State s)
    {
        state = s;
        stateStart = Time.time;
        sr.color = baseColor;
        line.enabled = false;
    }

    void Update()
    {
        switch (state)
        {
            case State.Scan:
                facing = Quaternion.Euler(0, 0, Mathf.Sin(Time.time * sweepSpeed) * sweepAngle) * baseFacing.normalized;
                if (CanSee(true)) SetState(State.Aim);
                break;

            case State.Aim:
                if (T < windUp - lockTime)
                {
                    aimDir = ((Vector2)player.position - (Vector2)transform.position).normalized;
                    facing = aimDir;
                    if (!CanSee(false)) { SetState(State.Scan); break; }
                }
                sr.color = Color.Lerp(baseColor, Color.red, T / windUp);
                line.enabled = true;
                line.SetPosition(0, transform.position);
                line.SetPosition(1, (Vector2)transform.position + aimDir * visionRange);
                if (T >= windUp) SetState(State.Fire);
                break;

            case State.Fire:
                {
                    Projectile b = Instantiate(bulletPrefab, (Vector2)transform.position + aimDir * 0.7f, Quaternion.identity);
                    b.Launch(aimDir);
                    SetState(State.Cooldown);
                    break;
                }

            case State.Cooldown:
                if (T >= cooldown) SetState(State.Scan);
                break;
        }
    }

    bool CanSee(bool useAngle)
    {
        Vector2 to = player.position - transform.position;
        float dist = to.magnitude;
        if (dist > visionRange) return false;
        if (useAngle && Vector2.Angle(facing, to) > visionAngle * 0.5f) return false;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, to.normalized, dist);
        Debug.DrawRay(transform.position, to, Color.cyan);
        return hit.collider != null && hit.collider.transform == player;
    }
}