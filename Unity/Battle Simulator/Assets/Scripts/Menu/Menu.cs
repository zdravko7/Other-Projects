using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Menu : MonoBehaviour
{

    public Canvas quitMenu;
    public Button play;
    public Button exit;

    // Use this for initialization
    void Start()
    {
        quitMenu = quitMenu.GetComponent<Canvas>();
        play = play.GetComponent<Button>();
        exit = exit.GetComponent<Button>();
        quitMenu.enabled = false;
    }

    public void ExitPress()
    {
        quitMenu.enabled = true;
        play.enabled = false;
        exit.enabled = false;
    }

    public void NoPress()
    {
        quitMenu.enabled = false;
        play.enabled = true;
        exit.enabled = true;
    }

    public void StartLevel()
    {
        Application.LoadLevel(1);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    // Update is called once per frame
    void Update()
    {


    }
}
