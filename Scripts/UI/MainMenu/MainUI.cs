using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainUI : MonoBehaviour
{
    public GameObject MenuUI, LevelsUI, AboutUI, QuitUI; //link to up gameobjects

    [Space(5)]
    public GameObject levelUIElementPrefab; //link to prefab, used for button in UI
    public GameObject levelPanelUI; //link to parent for prevoius thing
    public Text levelSelectedText; //link to text, shows last selected level param

    List<LevelsButtonSelectLevel> selectLevelsDelegates = new List<LevelsButtonSelectLevel>();

    void Start()
    {
        // Делаем это на случай, если забыли
        MenuUI.SetActive(true);
        LevelsUI.SetActive(false);
        AboutUI.SetActive(false);
        QuitUI.SetActive(false);

        //index of level
        int index = 0;
        //Прочесываем количество уровней, создаем необходимые кнопки для их загрузки
        foreach(LevelData level in LevelsData.RequestLevelsList())
        {
            var button = GameObject.Instantiate(levelUIElementPrefab);
            button.transform.SetParent(levelPanelUI.transform);
            button.transform.localScale = Vector3.one; //непонятный баг с размерами, улетают с родных в 3.146464, хотя на префабе все ок

            //задаем созданной кнопке необходимые данные
            var btttn = button.GetComponent<LevelsButtonSelectLevel>();
            btttn.SelectButtonLevel(index, level.LevelName);

            //сохраняем в памяти ссылку на этот метод для отписки от него в будущем
            selectLevelsDelegates.Add(btttn);
            btttn.onLevelSelect += SelectLevel;

            //increment index of level; always should be last in enumeration
            index++;
        }

        SelectLevel(0); //По умолчанию выбираем 1 уровень
    }

    public void SelectLevel(int level)
    {
        LevelsData.ChangeCurrentGameLevel((uint)level);
        var r = LevelsData.RequestLevel(level);

        string allPlayerBalls = "";
        foreach(var i in r.GetPlayerBalls())
        {
            allPlayerBalls += (BallsColor)i + " ";
        }

        levelSelectedText.text = "Level name: " + r.LevelName + "\nR: " + r.RedFactor + " G: " + r.GreenFactor + " B: " + r.BlueFactor + " O: " + r.OrangeFactor + "\n" + allPlayerBalls;
    }

    //Идентичный по сути код, for onClick events
    public void OpenLevelsUI()
    {
        MenuUI.SetActive(false);
        LevelsUI.SetActive(true);
        AboutUI.SetActive(false);
        QuitUI.SetActive(false);

    }
    public void OpenAboutUI()
    {
        MenuUI.SetActive(false);
        LevelsUI.SetActive(false);
        AboutUI.SetActive(true);
        QuitUI.SetActive(false);
    }
    public void OpenQuitUI()
    {
        MenuUI.SetActive(false);
        LevelsUI.SetActive(false);
        AboutUI.SetActive(false);
        QuitUI.SetActive(true);
    }
    public void BackToMainUI()
    {
        MenuUI.SetActive(true);
        LevelsUI.SetActive(false);
        AboutUI.SetActive(false);
        QuitUI.SetActive(false);
    }
    
    public void OpenDeveloperPageLink()
    {
        Application.OpenURL("https://vk.com/stal1n63");
    }

    /// <summary>
    /// Загрузить игровую сцену и перейти на неё
    /// </summary>
    /// <param name="level">id уровня</param>
    public void LoadGameLevel()
    {
        UnsubscribeFromEvents(); //отписываемся от всех событий
        SceneManager.LoadSceneAsync("Game", LoadSceneMode.Single);
    }

    /// <summary>
    /// Выход из игры
    /// </summary>
    public void QuitFromGame()
    {
        UnsubscribeFromEvents();

        Application.Quit();
    }

    /// <summary>
    /// Отписка от всех событий
    /// </summary>
    void UnsubscribeFromEvents()
    {
        foreach(var _event in selectLevelsDelegates)
        {
            _event.onLevelSelect -= SelectLevel;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            BackToMainUI(); //при нажатии esc принудительный возврат в главное меню, работает из всех сцен
        }
    }
}
