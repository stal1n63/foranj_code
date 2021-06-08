using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;

public class GameControl : MonoBehaviour
{
    public static GameControl GameController;
    public GridItem[] balls;

    [SerializeField] private float BallRadius = 1.5f;

    GameObject currentPlayerBall;
    Queue<GameObject> incomingPlayerBalls;

    public delegate void LevelStatus();
    public event LevelStatus onLevelCompleted;
    public event LevelStatus onLevelFailed;

    private void Start()
    {   
        //проверка на singleton
        if (GameController == null)
            GameController = this;
        if (GameController != null)
        {
            if (GameController != this)
            {
                Destroy(GameController.gameObject);
                GameController = this;
            }
        }

        StartGameLevel(); //генерируем новую пачку игроков
    }

    void StartGameLevel()
    {
        //Очищаем сцену
        if (incomingPlayerBalls != null)
        {
            var a = incomingPlayerBalls.ToList();
            if (a.Count != 0)
            {
                foreach (var b in a)
                {
                    Destroy(b);
                }
            }
        }

        if (currentPlayerBall != null)
        {
            Destroy(currentPlayerBall);
        }

        incomingPlayerBalls = new Queue<GameObject>();

        //Смотрим на вложенные ссылки Items, создаем временные ссылки на префабы, считываем какой они имеют цвет
        GameObject red = null;
        GameObject blue = null;
        GameObject green = null;
        GameObject orange = null;

        foreach (var item in balls)
        {
            if (item.ItemColor == BallsColor.Red)
                red = item.ItemPrefab;
            if (item.ItemColor == BallsColor.Blue)
                blue = item.ItemPrefab;
            if (item.ItemColor == BallsColor.Green)
                green = item.ItemPrefab;
            if (item.ItemColor == BallsColor.Orange)
                orange = item.ItemPrefab;
        }

        //проверка на дурака
        if (red == null)
            throw new System.Exception("Red ball in pool controller not assigned");
        if (blue == null)
            throw new System.Exception("Blue ball in pool controller not assigned");
        if (green == null)
            throw new System.Exception("Green ball in pool controller not assigned");
        if (orange == null)
            throw new System.Exception("Orange ball in pool controller not assigned");

        foreach (var element in LevelsData.RequestLevel(LevelsData.CurrentGameLevel).GetPlayerBalls()) //Создаем игроков согласно нашему параметру уровня
        {
            switch (element)
            {
                case (byte)BallsColor.Red:
                    GameObject redSpawned = Instantiate(red, this.transform);
                    var comp = redSpawned.AddComponent<PlayerBallController>();
                    comp.isLocked = true;
                    comp.OurRadius = 0.4f;
                    comp.PullingRadius = BallRadius;
                    comp.color = BallsColor.Red;
                    redSpawned.SetActive(false);
                    incomingPlayerBalls.Enqueue(redSpawned);
                    break;
                case (byte)BallsColor.Blue:
                    GameObject blueSpawned = Instantiate(blue, this.transform);
                    var b = blueSpawned.AddComponent<PlayerBallController>();
                    b.OurRadius = 0.4f;
                    b.PullingRadius = BallRadius;
                    b.isLocked = true;
                    b.color = BallsColor.Blue;
                    blueSpawned.SetActive(false);
                    incomingPlayerBalls.Enqueue(blueSpawned);
                    break;
                case (byte)BallsColor.Green:
                    GameObject greenSpawned = Instantiate(green, this.transform);
                    var g = greenSpawned.AddComponent<PlayerBallController>();
                    g.OurRadius = 0.4f;
                    g.PullingRadius = BallRadius;
                    g.isLocked = true;
                    g.color = BallsColor.Green;
                    greenSpawned.SetActive(false);
                    incomingPlayerBalls.Enqueue(greenSpawned);
                    break;
                case (byte)BallsColor.Orange:
                    GameObject orangeSpawned = Instantiate(orange, this.transform);
                    var o = orangeSpawned.AddComponent<PlayerBallController>();
                    o.OurRadius = 0.4f;
                    o.PullingRadius = BallRadius;
                    o.isLocked = true;
                    o.color = BallsColor.Orange;
                    orangeSpawned.SetActive(false);
                    incomingPlayerBalls.Enqueue(orangeSpawned);
                    break;
            }
        }

        // задаем 1 по очереди шар
        NextPlayer();
    }
    
    public void NextPlayer()
    {
        if (incomingPlayerBalls.Count > 0)
        {
            GameUI.UI.OnNextBall(incomingPlayerBalls.Count);

            currentPlayerBall = incomingPlayerBalls.Dequeue();
            currentPlayerBall.GetComponent<PlayerBallController>().isLocked = false;
            currentPlayerBall.SetActive(true);
        }
        else
        {
            CompleteLevel(); 
            DirectionRenderer.DirectionRendererController.SetPositions(0);
        }
    }

    public void CheckForComplete()
    {
        if (GridControl.GridController.CheckForEndOfGamemode30())
        {
            LevelsData.ChangeCurrentGameLevel((uint)LevelsData.CurrentGameLevel + 1);
            onLevelCompleted?.Invoke();
        }
    }

    /// <summary>
    /// Complete level without a reloading map
    /// </summary>
    public void CompleteLevel()
    {
        //LevelsData.ChangeCurrentGameLevel((uint)LevelsData.CurrentGameLevel + 1);
        onLevelFailed?.Invoke();
        /*
        StartGameLevel();
        GridControl.GridController.GenerateGrid();
        */
    }
}
