using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Collections.Concurrent;

public class GridControl : MonoBehaviour
{
    public static GridControl GridController;

    public GameObject GridPlacer; //левый верхний угол
    [SerializeField] private int Collums = 6; //в ширину количество элементов
    [SerializeField] private int Lines = 4; //в высоту количество элементов
    [SerializeField] float otstupX = 2f;
    [SerializeField] float otstupY = 1.5f;
    GridItemController[,] Grid; //ссылка на наши элементы
    List<GameObject> GridElements = new List<GameObject>(); //копия списка элементов из PoolControl

    /// <summary>
    /// Список элементов вне сетки. Key = parent object; Value = link to GridItemController
    /// </summary>
    ConcurrentDictionary<GridItemController, KeyValuePair<int, int>> OutsudeBounds = new ConcurrentDictionary<GridItemController, KeyValuePair<int, int>>();
    int RemainingElemengs; //Количество оставшихся шаров

    void Awake()
    {
        //проверка на singleton
        if (GridController == null)
            GridController = this;
        if (GridController != null)
        {
            if (GridController != this)
            {
                Destroy(GridController.gameObject);
                GridController = this;
            }
        }

        GenerateGrid();
    }

    /// <summary>
    /// Задает сетку на основе заданного количества элементов
    /// </summary>
    public void GenerateGrid()
    {
        if (Grid != null)
        {
            for (int x = 0; x < Collums; x++) //Идем в линию
            {
                for (int y = 0; y < Lines; y++) //идем в высоту
                {
                    if(Grid[x,y] != null)
                    {
                        Destroy(Grid[x, y]);
                    }
                }
            }
        }

        if(OutsudeBounds != null & OutsudeBounds.Count != 0) //если у нас есть объекты вне сетки | сетка инициализирована
        {
            foreach(var outsideElement in OutsudeBounds)
            {
                Destroy(outsideElement.Key.gameObject);
            }
        }

        RemainingElemengs = Lines * Collums;
        Grid = new GridItemController[Collums, Lines];
        GridElements = PoolControl.PoolController.InitializePool(Collums * Lines); //даем задание пулу обьектов сгенерировать необходимое число элементов
        OutsudeBounds = new ConcurrentDictionary<GridItemController, KeyValuePair<int, int>>(); //заданем новый пул объектов вне сетки

        int element = 0; //счетчик, на каком мы элементе в GridElements
        for(int x = 0; x < Collums; x++) //Идем в линию
        {
            for(int y = 0; y < Lines; y++) //идем в высоту
            {
                Grid[x, y] = GridElements[element].GetComponent<GridItemController>(); //записываем в двухмерный массив контроллер элемента
                Grid[x, y].posX = x;
                Grid[x, y].posY = y;
                GridElements[element].transform.position = new Vector2((GridPlacer.transform.position.x - ((otstupX * Collums) / 2) + (otstupX / 2)) + (x * otstupX), GridPlacer.transform.position.y + (y * -otstupY)); //задаем позицию элементу
                element++; //increment счетчик
            }
        }
    }

    /// <summary>
    /// Удаляем элементы из сетки на основании равенства цвета с этим элементом
    /// </summary>
    /// <param name="elementPosX"></param>
    /// <param name="elementPosY"></param>
    /// <param name="elementColor"></param>
    public void RemoveFromGridEveryNearWithSimillarColor(int elementPosX, int elementPosY, BallsColor elementColor)
    {
        if (elementPosX != -1 || elementPosY != -1)
        {
            if (Grid[elementPosX, elementPosY] != null)
            {
                var outsideToRemove = from ar in OutsudeBounds //если к нашему элементу есть прикрепленные внешние элементы
                                      where ar.Value.Key == elementPosX && ar.Value.Value == elementPosY
                                      select ar;

                foreach (var element in outsideToRemove)
                {
                    if (element.Key.Color == elementColor)
                    {
                        element.Key.ElementDelete(); //удаляем внешний элемент
                        bool suc = false;
                        while (!suc)
                            suc = OutsudeBounds.TryRemove(element.Key, out _);
                    }
                }
                

                Grid[elementPosX, elementPosY].ElementDelete(); //говорим элементу что его вычистили из сетки
                Grid[elementPosX, elementPosY] = null; //вычищаем сетку
                RemainingElemengs -= 1; //уменьшаем число оставшихся элементов

                if (RemainingElemengs != 0) //Если шаров не осталось, нет смысла проверять соседей - их нет
                {
                    //проверяем каждый элемент со всех сторон на соответствие с цветом
                    if (CheckForLeftElement(elementPosX, elementPosY, elementColor)) RemoveFromGridEveryNearWithSimillarColor(elementPosX - 1, elementPosY, elementColor);
                    if (CheckForRightElement(elementPosX, elementPosY, elementColor)) RemoveFromGridEveryNearWithSimillarColor(elementPosX + 1, elementPosY, elementColor);
                    if (CheckForUpElement(elementPosX, elementPosY, elementColor)) RemoveFromGridEveryNearWithSimillarColor(elementPosX, elementPosY - 1, elementColor);
                    if (CheckForDownElement(elementPosX, elementPosY, elementColor)) RemoveFromGridEveryNearWithSimillarColor(elementPosX, elementPosY + 1, elementColor);
                }
                else //вызываем завершение уровня
                {
                    GameControl.GameController.CompleteLevel();
                }
            }
        }
    }

