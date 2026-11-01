using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

// Başlık ekranı, talimatlar, ayarlar ve Play/Quit butonlarını yönetir.
// Oyun, Play'e basılana kadar durur: PlayerController ve EnenmyController kapalı kalır.
public class MenuManager : MonoBehaviour
{
    [Header("Game")]
    public PlayerController player;
    public GameObject[] hudObjects;

    [Header("Panels")]
    public GameObject titlePanel;
    public GameObject instructionsPanel;
    public GameObject settingsPanel;

    [Header("Settings")]
    public Slider volumeSlider;
    public Toggle musicToggle;
    public Button[] speedButtons;          // Slow, Medium, Fast
    public float[] speedMultipliers = { 0.6f, 1f, 2f };
    public Color speedNormalColor = new Color(0.10f, 0.62f, 0.70f);
    public Color speedSelectedColor = new Color(0.93f, 0.33f, 0.27f);

    // Sahnedeki tüm düşman script'leri ve NavMeshAgent'ları (birden fazla olabilir).
    private EnenmyController[] enemies;
    private bool[] enemyWasEnabled;
    private NavMeshAgent[] enemyAgents;
    private float[] baseEnemySpeeds;

    void Awake()
    {
        if (player != null)
        {
            player.enabled = false;
            if (player.winTextObject != null) player.winTextObject.SetActive(false);
            if (player.loseTextObject != null) player.loseTextObject.SetActive(false);
        }
        enemies = FindObjectsByType<EnenmyController>(FindObjectsInactive.Include);
        enemyWasEnabled = new bool[enemies.Length];
        for (int i = 0; i < enemies.Length; i++)
        {
            enemyWasEnabled[i] = enemies[i].enabled;   // sadece başta açık olanları Play'de tekrar açacağız
            enemies[i].enabled = false;
        }

        enemyAgents = FindObjectsByType<NavMeshAgent>(FindObjectsInactive.Include);
        baseEnemySpeeds = new float[enemyAgents.Length];
        for (int i = 0; i < enemyAgents.Length; i++) baseEnemySpeeds[i] = enemyAgents[i].speed;
        SetHud(false);
        ShowTitle();
        RefreshSpeedButtons(1);
    }

    void Start()
    {
        if (AudioManager.Instance != null)
        {
            if (volumeSlider != null) volumeSlider.SetValueWithoutNotify(AudioManager.Instance.musicVolume);
        }
    }

    void SetHud(bool on)
    {
        if (hudObjects == null) return;
        foreach (GameObject g in hudObjects) if (g != null) g.SetActive(on);
    }

    void ShowOnly(GameObject panel)
    {
        if (titlePanel != null) titlePanel.SetActive(panel == titlePanel);
        if (instructionsPanel != null) instructionsPanel.SetActive(panel == instructionsPanel);
        if (settingsPanel != null) settingsPanel.SetActive(panel == settingsPanel);
    }

    public void ShowTitle() { ShowOnly(titlePanel); }
    public void ShowInstructions() { ShowOnly(instructionsPanel); }
    public void ShowSettings() { ShowOnly(settingsPanel); }

    // Play: başlık/talimat/ayar panellerini kapatır ve oyunu başlatır.
    public void Play()
    {
        ShowOnly(null);
        SetHud(true);
        if (player != null) player.enabled = true;
        for (int i = 0; i < enemies.Length; i++)
            if (enemyWasEnabled[i]) enemies[i].enabled = true;
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ---- Settings ----

    public void SetMusicVolume(float volume)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.SetMusicVolume(volume);
    }

    public void SetMusicEnabled(bool on)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.SetMusicEnabled(on);
    }

    public void SetEnemySlow()   { SetEnemySpeed(0); }
    public void SetEnemyMedium() { SetEnemySpeed(1); }
    public void SetEnemyFast()   { SetEnemySpeed(2); }

    void SetEnemySpeed(int index)
    {
        for (int i = 0; i < enemyAgents.Length; i++)
            enemyAgents[i].speed = baseEnemySpeeds[i] * speedMultipliers[index];
        RefreshSpeedButtons(index);
    }

    void RefreshSpeedButtons(int selected)
    {
        if (speedButtons == null) return;
        for (int i = 0; i < speedButtons.Length; i++)
        {
            if (speedButtons[i] == null) continue;
            ColorBlock c = speedButtons[i].colors;
            c.normalColor = (i == selected) ? speedSelectedColor : speedNormalColor;
            c.selectedColor = c.normalColor;
            speedButtons[i].colors = c;
        }
    }
}
