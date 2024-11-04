using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ObjectSpawner : MonoBehaviour
{
    [SerializeField] private GameObject _meat; //de prefab voor een vlees object
    [SerializeField] private float _minutesUntilFastest; //hoeveel minuten tot de snelste spawnrate
    [SerializeField] private Stats _statsObj; //script stats
    [SerializeField] private SpriteRenderer _grassBackground;
    [SerializeField] private TextMeshProUGUI _timerText;
    private GameObject[] _gridCells; //lijst met de gridcellen
    private GridGenerator _gridGen; //de gridgenerator
    private GridFunctions _gridFunc; //de gridfuncties
    private Vector3[] _gridPoints; //lijst met de gridpunten
    private bool _gameActive = true; //of het spel bezig is of al gedaan is
    private float _spawnRate = 1f; //hoe vlug er vlees spawned op de barbecue
    private float _lastSpawnTime; //de laatste tijd wanneer er vlees gespawned is
    private float _lastDecreaseTime; //de laatste tijd wanneer de moeilijkheid hoger gezet is
    private float _timeLeft; //de laatste tijd wanneer de moeilijkheid hoger gezet is
    private float _difficulty; //moeilijkheid van het spel

    private GameObject Meat { get => _meat; set => _meat = value; }
    private float MinutesUntilFastest { get => _minutesUntilFastest; set => _minutesUntilFastest = value; }
    private GameObject[] GridCells { get => _gridCells; set => _gridCells = value; }
    private GridGenerator GridGen { get => _gridGen; set => _gridGen = value; }
    private GridFunctions GridFunc { get => _gridFunc; set => _gridFunc = value; }
    private Stats StatsObj { get => _statsObj; set => _statsObj = value; }
    private SpriteRenderer GrassBackground { get => _grassBackground; set => _grassBackground = value; }
    private TextMeshProUGUI TimerText { get => _timerText; set => _timerText = value; }
    private Vector3[] GridPoints { get => _gridPoints; set => _gridPoints = value; }
    private bool GameActive { get => _gameActive; set => _gameActive = value; }
    private float SpawnRate { get => _spawnRate; set => _spawnRate = value; }
    private float LastSpawnTime { get => _lastSpawnTime; set => _lastSpawnTime = value; }
    private float LastDecreaseTime { get => _lastDecreaseTime; set => _lastDecreaseTime = value; }
    private float TimeLeft { get => _timeLeft; set => _timeLeft = value; }
    private float Difficulty { get => _difficulty; set => _difficulty = value; }

    private void Awake() //grid genereren
    {
        TimeLeft = MinutesUntilFastest * 120;
        TimerText.text = Mathf.Round(TimeLeft) + "s";
        GridGen = GetComponent<GridGenerator>();
        GridFunc = GetComponent<GridFunctions>();
        Difficulty = PlayerPrefs.GetInt("rate");
        int gridSize = PlayerPrefs.GetInt("size");

        switch (gridSize)
        {
            case 1:
                SetGridProperties(2, 4, 4);
                break;
            case 2:
                SetGridProperties(2, 5, 5);
                break;
            case 3:
                SetGridProperties(1, 6, 6);
                break;
            case 4:
                SetGridProperties(1, 7, 7);
                break;
            default:
                SetGridProperties(1, 8, 8);
                break;
        }

        void SetGridProperties(int cellSize, int width, int height)
        {
            GridGen.GenerateGrid(width, height, cellSize);
            GridPoints = GridFunc.CenterGridPoints(cellSize);
        }

        GridCells = new GameObject[transform.childCount];

        for (int i = 0; i < transform.childCount; i++)
        {
            GridCells[i] = transform.GetChild(i).gameObject;
        }

        LastSpawnTime = Time.time;
        LastDecreaseTime = Time.time;
    }

    private void Update()
    {
        //tijd
        TimeLeft -= Time.deltaTime;
        if (TimeLeft < 0 )
        {
            TimeLeft = 0;
        }
        TimerText.text = Mathf.Round(TimeLeft) + "s";

        //aanpassen camera grootte voor beeldverhouding
        //schaal de camera grootte zodat de lengte en hoogte van een standaard camera met grootte 7 en beeldverhouding van 16:9 altijd in past
        float standardAspectRatio = 16.0f / 9.0f;
        //maak de camera niet kleiner, dan past de standaard hoogte niet meer
        float aspectMultiplier = Math.Max(standardAspectRatio / Camera.main.aspect, 1.0f);
        Camera.main.orthographicSize = 7.0f * aspectMultiplier;

        //Hou achtergrond (gras) in beeld
        if (GrassBackground != null)
            GrassBackground.size = new Vector2(Camera.main.orthographicSize * 2.0f * Camera.main.aspect, Camera.main.orthographicSize * 2.0f);

        //vlees spawnen op de barbecue
        if (Time.time - LastSpawnTime > (6 - Difficulty) / SpawnRate && GameActive)
        {
            //lijst lege gridpunten aanmaken
            List<Vector3> emptyGridPoints = new();
            for (int i = 0; i < GridPoints.Length; i++)
            {
                if (GridCells[i].transform.childCount == 0)
                {
                    emptyGridPoints.Add(GridPoints[i]);
                }
            }

            if (emptyGridPoints.Count > 0) //spawn vlees op een willekeurig vakje als er een of meerdere lege gridpunten beschikbaar zijn
            {
                int randomIndex = UnityEngine.Random.Range(0, emptyGridPoints.Count);
                Vector3 gridPoint = emptyGridPoints[randomIndex];
                int cellIndex = Array.IndexOf(GridPoints, gridPoint);

                Instantiate(Meat, new Vector3(gridPoint.x + 0.032f, gridPoint.y - 0.032f, gridPoint.z), Quaternion.identity, GridCells[cellIndex].transform);
            }

            LastSpawnTime = Time.time;
        }

        if (SpawnRate < 1.55f) //moeilijkheid omhoog doen
        {
            if (Time.time - LastDecreaseTime > 5 * MinutesUntilFastest)
            {
                SpawnRate += 0.05f;
                LastDecreaseTime = Time.time;
            }
        }
        else if (Time.time - LastDecreaseTime > 60 * MinutesUntilFastest && GameActive) //spawnen stopzetten na 2 minuten
        {
            GameActive = false;
        }
        else if (Time.time - LastDecreaseTime > 60 * MinutesUntilFastest && GridEmpty()) //game eindigen wanneer er geen vlees meer op de barbecue ligt
        {
            EndGame();
        }
    }

    private bool GridEmpty() //nakijken of er geen vlees meer op de barbecue ligt
    {
        for (int i = 0; i < GridGen.transform.childCount; i++)
        {
            if (GridGen.transform.GetChild(i).childCount > 0)
            {
                return false;
            }
        }

        return true;
    }

    public void EndGame() //spel beindigen
    {
        string score = string.Empty;
        for (int i = 0; i < StatsObj.StatNames.Length; i++)
        {
            if (i == 0)
            {
                score += $"{StatsObj.StatValues[i]}\n";
                score += "Aantal stukken vlees:\n";
            }
            else if (i == 1)
            {
                score += $"{StatsObj.StatValues[i]} {StatsObj.StatNames[i]}: {StatsObj.StatValues[i] * 1}\n";
            }
            else if (i == 2)
            {
                score += $"{StatsObj.StatValues[i]} {StatsObj.StatNames[i]}: {StatsObj.StatValues[i] * -1}\n";
            }
            else if (i == 3)
            {
                score += $"{StatsObj.StatValues[i]} {StatsObj.StatNames[i]}: {StatsObj.StatValues[i] * -2}\n";
            }
            else if (i == 4)
            {
                score += $"{StatsObj.StatValues[i]} {StatsObj.StatNames[i]}: {StatsObj.StatValues[i] * -1}\n";
            }
            else if (i == 5)
            {
                score += $"{StatsObj.StatValues[i]} {StatsObj.StatNames[i]}: {StatsObj.StatValues[i] * -3}\n";
            }
        }
        MenuLogic.SetDifficulty(6);
        EndScreenLogic.EndGame("ReactionMenu", "Vlees bakken", score);
        SceneManager.LoadScene("EndScreen");
    }
}
