using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// DEAD ZONE — The Docks
/// A walkable hub before the ship run. Talk to NPCs, board the ship,
/// press E at the helm to SET SAIL.
/// Attach to ONE empty GameObject in DockScene.
/// </summary>
public class DockSetup : MonoBehaviour
{
    // ── Layout ─────────────────────────────────────────────────────────────
    const float DOCK_Y     =  0f;
    const float DOCK_H     =  3.4f;
    const float DOCK_LEFT  = -13f;
    const float DOCK_RIGHT =  7.5f;
    const float PLANK_X    =  7.5f;   // gangplank center x
    const float SHIP_LEFT  =  8.8f;
    const float SHIP_RIGHT = 15.0f;
    const float SHIP_H     =  3.0f;
    const float HELM_X     = 13.8f;
    const float HELM_Y     =  0.0f;
    const float HELM_DIST  =  1.8f;
    const float TALK_DIST  =  1.9f;

    // ── Colors ─────────────────────────────────────────────────────────────
    static readonly Color C_SKY       = new Color(0.04f, 0.07f, 0.14f);
    static readonly Color C_WATER     = new Color(0.03f, 0.08f, 0.17f);
    static readonly Color C_WATER_S   = new Color(0.05f, 0.11f, 0.22f);
    static readonly Color C_DOCK      = new Color(0.28f, 0.18f, 0.09f);
    static readonly Color C_PLANK     = new Color(0.36f, 0.24f, 0.12f);
    static readonly Color C_PLANK2    = new Color(0.22f, 0.14f, 0.07f);
    static readonly Color C_BUILDING  = new Color(0.09f, 0.11f, 0.17f);
    static readonly Color C_BUILDING2 = new Color(0.12f, 0.14f, 0.21f);
    static readonly Color C_WIN_LIT   = new Color(0.92f, 0.76f, 0.34f);
    static readonly Color C_WIN_OFF   = new Color(0.06f, 0.08f, 0.13f);
    static readonly Color C_LANTERN   = new Color(1.00f, 0.68f, 0.18f);
    static readonly Color C_HULL      = new Color(0.20f, 0.12f, 0.06f);
    static readonly Color C_HULL_DARK = new Color(0.13f, 0.07f, 0.03f);
    static readonly Color C_MAST      = new Color(0.28f, 0.17f, 0.08f);
    static readonly Color C_SAIL      = new Color(0.80f, 0.74f, 0.58f);
    static readonly Color C_SAIL_SHD  = new Color(0.65f, 0.59f, 0.44f);
    static readonly Color C_ROPE      = new Color(0.50f, 0.38f, 0.18f);
    static readonly Color C_CRATE     = new Color(0.35f, 0.22f, 0.10f);

    // ── Runtime state ──────────────────────────────────────────────────────
    GameObject playerGO;
    Canvas     uiCanvas;
    GameObject promptGO;
    Text       promptText;
    GameObject dialoguePanel;
    Text       speakerText;
    Text       dialogueText;
    Text       hintText;

    List<DockNPC> npcs       = new List<DockNPC>();
    DockNPC       currentNPC = null;
    bool          transitioning;

    // ══════════════════════════════════════════════════════════════════════
    void Awake()
    {
        Camera.main.backgroundColor = C_SKY;
        Camera.main.orthographic    = true;
        Camera.main.orthographicSize = 6f;

        BuildSky();
        BuildEnvironment();
        BuildShip();
        BuildNPCs();
        playerGO = BuildPlayer();
        BuildCamera();
        BuildUI();
    }

    void Update()
    {
        if (transitioning || playerGO == null) return;

        var pPos = (Vector2)playerGO.transform.position;

        // ── Find nearest NPC ───────────────────────────────────────────────
        DockNPC nearNPC  = null;
        float   nearDist = TALK_DIST;
        foreach (var npc in npcs)
        {
            float d = Vector2.Distance(pPos, npc.WorldPos);
            if (d < nearDist) { nearDist = d; nearNPC = npc; }
        }

        // ── Helm distance ──────────────────────────────────────────────────
        float helmDist = Vector2.Distance(pPos, new Vector2(HELM_X, HELM_Y));
        bool  nearHelm = helmDist < HELM_DIST;

        // ── Prompt priority: NPC first, then helm ──────────────────────────
        if (nearNPC != null)
        {
            SetPrompt("[ E ]  TALK");
            if (Input.GetKeyDown(KeyCode.E)) TalkTo(nearNPC);
        }
        else if (nearHelm)
        {
            SetPrompt("[ E ]  SET SAIL");
            if (Input.GetKeyDown(KeyCode.E)) StartCoroutine(SetSail());
        }
        else
        {
            HidePrompt();
        }

        // Close dialogue if walked away from current NPC
        if (currentNPC != null &&
            Vector2.Distance(pPos, currentNPC.WorldPos) > TALK_DIST + 0.8f)
            CloseDialogue();
    }

