using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionRenderer : MonoBehaviour
{
    public static DirectionRenderer DirectionRendererController;

    List<LineRenderer> renders = new List<LineRenderer>();

    public int LinesCount = 6;
    // Start is called before the first frame update
    void Start()
    {
        //singleton check
        if (DirectionRendererController == null)
            DirectionRendererController = this;
        if (DirectionRendererController != null)
        {
            if (DirectionRendererController != this)
            {
                Destroy(DirectionRendererController.gameObject);
                DirectionRendererController = this;
            }
        }

        for (int i = 0; i < LinesCount; i++) //Инициализируем рендереры для линий
        {
            var obj = new GameObject(); //создаем пустышку и вешаем на него LineRenderer
            obj.transform.parent = this.transform;
            var lineRenderer = obj.AddComponent<LineRenderer>();

            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.widthMultiplier = 0.05f;
            lineRenderer.startColor = Color.black;
            lineRenderer.endColor = Color.black;
            lineRenderer.positionCount = 2; //необходимо всего 2 точки

            renders.Add(lineRenderer);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Отрендерить линии между точками. Точки должны идти парами, если линия разделяет одинаковые точки, продублировать ссылку на точку 
    /// Пример: SetPositions(2, pos1, pos2, pos2, pos3); 
    /// </summary>
    /// <param name="count">Количество линий</param>
    /// <param name="positions">Важно! Количество позиций должно быть равно count * 2 </param>
    public void SetPositions(int count, params Vector3[] positions)
    {
        if (positions.Length != count * 2)
            throw new System.Exception("You send wrong lenght of positions");

        int i = 0;
        foreach (var renderer in renders)
        {
            if (i < positions.Length - 1) // проверка на выезд из массива
            {
                renderer.SetPosition(0, positions[i] + Vector3.forward);
                renderer.SetPosition(1, positions[i + 1] + Vector3.forward);
                i += 2;
            }
            else
            {
                renderer.SetPosition(0, Vector3.zero);
                renderer.SetPosition(1, Vector3.zero);
            }
        }
    }
}
