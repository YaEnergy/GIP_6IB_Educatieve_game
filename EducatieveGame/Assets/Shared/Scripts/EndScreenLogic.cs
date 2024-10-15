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
    private static string _currentGame = "MainMenu"; //huidig spel scenename
    private static string _gameName = "Eindscherm"; //huidig spel naam
    private static string _score = "0/0%"; //behaalde score
    private static float _cameraSize = 5; //camera grootte instelling
    private static Vector3 _cameraPos = Vector3.zero; //camera positie instelling
    private static float _offsetY = 5; //camera Y offset

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
    private static float CameraSize { get => _cameraSize; set => _cameraSize = value; }
    private static Vector3 CameraPos { get => _cameraPos; set => _cameraPos = value; }
    private static float OffsetY { get => _offsetY; set => _offsetY = value; }

    private void Awake() //eindscherm instellen
    {
        AwakeBase();
        Camera.main.transform.position = CameraPos;
        Difficultys.GetChild(Difficulty - 1).gameObject.SetActive(true);
        ScoreText.text = Score;
        TitleText.text = GameName;
        GameObject gameView = GameObject.FindWithTag("GameView");
        if (gameView != null)
        {
            GameObject.FindWithTag("GameView").SetActive(true);
            GameObject.FindWithTag("GameView").transform.parent = GameView;
            GameObject.FindWithTag("GameView").transform.position = new(GameView.transform.position.x, GameView.transform.position.y - OffsetY, GameView.transform.position.z);
        }
        GameObject preview = GameObject.FindWithTag("Preview");
        if (preview != null)
        {
            GameObject.FindWithTag("Preview").SetActive(true);
            GameObject.FindWithTag("Preview").transform.SetParent(Preview);

            //FIX ME: Causes placement problems
            GameObject.FindWithTag("Preview").transform.position = new(Preview.transform.position.x, Preview.transform.position.y - OffsetY, Preview.transform.position.z);
        }
        Camera.main.orthographicSize = CameraSize;

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
                position.y -= 40 + OffsetY;
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

            GameObject selectedColors = GameStats.GetChild(4).GetChild(3).gameObject;
            Color[] usedColors = SortingMenuLogic.SelectedSortingColors;

            for (int i = 0; i < selectedColors.transform.childCount; i++)
            {
                if (usedColors.Contains(selectedColors.transform.GetChild(i).GetComponent<Image>().color))
                {
                    selectedColors.transform.GetChild(i).GetChild(0).GetComponent<Image>().enabled = true;
                }
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

    public void NewGame() //scene laden van laatst gespeelde spel
    {
        SceneManager.LoadScene(CurrentGame);
    }

    public static void EndGame(string sceneName, string gameName, string score, float cameraSize, Vector3 cameraPos, float offsetY) //spel beindigen waarden instellen
    {
        CurrentGame = sceneName;
        GameName = gameName;
        Score = score;
        CameraSize = cameraSize;
        CameraPos = cameraPos;
        OffsetY = offsetY;
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
