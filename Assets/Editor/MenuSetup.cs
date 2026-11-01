using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using TMPro;

// Menü: Lab/Setup Menu UI
// Plaj temalı başlık ekranı, talimatlar, ayarlar ve Play/Quit butonlarını sahneye kurar.
public static class MenuSetup
{
    static Sprite box, bgSprite, knobSprite, checkSprite;

    static readonly Color Sand = new Color(0.99f, 0.91f, 0.70f, 0.96f);
    static readonly Color Sea = new Color(0.05f, 0.25f, 0.45f, 0.95f);
    static readonly Color Turquoise = new Color(0.10f, 0.62f, 0.70f);
    static readonly Color TurquoiseHi = new Color(0.25f, 0.80f, 0.85f);
    static readonly Color TurquoiseLo = new Color(0.06f, 0.42f, 0.50f);
    static readonly Color Coral = new Color(0.93f, 0.33f, 0.27f);
    static readonly Color DeepBlue = new Color(0.04f, 0.22f, 0.40f);

    [MenuItem("Lab/Setup Menu UI")]
    public static void Setup()
    {
        var pc = Object.FindFirstObjectByType<PlayerController>();
        var enemy = Object.FindFirstObjectByType<EnenmyController>();
        if (pc == null || enemy == null)
        {
            EditorUtility.DisplayDialog("Menu", "Sahnede PlayerController veya EnenmyController bulunamadı. Önce SampleScene'i aç.", "OK");
            return;
        }

        box = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        bgSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
        knobSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        checkSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Checkmark.psd");

        foreach (var n in new[] { "MenuCanvas", "MenuManager" })
        {
            var old = GameObject.Find(n);
            if (old != null) Undo.DestroyObjectImmediate(old);
        }

        if (Object.FindFirstObjectByType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            var module = es.AddComponent<InputSystemUIInputModule>();
            module.AssignDefaultActions();
            Undo.RegisterCreatedObjectUndo(es, "Menu Setup");
        }

        // Canvas
        var canvasGo = new GameObject("MenuCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Undo.RegisterCreatedObjectUndo(canvasGo, "Menu Setup");
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        var mgrGo = new GameObject("MenuManager");
        Undo.RegisterCreatedObjectUndo(mgrGo, "Menu Setup");
        var mgr = mgrGo.AddComponent<MenuManager>();

        // ---------------- Title panel ----------------
        var title = Panel("TitlePanel", canvasGo.transform);
        var titleCard = Card("TitleCard", title.transform, new Vector2(0, 170), new Vector2(980, 340), Sand, Turquoise);
        Label("Title", titleCard.transform, "BEACH BALL\nCHASE", 110, Coral, Vector2.zero, new Vector2(920, 300));
        var playBtn = MakeButton("PlayButton", title.transform, "PLAY", new Vector2(-230, -90), new Vector2(430, 90), 52);
        var settingsBtn = MakeButton("SettingsButton", title.transform, "SETTINGS", new Vector2(230, -90), new Vector2(430, 90), 52);
        var instrBtn = MakeButton("InstructionsButton", title.transform, "INSTRUCTIONS", new Vector2(-230, -200), new Vector2(430, 90), 52);
        var quitBtn = MakeButton("QuitButton", title.transform, "QUIT", new Vector2(230, -200), new Vector2(430, 90), 52);

        // ---------------- Instructions panel ----------------
        var instr = Panel("InstructionsPanel", canvasGo.transform);
        var instrCard = Card("InstructionsCard", instr.transform, new Vector2(0, 40), new Vector2(1100, 560), Sand, Turquoise);
        Label("Heading", instrCard.transform, "INSTRUCTIONS", 80, Coral, new Vector2(0, 210), new Vector2(1000, 100));
        var body = Label("Body", instrCard.transform,
            "- Collect all 8 pickups to win.\n" +
            "- Don't let the enemy catch you!\n" +
            "- Use W, A, S, D (or the arrow keys) to roll the ball.\n" +
            "- Avoid the walls and obstacles.",
            44, DeepBlue, new Vector2(0, 10), new Vector2(1000, 340), TextAlignmentOptions.Left, FontStyles.Bold);
        var instrBack = MakeButton("BackButton", instr.transform, "BACK", new Vector2(0, -330), new Vector2(340, 90), 52);

        // ---------------- Settings panel ----------------
        var settings = Panel("SettingsPanel", canvasGo.transform);
        var sCard = Card("SettingsCard", settings.transform, new Vector2(0, 0), new Vector2(1000, 760), Sea, Sand);
        Label("Heading", sCard.transform, "SETTINGS", 96, Color.white, new Vector2(0, 300), new Vector2(800, 120));
        var closeBtn = MakeButton("CloseButton", sCard.transform, "X", new Vector2(430, 310), new Vector2(90, 90), 54, Coral, new Color(1f, 0.5f, 0.45f), new Color(0.7f, 0.2f, 0.18f));

        var res = new DefaultControls.Resources
        {
            standard = box, background = bgSprite, knob = knobSprite, checkmark = checkSprite
        };

        // Music toggle
        var tgo = DefaultControls.CreateToggle(res);
        tgo.name = "MusicToggle";
        tgo.transform.SetParent(sCard.transform, false);
        Object.DestroyImmediate(tgo.transform.Find("Label").gameObject);
        var trt = tgo.GetComponent<RectTransform>();
        Center(trt, new Vector2(-300, 170), new Vector2(60, 60));
        var tbg = tgo.transform.Find("Background").GetComponent<RectTransform>();
        tbg.anchorMin = Vector2.zero; tbg.anchorMax = Vector2.one;
        tbg.offsetMin = Vector2.zero; tbg.offsetMax = Vector2.zero;
        var chk = tgo.transform.Find("Background/Checkmark").GetComponent<RectTransform>();
        chk.sizeDelta = new Vector2(44, 44);
        tgo.transform.Find("Background/Checkmark").GetComponent<Image>().color = DeepBlue;
        var toggle = tgo.GetComponent<Toggle>();
        toggle.isOn = true;
        Label("MusicLabel", sCard.transform, "Music", 56, Color.white, new Vector2(-70, 170), new Vector2(380, 70), TextAlignmentOptions.Left, FontStyles.Normal);

        // Volume slider
        Label("VolumeLabel", sCard.transform, "Volume", 56, Color.white, new Vector2(-330, 60), new Vector2(260, 70), TextAlignmentOptions.Left, FontStyles.Normal);
        var sgo = DefaultControls.CreateSlider(res);
        sgo.name = "VolumeSlider";
        sgo.transform.SetParent(sCard.transform, false);
        Center(sgo.GetComponent<RectTransform>(), new Vector2(110, 60), new Vector2(520, 44));
        sgo.transform.Find("Background").GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.35f);
        sgo.transform.Find("Fill Area/Fill").GetComponent<Image>().color = Sand;
        var handle = sgo.transform.Find("Handle Slide Area/Handle");
        handle.GetComponent<Image>().color = Coral;
        handle.GetComponent<RectTransform>().sizeDelta = new Vector2(46, 0);
        var slideArea = sgo.transform.Find("Handle Slide Area").GetComponent<RectTransform>();
        slideArea.offsetMin = new Vector2(23, 0); slideArea.offsetMax = new Vector2(-23, 0);
        var slider = sgo.GetComponent<Slider>();
        slider.minValue = 0f; slider.maxValue = 1f;
        var am = Object.FindFirstObjectByType<AudioManager>();
        slider.value = am != null ? am.musicVolume : 0.4f;

        // Enemy speed
        Label("EnemySpeedLabel", sCard.transform, "Enemy Speed", 56, Color.white, new Vector2(0, -80), new Vector2(700, 70), TextAlignmentOptions.Center, FontStyles.Normal);
        var slow = MakeButton("SlowButton", sCard.transform, "SLOW", new Vector2(-270, -190), new Vector2(250, 90), 46);
        var medium = MakeButton("MediumButton", sCard.transform, "MEDIUM", new Vector2(0, -190), new Vector2(250, 90), 46);
        var fast = MakeButton("FastButton", sCard.transform, "FAST", new Vector2(270, -190), new Vector2(250, 90), 46);
        var settingsBack = MakeButton("BackButton", sCard.transform, "BACK", new Vector2(0, -310), new Vector2(340, 80), 48);

        // ---------------- Manager references ----------------
        mgr.player = pc;
        mgr.hudObjects = pc.countText != null ? new[] { pc.countText.gameObject } : new GameObject[0];
        mgr.titlePanel = title;
        mgr.instructionsPanel = instr;
        mgr.settingsPanel = settings;
        mgr.volumeSlider = slider;
        mgr.musicToggle = toggle;
        mgr.speedButtons = new[] { slow, medium, fast };

        // ---------------- OnClick events ----------------
        Hook(playBtn, mgr.Play);
        Hook(settingsBtn, mgr.ShowSettings);
        Hook(instrBtn, mgr.ShowInstructions);
        Hook(quitBtn, mgr.Quit);
        Hook(instrBack, mgr.ShowTitle);
        Hook(closeBtn, mgr.ShowTitle);
        Hook(settingsBack, mgr.ShowTitle);
        Hook(slow, mgr.SetEnemySlow);
        Hook(medium, mgr.SetEnemyMedium);
        Hook(fast, mgr.SetEnemyFast);
        UnityEventTools.AddPersistentListener(slider.onValueChanged, mgr.SetMusicVolume);
        UnityEventTools.AddPersistentListener(toggle.onValueChanged, mgr.SetMusicEnabled);

        // Editörde de düzenlenebilir olsun diye başlangıç durumu: sadece başlık paneli açık.
        instr.SetActive(false);
        settings.SetActive(false);

        EditorUtility.SetDirty(mgr);
        EditorSceneManager.MarkSceneDirty(mgrGo.scene);
        EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();

        Debug.Log("Menu UI kurulumu tamam.");
        EditorUtility.DisplayDialog("Menu", "Menü kurulumu tamam! Play'e bas.", "OK");
    }

