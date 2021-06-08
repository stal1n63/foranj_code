using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerBallController : MonoBehaviour
{
    public float OurRadius; //Наш радиус спрайта, для отсчета физики 
    public float PullingRadius; //Максимальный радиус натяжения
    [SerializeField] int Speed = 15;

    public Vector3 Direction; //init on zero (0, 0)
    public BallsColor color;

    //углы нынешней камеры
    Vector2 LeftUpBorder;
    Vector2 LeftDownBorder;
    Vector2 RightUpBorder;
    Vector2 RightDownBorder;

    //for animation
    Vector2 secondPoint;
    Vector2 thirdPoint;
    public float speedModifier;

    GridItemController thisController;

    void Start()
    {
        gameObject.layer = 9; //выдаем слой PlayerBall
        var camera = Camera.main;
        //Destroy(this.gameObject.GetComponent<GridItemController>());
        thisController  = this.gameObject.GetComponent<GridItemController>();
        thisController.posX = -1; //задаем шару игроку координаты  (-1, -1), для удаления его из проверок 
        thisController.posY = -1; 

        var rigidbody = this.gameObject.GetComponent<Rigidbody2D>();
        rigidbody.isKinematic = false;
        rigidbody.gravityScale = 0;

        LeftUpBorder = (Vector2)camera.ViewportToWorldPoint(new Vector3(0, 1, 0));
        LeftDownBorder = (Vector2)camera.ViewportToWorldPoint(new Vector3(0, 0, 0));
        RightUpBorder = (Vector2)camera.ViewportToWorldPoint(new Vector3(1, 1, 0));
        RightDownBorder = (Vector2)camera.ViewportToWorldPoint(new Vector3(1, 0, 0));

        secondPoint = Vector2.zero;
        thirdPoint = Vector2.zero;
    }

    // Update is called once per frame
    void Update()
    {
        if (isDragged)
        {
            if (Input.GetAxis("Fire1") == 0 && !isLocked) //мы осознаем, что обьект уже был перемещен, а кнопку мышки отпустили 
            {
                //реализация движения нашего шара 
                isLocked = true;
                isPlayed = true;

                //Передаем управление следующем игроку через 5 секунд в случае неудачи
                StartCoroutine("ToNextPlayer");
            }
        }
        if (isPlayed)
        {
            StartCoroutine("Pull");
        }
    }

    bool isDragged = false; //если уже был взят наш игрок
                            //true когда пользователь зажимает обьект
    public bool isLocked = false;  //активен ли данный контроллера

    public bool isPlayed = false; // true когда игрока уже натянули и отпустили

    public bool isSendNextPlayerRequest = false;
    public void OnMouseDrag()
    {
        if (!isLocked)
        {
            isDragged = true;

            Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10); //Получаем позицию, куда попадает raycast от мыши
            Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint); //переводим локальную позицию в мировую
            Direction = curPosition - GameControl.GameController.transform.position; //получаем направление от нынешнего положения к local.zero
            Direction = Vector3.ClampMagnitude(Direction, PullingRadius); //Не позволяем длине нашего вектора быть больше заданного радиуса
            curPosition = Direction + GameControl.GameController.transform.position; //пересчитываем нашу позицию относительно ограничения на радиус
            Direction.y = Mathf.Clamp(Direction.y, -PullingRadius, 0f); // не позволяем направлению смотреть назад

            transform.position = curPosition; //задаем позицию игроку
            transform.localPosition = new Vector3(transform.localPosition.x, Mathf.Clamp(transform.localPosition.y, -PullingRadius, 0f), transform.localPosition.z); //не позволяем шару превышать 0 локальную высоту

            Vector2 left = CheckForIntersection(LeftUpBorder, LeftDownBorder, transform.position, -Direction * 1000); //Получаем позиции пересечения с нашим направлением
            Vector2 right = CheckForIntersection(RightUpBorder, RightDownBorder, transform.position, -Direction * 1000);
            Vector2 up = CheckForIntersection(LeftUpBorder, RightUpBorder, transform.position, -Direction * 1000);

            //проверяем направление к сторонам, если отрицательное - мы не смотрим
            float leftDot, rightDot, upDot;
            leftDot = Vector2.Dot(-((Vector2)Direction).normalized, left);
            rightDot = Vector2.Dot(-((Vector2)Direction).normalized, right);
            upDot = Vector2.Dot(-((Vector2)Direction).normalized, up);

            if (leftDot > 0) //look at left
            {
                if (upDot > leftDot && transform.localPosition.y != 0)// если сторона ближе чем вверх
                {
                    secondPoint = new Vector2(left.x + OurRadius, left.y);
                    Vector2 secDirection = Vector2.Reflect(-Direction * 15f, Vector2.right);
                    //secDirection = Vector2.ClampMagnitude(secDirection, 15f);
                    thirdPoint = (secondPoint + secDirection) * 15;
                    var pRandom = new Vector2(0, Random.Range(-speedModifier * speedModifier, speedModifier * speedModifier)) * 15;

                    if (speedModifier > 0.5f)
                        DirectionRenderer.DirectionRendererController.SetPositions(4, transform.position, secondPoint, secondPoint, thirdPoint, secondPoint + new Vector2(0, speedModifier / 5), thirdPoint + new Vector2(0, speedModifier * speedModifier) * 15, secondPoint - new Vector2(0, speedModifier / 5), thirdPoint - new Vector2(0, speedModifier * speedModifier) * 15);
                    else
                        DirectionRenderer.DirectionRendererController.SetPositions(2, transform.position, secondPoint, secondPoint, thirdPoint);
                    thirdPoint += pRandom;
                }
                else
                {
                    secondPoint = up;
                    thirdPoint = up * 15;
                    DirectionRenderer.DirectionRendererController.SetPositions(1, transform.position, up);
                }
            }

            if (rightDot > 0) //look at right
            {
                if (upDot > rightDot && transform.localPosition.y != 0)// если сторона ближе чем вверх
                {
                    secondPoint = new Vector2(right.x - OurRadius, right.y);
                    Vector2 secDirection = Vector2.Reflect(-Direction * 15f, Vector2.left); //высчитываем направление рикошета
                    //secDirection = Vector2.ClampMagnitude(secDirection, 15f);
                    thirdPoint = (secondPoint + secDirection) * 15;
                    var pRandom = new Vector2(0, Random.Range(-speedModifier * speedModifier, speedModifier * speedModifier)) * 15;

                    if (speedModifier > 0.5f)
                        DirectionRenderer.DirectionRendererController.SetPositions(4, transform.position, secondPoint, secondPoint, thirdPoint, secondPoint + new Vector2(0, speedModifier / 5), thirdPoint + new Vector2(0, speedModifier * speedModifier) * 15, secondPoint - new Vector2(0, speedModifier / 5), thirdPoint - new Vector2(0, speedModifier * speedModifier * 15));
                    else
                        DirectionRenderer.DirectionRendererController.SetPositions(2, transform.position, secondPoint, secondPoint, thirdPoint);

                    thirdPoint += pRandom;
                }
                else
                {
                    secondPoint = up;
                    thirdPoint = up * 15;
                    DirectionRenderer.DirectionRendererController.SetPositions(1, transform.position, up);
                }
            }

            speedModifier = Direction.magnitude / PullingRadius;
        }
    }

    //Проверка на пересечение между 2 векторами
    Vector2 CheckForIntersection(Vector2 A1, Vector2 A2, Vector2 B1, Vector2 B2)
    {
        //Проверяем на парралельность, если = 0, решений бесконечно
        float tmp = (B2.x - B1.x) * (A2.y - A1.y) - (B2.y - B1.y) * (A2.x - A1.x);

        if (tmp == 0)
        {
            return Vector2.zero; //return 0,0
        }

        float mu = ((A1.x - B1.x) * (A2.y - A1.y) - (A1.y - B1.y) * (A2.x - A1.x)) / tmp;

        return new Vector2(
            B1.x + (B2.x - B1.x) * mu,
            B1.y + (B2.y - B1.y) * mu
        );
    }

    public void OnEnable()
    {
        DirectionRenderer.DirectionRendererController.SetPositions(0);
    }

    bool step = false;
    //Анимация передвижения
    IEnumerator Pull()
    {
        //дошли ли мы до стены или нет
        if (this.transform.position == (Vector3)secondPoint)
            step = true;

        if (!step)
        {
            this.transform.position = Vector2.MoveTowards(transform.position, secondPoint, Speed * speedModifier * Time.deltaTime); //перемещаемся к точке
        }
        else
        {
            if (transform.position == (Vector3)thirdPoint) 
            {
                isPlayed = true;
            }
            this.transform.position = Vector2.MoveTowards(transform.position, thirdPoint, Speed * speedModifier * Time.deltaTime); //перемещаемся к точке
        }
        yield return null;
    }


    IEnumerator ToNextPlayer()
    {
        yield return new WaitForSeconds(5);
        if(!isSendNextPlayerRequest)
            GameControl.GameController.NextPlayer();
    }

    bool isCollided = false;

    void OnCollisionEnter2D(Collision2D col)
    {
        if (!isCollided)
        {
            var gridComp = col.gameObject.GetComponent<GridItemController>();
            if (gridComp != null)
            {
                if (!isSendNextPlayerRequest)
                {
                    GameControl.GameController.NextPlayer(); //Мы столкнулись с шарами - незачем ждать минимальное время до следующего игрока
                }
                isSendNextPlayerRequest = true;

                if (speedModifier > 0.9f) //we  at full speed
                {
                    GridControl.GridController.RemoveSingleElement(gridComp.posX, gridComp.posY, thisController);
                    thisController.posY = gridComp.posY;
                    thisController.posX = gridComp.posX;

                    secondPoint = gridComp.transform.position;
                    thirdPoint = gridComp.transform.position;

                    GridControl.GridController.RemoveTwoNear(gridComp.posX, gridComp.posY, thisController.Color);
                }
                else
                {
                    GridControl.GridController.AddElementOutside(gridComp.posX, gridComp.posY, thisController);
                    GridControl.GridController.RemoveTwoNear(gridComp.posX, gridComp.posY, thisController.Color);
                    this.enabled = false;
                }

                this.gameObject.layer = 8;
            }

            isCollided = true;
            GameControl.GameController.CheckForComplete();
        }
    }
}