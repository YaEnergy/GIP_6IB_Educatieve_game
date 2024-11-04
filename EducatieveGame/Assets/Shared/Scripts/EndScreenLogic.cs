using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System.Linq;

public class EndScreenLogic : MenuLogic
{
    [SerializeField] private Transform _difficultys; //de moeilijkheiden
    [SerializeField] private Transform _gameView; //preview zonder canvas
    [SerializeField] private Transform _preview; //preview met canvas
    [SerializeField] private Transform _gameStats; //statistieken spel
    [SerializeField] private TextMeshProUGUI _scoreText; //text met score
    [SerializeField] private TextMeshProUGUI _titleText; //text spelnaam
    [SerializeField] private Toggle[] _pathToggles;
    private RectTransform previewRect;

    private bool gameViewPresent = false; //Gameobject met GameView tag aanwezig
    private GameObject gamePreview = null;

    private static string _currentGame = "MainMenu"; //huidig spel scenename
    private static string _gameName = "Eindscherm"; //huidig spel naam
    private static string _score = "0/0%"; //behaalde score

    private static Vector2 _gameViewFitRectSize = Vector2.one; //grootte rechthoek waarin GameView past
    private static Vector2 _gameViewNormalizedCameraOffset = Vector2.zero; //camera gameview offset, waarbij (1, 1) = (ortho * aspect * 2, ortho)
    private static Vector3 _gameViewCameraOffset = Vector3.zero; //camera gameview offset in units

    private static Vector2 _previewBaseRectSize = new(1000, 1000); //grootte rechthoek van Preview gameobject bij schaal 1

    private Transform Difficultys { get => _difficultys; set => _difficultys = value; }
    private Transform GameView { get => _gameView; set => _gameView = value; }
    private Transform Preview { get => _preview; set => _preview = value; }
    private Transform GameStats { get => _gameStats; set => _gameStats = value; }
    private TextMeshProUGUI ScoreText { get => _scoreText; set => _scoreText = value; }
    private TextMeshProUGUI TitleText { get => _titleText; set => _titleText = value; }
    private Toggle[] PathToggles { get => _pathToggles; set => _pathToggles = value; }
    private static string CurrentGame { get => _currentGame; set => _currentGame = value; }
    private static string GameName { get => _gameName; set => _gameName = value; }
    private static string Score { get => _score; set => _score = value; }

    private static Vector2 GameViewFitRectSize { get => _gameViewFitRectSize; set => _gameViewFitRectSize = value; }
    private static Vector2 GameViewNormalizedCameraOffset { get => _gameViewNormalizedCameraOffset; set => _gameViewNormalizedCameraOffset = value; }
    private static Vector3 GameViewCameraOffset { get => _gameViewCameraOffset; set => _gameViewCameraOffset = value; }

    private static Vector3 PreviewBaseRectSize { get => _previewBaseRectSize; set => _previewBaseRectSize = value; }