    // ══════════════════════════════════════════════════════════════════════
    //  TALK / DIALOGUE
    // ══════════════════════════════════════════════════════════════════════
    void TalkTo(DockNPC npc)
    {
        if (currentNPC == npc)
        {
            npc.Advance();
            if (npc.Finished) { CloseDialogue(); return; }
            UpdateDialogue();
        }
        else
        {
            currentNPC = npc;
            npc.ResetLine();
            UpdateDialogue();
        }
    }

    void UpdateDialogue()
    {
        dialoguePanel.SetActive(true);
        speakerText.text  = currentNPC.Name;
        speakerText.color = currentNPC.Color;
        dialogueText.text = currentNPC.Line;
        hintText.text     = currentNPC.Finished ? "[ E ] Close" : "[ E ] Next";
    }

    void CloseDialogue()
    {
        currentNPC = null;
        dialoguePanel.SetActive(false);
    }

    void SetPrompt(string txt)
    {
        if (promptGO) promptGO.SetActive(true);
        if (promptText) promptText.text = txt;
    }

    void HidePrompt() { if (promptGO) promptGO.SetActive(false); }

    // ══════════════════════════════════════════════════════════════════════
    //  SET SAIL
    // ══════════════════════════════════════════════════════════════════════
    IEnumerator SetSail()
    {
        transitioning = true;
        HidePrompt();
        CloseDialogue();

        // Fade overlay
        var fGO  = new GameObject("Fade");
        fGO.transform.SetParent(uiCanvas.transform, false);
        var fRT  = fGO.AddComponent<RectTransform>();
        fRT.anchorMin = Vector2.zero; fRT.anchorMax = Vector2.one;
        fRT.offsetMin = fRT.offsetMax = Vector2.zero;
        var fImg = fGO.AddComponent<Image>();
        fImg.color = new Color(0f, 0f, 0f, 0f);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 1.6f;
            fImg.color = new Color(0f, 0f, 0f, Mathf.Clamp01(t));
            yield return null;
        }

