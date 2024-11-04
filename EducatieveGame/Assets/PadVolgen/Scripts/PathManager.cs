using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PathManager : MonoBehaviour
{
    [SerializeField] private PathFunctions _functions;
    [SerializeField] private PathAStar _aStar;
    [SerializeField] private PathGenerator _generator;
    [SerializeField] private PathGrid _grid;
    [SerializeField] private PathFinder _finder;
    [SerializeField] private int _locations;
    [SerializeField] private SpriteRenderer _background;
    private List<PathTile> _randomPath = new(); //lijst met tiles van het pad
    private List<PathTile> _AStarPath = new(); //lijst met tiles van het A Star pad
    private List<PathTile> _checkpoints = new(); //tiles start, locaties en einde
    
    private SpriteRenderer Background { get => _background; set => _background = value; }

    private bool gameEnded = false;

    public PathFunctions Functions { get => _functions; private set => _functions = value; }
    public PathAStar AStar { get => _aStar; private set => _aStar = value; }
    public PathGenerator Generator { get => _generator; private set => _generator = value; }
    public PathGrid Grid { get => _grid; private set => _grid = value; }
    public PathFinder Finder { get => _finder; private set => _finder = value; }
    public int Locations { get => _locations; set => _locations = value; }
    public List<PathTile> RandomPath { get => _randomPath; set => _randomPath = value; }
    public List<PathTile> AStarPath { get => _AStarPath; set => _AStarPath = value; }
    public List<PathTile> Checkpoints { get => _checkpoints; set => _checkpoints = value; }

    private void Start()
    {
        Grid.GenerateGrid();
        Generator.GeneratePath(Generator.SpawnPlayer(), MenuLogic.Difficulty);
    }

    private void Update()
    {
        //aanpassen camera grootte voor beeldverhouding

        if (!gameEnded)
        {
            //Hou hele grid in beeld met margin percentage
            Camera.main.orthographicSize = CameraAspectRatioHelper.OrthographicSizeEnveloppeRect((Grid.Width + 2) / 0.8f, Grid.Height + 2, Camera.main.aspect);
            
            //Hou achtergrond (gras) in beeld
            if (Background != null)
                Background.size = new Vector2(Camera.main.orthographicSize * 2.0f * Camera.main.aspect, Camera.main.orthographicSize * 2.0f);
            
            //camera centreren op het grid
            Camera.main.transform.position = new Vector3((float)Grid.Width / 2 - 0.5f, (float)Grid.Height / 2 - 0.5f, -10);
        }

    }

    private double CalculateScore()
    {
        Player player = GameObject.FindWithTag("Player").GetComponent<Player>();

        if (Generator.Arrows && !Generator.ScrambledOrder)
            return Math.Round((double)RandomPath.Count / (player.Steps + player.WrongSteps), 2) * 100;
        else
            return Math.Round((double)AStarPath.Count / (player.Steps + player.WrongSteps), 2) * 100;
    }

    public void CancelGame()
    {
        EndScreenLogic.EndGame("PadVolgenMenu", "Pad volgen", "Finish niet behaald");
        EndGame();
    }

    public void WinGame()
    {
        EndScreenLogic.EndGame("PadVolgenMenu", "Pad volgen", CalculateScore().ToString() + "%");
        EndGame();
    }

    private void EndGame()
    {
        Grid.DisableAllTiles();

        GameObject gameview = GameObject.FindWithTag("GameView");
        gameview.transform.SetParent(null);
        gameview.transform.localScale = new(gameview.transform.localScale.x, gameview.transform.localScale.y, 1);

        EndScreenLogic.SetGameViewCameraOptions(new((Grid.Width * 2) / 0.6f, Grid.Height / 0.54f), Vector2.zero, new Vector3(Grid.Width * 0.7f - 0.5f, Grid.Height * 0.65f - 0.5f, 0.0f));

        gameEnded = true;
        DontDestroyOnLoad(gameview);
        SceneManager.LoadScene("EndScreen");
    }
}
