using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PuzzleManager : MonoBehaviour
{
    [SerializeField] private PuzzleSlicer _puzzleSlicer; //de puzzel slicer
    [SerializeField] private GameObject _puzzleSlot; //slot voor puzzelstuk
    [SerializeField] private GameObject _cornerSlot; //slot in de hoek
    [SerializeField] private GameObject _coordinateSlot; //slot met coordinaat
    [SerializeField] private TextMeshProUGUI _scoreText; //text met de score
    [SerializeField] private TextMeshProUGUI _checkButtonText; //text van controleerknop
    [SerializeField] private GameObject _coordinates; //gameobject coordinaten display
    private bool _extraChance = true; //extra kans om puzzel te maken
    private bool _resetting = false;
    private List<GameObject> _puzzleSlots; //de vakjes voor puzzelstukken

    private int rows;
    private int columns;

    private Transform grid;
    private RectTransform gridRect;

    private GridLayoutGroup puzzlePiecesViewGridLayout;

    private PuzzleSlicer PuzzleSlicer { get => _puzzleSlicer; set => _puzzleSlicer = value; }
    private GameObject PuzzleSlot { get => _puzzleSlot; set => _puzzleSlot = value; }
    private GameObject CornerSlot { get => _cornerSlot; set => _cornerSlot = value; }
    private GameObject CoordinateSlot { get => _coordinateSlot; set => _coordinateSlot = value; }
    private TextMeshProUGUI ScoreText { get => _scoreText; set => _scoreText = value; }
    private TextMeshProUGUI CheckButtonText { get => _checkButtonText; set => _checkButtonText = value; }
    private GameObject Coordinates { get => _coordinates; set => _coordinates = value; }
    private bool ExtraChance { get => _extraChance; set => _extraChance = value; }
    private bool Resetting { get => _resetting; set => _resetting = value; }
    private List<GameObject> PuzzleSlots { get => _puzzleSlots; set => _puzzleSlots = value; }

    void Awake()
    {
        int difficulty = MenuLogic.Difficulty;

        //hier kan je nog width, height en de afbeelding ophalen
        (columns, rows) = PuzzleSlicer.SliceImage(PuzzleMenuLogic.PuzzleImage.texture, difficulty * 3, difficulty * 3); //slice afbeelding met scale, aantal kolommen en aantal rijen

        //lijst van de de stukjes
        List<GameObject> parts = new();

        //lijsten instellen
        for (int i = 0; i < PuzzleSlicer.transform.GetChild(0).childCount; i++)
        {
            parts.Add(PuzzleSlicer.transform.GetChild(0).GetChild(i).gameObject);
        }

        //posities stukjes randomiseren
        for (int i = parts.Count - 1; i >= 0; i--)
        {
            int randomIndex = Random.Range(0, parts.Count);
            parts[randomIndex].transform.SetAsFirstSibling();
            parts.RemoveAt(randomIndex);
        }
        
        grid = transform.GetChild(0);
        grid.GetComponent<GridLayoutGroup>().constraintCount = columns + 1;
        gridRect = grid.GetComponent<RectTransform>();

        puzzlePiecesViewGridLayout = PuzzleSlicer.transform.GetChild(0).GetComponent<GridLayoutGroup>();

        PuzzleSlots = new();

        //voor elke rij
        for (int row = -1; row < rows; row++)
        {
            //voor elke kolom
            for (int col = -1; col < columns; col++)
            {
                if (col == -1 && row == -1)
                {
                    Instantiate(CornerSlot, Vector3.zero, Quaternion.identity, transform.GetChild(0).transform);
                }
                else if (row == -1)
                {
                    TextMeshProUGUI coords = Instantiate(CoordinateSlot, Vector3.zero, Quaternion.identity, transform.GetChild(0).transform).GetComponentInChildren<TextMeshProUGUI>();
                    coords.text = PuzzleSlicer.IntToChar(col + 1).ToString();
                }
                else if (col == -1)
                {
                    TextMeshProUGUI coords = Instantiate(CoordinateSlot, Vector3.zero, Quaternion.identity, transform.GetChild(0).transform).GetComponentInChildren<TextMeshProUGUI>();
                    coords.text = (row + 1).ToString();
                }
                else
                {
                    PuzzleSlots.Add(Instantiate(PuzzleSlot, Vector3.zero, Quaternion.identity, transform.GetChild(0).transform));
                    PuzzleSlots[^1].name = $"{PuzzleSlicer.IntToChar(col + 1)}-{row + 1}";
                }
            }
        }
    }

    private void Update()
    {
        //Update grid scale
        //Base puzzle size: 160x160

        float basePuzzleSize = 160.0f;

        float gridScale = Mathf.Min(gridRect.rect.height / (basePuzzleSize * (rows + 1)), gridRect.rect.width / (basePuzzleSize * (columns + 1)));
        
        grid.localScale = new(gridScale, gridScale, 1.0f);
        puzzlePiecesViewGridLayout.cellSize = new(basePuzzleSize * gridScale, basePuzzleSize * gridScale);
    }

    public void CheckPuzzleToggle()
    {
        if (CheckButtonText.text.Equals("C"))
        {
            CheckPuzzle();
        }
        else
        {
            ResetPuzzle();
        }
    }

    private void CheckPuzzle() //puzzel controleren
    {
        int score = 0;
        for (int i = 0; i < PuzzleSlots.Count; i++)
        {
            if (PuzzleSlots[i].transform.childCount > 0)
            {
                if (PuzzleSlots[i].name.Equals(PuzzleSlots[i].transform.GetChild(0).name))
                {
                    PuzzleSlots[i].transform.GetChild(0).GetComponent<Image>().color = Color.green;
                    PuzzleSlots[i].GetComponent<Image>().color = Color.green;
                    score++;
                }
                else
                {
                    PuzzleSlots[i].transform.GetChild(0).GetComponent<Image>().color = Color.red;
                    PuzzleSlots[i].GetComponent<Image>().color = Color.red;
                }
            }
            else
            {
                PuzzleSlots[i].GetComponent<Image>().color = Color.red;
            }
        }

        CheckButtonText.text = "R";
        Resetting = true;
        Coordinates.SetActive(false);
        ScoreText.text = score + "/" + PuzzleSlots.Count;

        GameObject[] puzzlePieces = GameObject.FindGameObjectsWithTag("PuzzlePiece");

        foreach (GameObject piece in puzzlePieces)
        {
            piece.GetComponent<PuzzlePiece>().enabled = false;
        }

        if (!ExtraChance || score == PuzzleSlots.Count)
        {
            EndGame(ScoreText.text);
        }
        ExtraChance = false;
    }

    private void ResetPuzzle() //puzzel resetten om verder te spelen met extra kans
    {
        for (int i = 0; i < PuzzleSlots.Count; i++)
        {
            if (PuzzleSlots[i].transform.childCount > 0)
            {
                PuzzleSlots[i].transform.GetChild(0).GetComponent<Image>().color = Color.white;
                if (!PuzzleSlots[i].name.Equals(PuzzleSlots[i].transform.GetChild(0).name))
                {
                    PuzzleSlots[i].transform.GetChild(0).SetParent(PuzzleSlicer.transform.GetChild(0));
                }
            }

            PuzzleSlots[i].GetComponent<Image>().color = new(1, 1, 0);
        }

        CheckButtonText.text = "C";
        Resetting = false;
        Coordinates.SetActive(true);
        ScoreText.text = "";

        GameObject[] puzzlePieces = GameObject.FindGameObjectsWithTag("PuzzlePiece");

        foreach (GameObject piece in puzzlePieces)
        {
            piece.GetComponent<PuzzlePiece>().enabled = true;
        }
    }

    private void EndGame(string score) //spel beingigen
    {
        EndScreenLogic.EndGame("PuzzelGameMenu", "Coördinaten puzzel", $"{score}");
        GameObject preview = GameObject.FindWithTag("Preview");
        preview.transform.SetParent(null);
        preview.transform.localScale = new(preview.transform.localScale.x * 0.75f, preview.transform.localScale.y * 0.75f, 1);
        DontDestroyOnLoad(preview);
        SceneManager.LoadScene("EndScreen");
    }

    public void DisablePieces()
    {
        if (!Resetting)
        {
            GameObject[] puzzlePieces = GameObject.FindGameObjectsWithTag("PuzzlePiece");

            foreach (GameObject piece in puzzlePieces)
            {
                piece.GetComponent<PuzzlePiece>().enabled = false;
            }
        }
    }

    public void EnablePieces()
    {
        if (!Resetting)
        {
            GameObject[] puzzlePieces = GameObject.FindGameObjectsWithTag("PuzzlePiece");

            foreach (GameObject piece in puzzlePieces)
            {
                piece.GetComponent<PuzzlePiece>().enabled = true;
            }
        }
    }
}
