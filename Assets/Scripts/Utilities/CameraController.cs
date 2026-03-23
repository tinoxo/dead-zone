using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }
    public Transform Target;
    public float SmoothSpeed = 9f;
    public float ShakeDuration = 0f;
    public float ShakeMagnitude = 0.3f;

    const float ARENA_HALF = 19f;
    Camera cam;

    void Awake()
    {
        Instance = this;
        cam = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        if (Target == null) return;

        Vector3 desired = new Vector3(Target.position.x, Target.position.y, -10f);

        // Clamp to arena
        float halfH = cam ? cam.orthographicSize : 10f;
        float halfW = cam ? cam.orthographicSize * cam.aspect : 16f;
        desired.x = Mathf.Clamp(desired.x, -ARENA_HALF + halfW, ARENA_HALF - halfW);
        desired.y = Mathf.Clamp(desired.y, -ARENA_HALF + halfH, ARENA_HALF - halfH);

        transform.position = Vector3.Lerp(transform.position, desired, SmoothSpeed * Time.deltaTime);

        // Screen shake — use unscaledDeltaTime so it always expires even when timeScale=0
        if (ShakeDuration > 0)
        {
            transform.position += (Vector3)Random.insideUnitCircle * ShakeMagnitude;
            ShakeDuration -= Time.unscaledDeltaTime;
        }
    }

    public void Shake(float duration = 0.2f, float magnitude = 0.25f)
    {
        ShakeDuration = duration;
        ShakeMagnitude = magnitude;
    }
}
