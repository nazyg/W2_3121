using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Menü: Lab/Setup VFX
// Parçacık efektlerini (pickup, patlama, kazanma konfetisi, toz izi) kodla üretir,
// prefab olarak kaydeder ve sahnedeki Player'a bağlar.
public static class VFXSetup
{
    const string Dir = "Assets/VFX";

    [MenuItem("Lab/Setup VFX")]
    public static void Setup()
    {
        if (!AssetDatabase.IsValidFolder(Dir)) AssetDatabase.CreateFolder("Assets", "VFX");

        Texture2D soft = CreateTexture("SoftCircle", true);
        Texture2D square = CreateTexture("Square", false);

        Material additive = CreateMaterial("VFX_Additive", soft, true);
        Material alpha = CreateMaterial("VFX_Alpha", soft, false);
        Material confettiMat = CreateMaterial("VFX_Confetti", square, false);

        GameObject pickup = BuildPickup(additive);
        GameObject explosion = BuildExplosion(additive, alpha);
        GameObject win = BuildWin(additive, confettiMat);
        GameObject trail = BuildTrail(alpha);

        PlayerController pc = Object.FindFirstObjectByType<PlayerController>();
        if (pc == null)
        {
            EditorUtility.DisplayDialog("VFX", "Sahnede PlayerController bulunamadı. Önce SampleScene'i aç.", "OK");
            return;
        }

        Undo.RecordObject(pc, "VFX Setup");
        pc.pickupEffect = pickup;
        pc.explosionEffect = explosion;
        pc.winEffect = win;
        EditorUtility.SetDirty(pc);

        PlayerTrail existing = Object.FindFirstObjectByType<PlayerTrail>();
        if (existing == null)
        {
            GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(trail);
            Undo.RegisterCreatedObjectUndo(inst, "VFX Setup");
            existing = inst.GetComponent<PlayerTrail>();
        }
        existing.target = pc.GetComponent<Rigidbody>();
        EditorUtility.SetDirty(existing);

        EditorSceneManager.MarkSceneDirty(pc.gameObject.scene);
        EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();

        Debug.Log("VFX kurulumu tamam: pickup, explosion, win confetti ve player trail bağlandı.");
        EditorUtility.DisplayDialog("VFX", "VFX kurulumu tamam! Play'e bas.", "OK");
    }

    // ---------- Texture & Material ----------

