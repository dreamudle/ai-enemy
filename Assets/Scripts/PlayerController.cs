using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float speed = 4.5f;

    Rigidbody2D rb;
    bool stickActive;
    Vector2 origin;    // где палец коснулся экрана (пиксели, y вверх)
    Vector2 current;   // где палец сейчас
    Texture2D disc;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        disc = MakeDisc();
    }

    void Update()
    {
        bool down = false;
        Vector2 p = Vector2.zero;

        if (Input.touchCount > 0)              
        {
            Touch t = Input.GetTouch(0);
            p = t.position;
            down = t.phase != TouchPhase.Ended && t.phase != TouchPhase.Canceled;
        }
        else if (Input.GetMouseButton(0))       
        {
            p = Input.mousePosition;
            down = true;
        }

        if (down && !stickActive) origin = p;
        stickActive = down;
        current = p;

        float radius = Screen.height * 0.16f;
        Vector2 dir;
        if (stickActive)
            dir = Vector2.ClampMagnitude((current - origin) / radius, 1f);
        else
            dir = Vector2.ClampMagnitude(new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")), 1f);

        rb.linearVelocity = dir * speed;
    }

    void OnGUI()
    {
        if (!stickActive) return;
        float r = Screen.height * 0.16f;
        Vector2 d = Vector2.ClampMagnitude(current - origin, r);
        DrawDisc(new Vector2(origin.x, Screen.height - origin.y), r * 2f, new Color(1, 1, 1, 0.2f));
        DrawDisc(new Vector2(origin.x + d.x, Screen.height - (origin.y + d.y)), r * 0.7f, new Color(1f, 0.4f, 0f, 0.8f));
    }

    void DrawDisc(Vector2 c, float dia, Color col)
    {
        GUI.color = col;
        GUI.DrawTexture(new Rect(c.x - dia / 2f, c.y - dia / 2f, dia, dia), disc);
        GUI.color = Color.white;
    }

    Texture2D MakeDisc()
    {
        int n = 64;
        var t = new Texture2D(n, n, TextureFormat.RGBA32, false);
        for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(n / 2f, n / 2f));
                t.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp01(n / 2f - d)));
            }
        t.Apply();
        return t;
    }
}