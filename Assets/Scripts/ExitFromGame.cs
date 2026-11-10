using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public void QuitGame()
    {
        Debug.Log("Игра закрывается...");
        Application.Quit();
    }
}
