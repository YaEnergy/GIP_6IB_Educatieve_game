using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;
using System.Linq;
using UnityEngine.UI;

public class DrawFigure : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_InputField _figureNameText; //invoervak voor de figuurnaam
    [SerializeField] private TextMeshProUGUI _instructionText; //instructie text
    [SerializeField] private GameObject _undoButtonObject;
    [SerializeField] private GameObject _redoButtonObject;
    [SerializeField] private GameObject _gridSettings; //object van de gridinstellingen
    [SerializeField] private Button _startButton; //object voor waarschuwing aanmaken figuur
    [Header("Settings")]
    [SerializeField] private int _minWidth; //minimum breedte grid
    [SerializeField] private int _minHeight; //minimum hoogte grid
    [SerializeField] private int _maxWidth; //maximum breedte grid
    [SerializeField] private int _maxHeight; //maximum hoogte grid
    [Header("Other")]
    [SerializeField] private GameObject _startDot; //startpunt object
    [SerializeField] private GameObject _currentDot; //huidig punt object
    [SerializeField] private LineRenderer _assistLineRend; //de linerenderer voor de figuur
    [SerializeField] private GameObject _menuLogObject; //script voor menu logica
    private string _figureName = ""; //de figuurnaam
    private string[] _figures; //lijst met alle figuren
    private List<string> _actions; //lijst met alle genomen acties
    private List<(string, Vector3)> _undoneActions; //lijst met acties die ongedaan zijn gemaakt
    private int _width; //breedte grid
    private int _height; //hoogte grid
    private int _cellSize = 1; //grootte cel grid
    private bool _settingGridValues = true; //of je het grid aan het instelllen bent
    private bool _startDotPlaced = false; //of het startpunt al geplaatst is
    private bool _figureDone = false;
    private Vector3 _undoneStartDot = Vector3.zero; //houd het verwijderde startpunt bij
    private GameObject _start; //Het object voor het startpunt
    private LineRenderer _lineRend; //de linerenderer voor de figuur
    private GridGenerator _gridGen; //de gridgenerator
    private GridFunctions _gridFuncs; //de gridfuncties
    private MenuLogic _menuLog; //de menu logica
    private Dictionary<(float, float), string> _directions = new Dictionary<(float, float), string>  //een dictionary om de tekst uit het figuurbestand om te zetten naar ints voor de richtingen of omgekeerd
    {
        { (-1, -1), "Left-Down" },
        { (0, -1), "Down" },
        { (1, -1), "Right-Down" },
        { (-1, 0), "Left" },
        { (1, 0), "Right" },
        { (-1, 1), "Left-Up" },
        { (0, 1), "Up" },
        { (1, 1), "Right-Up" }
    };

    private TMP_InputField FigureNameText { get => _figureNameText; set => _figureNameText = value; }
    private TextMeshProUGUI InstructionText { get => _instructionText; set => _instructionText = value; }
    private GameObject UndoButtonObject { get => _undoButtonObject; set => _undoButtonObject = value; }
    private GameObject RedoButtonObject { get => _redoButtonObject; set => _redoButtonObject = value; }
    private GameObject GridSettings { get => _gridSettings; set => _gridSettings = value; }
    private Button StartButton { get => _startButton; set => _startButton = value; }
    private int MinWidth { get => _minWidth; set => _minWidth = value; }
    private int MinHeight { get => _minHeight; set => _minHeight = value; }
    private int MaxWidth { get => _maxWidth; set => _maxWidth = value; }
    private int MaxHeight { get => _maxHeight; set => _maxHeight = value; }
    private GameObject StartDot { get => _startDot; set => _startDot = value; }
    private GameObject CurrentDot { get => _currentDot; set => _currentDot = value; }
    private LineRenderer AssistLineRend { get => _assistLineRend; set => _assistLineRend = value; }
    private GameObject MenuLogObject { get => _menuLogObject; set => _menuLogObject = value; }
    private string FigureName { get => _figureName; set => _figureName = value; }
    private string[] Figures { get => _figures; set => _figures = value; }
    private List<string> Actions { get => _actions; set => _actions = value; }
    private List<(string, Vector3)> UndoneActions { get => _undoneActions; set => _undoneActions = value; }
    private int Width { get => _width; set => _width = value; }
    private int Height { get => _height; set => _height = value; }
    private int CellSize { get => _cellSize; set => _cellSize = value; }
    private bool SettingGridValues { get => _settingGridValues; set => _settingGridValues = value; }
    private bool StartDotPlaced { get => _startDotPlaced; set => _startDotPlaced = value; }
    private bool FigureDone { get => _figureDone; set => _figureDone = value; }
    private Vector3 UndoneStartDot { get => _undoneStartDot; set => _undoneStartDot = value; }
    private GameObject Start { get => _start; set => _start = value; }
    private LineRenderer LineRend { get => _lineRend; set => _lineRend = value; }
    private GridGenerator GridGen { get => _gridGen; set => _gridGen = value; }
    private GridFunctions GridFuncs { get => _gridFuncs; set => _gridFuncs = value; }
    private MenuLogic MenuLog { get => _menuLog; set => _menuLog = value; }
    private Dictionary<(float, float), string> Directions { get => _directions; set => _directions = value; }

    private void Awake() //instellingen toepassen
    {
        LineRend = GetComponent<LineRenderer>();
        GridGen = GetComponent<GridGenerator>();
        GridFuncs = GetComponent<GridFunctions>();
        MenuLog = MenuLogObject.GetComponent<MenuLogic>();
        Actions = new List<string>();
        UndoneActions = new List<(string, Vector3)>();
        Start = Instantiate(StartDot, Vector3.zero, Quaternion.identity, transform);
        Width = MaxWidth;
        Height = MaxHeight;
        LoadFigures();
    }

    private void Update()
    {
        UpdateCamera();

        if (!SettingGridValues)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (GridFuncs.PositionInGrid(mousePosition) && Time.timeScale != 0) //logica voor de figuur te tekenen
            {
                if (!CurrentDot.activeInHierarchy)
                    CurrentDot.SetActive(true);

                if (!StartDotPlaced)
                {
                    Vector3 closestPositionOnGrid = GridFuncs.ClosestPositionOnGrid(mousePosition);
                    Start.transform.position = closestPositionOnGrid;
                    if (Input.GetMouseButtonDown(0))
                    {
                        UndoneActions.Clear();
                        UndoneStartDot = Vector3.zero;
                        StartDotPlaced = true;
                        Start.SetActive(true);
                        AddStartPos(closestPositionOnGrid);
                        LineRend.positionCount++;
                        LineRend.SetPosition(LineRend.positionCount - 1, closestPositionOnGrid);
                        AssistLineRend.positionCount++;
                        AssistLineRend.SetPosition(AssistLineRend.positionCount - 1, closestPositionOnGrid);
                        AssistLineRend.positionCount++;
                        AssistLineRend.SetPosition(AssistLineRend.positionCount - 1, closestPositionOnGrid);
                    }
                    CurrentDot.transform.position = closestPositionOnGrid;
                }
                else
                {
                    Vector3 closestPositionOnGrid = GridFuncs.ClosestPosition(mousePosition, LineRend.GetPosition(LineRend.positionCount - 1), CellSize);
                    AssistLineRend.SetPosition(AssistLineRend.positionCount - 1, closestPositionOnGrid);

                    if (Directions.ContainsKey(((closestPositionOnGrid.x - LineRend.GetPosition(LineRend.positionCount - 1).x) / CellSize, (closestPositionOnGrid.y - LineRend.GetPosition(LineRend.positionCount - 1).y) / CellSize)) && Input.GetMouseButtonDown(0))
                    {
                        UndoneActions.Clear();
                        UndoneStartDot = Vector3.zero;
                        string direction = Directions[((closestPositionOnGrid.x - LineRend.GetPosition(LineRend.positionCount - 1).x) / CellSize, (closestPositionOnGrid.y - LineRend.GetPosition(LineRend.positionCount - 1).y) / CellSize)];
                        AddLineSegment(direction);
                        LineRend.positionCount++;
                        LineRend.SetPosition(LineRend.positionCount - 1, closestPositionOnGrid);
                        AssistLineRend.SetPosition(AssistLineRend.positionCount - 2, closestPositionOnGrid);
                        AssistLineRend.SetPosition(AssistLineRend.positionCount - 1, closestPositionOnGrid);
                    }
                    CurrentDot.transform.position = closestPositionOnGrid;
                }
            }
            else
            {
                //ervoor zorgen als de muis niet in het grid is dat het startpunt dat nog niet gezet is verdwijnt
                if (Start.activeInHierarchy != StartDotPlaced)
                    Start.SetActive(StartDotPlaced);

                if (CurrentDot.activeInHierarchy)
                    CurrentDot.SetActive(false);

                //ervoor zorgen dat als de muis niet in het grid is dat de laatste lijn niet blijft staan
                if (StartDotPlaced)
                    AssistLineRend.SetPosition(AssistLineRend.positionCount - 1, AssistLineRend.GetPosition(AssistLineRend.positionCount - 2));
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

        //Hou hele grid in beeld en plaats voor bord overhouden
        Camera.main.orthographicSize = Mathf.Max(Width + 3, Height + 3) * aspectMultiplier * ((float)CellSize * 0.32f);
        Camera.main.transform.localPosition = new Vector3(0.0f, Camera.main.orthographicSize * 0.23f, -10.0f);
    }

    public void GenerateGrid() //genereer het grid, verberg de gridinstellingen en maak de instellingen voor tekenmodus zichtbaar
    {
        if (SettingGridValues)
        {
            SettingGridValues = false;
            GridGen.GenerateGrid(Width, Height, CellSize);
            GridSettings.SetActive(false);
            GameObject.FindWithTag("MenuLogic").GetComponent<FigureMenuLogic>().DisableMenuButtons();
            FigureName = FigureNameText.text + ".txt";
            StartButton.interactable = false;

            InstructionText.text = "Teken een figuur";
            UndoButtonObject.SetActive(true);
            RedoButtonObject.SetActive(true);
        }
        else
        {
            FigureDone = true;
            MenuLog.LoadScene("SelectDrawMode");
        }
    }

    public void ValidateName()
    {
        if (!string.IsNullOrWhiteSpace(FigureNameText.text) && !Figures.Contains(FigureNameText.text)) //kijkt na of de figuurnaam niet leeg is of geen spaties zijn en of de figuurnaam nog niet bestaat
        {
            StartButton.interactable = true;
        }
        else
        {
            StartButton.interactable = false;
        }
    }

    public void ValueUp(int index) //functie voor grid in te stellen
    {
        if (index == 0)
        {
            if (Width < MaxWidth / CellSize)
            {
                Width++;
            }
        }
        else if (index == 1)
        {
            if (Height < MaxHeight / CellSize)
            {
                Height++;
            }
        }
        else if (index == 2)
        {
            if (CellSize < 2)
            {
                CellSize++;
                if (Width > MaxWidth / CellSize)
                {
                    Width = MaxWidth / CellSize;
                }
                if (Height > MaxHeight / CellSize)
                {
                    Height = MaxHeight / CellSize;
                }
            }
        }
    }

    public void ValueDown(int index) //functie voor grid in te stellen
    {
        if (index == 0)
        {
            if (Width > MinWidth / CellSize)
            {
                Width--;
            }
        }
        else if (index == 1)
        {
            if (Height > MinHeight / CellSize)
            {
                Height--;
            }
        }
        else if (index == 2)
        {
            if (CellSize > 1)
            {
                CellSize--;
                if (Width < MinWidth * CellSize)
                {
                    Width = MinWidth * CellSize;
                }
                if (Height < MinHeight * CellSize)
                {
                    Height = MinHeight * CellSize;
                }
            }
        }
    }

    public void Undo() //laatste actie ongedaan maken
    {
        if (StartDotPlaced)
        {
            if (Actions.Count != 0)
            {
                string action = Actions[Actions.Count - 1];
                RemoveLineSegment(action, LineRend.GetPosition(LineRend.positionCount - 1));
                LineRend.positionCount--;
                AssistLineRend.SetPosition(AssistLineRend.positionCount - 2, LineRend.GetPosition(LineRend.positionCount - 1));
                AssistLineRend.SetPosition(AssistLineRend.positionCount - 1, LineRend.GetPosition(LineRend.positionCount - 1));
            }
            else
            {
                LineRend.positionCount--;
                AssistLineRend.positionCount--;
                AssistLineRend.positionCount--;
                UndoneStartDot = Start.transform.position;
                Start.transform.position = Vector3.zero;
                StartDotPlaced = false;
            }
        }
    }

    public void Redo() //laatste actie die ongedaan gemaakt is herstellen
    {
        if (UndoneStartDot != Vector3.zero)
        {
            Start.transform.position = UndoneStartDot;
            AddStartPos(UndoneStartDot);
            LineRend.positionCount++;
            LineRend.SetPosition(LineRend.positionCount - 1, UndoneStartDot);
            AssistLineRend.positionCount++;
            AssistLineRend.SetPosition(AssistLineRend.positionCount - 1, UndoneStartDot);
            AssistLineRend.positionCount++;
            AssistLineRend.SetPosition(AssistLineRend.positionCount - 1, UndoneStartDot);
            UndoneStartDot = Vector3.zero;
            StartDotPlaced = true;
        }
        else if (UndoneActions.Count != 0)
        {
            StartButton.interactable = true;
            var lastItem = UndoneActions[UndoneActions.Count - 1];
            string action = lastItem.Item1;
            Vector3 position = lastItem.Item2;
            LineRend.positionCount++;
            LineRend.SetPosition(LineRend.positionCount - 1, position);
            AssistLineRend.SetPosition(AssistLineRend.positionCount - 2, LineRend.GetPosition(LineRend.positionCount - 1));
            AssistLineRend.SetPosition(AssistLineRend.positionCount - 1, LineRend.GetPosition(LineRend.positionCount - 1));
            UndoneActions.RemoveAt(UndoneActions.Count - 1);
            AddLineSegment(action);
        }
    }

    private void AddStartPos(Vector3 startPos) //voegt de instellingen van het grid van de huidige figuur toe en het startpunt van de figuur
    {
        if (!Directory.Exists(Path.Combine(Application.persistentDataPath, "Figuren"))) //maakt de map voor figuren op te slaan aan als deze nog niet bestaat
        {
            Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, "Figuren"));
        }

        string filePath = Path.Combine(Application.persistentDataPath, "Figuren", FigureName);

        using (StreamWriter writer = new StreamWriter(filePath))
        {
            writer.WriteLine("CellSize: " + CellSize);
            writer.WriteLine("Width: " + Width);
            writer.WriteLine("Height: " + Height);
            writer.WriteLine("StartPos: " + startPos.x + ", " + startPos.y + ", " + startPos.z);
            writer.WriteLine("LineSegments:");
        }
    }

    private void AddLineSegment(string direction) //voegt de delen van de figuur toe aan het figuurbestand
    {
        Actions.Add(direction);
        StartButton.interactable = true;
        string filePath = Path.Combine(Application.persistentDataPath, "Figuren", FigureName);

        using (StreamWriter writer = new StreamWriter(filePath, true))
        {
            writer.WriteLine(direction);
        }
    }

    private void RemoveLineSegment(string direction, Vector3 position) //voegt de delen van de figuur toe aan het figuurbestand
    {
        Actions.RemoveAt(Actions.Count - 1);
        UndoneActions.Add((direction, position));
        if (Actions.Count == 0)
        {
            StartButton.interactable = false;
        }
        string filePath = Path.Combine(Application.persistentDataPath, "Figuren", FigureName);

        List<string> lines = File.ReadAllLines(filePath).ToList();

        // Check if there are any lines to remove
        if (lines.Count > 5)
        {
            // Remove the last line
            lines.RemoveAt(lines.Count - 1);

            // Open the file for writing and write the modified lines back
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                foreach (string line in lines)
                {
                    writer.WriteLine(line);
                }
            }
        }
    }

    private void LoadFigures() //laad de lijst met bestaande figuren
    {
        string figureDirectory = Path.Combine(Application.persistentDataPath, "Figuren");

        if (!Directory.Exists(figureDirectory))
        {
            Directory.CreateDirectory(figureDirectory);
        }

        if (Directory.Exists(figureDirectory))
        {
            string[] allFiles = Directory.GetFiles(figureDirectory, "*.txt");
            Figures = allFiles.Select(Path.GetFileNameWithoutExtension).ToArray();
        }
    }

    private void OnDestroy()
    {
        if (!FigureDone && !FigureName.Equals(string.Empty) && File.Exists(Path.Combine(Application.persistentDataPath, "Figuren", FigureName)))
        {
            File.Delete(Path.Combine(Application.persistentDataPath, "Figuren", FigureName));
        }
    }
}