    /// <summary>
    /// Удаляет из сетки единичный объект
    /// </summary>
    /// <param name="elementPosX"></param>
    /// <param name="elementPosY"></param>
    public void RemoveSingleElement(int elementPosX, int elementPosY)
    {
        if (elementPosX != -1 || elementPosY != -1)
        {
            if (Grid[elementPosX, elementPosY] != null)
            {
                var outsideToRemove = from ar in OutsudeBounds //если к нашему элементу есть прикрепленные внешние элементы
                                      where ar.Value.Key == elementPosX && ar.Value.Value == elementPosY
                                      select ar;

                foreach (var element in outsideToRemove)
                {
                    if (element.Key.Color == Grid[elementPosX, elementPosY].Color)
                    {
                        element.Key.ElementDelete(); //удаляем внешний элемент
                        bool suc = false;
                        while (!suc)
                            suc = OutsudeBounds.TryRemove(element.Key, out _);
                    }
                }

                Grid[elementPosX, elementPosY].ElementDelete(); //говорим элементу что его вычистили из сетки
                Grid[elementPosX, elementPosY] = null; //вычищаем сетку
                RemainingElemengs -= 1;
            }
        }
    }

    /// <summary>
    /// Заменяет объект в сетке указанным новым элементом
    /// </summary>
    /// <param name="elementPosX"></param>
    /// <param name="elementPosY"></param>
    /// <param name="newElement"></param>
    public void RemoveSingleElement(int elementPosX, int elementPosY, GridItemController newElement)
    {
        if (elementPosX != -1 || elementPosY != -1)
        {
            if (Grid[elementPosX, elementPosY] != null)
            {
                Grid[elementPosX, elementPosY].ElementDelete(); //говорим элементу что его вычистили из сетки
                Grid[elementPosX, elementPosY] = newElement; //заменяем объект
            }
        }
    }

