using UnityEngine;
using UnityEngine.SceneManagement;

public class RotateGame : MonoBehaviour
{
    [SerializeField] private GameObject _correctGrid; //object van het voorbeeld grid
    [SerializeField] private GameObject _gameGrid; //object van het game grid
    [SerializeField] private SpriteRenderer _background;
    [SerializeField] private Sprite _partSprite; //part sprite zonder rooster
    private GridGenerator _correctGridGen; //gridgenerator van het voorbeeldgrid
    private GridGenerator _gameGridGen; //gridgenerator van het gamegrid
    private bool _gameInProgress = true; //zorgen dat je geen vakjes meer kunt draaien als het spel gedaan is
    [SerializeField] private int _width; //breedte grid
    [SerializeField] private int _height; //hoogte grid

    private GameObject CorrectGrid { get => _correctGrid; set => _correctGrid = value; }
    private GameObject GameGrid { get => _gameGrid; set => _gameGrid = value; }
    private SpriteRenderer Background { get => _background; set => _background = value; }
    private Sprite PartSprite { get => _partSprite; set => _partSprite = value; }
    private GridGenerator CorrectGridGen { get => _correctGridGen; set => _correctGridGen = value; }
    private GridGenerator GameGridGen { get => _gameGridGen; set => _gameGridGen = value; }
    public bool GameInProgress { get => _gameInProgress; private set => _gameInProgress = value; }
    private int Width { get => _width; set => _width = value; }
    private int Height { get => _height; set => _height = value; }

    private void Awake() //grid instellen op moeilijkheid
    {
        CorrectGridGen = CorrectGrid.GetComponent<GridGenerator>();
        GameGridGen = GameGrid.GetComponent<GridGenerator>();

        //difficulty toepassen
        int difficulty = MenuLogic.Difficulty;
        int symmetrical = PlayerPrefs.GetInt("symmetrical");

        switch (difficulty)
        {
            case 1:
                SetGridParameters(2, 4);
                break;

            case 2:
                SetGridParameters(4, 8);
                break;

            case 3:
                SetGridParameters(6, 12);
                break;

            case 4:
                SetGridParameters(8, 16);
                break;

            default:
                SetGridParameters(10, 20);
                break;
        }

        //grid aanmaken met parameters
        void SetGridParameters(int width, int height)
        {
            const int CELL_SIZE = 1;

            CorrectGridGen.GenerateGrid(width, height, CELL_SIZE);
            GameGridGen.GenerateGrid(width, height, CELL_SIZE);
            Width = width;
            Height = height;

            GameGrid.transform.position = new Vector3(width / 2.0f + CELL_SIZE / 2.0f, 0.0f, 0.0f);
            CorrectGrid.transform.position = new Vector3(-width / 2.0f - CELL_SIZE / 2.0f, 0.0f, 0.0f);
        }

        int col = 0;
        int row = 0;
        for (int i = 0; i < CorrectGrid.transform.childCount; i++)
        {
            if (col == Width)
            {
                col = 0;
                row++;
            }
            CorrectGrid.transform.GetChild(i).name = $"{col}-{row}";
            col++;
        }

        col = 0;
        row = 0;
        //figuurstukken van voorbeeldgrid een willekeurige rotatie geven
        for (int i = 0; i < CorrectGrid.transform.childCount; i++)
        {
            if (col == Width)
            {
                col = 0;
                row++;
            }

            if (symmetrical == 1)
            {
                if (col < Width / 2 && row < Height / 2)
                {
                    CorrectGrid.transform.GetChild(i).transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 4) * 90.0f);
                }
                else if (row < Height / 2)
                {
                    Vector3 symmetricalRotation = GameObject.Find($"{Width-1-col}-{row}").transform.eulerAngles;

                    CorrectGrid.transform.GetChild(i).transform.eulerAngles = symmetricalRotation;
                    if (symmetricalRotation.z != 90 && symmetricalRotation.z != 270)
                    {
                        CorrectGrid.transform.GetChild(i).transform.localScale = new(-CorrectGrid.transform.GetChild(i).transform.localScale.x, CorrectGrid.transform.GetChild(i).transform.localScale.y, CorrectGrid.transform.GetChild(i).transform.localScale.z);
                        GameGrid.transform.GetChild(i).transform.localScale = new(CorrectGrid.transform.GetChild(i).transform.localScale.x, CorrectGrid.transform.GetChild(i).transform.localScale.y, CorrectGrid.transform.GetChild(i).transform.localScale.z);
                        GameGrid.transform.GetChild(i).transform.rotation = Quaternion.Euler(0, 0, 90);
                    }
                    else
                    {
                        CorrectGrid.transform.GetChild(i).transform.localScale = new(CorrectGrid.transform.GetChild(i).transform.localScale.x, -CorrectGrid.transform.GetChild(i).transform.localScale.y, CorrectGrid.transform.GetChild(i).transform.localScale.z);
                        GameGrid.transform.GetChild(i).transform.localScale = new(CorrectGrid.transform.GetChild(i).transform.localScale.x, CorrectGrid.transform.GetChild(i).transform.localScale.y, CorrectGrid.transform.GetChild(i).transform.localScale.z);
                        GameGrid.transform.GetChild(i).transform.rotation = Quaternion.Euler(0, 0, -90);
                    }
                }
                else if (col < Width / 2)
                {
                    Vector3 symmetricalRotation = GameObject.Find($"{col}-{Height-1-row}").transform.eulerAngles;
                    
                    CorrectGrid.transform.GetChild(i).transform.eulerAngles = symmetricalRotation;

                    if (symmetricalRotation.z != 90 && symmetricalRotation.z != 270)
                    {
                        CorrectGrid.transform.GetChild(i).transform.localScale = new(CorrectGrid.transform.GetChild(i).transform.localScale.x, -CorrectGrid.transform.GetChild(i).transform.localScale.y, CorrectGrid.transform.GetChild(i).transform.localScale.z);
                        GameGrid.transform.GetChild(i).transform.localScale = new(CorrectGrid.transform.GetChild(i).transform.localScale.x, CorrectGrid.transform.GetChild(i).transform.localScale.y, CorrectGrid.transform.GetChild(i).transform.localScale.z);
                        GameGrid.transform.GetChild(i).transform.rotation = Quaternion.Euler(0, 0, -90);
                    }
                    else
                    {
                        CorrectGrid.transform.GetChild(i).transform.localScale = new(-CorrectGrid.transform.GetChild(i).transform.localScale.x, CorrectGrid.transform.GetChild(i).transform.localScale.y, CorrectGrid.transform.GetChild(i).transform.localScale.z);
                        GameGrid.transform.GetChild(i).transform.localScale = new(CorrectGrid.transform.GetChild(i).transform.localScale.x, CorrectGrid.transform.GetChild(i).transform.localScale.y, CorrectGrid.transform.GetChild(i).transform.localScale.z);
                        GameGrid.transform.GetChild(i).transform.rotation = Quaternion.Euler(0, 0, 90);
                    }
                }
                else
                {
                    Vector3 symmetricalRotation = GameObject.Find($"{col}-{Height - 1 - row}").transform.eulerAngles;

                    CorrectGrid.transform.GetChild(i).transform.eulerAngles = symmetricalRotation;

                    CorrectGrid.transform.GetChild(i).transform.localScale = new(-CorrectGrid.transform.GetChild(i).transform.localScale.x, -CorrectGrid.transform.GetChild(i).transform.localScale.y, CorrectGrid.transform.GetChild(i).transform.localScale.z);
                    GameGrid.transform.GetChild(i).transform.localScale = new(CorrectGrid.transform.GetChild(i).transform.localScale.x, CorrectGrid.transform.GetChild(i).transform.localScale.y, CorrectGrid.transform.GetChild(i).transform.localScale.z);
                    GameGrid.transform.GetChild(i).transform.rotation = Quaternion.Euler(0, 0, 180);
                }
            }
            else

