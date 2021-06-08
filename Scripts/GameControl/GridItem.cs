using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Grid Item", menuName = "ScriptableObjects/Grid Item", order = 1)]
public class GridItem : ScriptableObject
{
    /// <summary>
    /// Ссылка на префаб
    /// </summary>
    public GameObject ItemPrefab;

    public float ItemRadius = 0.4f;

    /// <summary>
    /// Цена элемента в игровом режиме
    /// </summary>
    public int ItemScore = 1000;

    /// <summary>
    /// Цвет шара
    /// </summary>
    public BallsColor ItemColor = BallsColor.Red;

}
