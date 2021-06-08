using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Globalization;

public class LevelsData
{
    /// <summary>
    /// Список уровней
    /// </summary>
    private static List<LevelData> Levels = new List<LevelData>();

    /// <summary>
    /// Выбранный игровой уровень в данный момент. 0 равен первому уровню
    /// </summary>
    public static int CurrentGameLevel { get; private set; }


    /// <summary>
    /// Повысить счетчик уровня
    /// </summary>
    public static void ChangeCurrentGameLevel(uint level)
    {
        CurrentGameLevel = (int)level;
        if (CurrentGameLevel >= Levels.Count) //in in increment we get out levels
        {
            CurrentGameLevel = Levels.Count - 1;
        }
    }

    /// <summary>
    /// Загрузка с текстового файла данных об уровнях
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
    public static void LoadLevelsData() 
    {
#if UNITY_EDITOR||UNITY_STANDALONE
        string levelFull = File.ReadAllText(Application.streamingAssetsPath + "/Levels/Levels.txt"); //читаем файл с данными об уровнях
#endif  
        string[] levels = levelFull.Split('/');

        foreach (string level in levels)  //Сериализируем всю информацию об уровнях в соответствии с LevelData
        {
            //Определяем данные заранее для передачи в новый класс / данные меняются на каждой итерации цикла ниже
            float rF = 0f;
            float gF = 0f;
            float bF = 0f;
            float oF = 0f;
            List<byte> balls = new List<byte>();

            //factor part
            int fromF = level.IndexOf('{');
            int toF = level.IndexOf('}');
            string factorFull = level.Substring(fromF + 1, toF - fromF - 1);
            string[] factors = factorFull.Split(',');

            //итерируем каждый фактор
            foreach(string factor in factors) 
            {
                string[] f = factor.Split(':'); //0 - цвет / 1 - float 

                /* Не самый лучший вариант реализации, можно более удобным способом реализовать - 
                   Создать enum, и затем насильно парсировать эту букву со значением перечисления
                   данный вариант также тяжело маштабировать, но так быстрее все можно сделать на данном этапе
                */
                switch (f[0]) 
                {
                    case "R":
                        rF = float.Parse(f[1], CultureInfo.InvariantCulture);
                        break;
                    case "B":
                        bF = float.Parse(f[1], CultureInfo.InvariantCulture);
                        break;
                    case "G":
                        gF = float.Parse(f[1], CultureInfo.InvariantCulture);
                        break;
                    case "O":
                        oF = float.Parse(f[1], CultureInfo.InvariantCulture);
                        break;
                    default:
                        throw new Exception(f[0] + " " + f[1]);
                }
            }

            //player balls part
            int fromB = level.IndexOf('[');
            int toB = level.IndexOf(']');
            string ballsFull = level.Substring(fromB + 1, toB - fromB - 1);
            string[] ballsStr = ballsFull.Split(',');

            foreach(string ball in ballsStr)
            {
                switch (ball)
                {
                    case "R":
                        balls.Add((byte)BallsColor.Red);
                        break;
                    case "B":
                        balls.Add((byte)BallsColor.Blue);
                        break;
                    case "G":
                        balls.Add((byte)BallsColor.Green);
                        break;
                    case "O":
                        balls.Add((byte)BallsColor.Orange);
                        break;
                    default:
                        throw new Exception("Levels data nonformat changed, check file");
                }
            }
            //level name part

            int toL = level.IndexOf('{');
            string levelName = level.Substring(0, toL );

            //Сериализуем все это дело в класс LevelData и добавляем в общий список
            Levels.Add(new LevelData(rF,gF,bF,oF, balls, levelName));
        }
    }

    public static List<LevelData> RequestLevelsList()
    {
        return Levels;
    }

    public static LevelData RequestLevel(int levelID)
    {
        return Levels[levelID];
    }
}


/// <summary>
/// Только для чтения. Хранит данные об уровнях
/// </summary>
public class LevelData
{
    public string LevelName { get; private set; }
    public float RedFactor { get; private set; }
    public float GreenFactor { get; private set; }
    public float BlueFactor { get; private set; }
    public float OrangeFactor { get; private set; }

    /// <summary>
    /// Последовательность шаров у игрока
    /// </summary>
    List<byte> PlayerBalls = new List<byte>();

    /// <summary>
    /// Инициализация нового уровня в памяти
    /// </summary>
    /// <param name="redFactor"></param>
    /// <param name="greenFactor"></param>
    /// <param name="blueFactor"></param>
    /// <param name="orangeFactor"></param>
    /// <param name="balls">Массив byte, последовательность шаров у игрока на карте</param>
    public LevelData(float redFactor, float greenFactor, float blueFactor, float orangeFactor, List<byte> balls, string levelName)
    {
        RedFactor = redFactor;
        GreenFactor = greenFactor;
        BlueFactor = blueFactor;
        OrangeFactor = orangeFactor;
        LevelName = levelName;
        PlayerBalls = balls;
    }

    public List<byte> GetPlayerBalls()
    {
        return PlayerBalls;
    }
}

/// <summary>
/// Перечесление цвета шаров. Возвращает byte
/// </summary>
public enum BallsColor : byte
{
    Red,
    Green,
    Blue,
    Orange
}