using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenuController : MonoBehaviour
{
    private VisualElement root;
    private VisualElement mainPanel;
    private VisualElement levelsPanel;

    // OnEnable instead of Start so buttons re-register if the object gets disabled and re-enabled
    private void OnEnable()
    {
        root = GetComponent<UIDocument>().rootVisualElement;

        mainPanel = root.Q("main-panel");
        levelsPanel = root.Q("levels-panel");

        root.Q<Button>("play-button").clicked    += () => LoadLevel("Level 1");
        root.Q<Button>("levels-button").clicked  += ShowLevels;
        root.Q<Button>("quit-button").clicked    += OnQuit;
        root.Q<Button>("level-1-button").clicked += () => LoadLevel("Level 1");
        root.Q<Button>("level-2-button").clicked += () => LoadLevel("Level 2");
        root.Q<Button>("level-3-button").clicked += () => LoadLevel("Level 3");
        root.Q<Button>("back-button").clicked    += ShowMain;
    }

    // swap panels by hiding one and showing the other
    private void ShowLevels()
    {
        mainPanel.style.display = DisplayStyle.None;
        levelsPanel.style.display = DisplayStyle.Flex;
    }

    private void ShowMain()
    {
        levelsPanel.style.display = DisplayStyle.None;
        mainPanel.style.display = DisplayStyle.Flex;
    }

    private void LoadLevel(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    private void OnQuit()
    {
        Application.Quit();
    }
}