    /// <summary>
    /// Удаляет из сетки два и более соседних объекта
    /// </summary>
    /// <param name="elementPosX"></param>
    /// <param name="elementPosY"></param>
    /// <param name="elementColor"></param>
    public void RemoveTwoNear(int elementPosX, int elementPosY, BallsColor elementColor)
    {
        if (elementPosX != -1 || elementPosY != -1)
        {
            if (Grid[elementPosX, elementPosY] != null)
            {
                List<KeyValuePair<int, int>> toDelete = new List<KeyValuePair<int, int>>();
                toDelete.Add(new KeyValuePair<int, int>(elementPosX, elementPosY));

                //проверяем каждый элемент со всех сторон на соответствие с цветом
                if (CheckForLeftElement(elementPosX, elementPosY, elementColor)) toDelete.Add(new KeyValuePair<int, int>(elementPosX - 1, elementPosY));
                if (CheckForRightElement(elementPosX, elementPosY, elementColor)) toDelete.Add(new KeyValuePair<int, int>(elementPosX + 1, elementPosY));
                if (CheckForUpElement(elementPosX, elementPosY, elementColor)) toDelete.Add(new KeyValuePair<int, int>(elementPosX, elementPosY - 1));
                if (CheckForDownElement(elementPosX, elementPosY, elementColor)) toDelete.Add(new KeyValuePair<int, int>(elementPosX, elementPosY + 1));

                if (toDelete.Count > 2) //если соседей с одинаковым цветом больше двух, удаляем всех соседей
                {
                    foreach (var element in toDelete)
                    {
                        RemoveSingleElement(element.Key, element.Value);
                    }
                }
            }
        }
    }
    bool CheckForUpElement(int elementPosX, int elementPosY, BallsColor elementColor)
    {
        //примечание, null элементы в сетке означает их отсутствие
        if (elementPosY - 1 >= 0)
        {
            if (Grid[elementPosX, elementPosY - 1] != null) //прверяем соседа сверху
            {
                if (Grid[elementPosX, elementPosY - 1].Color == elementColor)
                {
                    return true;
                }
            }
        }
        return false;
    }
    bool CheckForDownElement(int elementPosX, int elementPosY, BallsColor elementColor)
    {
        //примечание, null элементы в сетке означает их отсутствие
        if (elementPosY + 1 < Lines)
        {
            if (Grid[elementPosX, elementPosY + 1] != null) //прверяем соседа снизу
            {
                if (Grid[elementPosX, elementPosY + 1].Color == elementColor)
                {
                    return true;
                }
            }
        }
        return false;
    }
    bool CheckForLeftElement(int elementPosX, int elementPosY, BallsColor elementColor)
    {
        //примечание, null элементы в сетке означает их отсутствие
        if (elementPosX - 1 >= 0)
        {
            if (Grid[elementPosX - 1, elementPosY] != null) //прверяем соседа слева
            {
                if (Grid[elementPosX - 1, elementPosY].Color == elementColor)
                {
                    return true;
                }
            }
        }
        return false;
    }
    bool CheckForRightElement(int elementPosX, int elementPosY, BallsColor elementColor)
    {
        //примечание, null элементы в сетке означает их отсутствие
        if (elementPosX + 1 < Collums)
        {
            if (Grid[elementPosX + 1, elementPosY] != null) //прверяем соседа справа
            {
                if (Grid[elementPosX + 1, elementPosY].Color == elementColor)
                {
                    return true;
                }
            }
        }
        return false;
    }


    /// <summary>
    /// Добавляем элемент вне сетки
    /// </summary>
    /// <param name="parentX"></param>
    /// <param name="parentY"></param>
    /// <param name="item"></param>
    public void AddElementOutside(int parentX, int parentY, GridItemController item)
    {
        if (!OutsudeBounds.ContainsKey(item))
        {
            bool suc = false;
            while(!suc)
                suc = OutsudeBounds.TryAdd(item, new KeyValuePair<int, int>(parentX, parentY));
        }
    }

    //отладочная часть кода
    /// <summary>
    /// Вызывает у рандомного элемента сетки RemoveFromGrid(). При отсутствии элементов в сетке выдает ошибку StackNullExeption
    /// </summary>
    public void TestRemoveObj()
    {

        int x;
        int y;

        x = Random.Range(0, Collums);
        y = Random.Range(0, Lines);

        if (Grid[x, y] == null)
        {
            TestRemoveObj();
        }
        else
        {
            BallsColor color = Grid[x, y].Color;
            RemoveFromGridEveryNearWithSimillarColor(x, y, color);
        }
    }

    public bool CheckForEndOfGamemode30()
    {
        float notNull = Collums;
        for(int x = 0; x < Collums; x++)
        {
            if (Grid[x, 0] == null)
                notNull-=1;
        }

        if ((notNull * 100/ Collums)< 30)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Запрос всех элементов сетки
    /// </summary>
    /// <returns>List<GridItemController></returns>
    public Task<List<GridItemController>> RequstGridControllersList()
    {
        return Task.Run(() =>
        {
            List<GridItemController> grid = new List<GridItemController>();
            while (Grid == null)
            {
                Task.Delay(50);
            }
            for (int x = 0; x < Collums; x++) //Идем в линию
            {
                for (int y = 0; y < Lines; y++) //идем в высоту
                {
                    grid?.Add(Grid[x, y]);
                }
            };
            return grid;
        }
        );
    }
}
