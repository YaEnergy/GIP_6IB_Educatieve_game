using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class FigureGame : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject _instructions; //object van de instructies
    [SerializeField] private TextMeshProUGUI _count; //de tekst voor het pijltje dat aanduid hoeveel keer die richting uit
    [SerializeField] private RectTransform _arrow; //het pijltje dat de richting voor de volgende lijn aanduid
    [SerializeField] private Stats _statsObj; //scorescript
    [Header("Other")]
    [SerializeField] private GameObject _startDot; //startpunt object
    [SerializeField] private GameObject _currentDot; //huidig punt object
    [SerializeField] private LineRenderer _assistLineRend; //de linerenderer om te gebruiken in hulpmodus
    [SerializeField] private Material _lineCorrect; //material voor in hulpmodus
    [SerializeField] private Material _lineWrong; //material voor in hulpmodus
    [SerializeField] private Material _lineStandard; //material voor lijn
    private LineRenderer _lineRend; //de linerenderer voor de figuur
    private GridGenerator _gridGen; //de gridgenerator
    private GridFunctions _gridFuncs; //de functies van het grid
    private string _figureName; //de naam van de figuur
    private bool _assistMode; //hulpmodus aan/uit
    private int _width; //breedte grid
    private int _height; //hoogte grid
    private int _cellSize; //grootte cel grid
    private int i = 0; //houd bij aan welke lijn je zit
    private int _original; //of het een originele figuur is
    private int _lines = 0;
    private List<Vector3> _linePoints; //lijst met alle punten voor de linerenderer
    private List<(int, int)> _arrows; //lijst die bijhoud hoeveel keer je elke kant op moet
    private Dictionary<(float, float), string> _directions = new Dictionary<(float, float), string> //een dictionary om de tekst uit het figuurbestand om te zetten naar ints voor de richtingen of omgekeerd
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

    private bool gameEnded = false;

    private GameObject Instructions { get => _instructions; set => _instructions = value; }
    private TextMeshProUGUI Count { get => _count; set => _count = value; }
    private RectTransform Arrow { get => _arrow; set => _arrow = value; }
    private Stats StatsObj { get => _statsObj; set => _statsObj = value; }
    private GameObject StartDot { get => _startDot; set => _startDot = value; }
    private GameObject CurrentDot { get => _currentDot; set => _currentDot = value; }
    private LineRenderer AssistLineRend { get => _assistLineRend; set => _assistLineRend = value; }
    private Material LineCorrect { get => _lineCorrect; set => _lineCorrect = value; }
    private Material LineWrong { get => _lineWrong; set => _lineWrong = value; }
    private Material LineStandard { get => _lineStandard; set => _lineStandard = value; }
    private LineRenderer LineRend { get => _lineRend; set => _lineRend = value; }
    private GridGenerator GridGen { get => _gridGen; set => _gridGen = value; }
    private GridFunctions GridFuncs { get => _gridFuncs; set => _gridFuncs = value; }
    private string FigureName { get => _figureName; set => _figureName = value; }
    private bool AssistMode { get => _assistMode; set => _assistMode = value; }
    private int Width { get => _width; set => _width = value; }
    private int Height { get => _height; set => _height = value; }
    private int CellSize { get => _cellSize; set => _cellSize = value; }
    private int I { get => i; set => i = value; }
    private int Original { get => _original; set => _original = value; }
    private List<Vector3> LinePoints { get => _linePoints; set => _linePoints = value; }
    private List<(int, int)> Arrows { get => _arrows; set => _arrows = value; }
    private Dictionary<(float, float), string> Directions { get => _directions; set => _directions = value; }

    private void Awake() //het figuurbestand uitlezen en het grid aanmaken + het startpunt plaatsen
    {
        LineRend = gameObject.GetComponent<LineRenderer>();
        GridGen = gameObject.GetComponent<GridGenerator>();
        GridFuncs = GetComponent<GridFunctions>();
        Original = PlayerPrefs.GetInt("original");
        FigureName = FigureMenuLogic.Figure;
        FigureName += ".txt";
        LinePoints = new List<Vector3>();
        Arrows = new List<(int, int)>();
        ReadFigure();
        GridGen.GenerateGrid(Width, Height, CellSize);

        if (PlayerPrefs.GetInt("figure-assist") == 1)
        {
            AssistMode = true;
            StatsObj.gameObject.SetActive(false);
        }
        else
        {
            AssistMode = false;
        }
    }

    private void Update()
    {
        UpdateCamera();

        if (gameEnded)
            return;

        if (I == 0) //startpunt plaatsen zodat de gebruiker weet waar de figuur begint
        {
            Arrows = TransformList(Arrows);
            AddLinePoint(LineRend);
            AddLinePoint(AssistLineRend);
            AddLinePoint(AssistLineRend);
            I++;
        }
        else if (I != LinePoints.Count) //de gebruiker de figuur laten tekenen
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            int count;
            int rotation;
            (count, rotation) = Arrows[I - 1];
            Count.text = count.ToString();
            Arrow.localEulerAngles = new Vector3(Arrow.localEulerAngles.x, Arrow.localEulerAngles.y, rotation);

            if (GridFuncs.PositionInGrid(mousePosition) && Time.timeScale != 0)
            {
                if (!CurrentDot.activeInHierarchy)
                    CurrentDot.SetActive(true);

                Vector3 closestPositionOnGrid;

                closestPositionOnGrid = GridFuncs.ClosestPosition(mousePosition, LineRend.GetPosition(LineRend.positionCount - 1), CellSize);
                CurrentDot.transform.position  = closestPositionOnGrid;
                AssistLineRend.SetPosition(AssistLineRend.positionCount - 1, closestPositionOnGrid);

                if (closestPositionOnGrid == LinePoints[I])
                {
                    if (AssistMode)
                    {
                        AssistLineRend.sharedMaterial = LineCorrect;
                    }
                    else
                    {
                        AssistLineRend.sharedMaterial = LineStandard;
                    }

                    if (Input.GetMouseButtonDown(0))
                    {
                        if (!AssistMode)
                        {
                            StatsObj.AddStat(0, 1);
                        }

                        AssistLineRend.SetPosition(AssistLineRend.positionCount - 2, LinePoints[I]);
                        AssistLineRend.SetPosition(AssistLineRend.positionCount - 1, LinePoints[I]);
                        AddLinePoint(LineRend);
                        I++;
                    }
                }

                else
                {
                    if (AssistMode)
                    {
                        AssistLineRend.sharedMaterial = LineWrong;
                    }
                    else
                    {
                        AssistLineRend.sharedMaterial = LineStandard;
                    }

                    if (Input.GetMouseButtonDown(0) && !AssistMode && LineRend.GetPosition(LineRend.positionCount - 1) != closestPositionOnGrid)
                    {
                        StatsObj.AddStat(0, -1);
                    }
                }
            }
            else //ervoor zorgen dat als de muis niet in het grid is dat de laatste lijn niet blijft staan
            {
                if (CurrentDot.activeInHierarchy)
                    CurrentDot.SetActive(false);

                AssistLineRend.SetPosition(AssistLineRend.positionCount - 1, AssistLineRend.GetPosition(AssistLineRend.positionCount - 2));
            }
        }
        else //figuur is af, spel beindigen
        {
            if (AssistLineRend.positionCount > 0)
            {
                AssistLineRend.positionCount = 0;
                EndGame();
            }
        }
    }

    private void UpdateCamera()
    {
        //Hou hele grid in beeld en plaats voor bord overhouden

        //W: 0 to 1 (1.0)
        //H: 0.15 to 0.77 (0.62)

        if (gameEnded)
        {
            Camera.main.orthographicSize = CameraAspectRatioHelper.OrthographicSizeEnveloppeRect(Width * CellSize / 0.6f, Height * CellSize / 0.54f, Camera.main.aspect);
            Camera.main.transform.localPosition = new Vector3(0.0f, Camera.main.orthographicSize * 0.08f, -10.0f);
        }
        else
        {
            Camera.main.orthographicSize = CameraAspectRatioHelper.OrthographicSizeEnveloppeRect((Width + 2) * CellSize, (Height + 2) * CellSize / 0.62f, Camera.main.aspect);
            Camera.main.transform.localPosition = new Vector3(40.0f, 40.0f + Camera.main.orthographicSize * 0.08f, -10.0f); //Offset is required due to being included in preset figures
        }
    }

    private List<(int, int)> TransformList(List<(int, int)> inputList) //lijst met instructies aanpassen zodat als het meerdere keren na elkaar dezelfde kant op is dat dit klopt in de instructie
    {
        List<(int, int)> transformedList = new List<(int, int)>();

        int count = 1;

        for (int i = 1; i < inputList.Count; i++)
        {
            if (inputList[i].Item2 == inputList[i - 1].Item2)
            {
                count++;
            }
            else
            {
                for (int j = 0; j < count; j++)
                {
                    transformedList.Add((count, inputList[i - 1].Item2));
                }

                count = 1;
            }
        }

        for (int j = 0; j < count; j++)
        {
            transformedList.Add((count, inputList[inputList.Count - 1].Item2));
        }

        return transformedList;
    }

    private void AddLinePoint(LineRenderer lineRenderer) //hulpfunctie om dubbele code te vermijden
    {
        lineRenderer.positionCount++;
        lineRenderer.SetPosition(lineRenderer.positionCount - 1, LinePoints[I]);
    }

    private void ReadFigure() //leest het figuurbestand uit
    {
        string filePath;
        if (Original == 0)
        {
            filePath = Path.Combine(Application.persistentDataPath, "Figuren", FigureName);
        }
        else
        {
            filePath = Path.Combine(Application.streamingAssetsPath, "TekenGame/Figures", FigureName);
        }

        if (File.Exists(filePath))
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                bool readingLineSegments = false;
                Vector3 startPos = Vector3.zero;

                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith("CellSize: "))
                    {
                        string cellSizeString = line.Substring("CellSize: ".Length);
                        CellSize = int.Parse(cellSizeString.Trim());
                    }
                    else if (line.StartsWith("Width: "))
                    {
                        string widthString = line.Substring("Width: ".Length);
                        Width = int.Parse(widthString.Trim());
                    }
                    else if (line.StartsWith("Height: "))
                    {
                        string heightString = line.Substring("Height: ".Length);
                        Height = int.Parse(heightString.Trim());
                    }
                    else if (line.StartsWith("StartPos: "))
                    {
                        string startPosString = line.Substring("StartPos: ".Length);
                        string[] startPosParts = startPosString.Split(',');
                        if (startPosParts.Length == 3)
                        {
                            float startX = float.Parse(startPosParts[0].Trim());
                            float startY = float.Parse(startPosParts[1].Trim());
                            float startZ = float.Parse(startPosParts[2].Trim());
                            startPos = new(startX, startY, startZ);
                            Instantiate(StartDot, startPos, Quaternion.identity, transform);
                            LinePoints.Add(startPos);
                        }
                    }
                    else if (line.StartsWith("LineSegments:"))
                    {
                        readingLineSegments = true;
                    }
                    else if (readingLineSegments)
                    {
                        if (Directions.ContainsValue(line.Trim()))
                        {
                            _lines++;
                            Vector3 previousPoint = LinePoints.Count > 0 ? LinePoints[LinePoints.Count - 1] : startPos;
                            Vector3 newPoint = ComputeNextPoint(previousPoint, line.Trim());

                            if (line.Trim() == "Right")
                            {
                                Arrows.Add((1, 0));
                            }
                            else if (line.Trim() == "Right-Up")
                            {
                                Arrows.Add((1, 45));
                            }
                            else if (line.Trim() == "Up")
                            {
                                Arrows.Add((1, 90));
                            }
                            else if (line.Trim() == "Left-Up")
                            {
                                Arrows.Add((1, 135));
                            }
                            else if (line.Trim() == "Left")
                            {
                                Arrows.Add((1, 180));
                            }
                            else if (line.Trim() == "Left-Down")
                            {
                                Arrows.Add((1, 225));
                            }
                            else if (line.Trim() == "Down")
                            {
                                Arrows.Add((1, 270));
                            }
                            else if (line.Trim() == "Right-Down")
                            {
                                Arrows.Add((1, 315));
                            }
                            LinePoints.Add(newPoint);
                        }
                    }
                }
            }
        }
    }

    private Vector3 ComputeNextPoint(Vector3 startPoint, string direction) //bereken het volgende punt voor de linerenderer
    {
        float dx = 0, dy = 0;
        foreach (var entry in Directions)
        {
            if (entry.Value == direction)
            {
                dx = entry.Key.Item1;
                dy = entry.Key.Item2;
                break;
            }
        }

        return new Vector3(startPoint.x + dx * CellSize, startPoint.y + dy * CellSize, startPoint.z);
    }


    public void EndGame()
    {
        if (PlayerPrefs.GetInt("figure-assist") == 0)
        {
            EndScreenLogic.EndGame("SelectDrawMode", "Figuur namaken", $"{StatsObj.StatValues[0]}/{_lines}", Camera.main.orthographicSize * 1.75f, new(0, 0, -10), 5);
        }
        else
        {
            EndScreenLogic.EndGame("SelectDrawMode", "Figuur namaken", $"/", Camera.main.orthographicSize * 1.75f, new(0, 0, -10), 5);
        }
        //enabled = false;
        gameEnded = true;
        DontDestroyOnLoad(gameObject);
        SceneManager.LoadScene("EndScreen");
    }
}
