using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public GameObject disclaimer;
    public GameObject mainMenu;
    public GameObject questionnairePopUp;

    private void Awake()
    {
        if (SceneManager.GetActiveScene().buildIndex != 0) return;
        disclaimer.SetActive(true);
        mainMenu.SetActive(false);
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }

    //main menu
    #region MainMenu
    public void QuitGame()
    {
        Application.Quit();
    }

    public void StartGame()
    {
        GameObject.Find("DontDestroyRandomProgression").GetComponent<ProgressionManager>().PickNextScene();
    }

    public void DisclaimerAccepted()
    {
        disclaimer.SetActive(false);
        mainMenu.SetActive(true);
    }
    
    public void ShowQuestionnaire()
    {
        mainMenu.SetActive(false);
        questionnairePopUp.SetActive(true);
    }

    public void OpenQuestionnaire()
    {
        Application.OpenURL("https://forms.office.com/e/6Kqv4t13Xn");
    }
    #endregion
}