            {
                CorrectGrid.transform.GetChild(i).transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 4) * 90.0f);
            }
            col++;
        }

        //configuratie rooster toepassen
        if (PlayerPrefs.GetInt("rotate-assist") == 1)
        {
            for (int i = 0; i < CorrectGrid.transform.childCount; i++)
            {
                CorrectGrid.transform.GetChild(i).GetComponent<SpriteRenderer>().sprite = PartSprite;
            }
            for (int i = 0; i < GameGrid.transform.childCount; i++)
            {
                GameGrid.transform.GetChild(i).GetComponent<SpriteRenderer>().sprite = PartSprite;
            }
        }
    }

    private void Update()
    {
        if (GameInProgress)
        {
            //aanpassen camera grootte voor beeldverhouding
            //schaal de camera grootte zodat de lengte en hoogte van een standaard camera met beeldverhouding van 16:9 altijd in past
            float standardAspectRatio = 16.0f / 9.0f;
            //maak de camera niet kleiner, dan past de standaard hoogte niet meer
            float aspectMultiplier = Mathf.Max(standardAspectRatio / Camera.main.aspect, 1.0f);

            //Hou hele grid in beeld
            Camera.main.orthographicSize = Mathf.Max(Width * 2 + 3, Height + 3) * aspectMultiplier / 2.0f;

            //Hou achtergrond (hout) in beeld
            if (Background != null)
                Background.size = new Vector2(Camera.main.orthographicSize * 2.0f * Camera.main.aspect, Camera.main.orthographicSize * 2.0f);
        }
    }

    public void CheckGameGrid() //figuurstukken uit het voorbeeld grid vergelijken met het game grid, ze groen of rood kleuren en de score teruggeven
    {
        int correctCells = 0;
        for (int i = 0; i < CorrectGrid.transform.childCount; i++)
        {
            if (CorrectGrid.transform.GetChild(i).transform.rotation.eulerAngles.z == GameGrid.transform.GetChild(i).transform.rotation.eulerAngles.z)
            {
                GameGrid.transform.GetChild(i).GetComponent<SpriteRenderer>().color = Color.green;
                correctCells++;
            }
            else
            {
                GameGrid.transform.GetChild(i).GetComponent<SpriteRenderer>().color = Color.red;
            }
        }

        GameInProgress = false;
        EndScreenLogic.EndGame("RotateFigure", "Figuur draaien", $"{correctCells}/{CorrectGrid.transform.childCount}", Camera.main.orthographicSize * 1.75f, Camera.main.transform.position, 5);
        DontDestroyOnLoad(GameGrid.transform.parent);
        SceneManager.LoadScene("EndScreen");
    }
}
