using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelsButtonSelectLevel : MonoBehaviour
{
    /// <summary>
    /// Положение в Levels выбранного уровня
    /// </summary>
    int attachedLevel;
    public Text levelText;

    public delegate void SelectLevel(int levelId);
    public event SelectLevel onLevelSelect;

    void Start()
    {
        //При создании создаем отклик на событие нажатия кнопки
        this.gameObject.GetComponent<Button>().onClick.AddListener(() => onClick());
    }

    //событие при нажатии кнопки
    void onClick()
    {
        //Вызываем событие о выбранном уровне, отсылаем в никуда конечно по сути, в надежде что кто-то его получит...
        onLevelSelect?.Invoke(attachedLevel);
    }

    /// <summary>
    /// Задать уровень, который будет выбирать данная кнопка
    /// </summary>
    public void SelectButtonLevel(int levelID, string levelName)
    {
        attachedLevel = levelID;
        levelText.text = levelName;
    }
}
