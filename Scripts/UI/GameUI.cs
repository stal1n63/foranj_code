using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class GameUI : MonoBehaviour
{
    public Text ScoreText, NextLevelScoreText, nextlevelbttn, NextBallText; //link to score text gameobj component
    public GameObject NextLevelPanel;

    int localScore = 0;
    List<GridItemController> GridItemsControllerEvents = new List<GridItemController>();

    public static GameUI UI;

    public void Awake()
    {
        //проверка на singleton
        if (UI == null)
            UI = this;
        if (UI != null)
        {
            if (UI != this)
            {
                Destroy(UI.gameObject);
                UI = this;
            }
        }
    }

    async void Start()
    {
        OnUpdateScore(0); //init

        GridItemsControllerEvents = await GridControl.GridController.RequstGridControllersList(); //подписываемся на все события получения очков в игре
        foreach(var element in GridItemsControllerEvents)
        {
            element.addScore += OnUpdateScore;
        }

        GameControl.GameController.onLevelCompleted += OnLevelCompleted;
        GameControl.GameController.onLevelFailed += OnLevelFailed;
    }

    void OnUpdateScore(int score) 
    {
        localScore += score; //обновляем общие очки
        ScoreText.text = "Score: " + localScore;
    }

    void OnLevelCompleted()
    {
        UnsubscribeFromAllEvents();
        NextLevelScoreText.text = "Level completed!" + "\n" + "Your score: " + localScore;
        NextLevelPanel.SetActive(true);
    }
    void OnLevelFailed()
    {
        UnsubscribeFromAllEvents();
        NextLevelScoreText.text = "Level Failed..." + "\n" + "Your score: " + localScore;
        nextlevelbttn.text = "try again";
        NextLevelPanel.SetActive(true);
    }
    public void OnNextBall(int remain)
    {
        if (remain > 0)
        {
            NextBallText.text = "Remain balls: " + remain ;
        }
        else
        {
            NextBallText.text = "Remain balls: " + remain;
        }
    }

    private void OnDestroy()
    {
        UnsubscribeFromAllEvents();
    }

    public void BackToMainMenu()
    {
        UnsubscribeFromAllEvents(); //обязательно отписываемся от всех подписанных событий
        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(0, UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    public void LoadNextLevel()
    {
        UnsubscribeFromAllEvents();
        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Game", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    /// <summary>
    /// Отписываемся от всех событий
    /// </summary>
    public void UnsubscribeFromAllEvents()
    {
        foreach(var element in GridItemsControllerEvents)
        {
            element.addScore -= OnUpdateScore;
        }

        GameControl.GameController.onLevelFailed -= OnLevelFailed;
        GameControl.GameController.onLevelCompleted -= OnLevelCompleted;
    }
}