    // ---------- Helpers ----------

    static void Hook(Button b, UnityEngine.Events.UnityAction action)
    {
        UnityEventTools.AddPersistentListener(b.onClick, action);
    }

    static void Center(RectTransform rt, Vector2 pos, Vector2 size)
    {
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    static GameObject Panel(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        return go;
    }

    static GameObject Card(string name, Transform parent, Vector2 pos, Vector2 size, Color fill, Color border)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        Center(go.GetComponent<RectTransform>(), pos, size);
        var img = go.GetComponent<Image>();
        img.sprite = box; img.type = Image.Type.Sliced; img.color = fill;
        var o = go.AddComponent<Outline>();
        o.effectColor = border;
        o.effectDistance = new Vector2(6, -6);
        return go;
    }

    static TextMeshProUGUI Label(string name, Transform parent, string text, float size, Color color,
        Vector2 pos, Vector2 box, TextAlignmentOptions align = TextAlignmentOptions.Center, FontStyles style = FontStyles.Bold)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        Center(go.GetComponent<RectTransform>(), pos, box);
        var t = go.AddComponent<TextMeshProUGUI>();
        t.text = text;
        t.fontSize = size;
        t.color = color;
        t.alignment = align;
        t.fontStyle = style;
        t.raycastTarget = false;
        t.textWrappingMode = TextWrappingModes.Normal;
        return t;
    }

    static Button MakeButton(string name, Transform parent, string text, Vector2 pos, Vector2 size, float fontSize)
    {
        return MakeButton(name, parent, text, pos, size, fontSize, Turquoise, TurquoiseHi, TurquoiseLo);
    }

    static Button MakeButton(string name, Transform parent, string text, Vector2 pos, Vector2 size, float fontSize,
        Color normal, Color highlighted, Color pressed)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        Center(go.GetComponent<RectTransform>(), pos, size);
        var img = go.GetComponent<Image>();
        img.sprite = box; img.type = Image.Type.Sliced; img.color = Color.white;
        var o = go.AddComponent<Outline>();
        o.effectColor = Color.white;
        o.effectDistance = new Vector2(3, -3);

        var b = go.GetComponent<Button>();
        b.targetGraphic = img;
        var cb = ColorBlock.defaultColorBlock;
        cb.normalColor = normal;
        cb.highlightedColor = highlighted;
        cb.pressedColor = pressed;
        cb.selectedColor = normal;
        cb.colorMultiplier = 1f;
        b.colors = cb;

        Label("Text", go.transform, text, fontSize, Color.white, Vector2.zero, size);
        return b;
    }
}
