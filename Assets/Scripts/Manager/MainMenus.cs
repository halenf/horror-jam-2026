using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenus : MonoBehaviour
{

    public int GameSceneID;

    public void StartButton()
    {
        SceneManager.LoadScene(GameSceneID);
    }

    public void QuitButton()
    {
        Application.Quit();
    }
}
