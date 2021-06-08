using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PoolControl : MonoBehaviour
{
    public static PoolControl PoolController;

    static List<GameObject> PoolObjects = new List<GameObject>();
    public GridItem[] Items;

    private void Awake()
    {
        //проверка на singleton
        if (PoolController == null)
            PoolController = this;
        if(PoolController != null)
        {
            if(PoolController != this)
            {
                Destroy(PoolController.gameObject);
                PoolController = this;
            }
        }
    }

    /// <summary>
    /// Инициализация пула. Выполнять желательно в асинхронном режиме.
    /// </summary>
    /// <param name="poolSize">Размер пула в элементах</param>
    public List<GameObject> InitializePool(int poolSize)
    {
        PoolObjects = new List<GameObject>();
        //Смотрим на вложенные ссылки Items, создаем временные ссылки на префабы, считываем какой они имеют цвет
        GameObject red = null;
        GameObject blue = null;
        GameObject green = null;
        GameObject orange = null;

        foreach(var item in Items)
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

        for (int i = 0; i < poolSize; i++)
        {
            //Решаем какой из шаров будем инициализировать
            //Мы знаем, что всего 4 фактора есть при решении, какой из шаров будет выставлен
            //Данный поход не самый оптимальный при маштабировании типов шаров, но достаточен для данного примера
            float redFactor = Random.Range(0f, 100f) * LevelsData.RequestLevel(LevelsData.CurrentGameLevel).RedFactor;
            float blueFactor = Random.Range(0f, 100f) * LevelsData.RequestLevel(LevelsData.CurrentGameLevel).BlueFactor;
            float orangeFactor = Random.Range(0f, 100f) * LevelsData.RequestLevel(LevelsData.CurrentGameLevel).OrangeFactor;
            float greenFactor = Random.Range(0f, 100f) * LevelsData.RequestLevel(LevelsData.CurrentGameLevel).GreenFactor;

            float winner = Mathf.Max(redFactor, blueFactor, orangeFactor, greenFactor); //находим самый сильный вес среди факторов

            //Создаем геймобьекты в соответствии с самым сильным весом 
            if (winner == redFactor)
            {
                PoolObjects.Add(Instantiate(red, this.transform));
            }
            if (winner == blueFactor)
            {
                PoolObjects.Add(Instantiate(blue, this.transform));
            }
            if (winner == greenFactor)
            {
                PoolObjects.Add(Instantiate(green, this.transform));
            }
            if (winner == orangeFactor)
            {
                PoolObjects.Add(Instantiate(orange, this.transform));
            }
        }

        return PoolObjects;
    }

    /// <summary>
    /// Полная переинициализация пула, с его очисткой
    /// </summary>
    /// <param name="poolSize">Размер пула в активных элементах</param>
    public void ReInitializePool(int poolSize)
    {
        foreach(var obj in PoolObjects) 
        {
            Destroy(obj); // удаляем каждый объект в пуле
        }

        InitializePool(poolSize); //создаем новый пул
    }
}