    private void Awake() //eindscherm instellen
    {
        AwakeBase();

        Camera.main.transform.position = Vector3.zero;
        Camera.main.orthographicSize = 5.0f;

        Difficultys.GetChild(Difficulty - 1).gameObject.SetActive(true);
        ScoreText.text = Score;
        TitleText.text = GameName;

        GameObject gameView = GameObject.FindWithTag("GameView");
        if (gameView != null)
        {
            gameView.SetActive(true);
            gameView.transform.parent = GameView;
            gameViewPresent = true;
        }

        previewRect = Preview.GetComponent<RectTransform>();
        gamePreview = GameObject.FindWithTag("Preview");
        if (gamePreview != null)
        {
            gamePreview.SetActive(true);
            gamePreview.transform.SetParent(Preview);
            gamePreview.transform.position = new(Preview.transform.position.x, Preview.transform.position.y, Preview.transform.position.z);
        }
    
        if (CurrentGame.Equals("RotateFigure"))
        {
            GameStats.GetChild(0).gameObject.SetActive(true);
            if (PlayerPrefs.GetInt("symmetrical") == 1)
            {
                GameStats.GetChild(0).GetChild(0).GetChild(0).GetComponent<Toggle>().isOn = true;
            }
            if (PlayerPrefs.GetInt("rotate-assist") == 1)
            {
                GameStats.GetChild(0).GetChild(1).GetChild(0).GetComponent<Toggle>().isOn = true;
            }
        }
        else if (CurrentGame.Equals("SelectDrawMode"))
        {
            GameStats.GetChild(1).gameObject.SetActive(true);
            if (PlayerPrefs.GetInt("figure-assist") == 1)
            {
                GameStats.GetChild(1).GetChild(0).GetChild(0).GetComponent<Toggle>().isOn = true;
            }
            LineRenderer lineRend = GameView.GetChild(0).GetComponent<LineRenderer>();
            for (int i = 0; i < lineRend.positionCount; i++)
            {
                Vector3 position = lineRend.GetPosition(i);
                position.x -= 40;
                position.y -= 40;
                lineRend.SetPosition(i, position);
            } 
        }
        else if (CurrentGame.Equals("ReactionMenu"))
        {
            GameStats.GetChild(2).gameObject.SetActive(true);
            Difficultys.GetChild(Difficulty - 1).GetChild(0).GetChild(PlayerPrefs.GetInt("meat")-1).gameObject.SetActive(true);
            Difficultys.GetChild(Difficulty - 1).GetChild(1).GetChild(PlayerPrefs.GetInt("rate")-1).gameObject.SetActive(true);
            Difficultys.GetChild(Difficulty - 1).GetChild(2).GetChild(PlayerPrefs.GetInt("size")-1).gameObject.SetActive(true);
        }
        else if (CurrentGame.Equals("PuzzelGameMenu"))
        {
            GameStats.GetChild(3).gameObject.SetActive(true);
        }
        else if (CurrentGame.Equals("KleurGameMenu"))
        {
            GameStats.GetChild(4).gameObject.SetActive(true);
            if (PlayerPrefs.GetInt("sort-assist") == 1)
            {
                GameStats.GetChild(4).GetChild(0).GetChild(0).GetComponent<Toggle>().isOn = true;
            }
            if (PlayerPrefs.GetInt("trashcan") == 1)
            {
                GameStats.GetChild(4).GetChild(1).GetChild(0).GetComponent<Toggle>().isOn = true;
            }
            if (PlayerPrefs.GetInt("conveyor") == 1)
            {
                GameStats.GetChild(4).GetChild(2).GetChild(0).GetComponent<Toggle>().isOn = true;
            }

            GameObject selectedColors = GameStats.GetChild(4).GetChild(3).GetChild(0).GetChild(0).gameObject;
            Color[] usedColors = SortingMenuLogic.SelectedSortingColors;

            for (int i = 0; i < selectedColors.transform.childCount; i++)
            {
                Image colorImage = selectedColors.transform.GetChild(i).GetChild(2).GetComponent<Image>();
                Image colorCheck = selectedColors.transform.GetChild(i).GetChild(1).GetComponent<Image>();
                colorCheck.enabled = usedColors.Contains(colorImage.color);
            }
        }
        else if (CurrentGame.Equals("PadVolgenMenu"))
        {
            GameStats.GetChild(5).gameObject.SetActive(true);
            if (PlayerPrefs.GetInt("pad-assist") == 1)
            {
                GameStats.GetChild(5).GetChild(0).GetChild(0).GetComponent<Toggle>().isOn = true;
            }
            if (PlayerPrefs.GetInt("randomorder") == 1)
            {
                GameStats.GetChild(5).GetChild(1).GetChild(0).GetComponent<Toggle>().isOn = true;
            }
            if (PlayerPrefs.GetInt("chooseorder") == 1)
            {
                GameStats.GetChild(5).GetChild(2).GetChild(0).GetComponent<Toggle>().isOn = true;
            }
        }
    }

    private void Update()
    {
        if (gameViewPresent)
        {
            Camera.main.orthographicSize = CameraAspectRatioHelper.OrthographicSizeEnveloppeRect(GameViewFitRectSize, Camera.main.aspect);
            Camera.main.transform.position = new
            (
                Camera.main.orthographicSize * Camera.main.aspect * 2.0f * GameViewNormalizedCameraOffset.x + GameViewCameraOffset.x,
                Camera.main.orthographicSize * 2.0f * GameViewNormalizedCameraOffset.y + GameViewCameraOffset.y,
                GameViewCameraOffset.z - 10.0f
            );
        }

        if (gamePreview != null)
        {
            float gridScale = Mathf.Min(previewRect.rect.width / PreviewBaseRectSize.x, previewRect.rect.height / PreviewBaseRectSize.y);

            gamePreview.transform.localScale = new(gridScale, gridScale, 1.0f);
        }
    }

    public void NewGame() //scene laden van laatst gespeelde spel
    {
        SceneManager.LoadScene(CurrentGame);
    }

    public static void EndGame(string sceneName, string gameName, string score) //spel beindigen waarden instellen
    {
        CurrentGame = sceneName;
        GameName = gameName;
        Score = score;
    }

    public static void SetGameViewCameraOptions(Vector2 fitRectSize, Vector2 normalizedCameraOffset, Vector3 offset)
    {
        GameViewFitRectSize = fitRectSize;
        GameViewNormalizedCameraOffset = normalizedCameraOffset;
        GameViewCameraOffset = offset;
    }

    public static void SetPreviewUIOptions(Vector2 baseRectSize)
    {
        PreviewBaseRectSize = baseRectSize;
    }

    public void ToggleShortestPath()
    {
        GameObject[] highlights = GameObject.FindGameObjectsWithTag("AstarrHighlight");
        foreach (GameObject highlight in highlights)
        {
            highlight.GetComponent<SpriteRenderer>().enabled = PathToggles[0].isOn;
        }
    }

    public void ToggleOwnPath()
    {
        GameObject[] highlights = GameObject.FindGameObjectsWithTag("PlayerHighlight");
        foreach (GameObject highlight in highlights)
        {
            highlight.GetComponent<SpriteRenderer>().enabled = PathToggles[1].isOn;
        }
    }
}
