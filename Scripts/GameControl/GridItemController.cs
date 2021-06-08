using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GridItemController : MonoBehaviour
{
    public int posX; //Позиция элемента в Grid by X
    public int posY; //Position on Grid by Y
    public int Score;
    public BallsColor Color;
    public GridItem ParentScriptableObj;

    public delegate void CollisionScore(int score);
    public event CollisionScore addScore;

    private void Start()
    {
        Color = ParentScriptableObj.ItemColor;
        Score = ParentScriptableObj.ItemScore;
    }

    /// <summary>
    /// Метод, при вызове оповещает данный обьект об его удаление из списка сетки
    /// </summary>
    public void ElementDelete()
    {
        var rigidbody = this.gameObject.GetComponent<Rigidbody2D>();
        this.gameObject.GetComponent<CircleCollider2D>().enabled = false;

        rigidbody.isKinematic = false;
        rigidbody.AddForce(new Vector2(Random.Range(-5f, 5f), -20f) * 200, ForceMode2D.Force); //даем силу в разном направлении, для красоты
        rigidbody.gravityScale = 1;
        addScore?.Invoke(Score);

        StartCoroutine("Destroy");

        var c = gameObject.GetComponent<PlayerBallController>();
        if (c != null)
            c.enabled = false;
    }

    IEnumerator Destroy()
    {
        yield return new WaitForSeconds(6f);
        Destroy(gameObject);
    }

    public void SetParentPosition(int x, int y)
    {
        posX = x;
        posY = y;
    }

    /*
    void OnCollisionEnter2D(Collision2D col)
    {
        var obj = col.gameObject.GetComponent<PlayerBallController>();
        if (obj.color == Color)
        {
            GridControl.GridController.RemoveFromGridEveryNearWithSimillarColor(posX, posY, Color);
        }

        Destroy(col.gameObject);
    }*/
}

