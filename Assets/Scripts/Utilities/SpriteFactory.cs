using UnityEngine;

/// <summary>
/// Generates Sprite objects at runtime from code — no art assets needed yet.
/// </summary>
public static class SpriteFactory
{
    public static Sprite Circle(Color col, int px = 64)
    {
        int sz = px * 2;
        var tex = new Texture2D(sz, sz, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        var pixels = new Color[sz * sz];
        var center = new Vector2(px, px);
        float r = px - 1.5f;
        for (int y = 0; y < sz; y++)
            for (int x = 0; x < sz; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), center);
                float a = d <= r ? col.a : col.a * Mathf.Max(0f, 1f - (d - r));
                pixels[y * sz + x] = new Color(col.r, col.g, col.b, a);
            }
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, sz, sz), Vector2.one * 0.5f, px * 2f);
    }

    public static Sprite Square(Color col, int sz = 64)
    {
        var tex = new Texture2D(sz, sz, TextureFormat.RGBA32, false);
        var pixels = new Color[sz * sz];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = col;
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, sz, sz), Vector2.one * 0.5f, sz);
    }

    public static Sprite Triangle(Color col, int sz = 64)
    {
        var tex = new Texture2D(sz, sz, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        var pixels = new Color[sz * sz];
        Vector2 v0 = new Vector2(sz * 0.5f, sz - 3f);
        Vector2 v1 = new Vector2(3f, 3f);
        Vector2 v2 = new Vector2(sz - 3f, 3f);
        for (int y = 0; y < sz; y++)
            for (int x = 0; x < sz; x++)
                pixels[y * sz + x] = PointInTri(new Vector2(x, y), v0, v1, v2) ? col : Color.clear;
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, sz, sz), new Vector2(0.5f, 0.33f), sz);
    }

    public static Sprite Diamond(Color col, int sz = 64)
    {
        var tex = new Texture2D(sz, sz, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        var pixels = new Color[sz * sz];
        float cx = sz * 0.5f, cy = sz * 0.5f, half = sz * 0.5f - 3f;
        for (int y = 0; y < sz; y++)
            for (int x = 0; x < sz; x++)
            {
                float dist = Mathf.Abs(x - cx) / half + Mathf.Abs(y - cy) / half;
                pixels[y * sz + x] = dist <= 1f ? col : Color.clear;
            }
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, sz, sz), Vector2.one * 0.5f, sz);
    }

    static bool PointInTri(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        float d1 = Cross(p, a, b), d2 = Cross(p, b, c), d3 = Cross(p, c, a);
        return !((d1 < 0 || d2 < 0 || d3 < 0) && (d1 > 0 || d2 > 0 || d3 > 0));
    }

    static float Cross(Vector2 p1, Vector2 p2, Vector2 p3) =>
        (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
}
