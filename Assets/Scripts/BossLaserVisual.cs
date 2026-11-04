using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BossLaserVisual : MonoBehaviour
{
    public Sprite[] frames;
    public float framesPerSecond = 12;
    [HideInInspector] public float length = 20;
    [HideInInspector] public float thickness = 0.7f;
    private SpriteRenderer sr;
    private float timer;

    void Awake() { sr = GetComponent<SpriteRenderer>(); }

    void LateUpdate()
    {
        if (frames == null || frames.Length == 0) return;
        timer += Time.deltaTime;
        int index = Mathf.FloorToInt(timer * Mathf.Max(1, framesPerSecond)) % frames.Length;
        Sprite frame = frames[index];
        if (frame == null) return;
        sr.sprite = frame;
        // Tight mesh bounds ignore most transparent padding around each beam.
        Vector2 min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
        Vector2 max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
        foreach (Vector2 v in frame.vertices)
        { min = Vector2.Min(min, v); max = Vector2.Max(max, v); }
        Vector2 size = max - min;
        if (size.x > 0 && size.y > 0)
            transform.localScale = new Vector3(length / size.x, thickness / size.y, 1);
    }
}