    static Texture2D CreateTexture(string name, bool round)
    {
        string path = Dir + "/" + name + ".png";
        if (!File.Exists(path))
        {
            int size = round ? 64 : 8;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float a = 1f;
                    if (round)
                    {
                        float dx = (x + 0.5f) / size * 2f - 1f;
                        float dy = (y + 0.5f) / size * 2f - 1f;
                        float d = Mathf.Sqrt(dx * dx + dy * dy);
                        a = Mathf.Clamp01(1f - d);
                        a = a * a * (3f - 2f * a);
                    }
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                }
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path);
            var imp = (TextureImporter)AssetImporter.GetAtPath(path);
            imp.alphaIsTransparency = true;
            imp.mipmapEnabled = false;
            imp.wrapMode = TextureWrapMode.Clamp;
            imp.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }

    static Material CreateMaterial(string name, Texture2D tex, bool additive)
    {
        string path = Dir + "/" + name + ".mat";
        Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m != null) return m;

        Shader sh = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (sh == null) sh = Shader.Find("Sprites/Default");
        m = new Material(sh);
        m.mainTexture = tex;
        if (m.HasProperty("_BaseMap")) m.SetTexture("_BaseMap", tex);
        if (m.HasProperty("_Surface")) m.SetFloat("_Surface", 1f);
        if (m.HasProperty("_Blend")) m.SetFloat("_Blend", additive ? 2f : 0f);
        if (m.HasProperty("_SrcBlend")) m.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        if (m.HasProperty("_DstBlend")) m.SetFloat("_DstBlend", additive ? (float)UnityEngine.Rendering.BlendMode.One : (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        if (m.HasProperty("_ZWrite")) m.SetFloat("_ZWrite", 0f);
        m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        m.SetOverrideTag("RenderType", "Transparent");
        m.renderQueue = 3000;
        AssetDatabase.CreateAsset(m, path);
        return m;
    }

    // ---------- Helpers ----------

    static ParticleSystem NewSystem(string name, Transform parent, Material mat)
    {
        var go = new GameObject(name);
        if (parent != null) go.transform.SetParent(parent, false);
        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        go.GetComponent<ParticleSystemRenderer>().sharedMaterial = mat;
        return ps;
    }

    static Gradient MakeGradient(Color a, Color b, float endAlpha = 0f)
    {
        var g = new Gradient();
        g.SetKeys(
            new[] { new GradientColorKey(a, 0f), new GradientColorKey(b, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(endAlpha, 1f) });
        return g;
    }

    static GameObject Save(GameObject go, string name)
    {
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, Dir + "/" + name + ".prefab");
        Object.DestroyImmediate(go);
        return prefab;
    }

    // ---------- Prefabs ----------

    // Pickup: altın renkli, hızlı bir kıvılcım patlaması + yukarı süzülen ışık.
    static GameObject BuildPickup(Material additive)
    {
        var root = new GameObject("VFX_Pickup");

        var sparks = NewSystem("Sparks", root.transform, additive);
        var m = sparks.main;
        m.duration = 0.6f; m.loop = false; m.playOnAwake = true;
        m.startLifetime = new ParticleSystem.MinMaxCurve(0.35f, 0.7f);
        m.startSpeed = new ParticleSystem.MinMaxCurve(2f, 5f);
        m.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.3f);
        m.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.85f, 0.2f), new Color(1f, 1f, 0.7f));
        m.simulationSpace = ParticleSystemSimulationSpace.World;
        var e = sparks.emission; e.rateOverTime = 0f;
        e.SetBursts(new[] { new ParticleSystem.Burst(0f, 24) });
        var s = sparks.shape; s.shapeType = ParticleSystemShapeType.Sphere; s.radius = 0.2f;
        var col = sparks.colorOverLifetime; col.enabled = true;
        col.color = MakeGradient(Color.white, new Color(1f, 0.5f, 0.1f));
        var sz = sparks.sizeOverLifetime; sz.enabled = true;
        sz.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));

        var glow = NewSystem("Glow", root.transform, additive);
        var gm = glow.main;
        gm.duration = 0.5f; gm.loop = false; gm.playOnAwake = true;
        gm.startLifetime = 0.5f;
        gm.startSpeed = 0f;
        gm.startSize = 1.2f;
        gm.startColor = new Color(1f, 0.9f, 0.4f, 0.6f);
        gm.simulationSpace = ParticleSystemSimulationSpace.World;
        var ge = glow.emission; ge.rateOverTime = 0f;
        ge.SetBursts(new[] { new ParticleSystem.Burst(0f, 1) });
        var gs = glow.shape; gs.enabled = false;
        var gc = glow.colorOverLifetime; gc.enabled = true;
        gc.color = MakeGradient(Color.white, Color.white);
        var gsz = glow.sizeOverLifetime; gsz.enabled = true;
        gsz.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 0.3f, 1f, 1.5f));

        return Save(root, "VFX_Pickup");
    }

    // Patlama: ateş + kıvılcım + duman.
    static GameObject BuildExplosion(Material additive, Material alpha)
    {
        var root = new GameObject("VFX_Explosion");

        var fire = NewSystem("Fire", root.transform, additive);
        var m = fire.main;
        m.duration = 1f; m.loop = false; m.playOnAwake = true;
        m.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.9f);
        m.startSpeed = new ParticleSystem.MinMaxCurve(3f, 9f);
        m.startSize = new ParticleSystem.MinMaxCurve(0.4f, 1.0f);
        m.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.6f, 0.1f), new Color(1f, 0.2f, 0.05f));
        m.simulationSpace = ParticleSystemSimulationSpace.World;
        var e = fire.emission; e.rateOverTime = 0f;
        e.SetBursts(new[] { new ParticleSystem.Burst(0f, 40) });
        var s = fire.shape; s.shapeType = ParticleSystemShapeType.Sphere; s.radius = 0.3f;
        var col = fire.colorOverLifetime; col.enabled = true;
        col.color = MakeGradient(new Color(1f, 0.9f, 0.5f), new Color(0.8f, 0.1f, 0f));
        var sz = fire.sizeOverLifetime; sz.enabled = true;
        sz.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0.2f));

        var sparks = NewSystem("Sparks", root.transform, additive);
        var sm = sparks.main;
        sm.duration = 1f; sm.loop = false; sm.playOnAwake = true;
        sm.startLifetime = new ParticleSystem.MinMaxCurve(0.6f, 1.2f);
        sm.startSpeed = new ParticleSystem.MinMaxCurve(5f, 12f);
        sm.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
        sm.startColor = new Color(1f, 0.85f, 0.4f);
        sm.gravityModifier = 1.2f;
        sm.simulationSpace = ParticleSystemSimulationSpace.World;
        var se = sparks.emission; se.rateOverTime = 0f;
        se.SetBursts(new[] { new ParticleSystem.Burst(0f, 40) });
        var ss = sparks.shape; ss.shapeType = ParticleSystemShapeType.Sphere; ss.radius = 0.2f;
        var sc = sparks.colorOverLifetime; sc.enabled = true;
        sc.color = MakeGradient(Color.white, new Color(1f, 0.4f, 0f));

        var smoke = NewSystem("Smoke", root.transform, alpha);
        var mm = smoke.main;
        mm.duration = 1f; mm.loop = false; mm.playOnAwake = true;
        mm.startLifetime = new ParticleSystem.MinMaxCurve(1.2f, 2.2f);
        mm.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 2f);
        mm.startSize = new ParticleSystem.MinMaxCurve(1f, 2.2f);
        mm.startColor = new Color(0.25f, 0.25f, 0.25f, 0.7f);
        mm.gravityModifier = -0.05f;
        mm.simulationSpace = ParticleSystemSimulationSpace.World;
        var me = smoke.emission; me.rateOverTime = 0f;
        me.SetBursts(new[] { new ParticleSystem.Burst(0.05f, 18) });
        var ms = smoke.shape; ms.shapeType = ParticleSystemShapeType.Sphere; ms.radius = 0.4f;
        var mc = smoke.colorOverLifetime; mc.enabled = true;
        mc.color = MakeGradient(Color.white, Color.white);
        var msz = smoke.sizeOverLifetime; msz.enabled = true;
        msz.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 0.5f, 1f, 1.5f));

        return Save(root, "VFX_Explosion");
    }

    // Kazanma: yukarı fışkıran renkli konfeti + altın parıltı.
    static GameObject BuildWin(Material additive, Material confettiMat)
    {
        var root = new GameObject("VFX_WinConfetti");

        var confetti = NewSystem("Confetti", root.transform, confettiMat);
        confetti.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
        var m = confetti.main;
        m.duration = 1.5f; m.loop = false; m.playOnAwake = true;
        m.startLifetime = new ParticleSystem.MinMaxCurve(2.5f, 3.5f);
        m.startSpeed = new ParticleSystem.MinMaxCurve(6f, 11f);
        m.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.25f);
        m.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        m.gravityModifier = 1.2f;
        m.simulationSpace = ParticleSystemSimulationSpace.World;
        var g = new Gradient();
        g.SetKeys(
            new[] {
                new GradientColorKey(new Color(1f, 0.25f, 0.3f), 0f),
                new GradientColorKey(new Color(1f, 0.85f, 0.2f), 0.25f),
                new GradientColorKey(new Color(0.3f, 0.9f, 0.4f), 0.5f),
                new GradientColorKey(new Color(0.3f, 0.6f, 1f), 0.75f),
                new GradientColorKey(new Color(0.8f, 0.4f, 1f), 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
        m.startColor = new ParticleSystem.MinMaxGradient(g) { mode = ParticleSystemGradientMode.RandomColor };
        var e = confetti.emission; e.rateOverTime = 0f;
        e.SetBursts(new[] { new ParticleSystem.Burst(0f, 120), new ParticleSystem.Burst(0.35f, 80) });
        var s = confetti.shape; s.shapeType = ParticleSystemShapeType.Cone; s.angle = 35f; s.radius = 0.4f;
        var rot = confetti.rotationOverLifetime; rot.enabled = true;
        rot.z = new ParticleSystem.MinMaxCurve(-4f, 4f);
        var col = confetti.colorOverLifetime; col.enabled = true;
        var fade = new Gradient();
        fade.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.75f), new GradientAlphaKey(0f, 1f) });
        col.color = fade;

        var sparkle = NewSystem("Sparkle", root.transform, additive);
        var sm = sparkle.main;
        sm.duration = 1.5f; sm.loop = false; sm.playOnAwake = true;
        sm.startLifetime = new ParticleSystem.MinMaxCurve(0.6f, 1.2f);
        sm.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 2f);
        sm.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.4f);
        sm.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.9f, 0.4f), new Color(1f, 1f, 1f));
        sm.simulationSpace = ParticleSystemSimulationSpace.World;
        var se = sparkle.emission; se.rateOverTime = 40f;
        var ss = sparkle.shape; ss.shapeType = ParticleSystemShapeType.Sphere; ss.radius = 1.8f;
        var sc = sparkle.colorOverLifetime; sc.enabled = true;
        sc.color = MakeGradient(Color.white, new Color(1f, 0.7f, 0.1f));
        var ssz = sparkle.sizeOverLifetime; ssz.enabled = true;
        ssz.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));

        return Save(root, "VFX_WinConfetti");
    }

    // Toz izi: PlayerTrail script'i emisyonu hıza ve yöne göre ayarlar.
    static GameObject BuildTrail(Material alpha)
    {
        var go = new GameObject("PlayerTrail");
        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        go.GetComponent<ParticleSystemRenderer>().sharedMaterial = alpha;

        var m = ps.main;
        m.loop = true; m.playOnAwake = true;
        m.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.8f);
        m.startSpeed = new ParticleSystem.MinMaxCurve(0.3f, 1.2f);
        m.startSize = new ParticleSystem.MinMaxCurve(0.2f, 0.45f);
        m.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        m.startColor = new ParticleSystem.MinMaxGradient(new Color(0.85f, 0.75f, 0.6f, 0.55f), new Color(0.7f, 0.7f, 0.7f, 0.45f));
        m.gravityModifier = -0.03f;
        m.simulationSpace = ParticleSystemSimulationSpace.World;
        m.maxParticles = 300;
        var e = ps.emission; e.rateOverTime = 0f;
        var s = ps.shape; s.shapeType = ParticleSystemShapeType.Cone; s.angle = 14f; s.radius = 0.12f;
        var col = ps.colorOverLifetime; col.enabled = true;
        col.color = MakeGradient(Color.white, Color.white);
        var sz = ps.sizeOverLifetime; sz.enabled = true;
        sz.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 0.4f, 1f, 1.4f));

        go.AddComponent<PlayerTrail>();
        ps.Play();
        return Save(go, "VFX_PlayerTrail");
    }
}