        GameData.IsShipRun = true;
        SceneManager.LoadScene("SampleScene");
    }

    // ══════════════════════════════════════════════════════════════════════
    //  SKY & STARS
    // ══════════════════════════════════════════════════════════════════════
    void BuildSky()
    {
        var skyGO = new GameObject("Sky");

        // Water/ocean fill
        MakeRect("Ocean", skyGO.transform, new Vector2(1f, 0f),
                 new Vector2(40f, 22f), C_WATER, -20);

        // Moon
        MakeCircleObj("Moon", skyGO.transform,
                      new Vector2(-11f, 6f), 0.7f,
                      new Color(0.92f, 0.92f, 0.78f), -18);

        // Stars
        var rng = new System.Random(7);
        for (int i = 0; i < 60; i++)
        {
            float sx = (float)(rng.NextDouble() * 34 - 16);
            float sy = (float)(rng.NextDouble() * 6 + 2.5f);
            float sz = (float)(rng.NextDouble() * 0.06f + 0.03f);
            float br = (float)(rng.NextDouble() * 0.5f + 0.5f);
            MakeRect($"Star{i}", skyGO.transform,
                     new Vector2(sx, sy), new Vector2(sz, sz),
                     new Color(br, br, br * 0.9f), -17);
        }

        // Water shimmer lines
        for (int i = -7; i <= 8; i++)
        {
            MakeRect($"Shim{i}", skyGO.transform,
                     new Vector2(i * 2.6f + (float)new System.Random(i*13).NextDouble(),
                                 DOCK_Y - DOCK_H * 0.5f - 0.9f),
                     new Vector2(2.0f, 0.09f), C_WATER_S, -16);
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    //  ENVIRONMENT
    // ══════════════════════════════════════════════════════════════════════
    void BuildEnvironment()
    {
        var env = new GameObject("Environment");

        // === DOCK BASE ===
        float dockW  = DOCK_RIGHT - DOCK_LEFT;
        float dockCX = (DOCK_LEFT + DOCK_RIGHT) * 0.5f;
        MakeRect("DockBase", env.transform,
                 new Vector2(dockCX, DOCK_Y), new Vector2(dockW, DOCK_H), C_DOCK, -15);

        // Plank lines (vertical grain)
        int planks = Mathf.RoundToInt(dockW / 1.3f);
        for (int i = 0; i < planks; i++)
        {
            float px = DOCK_LEFT + (i + 0.5f) * dockW / planks;
            Color pc  = i % 2 == 0 ? C_PLANK : C_PLANK2;
            MakeRect($"Pl{i}", env.transform,
                     new Vector2(px, DOCK_Y), new Vector2(0.06f, DOCK_H * 0.92f), pc, -14);
        }

        // Dock top & bottom edges
        MakeRect("EdgeTop", env.transform,
                 new Vector2(dockCX, DOCK_Y + DOCK_H * 0.5f),
                 new Vector2(dockW, 0.16f), C_PLANK2, -13);
        MakeRect("EdgeBot", env.transform,
                 new Vector2(dockCX, DOCK_Y - DOCK_H * 0.5f),
                 new Vector2(dockW, 0.16f), C_PLANK2, -13);

        // Mooring posts with lanterns
        float[] postXs = { -11f, -7f, -3f, 1f, 5f };
        foreach (float px in postXs)
        {
            // Post top
            MakeRect($"PostT{px}", env.transform,
                     new Vector2(px, DOCK_Y + DOCK_H * 0.5f - 0.12f),
                     new Vector2(0.26f, 0.26f), C_PLANK2, -12);
            // Post bottom
            MakeRect($"PostB{px}", env.transform,
                     new Vector2(px, DOCK_Y - DOCK_H * 0.5f + 0.12f),
                     new Vector2(0.26f, 0.26f), C_PLANK2, -12);
            // Lantern glow
            MakeCircleObj($"Lantern{px}", env.transform,
                          new Vector2(px, DOCK_Y + DOCK_H * 0.5f + 0.45f),
                          0.18f, C_LANTERN, -12);
            // Lantern pole
            MakeRect($"Pole{px}", env.transform,
                     new Vector2(px, DOCK_Y + DOCK_H * 0.5f + 0.2f),
                     new Vector2(0.05f, 0.5f), C_PLANK2, -13);
        }

        // === CRATES & BARRELS (props) ===
        BuildProps(env.transform);

        // === CITY BUILDINGS ===
        BuildCity(env.transform);

        // === INVISIBLE WALLS ===
        // Keep player on dock + ship
        AddWall(env.transform, new Vector2(dockCX,   DOCK_Y + DOCK_H * 0.5f + 0.3f), new Vector2(dockW + 2f, 0.5f));
        AddWall(env.transform, new Vector2(dockCX,   DOCK_Y - DOCK_H * 0.5f - 0.3f), new Vector2(dockW + 2f, 0.5f));
        AddWall(env.transform, new Vector2(DOCK_LEFT - 0.3f, DOCK_Y),                  new Vector2(0.5f,  DOCK_H + 2f));
        // Ship walls (top/bottom/right)
        float shipCX = (SHIP_LEFT + SHIP_RIGHT) * 0.5f;
        AddWall(env.transform, new Vector2(shipCX,   DOCK_Y + SHIP_H * 0.5f + 0.3f),  new Vector2(SHIP_RIGHT - SHIP_LEFT + 1f, 0.5f));
        AddWall(env.transform, new Vector2(shipCX,   DOCK_Y - SHIP_H * 0.5f - 0.3f),  new Vector2(SHIP_RIGHT - SHIP_LEFT + 1f, 0.5f));
        AddWall(env.transform, new Vector2(SHIP_RIGHT + 0.3f, DOCK_Y),                 new Vector2(0.5f,  SHIP_H + 2f));
        // Gangplank walls (narrow corridor between dock and ship)
        AddWall(env.transform, new Vector2(PLANK_X, DOCK_Y + 0.7f), new Vector2(2.0f, 0.3f));
        AddWall(env.transform, new Vector2(PLANK_X, DOCK_Y - 0.7f), new Vector2(2.0f, 0.3f));
    }

    void BuildProps(Transform parent)
    {
        // Crates
        (float x, float y, float w, float h)[] crates =
        {
            (-9.5f,  DOCK_Y + 0.9f, 0.7f, 0.7f),
            (-9.0f,  DOCK_Y + 0.9f, 0.5f, 0.5f),
            (-9.2f,  DOCK_Y - 0.9f, 0.7f, 0.7f),
            ( 3.5f,  DOCK_Y + 0.9f, 0.8f, 0.8f),
            ( 4.2f,  DOCK_Y + 0.9f, 0.6f, 0.5f),
            ( 3.8f,  DOCK_Y - 0.9f, 0.7f, 0.7f),
        };
        int ci = 0;
        foreach (var (cx, cy, cw, ch) in crates)
            MakeRect($"Crate{ci++}", parent, new Vector2(cx, cy),
                     new Vector2(cw, ch), C_CRATE, -11);

        // Barrels (circles)
        (float x, float y, float r)[] barrels =
        {
            (-8.2f, DOCK_Y + 0.85f, 0.28f),
            (-8.2f, DOCK_Y - 0.85f, 0.28f),
            ( 5.0f, DOCK_Y - 0.90f, 0.28f),
        };
        int bi = 0;
        foreach (var (bx, by, br) in barrels)
            MakeCircleObj($"Barrel{bi++}", parent, new Vector2(bx, by), br,
                          new Color(0.30f, 0.18f, 0.08f), -11);
    }

    void BuildCity(Transform parent)
    {
        float baseY = DOCK_Y + DOCK_H * 0.5f + 0.15f;
        var rng = new System.Random(42);

        (float x, float w, float h)[] blds =
        {
            (-12f, 4.5f, 6.5f),
            ( -7f, 3.2f, 5.0f),
            ( -2f, 5.5f, 8.0f),
            (  4f, 3.8f, 6.0f),
            (  9f, 4.0f, 7.0f),
            ( 13f, 3.5f, 5.5f),
        };

        foreach (var (bx, bw, bh) in blds)
        {
            Color bc = rng.Next(2) == 0 ? C_BUILDING : C_BUILDING2;
            MakeRect($"Bld{bx}", parent,
                     new Vector2(bx, baseY + bh * 0.5f), new Vector2(bw, bh), bc, -19);
            // Roof strip
            MakeRect($"BldRoof{bx}", parent,
                     new Vector2(bx, baseY + bh), new Vector2(bw + 0.12f, 0.22f),
                     new Color(0.05f, 0.06f, 0.10f), -18);
            // Windows
            int wc = Mathf.Max(1, Mathf.RoundToInt(bw / 1.2f));
            int wr = Mathf.Max(1, Mathf.RoundToInt(bh / 1.6f));
            for (int wy = 0; wy < wr; wy++)
            for (int wx = 0; wx < wc; wx++)
            {
                float winX = bx + (wx - (wc - 1) * 0.5f) * (bw / (wc + 0.3f));
                float winY = baseY + 0.5f + wy * (bh / (wr + 0.4f));
                bool lit   = rng.Next(3) != 0;
                MakeRect($"Win{bx}_{wx}_{wy}", parent, new Vector2(winX, winY),
                         new Vector2(0.35f, 0.46f),
                         lit ? C_WIN_LIT : C_WIN_OFF, -17);
            }
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    //  SHIP
    // ══════════════════════════════════════════════════════════════════════
    void BuildShip()
    {
        var shipGO = new GameObject("Ship");
        float cx = (SHIP_LEFT + SHIP_RIGHT) * 0.5f;

        // Hull main
        MakeRect("Hull",     shipGO.transform, new Vector2(cx,   DOCK_Y),
                 new Vector2(SHIP_RIGHT - SHIP_LEFT, SHIP_H), C_HULL, -15);
        // Hull keel
        MakeRect("Keel",     shipGO.transform, new Vector2(cx,   DOCK_Y - SHIP_H * 0.45f),
                 new Vector2(SHIP_RIGHT - SHIP_LEFT - 0.8f, SHIP_H * 0.3f), C_HULL_DARK, -14);
        // Bow (pointed left end)
        MakeRect("Bow",      shipGO.transform, new Vector2(SHIP_LEFT - 0.3f, DOCK_Y),
                 new Vector2(1.0f, SHIP_H * 0.6f), C_HULL_DARK, -14);

        // Deck planks
        float dw = SHIP_RIGHT - SHIP_LEFT;
        int dp   = Mathf.RoundToInt(dw / 1.1f);
        for (int i = 0; i < dp; i++)
        {
            float px = SHIP_LEFT + (i + 0.5f) * dw / dp;
            Color pc  = i % 2 == 0 ? C_PLANK : C_PLANK2;
            MakeRect($"DeckPl{i}", shipGO.transform,
                     new Vector2(px, DOCK_Y), new Vector2(0.07f, SHIP_H * 0.85f), pc, -13);
        }

        // Deck railing
        MakeRect("RailTop", shipGO.transform,
                 new Vector2(cx, DOCK_Y + SHIP_H * 0.5f),
                 new Vector2(dw, 0.18f), C_MAST, -12);
        MakeRect("RailBot", shipGO.transform,
                 new Vector2(cx, DOCK_Y - SHIP_H * 0.5f),
                 new Vector2(dw, 0.18f), C_MAST, -12);

        // Gangplank
        MakeRect("Gangplank", shipGO.transform,
                 new Vector2(PLANK_X, DOCK_Y),
                 new Vector2(SHIP_LEFT - DOCK_RIGHT + 0.8f, 1.1f), C_PLANK, -13);

        // Main mast
        float mastX = cx - 1.0f;
        MakeRect("Mast",    shipGO.transform,
                 new Vector2(mastX, DOCK_Y + 3.0f), new Vector2(0.16f, 6.0f), C_MAST, -11);
        MakeRect("Yard",    shipGO.transform,
                 new Vector2(mastX, DOCK_Y + 1.8f), new Vector2(3.2f, 0.12f), C_MAST, -11);
        MakeRect("TopYard", shipGO.transform,
                 new Vector2(mastX, DOCK_Y + 4.5f), new Vector2(2.0f, 0.10f), C_MAST, -11);
        // Sails
        MakeRect("SailMain", shipGO.transform,
                 new Vector2(mastX + 0.5f, DOCK_Y + 3.0f), new Vector2(2.0f, 2.2f), C_SAIL, -12);
        MakeRect("SailTop",  shipGO.transform,
                 new Vector2(mastX + 0.4f, DOCK_Y + 4.8f), new Vector2(1.3f, 1.0f), C_SAIL_SHD, -12);
        // Ropes
        MakeRect("Rope1", shipGO.transform,
                 new Vector2(mastX + 0.8f, DOCK_Y + 2.5f), new Vector2(0.07f, 2.0f), C_ROPE, -11);
        MakeRect("Rope2", shipGO.transform,
                 new Vector2(mastX - 0.6f, DOCK_Y + 2.5f), new Vector2(0.07f, 2.0f), C_ROPE, -11);
        // Flag
        MakeRect("Flag", shipGO.transform,
                 new Vector2(mastX + 0.5f, DOCK_Y + 5.6f),
                 new Vector2(0.7f, 0.38f), new Color(0.65f, 0.08f, 0.08f), -11);

        // Cannons (visual)
        float[] cannonY = { DOCK_Y + SHIP_H * 0.38f, DOCK_Y - SHIP_H * 0.38f };
        float[] cannonX = { cx - 2f, cx, cx + 2f };
        foreach (float cy2 in cannonY)
        foreach (float cx2 in cannonX)
            MakeRect($"Cannon{cx2}{cy2}", shipGO.transform,
                     new Vector2(cx2, cy2), new Vector2(0.55f, 0.22f),
                     C_HULL_DARK, -10);

        // Helm (ship wheel) at back-right of deck
        BuildHelm(shipGO.transform, new Vector2(HELM_X, HELM_Y));

        // "SET SAIL" world label above helm
        BuildWorldLabel(shipGO.transform, new Vector2(HELM_X, DOCK_Y + 2.4f),
                        "[ E ]  SET SAIL", new Color(1f, 0.86f, 0.38f));
    }

    void BuildHelm(Transform parent, Vector2 pos)
    {
        var go = new GameObject("Helm");
        go.transform.SetParent(parent, false);
        go.transform.position = pos;

        // Wheel hub
        MakeCircleObj("Hub", go.transform, pos, 0.30f,
                      new Color(0.38f, 0.24f, 0.10f), -9);
        // Spokes (8 thin rectangles radiating out)
        for (int i = 0; i < 8; i++)
        {
            float ang = i * 45f * Mathf.Deg2Rad;
            var spoke = MakeRect($"Spoke{i}", go.transform,
                         pos + new Vector2(Mathf.Cos(ang) * 0.35f, Mathf.Sin(ang) * 0.35f),
                         new Vector2(0.10f, 0.55f),
                         new Color(0.32f, 0.20f, 0.09f), -9);
            spoke.transform.rotation = Quaternion.Euler(0, 0, i * 45f + 90f);
        }
        // Outer rim
        MakeCircleObj("Rim", go.transform, pos, 0.58f,
                      new Color(0.28f, 0.17f, 0.07f), -10);
        MakeCircleObj("RimInner", go.transform, pos, 0.46f,
                      new Color(0.20f, 0.12f, 0.06f), -9);
    }

    // ══════════════════════════════════════════════════════════════════════
    //  NPCs
    // ══════════════════════════════════════════════════════════════════════
    void BuildNPCs()
    {
        AddNPC("DOCKMASTER",
               new Vector2(-10f, DOCK_Y),
               new Color(0.30f, 0.75f, 0.90f),
               new[]
               {
                   "Welcome back, captain. Ship's been restless.",
                   "Rough waters lately. The islands are stirring.",
                   "Board when you're ready. She won't wait forever.",
               });

        AddNPC("NAVIGATOR",
               new Vector2(-3.5f, DOCK_Y),
               new Color(0.90f, 0.72f, 0.30f),
               new[]
               {
                   "I've charted all four islands. None are safe.",
                   "The fragments... they call to each other. Find them all and something wakes.",
                   "Every captain who sailed for the final boss came back different. Or didn't.",
               });

        AddNPC("MERCHANT",
               new Vector2(4f, DOCK_Y),
               new Color(0.60f, 0.90f, 0.40f),
               new[]
               {
                   "Shop's closed here. The real deals are out on the islands.",
                   "Gilded Sands has a market worth risking your life for. Just saying.",
                   "Bring back gold. I'll be right here.",
               });
    }

    void AddNPC(string name, Vector2 pos, Color col, string[] dialogue)
    {
        var go = new GameObject($"NPC_{name}");
        go.transform.position = pos;

        // Body (circle)
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 3;
        sr.sprite       = MakeCircleSprite(col, 48);
        go.transform.localScale = new Vector3(0.55f, 0.55f, 1f);

        // Name label (world canvas)
        BuildWorldLabel(go.transform, pos + new Vector2(0, 0.7f), name, col, 0.008f, 18);

        var npc = new DockNPC(name, pos, col, dialogue);
        npcs.Add(npc);
    }

    // ══════════════════════════════════════════════════════════════════════
    //  PLAYER
    // ══════════════════════════════════════════════════════════════════════
    GameObject BuildPlayer()
    {
        var go   = new GameObject("Player");
        go.tag   = "Player";
        go.transform.position = new Vector2(DOCK_LEFT + 2.5f, DOCK_Y);

        Color col = GameData.SelectedCharacter?.PrimaryColor ?? Color.cyan;
        var sr    = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 5;
        sr.sprite       = MakeCircleSprite(col, 64);
        go.transform.localScale = new Vector3(0.62f, 0.62f, 1f);

        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale   = 0f;
        rb.freezeRotation = true;
        rb.linearDamping  = 12f;

        go.AddComponent<CircleCollider2D>().radius = 0.5f;
        go.AddComponent<DockPlayerWalk>();
        return go;
    }

    // ══════════════════════════════════════════════════════════════════════
    //  CAMERA
    // ══════════════════════════════════════════════════════════════════════
    void BuildCamera()
    {
        var cam = Camera.main;
        cam.transform.position = new Vector3(0f, 0f, -10f);
        cam.orthographicSize   = 6f;

        var follow = cam.gameObject.AddComponent<DockCameraFollow>();
        follow.Target    = playerGO.transform;
        follow.BoundsMin = new Vector2(-9f,  -1f);
        follow.BoundsMax = new Vector2(11f,   1f);
    }

    // ══════════════════════════════════════════════════════════════════════
    //  UI
    // ══════════════════════════════════════════════════════════════════════
    void BuildUI()
    {
        var cGO = new GameObject("UICanvas");
        uiCanvas = cGO.AddComponent<Canvas>();
        uiCanvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        uiCanvas.sortingOrder = 50;
        var sc = cGO.AddComponent<CanvasScaler>();
        sc.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sc.referenceResolution = new Vector2(1920, 1080);
        sc.matchWidthOrHeight  = 0.5f;
        cGO.AddComponent<GraphicRaycaster>();

        // ── Interaction prompt (bottom center) ────────────────────────────
        promptGO = new GameObject("Prompt");
        promptGO.transform.SetParent(cGO.transform, false);
        var pRT = promptGO.AddComponent<RectTransform>();
        pRT.anchorMin = new Vector2(0.5f, 0f);
        pRT.anchorMax = new Vector2(0.5f, 0f);
        pRT.pivot     = new Vector2(0.5f, 0f);
        pRT.anchoredPosition = new Vector2(0f, 28f);
        pRT.sizeDelta        = new Vector2(320f, 52f);
        promptGO.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.78f);
        promptText = AddUIText(promptGO.transform, "", 23, FontStyle.Bold,
                               new Color(1f, 0.88f, 0.40f), TextAnchor.MiddleCenter);
        promptGO.SetActive(false);

        // ── Dialogue box (bottom) ─────────────────────────────────────────
        dialoguePanel = new GameObject("Dialogue");
        dialoguePanel.transform.SetParent(cGO.transform, false);
        var dRT = dialoguePanel.AddComponent<RectTransform>();
        dRT.anchorMin = new Vector2(0.1f, 0f);
        dRT.anchorMax = new Vector2(0.9f, 0f);
        dRT.pivot     = new Vector2(0.5f, 0f);
        dRT.anchoredPosition = new Vector2(0f, 18f);
        dRT.sizeDelta        = new Vector2(0f, 130f);
        dialoguePanel.AddComponent<Image>().color = new Color(0.03f, 0.05f, 0.10f, 0.94f);

        // Border accent
        var brd = new GameObject("Border");
        brd.transform.SetParent(dialoguePanel.transform, false);
        var bImg = brd.AddComponent<Image>();
        bImg.color = new Color(0.30f, 0.55f, 0.90f, 0.9f);
        bImg.rectTransform.anchorMin = new Vector2(0f, 1f);
        bImg.rectTransform.anchorMax = new Vector2(1f, 1f);
        bImg.rectTransform.pivot     = new Vector2(0.5f, 1f);
        bImg.rectTransform.offsetMin = Vector2.zero;
        bImg.rectTransform.offsetMax = Vector2.zero;
        bImg.rectTransform.sizeDelta = new Vector2(0f, 2.5f);

        speakerText = AddUIText(dialoguePanel.transform, "SPEAKER", 16, FontStyle.Bold,
                                Color.white, TextAnchor.UpperLeft, new Vector2(16f, -10f),
                                new Vector2(-120f, -36f));

        dialogueText = AddUIText(dialoguePanel.transform, "", 18, FontStyle.Normal,
                                 new Color(0.82f, 0.84f, 0.90f), TextAnchor.UpperLeft,
                                 new Vector2(16f, -38f), new Vector2(-24f, -80f));
        dialogueText.horizontalOverflow = HorizontalWrapMode.Wrap;
        dialogueText.verticalOverflow   = VerticalWrapMode.Overflow;

        hintText = AddUIText(dialoguePanel.transform, "[ E ] Next", 13, FontStyle.Normal,
                             new Color(0.45f, 0.50f, 0.60f), TextAnchor.LowerRight,
                             new Vector2(-16f, 10f), new Vector2(-20f, 30f), true);

        dialoguePanel.SetActive(false);

        // ── Location & char label (top) ───────────────────────────────────
        var locGO = new GameObject("LocLabel");
        locGO.transform.SetParent(cGO.transform, false);
        var locRT = locGO.AddComponent<RectTransform>();
        locRT.anchorMin = new Vector2(0.5f, 1f);
        locRT.anchorMax = new Vector2(0.5f, 1f);
        locRT.pivot = new Vector2(0.5f, 1f);
        locRT.anchoredPosition = new Vector2(0f, -16f);
        locRT.sizeDelta = new Vector2(400f, 28f);
        var locTxt = locGO.AddComponent<Text>();
        locTxt.text      = "THE DOCKS";
        locTxt.alignment = TextAnchor.UpperCenter;
        locTxt.fontSize  = 14;
        locTxt.color     = new Color(0.38f, 0.48f, 0.65f);
        locTxt.font      = Font();

        var nmGO = new GameObject("CharLabel");
        nmGO.transform.SetParent(cGO.transform, false);
        var nmRT = nmGO.AddComponent<RectTransform>();
        nmRT.anchorMin = new Vector2(0f, 1f); nmRT.anchorMax = new Vector2(0f, 1f);
        nmRT.pivot = new Vector2(0f, 1f);
        nmRT.anchoredPosition = new Vector2(16f, -16f);
        nmRT.sizeDelta = new Vector2(260f, 30f);
        var nmTxt = nmGO.AddComponent<Text>();
        nmTxt.text      = GameData.SelectedCharacter?.Name?.ToUpper() ?? "PLAYER";
        nmTxt.fontSize  = 16; nmTxt.fontStyle = FontStyle.Bold;
        nmTxt.color     = GameData.SelectedCharacter?.PrimaryColor ?? Color.white;
        nmTxt.font      = Font();
    }

    // ══════════════════════════════════════════════════════════════════════
    //  HELPERS
    // ══════════════════════════════════════════════════════════════════════
    void BuildWorldLabel(Transform parent, Vector2 worldPos, string txt, Color col,
                         float scale = 0.010f, int fontSize = 28)
    {
        var cGO  = new GameObject("WLabel");
        cGO.transform.SetParent(parent, false);
        cGO.transform.position = worldPos;
        var canv = cGO.AddComponent<Canvas>();
        canv.renderMode   = RenderMode.WorldSpace;
        canv.sortingOrder = 20;
        var rt = cGO.GetComponent<RectTransform>();
        rt.sizeDelta  = new Vector2(500f, 70f);
        rt.localScale = new Vector3(scale, scale, 1f);
        var t = new GameObject("T").AddComponent<Text>();
        t.transform.SetParent(cGO.transform, false);
        var tRT = t.rectTransform;
        tRT.anchorMin = Vector2.zero; tRT.anchorMax = Vector2.one;
        tRT.offsetMin = tRT.offsetMax = Vector2.zero;
        t.text = txt; t.alignment = TextAnchor.MiddleCenter;
        t.fontSize = fontSize; t.fontStyle = FontStyle.Bold;
        t.color = col; t.font = Font();
    }

    Text AddUIText(Transform parent, string content, int size, FontStyle style,
                   Color col, TextAnchor anchor,
                   Vector2 anchMin = default, Vector2 anchMax = default,
                   bool anchorToCorner = false)
    {
        var go  = new GameObject("Txt");
        go.transform.SetParent(parent, false);
        var rt  = go.AddComponent<RectTransform>();
        if (anchorToCorner)
        {
            rt.anchorMin = Vector2.one; rt.anchorMax = Vector2.one;
            rt.pivot     = Vector2.one;
            rt.anchoredPosition = anchMin;
            rt.sizeDelta = anchMax;
        }
        else
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = anchMin; rt.offsetMax = anchMax;
        }
        var t   = go.AddComponent<Text>();
        t.text      = content; t.fontSize = size;
        t.fontStyle = style;   t.color    = col;
        t.alignment = anchor;  t.font     = Font();
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow   = VerticalWrapMode.Overflow;
        return t;
    }

    void AddWall(Transform parent, Vector2 pos, Vector2 size)
    {
        var go = new GameObject("Wall");
        go.transform.SetParent(parent, false);
        go.transform.position = pos;
        go.AddComponent<BoxCollider2D>().size = size;
    }

    GameObject MakeRect(string name, Transform parent, Vector2 pos, Vector2 size,
                        Color col, int order)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position   = pos;
        go.transform.localScale = new Vector3(size.x, size.y, 1f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = order;
        sr.sprite = MakeSolid(col); sr.color = col;
        return go;
    }

    void MakeCircleObj(string name, Transform parent, Vector2 pos, float radius,
                       Color col, int order)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position   = pos;
        go.transform.localScale = new Vector3(radius * 2f, radius * 2f, 1f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = order;
        sr.sprite = MakeCircleSprite(col, 32);
    }

    Sprite MakeSolid(Color c)
    {
        var t = new Texture2D(1, 1);
        t.SetPixel(0, 0, c); t.Apply();
        return Sprite.Create(t, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
    }

    Sprite MakeCircleSprite(Color c, int res)
    {
        var t = new Texture2D(res, res);
        float r = res * 0.5f;
        for (int py = 0; py < res; py++)
        for (int px = 0; px < res; px++)
        {
            float dx = px - r, dy = py - r;
            float a  = Mathf.Clamp01(r - 1.5f - Mathf.Sqrt(dx * dx + dy * dy));
            t.SetPixel(px, py, new Color(c.r, c.g, c.b, a));
        }
        t.Apply();
        return Sprite.Create(t, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f), res);
    }

    Font Font() => Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
}

// ══════════════════════════════════════════════════════════════════════════
public class DockNPC
{
    public string   Name;
    public Vector2  WorldPos;
    public Color    Color;
    string[]        lines;
    int             idx;

    public string CurrentLine => idx < lines.Length ? lines[idx] : "";
    public string Line        => CurrentLine;
    public bool   Finished    => idx >= lines.Length - 1;

    public DockNPC(string name, Vector2 pos, Color col, string[] dialogue)
    {
        Name = name; WorldPos = pos; Color = col; lines = dialogue;
    }

    public void Advance()   => idx = Mathf.Min(idx + 1, lines.Length);
    public void ResetLine() => idx = 0;
}

// ══════════════════════════════════════════════════════════════════════════
public class DockPlayerWalk : MonoBehaviour
{
    Rigidbody2D rb;
    const float SPEED = 5.5f;
    void Awake() => rb = GetComponent<Rigidbody2D>();
    void FixedUpdate()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        rb.linearVelocity = new Vector2(x, y).normalized * SPEED;
    }
}

// ══════════════════════════════════════════════════════════════════════════
public class DockCameraFollow : MonoBehaviour
{
    public Transform Target;
    public Vector2   BoundsMin, BoundsMax;
    const float SMOOTH = 5f;
    void LateUpdate()
    {
        if (!Target) return;
        float cx = Mathf.Clamp(Target.position.x, BoundsMin.x, BoundsMax.x);
        float cy = Mathf.Clamp(Target.position.y, BoundsMin.y, BoundsMax.y);
        transform.position = Vector3.Lerp(transform.position,
                                          new Vector3(cx, cy, -10f),
                                          SMOOTH * Time.deltaTime);
    }
}
