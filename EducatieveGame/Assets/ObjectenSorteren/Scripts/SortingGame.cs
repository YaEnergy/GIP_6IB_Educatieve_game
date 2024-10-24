using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SortingGame : MonoBehaviour
{
    [SerializeField] private Vector3[] _spawnLocations; //de locaties waar objecten spawnen
    [SerializeField] private GameObject _conveyorItemSpawn; //conveyor item spawn location object, locatie waar object spawned op conveyor
    [SerializeField] private GameObject _sortingItem; //prefab sorteer object
    [SerializeField] private GameObject _sortingBox; //prefab sorteer box
    [SerializeField] private GameObject _conveyorEnd; //einde loopband
    [SerializeField] private TextMeshProUGUI _sortModeText; //de tekst van sorteermodus
    [SerializeField] private TextMeshProUGUI _scoreText; //de tekst van de score
    [SerializeField] private TextMeshProUGUI _timeText; //de tekst van de tijd
    [SerializeField] private GameObject _conveyor; //de loopband
    private Color[] _sortingColors; //de kleuren om mee te sorteren
    private Color[] _selectedSortingColors; //de geselecteerde kleuren om mee te sorteren
    private string[] _sortingTexts; //de kleur teksten om mee te sorteren
    private string[] _selectedSortingTexts; //de geselecteerde kleur teksten om mee te sorteren
    private bool _sortByColor = false; //of er volgen kleur wordt gesorteerd
    private bool _trashcanMode = false; //of de vuilbakmodus aan is
    private bool _conveyorMode = false; //of de loopbandmodus aan is
    private bool _assistMode = false;
    private int _score = 0; //behaalde score
    private float _timer = 90f; //tijd tot spel beindigd
    private float _conveyorSpawnRate = 8f; //spawnrate objecten loopband
    private float _lastSpawnTime = 0f; //laatste object spawntijd op loopband
    private int _amountSpawned = 0; //teller loopband sorteermodus switchen
    private float _startTime = 0f; //startijd van het spel
    private bool _dragging = false;

    private Vector3[] SpawnLocations { get => _spawnLocations; set => _spawnLocations = value; }
    private GameObject ConveyorItemSpawn { get => _conveyorItemSpawn; set => _conveyorItemSpawn = value; }
    private GameObject SortingItem { get => _sortingItem; set => _sortingItem = value; }
    private GameObject SortingBox { get => _sortingBox; set => _sortingBox = value; }
    private GameObject ConveyorEnd { get => _conveyorEnd; set => _conveyorEnd = value; }
    private TextMeshProUGUI SortModeText { get => _sortModeText; set => _sortModeText = value; }
    private TextMeshProUGUI ScoreText { get => _scoreText; set => _scoreText = value; }
    private TextMeshProUGUI TimeText { get => _timeText; set => _timeText = value; }
    private Color[] SortingColors { get => _sortingColors; set => _sortingColors = value; }
    private Color[] SelectedSortingColors { get => _selectedSortingColors; set => _selectedSortingColors = value; }
    private string[] SortingTexts { get => _sortingTexts; set => _sortingTexts = value; }
    private string[] SelectedSortingTexts { get => _selectedSortingTexts; set => _selectedSortingTexts = value; }
    public bool SortByColor { get => _sortByColor; private set => _sortByColor = value; }
    public bool TrashcanMode { get => _trashcanMode; private set => _trashcanMode = value; }
    public bool ConveyorMode { get => _conveyorMode; private set => _conveyorMode = value; }
    private bool AssistMode { get => _assistMode; set => _assistMode = value; }
    private int Score { get => _score; set => _score = value; }
    private float Timer { get => _timer; set => _timer = value; }
    private float ConveyorSpawnRate { get => _conveyorSpawnRate; set => _conveyorSpawnRate = value; }
    private float LastSpawnTime { get => _lastSpawnTime; set => _lastSpawnTime = value; }
    private int AmountSpawned { get => _amountSpawned; set => _amountSpawned = value; }
    private float StartTime { get => _startTime; set => _startTime = value; }
    private GameObject Conveyor { get => _conveyor; set => _conveyor = value; }
    public bool Dragging { get => _dragging; set => _dragging = value; }

    private void Awake() //spel starten met juiste instellingen
    {
        ConveyorEnd.GetComponent<SortBox>().Create(Color.white, string.Empty, true);

        if (PlayerPrefs.GetInt("trashcan") == 1) //vuilbak instellen
        {
            TrashcanMode = true;
        }

        if (PlayerPrefs.GetInt("conveyor") == 1) //loopband instellen
        {
            ConveyorMode = true;
            ConveyorSpawnRate /= (MenuLogic.Difficulty / 1.5f);
            Conveyor.GetComponent<Animator>().enabled = true;
            ConveyorItemSpawn.SetActive(true);
        }

        if (!TrashcanMode && !ConveyorMode)
        {
            ConveyorEnd.GetComponent<Collider2D>().enabled = false;
        }

        int difficulty = MenuLogic.Difficulty + 1;

        //kleuren laden
        SortingColors = SortingMenuLogic.SortingColors;
        List<Color> selectedSortingColors = SortingMenuLogic.SelectedSortingColors.ToList();
        SortingTexts = SortingMenuLogic.SortingTexts;
        List<string> selectedSortingTexts = SortingMenuLogic.SelectedSortingTexts.ToList();
        SelectedSortingColors = new Color[difficulty];
        SelectedSortingTexts = new string[difficulty];

        for (int i = 0; i < difficulty; i++)
        {
            int randomIndex = Random.Range(0, selectedSortingColors.Count);

            SelectedSortingColors[i] = selectedSortingColors[randomIndex];
            SelectedSortingTexts[i] = selectedSortingTexts[randomIndex];

            selectedSortingColors.RemoveAt(randomIndex);
            selectedSortingTexts.RemoveAt(randomIndex);
        }

        SortingMenuLogic.SelectedSortingColors = SelectedSortingColors;

        //begin spawnen

        SpawnSortBoxes(MenuLogic.Difficulty + 1);

        if (ConveyorMode)
        {
            SpawnSortItem(ConveyorItemSpawn.transform.position);
            AmountSpawned++;
            LastSpawnTime = Time.time;
        }
        else
        {
            foreach (Vector3 spawnLocation in SpawnLocations)
            {
                SpawnSortItem(spawnLocation);
            }
        }

        //tekst en timer instellen
        StartTime = Time.time;
        SortModeText.text = "woord";
        ScoreText.text = $"Score: {Score}";
        if (PlayerPrefs.GetInt("sort-assist") != 1)
        {
            TimeText.text = $"Tijd: {Mathf.Round(Timer - (Time.time - StartTime))}";
        }
        else
        {
            AssistMode = true;
            TimeText.text = $"";
        }
    }

    private void Update()
    {
        UpdateCamera();

        if (!AssistMode)
        {
            if (Mathf.Round(Timer - (Time.time - StartTime)) <= 0)
            {
                EndGame();
            }

            TimeText.text = $"Tijd: {Mathf.Round(Timer - (Time.time - StartTime))}";
        }

        //sorteer objecten op loopband spawnen

        if (!ConveyorMode && GameObject.FindWithTag("SortItem") == null)
        {
            if (Random.value > 0.5f)
            {
                SortByColor = !SortByColor;
                if (SortByColor)
                {
                    SortModeText.text = "kleur";
                }
                else
                {
                    SortModeText.text = "woord";
                }
            }

            foreach (Vector3 spawnLocation in SpawnLocations)
            {
                SpawnSortItem(spawnLocation);
            }
        }

        if (ConveyorMode)
        {
            if (AmountSpawned == 5)
            {
                AmountSpawned = 0;
                if (Random.value > 0.5f)
                {
                    StartCoroutine(ChangeMode());
                }
            }

            if (Time.time - LastSpawnTime > ConveyorSpawnRate)
            {
                SpawnSortItem(ConveyorItemSpawn.transform.position);
                AmountSpawned++;
                LastSpawnTime = Time.time;
            }
        }
    }

    private void UpdateCamera()
    {
        //aanpassen camera grootte voor beeldverhouding
        //schaal de camera grootte zodat de lengte en hoogte van een standaard camera met beeldverhouding van 16:9 altijd in past
        float standardAspectRatio = 16.0f / 9.0f;
        //maak de camera niet kleiner, dan past de standaard hoogte niet meer
        float aspectMultiplier = Mathf.Max(standardAspectRatio / Camera.main.aspect, 1.0f);

        Camera.main.orthographicSize = 5.0f * aspectMultiplier;
        Camera.main.transform.localPosition = new Vector3(0.0f, 0.0f, -10.0f); //Offset is required due to being included in preset figures
    }

    public void ItemSorted() //item goed gesorteerd
    {
        Score++;
        ScoreText.text = $"Score: {Score}";
    }

    public void ItemLost() //item verloren gegaan of fout gesorteerd
    {
        Score--;
        ScoreText.text = $"Score: {Score}";
    }

    public void EndGame() //einde spel --> naar eindscherm gaan
    {
        EndScreenLogic.EndGame("KleurGameMenu", "Objecten sorteren", Score.ToString(), Camera.main.orthographicSize, Camera.main.transform.position, 0);
        SceneManager.LoadScene("EndScreen");
    }

    private void SpawnSortItem(Vector3 position) //sorteer object spawnen op gegeven positie
    {
        if (TrashcanMode && Random.value > 0.75f) //25% kans op vuilbak item als vuilbakmodus
        {
            int randomIndexColor = Random.Range(0, SortingColors.Length);
            int randomIndexText = Random.Range(0, SortingTexts.Length);

            while (SelectedSortingColors.Contains(SortingColors[randomIndexColor])) //ervoor zorgen dat het zeker vuilbak is
            {
                randomIndexColor = Random.Range(0, SortingColors.Length);
            }
            while (SelectedSortingTexts.Contains(SortingTexts[randomIndexText])) //ervoor zorgen dat het zeker vuilbak is
            {
                randomIndexText = Random.Range(0, SortingTexts.Length);
            }

            SortItem sortItem = Instantiate(SortingItem, position, Quaternion.identity, transform).GetComponent<SortItem>();
            sortItem.Create(SortingColors[randomIndexColor], SortingTexts[randomIndexText], true, ConveyorMode, this);
        }
        else //een sorteerobject spawnen dat niet in de vuilbak hoort
        {
            int randomIndexColor = Random.Range(0, SelectedSortingColors.Length);
            int randomIndexText = Random.Range(0, SelectedSortingTexts.Length);
            SortItem sortItem = Instantiate(SortingItem, position, Quaternion.identity, transform).GetComponent<SortItem>();
            sortItem.Create(SelectedSortingColors[randomIndexColor], SelectedSortingTexts[randomIndexText], false, ConveyorMode, this);
        }
    }

    private void SpawnSortBoxes(int amount) //de sorteer dozen spawnen
    {
        //hulp centreren dozen
        float startX = -((amount - 1) * 2.5f) / 2f;

        for (int i = 0; i < amount; i++)
        {
            SortBox sortBox = Instantiate(SortingBox, new Vector3(startX + i * 2.5f, 2f, 0f), Quaternion.identity, transform).GetComponent<SortBox>();
            sortBox.Create(SelectedSortingColors[i], SelectedSortingTexts[i], false);
        }
    }

    private IEnumerator ChangeMode()
    {
        while (Dragging)
        {
            yield return null;
        }
        SortByColor = !SortByColor;
        if (SortByColor)
        {
            SortModeText.text = "kleur";
        }
        else
        {
            SortModeText.text = "woord";
        }
    }

    public void DisableSortItems()
    {
        GameObject[] sortItems = GameObject.FindGameObjectsWithTag("SortItem");
        foreach (GameObject sortItem in sortItems)
        {
            sortItem.GetComponent<SortItem>().enabled = false;
        }
    }

    public void EnableSortItems()
    {
        GameObject[] sortItems = GameObject.FindGameObjectsWithTag("SortItem");
        foreach (GameObject sortItem in sortItems)
        {
            sortItem.GetComponent<SortItem>().enabled = true;
        }
    }
}
