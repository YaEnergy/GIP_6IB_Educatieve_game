using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MailChecker : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] BoxCollider2D boxCollider;

    [SerializeField] GameObject _organisingOnTextObject;
    [SerializeField] GameObject _pointsCounterObject;
    [SerializeField] GameObject _gameScriptManager;

    [SerializeField] string _mailboxColor;

    private static int _points = 0;

    void Start()
    {
        _organisingOnTextObject = GameObject.FindGameObjectWithTag("TextObject");
        _gameScriptManager = GameObject.FindGameObjectWithTag("GameScriptManager");
        _pointsCounterObject = GameObject.Find("PointsCounter");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public string GetMailboxColor()
    {
        return _mailboxColor;
    }

    public void SetMailboxColor(string mailboxColor)
    {
        _mailboxColor = mailboxColor;
    }


    //het kijken als er een mailobject wordt geraakt met de mailbox
    //checkt daarna welke sorting method er werd gekozen
    private void OnCollisionEnter2D(Collision2D collision)
    {

        if (collision.gameObject.CompareTag("MailItem"))
        {
            //checkt als er wordt gesorteerd op kleur of tekst
            if (_gameScriptManager.GetComponent<StatsScript>().GetSortingMethod().Equals("kleur"))
            {
                //checkt als de kleur van de postbus overeenkomt met de kleur van het woord
                if (collision.gameObject.GetComponent<MailScript>().GetColor() == gameObject.GetComponent<SpriteRenderer>().color)
                {
                    GainPoints(collision.gameObject);
                }
                else
                {
                    collision.gameObject.GetComponent<Dragging>().SetDragging(false);
                    collision.gameObject.transform.position = collision.gameObject.GetComponent<MailScript>().GetoriginalPosition();

                    LosePoints();
                }
            }
            else
            {
                //checkt verder als de kleur van de postbus overeenkomt met het woord
                if (collision.gameObject.GetComponent<MailScript>().GetText() == _mailboxColor)
                {
                    GainPoints(collision.gameObject);
                }
                else
                {
                    collision.gameObject.GetComponent<Dragging>().SetDragging(false);
                    collision.gameObject.transform.position = collision.gameObject.GetComponent<MailScript>().GetoriginalPosition();

                    LosePoints();
                }
            }
        }
    }

    public void GainPoints(GameObject gameObject)
    {
        Destroy(gameObject);

        SpawnMailScript spawnMailScript = _gameScriptManager.GetComponent<SpawnMailScript>();

        //aantal mailitem objecten - 1 doen.
        spawnMailScript.SetAmountOfMailItemsLeft(spawnMailScript.GetAmountOfMailItemsLeft() - 1);

        //aantal punten verhogen met 1.
        SetPoints(GetPoints() + 1);

        _pointsCounterObject.GetComponent<Text>().text = "punten: " + GetPoints().ToString();
    }

    public void LosePoints()
    {
        SpawnMailScript spawnMailScript = _gameScriptManager.GetComponent<SpawnMailScript>();

        //aantal punten verlagen met 1.
        SetPoints(GetPoints() - 1);

        _pointsCounterObject.GetComponent<Text>().text = "punten: " + GetPoints().ToString();
    }

    public static int GetPoints()
    {
        return _points;
    }

    public static void SetPoints(int value)
    {
        if (GetPoints() + value >= 0)
        {
            _points = value;
        }
        else
        {
            _points = 0;
        }
    }
}
