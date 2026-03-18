using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private PlayerController player;

    private VisualElement detectedPanel;
    private VisualElement winPanel;
    private VisualElement chargeDots;
    private Label crouchLabel;
    private Label alertLabel;

    private List<VisualElement> dots = new List<VisualElement>();
    private List<EnemyController> enemies = new List<EnemyController>();

    public bool GameOver;
    private bool won; // separate from GameOver so we know to advance to next level vs restart

    // destroy if theres already one
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        VisualElement rootElement = uiDocument.rootVisualElement;

        detectedPanel = rootElement.Q("detected-panel");
        winPanel = rootElement.Q("win-panel");
        chargeDots = rootElement.Q("charges-dots");
        crouchLabel = rootElement.Q<Label>("crouch-label");
        alertLabel = rootElement.Q<Label>("alert-label");

        if (player != null)
        {
            BuildChargeDots(player.maxCharges);
        }
    }

    private void Update()
    {
        if (won && Input.GetKeyDown(KeyCode.R))
        {
            Time.timeScale = 1f;
            int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
            if (nextIndex >= SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(0); // Back to main menu after final level
            }
            else
            {
                SceneManager.LoadScene(nextIndex);
            }
            return;
        }

        if (!GameOver)
        {
            UpdateHUD();
        }
    }

    // builds the charge dots ui based on how many charges the player has
    private void BuildChargeDots(int max)
    {
        chargeDots.Clear();
        dots.Clear();
        for (int dotIndex = 0; dotIndex < max; dotIndex++)
        {
            VisualElement dot = new VisualElement();
            dot.AddToClassList("dot");
            dot.AddToClassList("dot-active");
            chargeDots.Add(dot);
            dots.Add(dot); // keep reference so we can toggle active/empty in UpdateHUD
        }
    }

    private void UpdateHUD()
    {
        if (player == null)
        {
            return;
        }

        int charges = player.charges;
        for (int dotIndex = 0; dotIndex < dots.Count; dotIndex++)
        {
            bool active = dotIndex < charges;
            if (active)
            {
                dots[dotIndex].RemoveFromClassList("dot-empty");
                dots[dotIndex].AddToClassList("dot-active");
            }
            else
            {
                dots[dotIndex].RemoveFromClassList("dot-active");
                dots[dotIndex].AddToClassList("dot-empty");
            }
        }

        if (player.IsCrouching)
        {
            crouchLabel.style.display = DisplayStyle.Flex;
        }
        else
        {
            crouchLabel.style.display = DisplayStyle.None;
        }

        // check if any guard noticed something
        bool anyAlert = false;
        foreach (EnemyController enemy in enemies)
        {
            if (enemy != null && enemy.detectionMeter > 0.28f)
            {
                anyAlert = true;
                break;
            }
        }
        if (anyAlert)
        {
            alertLabel.style.display = DisplayStyle.Flex;
        }
        else
        {
            alertLabel.style.display = DisplayStyle.None;
        }
    }

    // enemies call this themselves in their Start so the manager knows about them
    public void RegisterEnemy(EnemyController enemy)
    {
        if (!enemies.Contains(enemy))
        {
            enemies.Add(enemy);
        }
    }

    public void PlayerDetected()
    {
        if (GameOver)
        {
            return;
        }
        GameOver = true;
        if (detectedPanel != null)
        {
            detectedPanel.style.display = DisplayStyle.Flex;
        }
        StartCoroutine(RestartAfterDelay(2f));
    }

    public void PlayerWon()
    {
        if (GameOver)
        {
            return;
        }
        GameOver = true;
        won = true;
        Time.timeScale = 0f; // freeze the game on win, R will unfreeze and load next level
        if (winPanel != null)
        {
            winPanel.style.display = DisplayStyle.Flex;
        }
    }

    // realtime so this still works when timescale is 0
    private IEnumerator RestartAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
