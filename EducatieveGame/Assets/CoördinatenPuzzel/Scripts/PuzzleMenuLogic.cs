using SFB;
using System.Collections;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleMenuLogic : MenuLogic
{
    [SerializeField] private GameObject _pzButton; //knop van een puzzel
    [SerializeField] private Transform _puzzles; //de originele puzzels
    [SerializeField] private Transform _pokemonPuzzles; //de pokemon puzzels
    [SerializeField] private GameObject _deleteButton; //de verwijder knop
    [SerializeField] private Button _startButton; //de start knop
    [SerializeField] private TextMeshProUGUI _piecesText;

    [SerializeField] private GameObject[] puzzleTypeMenus; //puzzle soort menus
    [SerializeField] private Sprite[] puzzleTypeButtonSprites; //puzzle soort knop sprites
    [SerializeField] private Image puzzleSwitchTypeButtonIcon; //puzzle verander soort knop icoon

    private int currentPuzzleTypeSelection = 0;

    private static Sprite _puzzleImage; //afbeelding van puzzel
    private static string _puzzleName; //naam puzzel

    private GameObject PzButton { get => _pzButton; set => _pzButton = value; }
    private Transform Puzzles { get => _puzzles; set => _puzzles = value; }
    private Transform PokemonPuzzles { get => _pokemonPuzzles; set => _pokemonPuzzles = value; }
    private GameObject DeleteButton { get => _deleteButton; set => _deleteButton = value; }
    private Button StartButton { get => _startButton; set => _startButton = value; }
    private TextMeshProUGUI PiecesText { get => _piecesText; set => _piecesText = value; }

    public static Sprite PuzzleImage { get => _puzzleImage; set => _puzzleImage = value; }
    public static string PuzzleName { get => _puzzleName; set => _puzzleName = value; }

    private void Awake() //puzzels laden
    {
        AwakeBase();
        if (!Directory.Exists(Path.Combine(Application.persistentDataPath, "Puzzels")))
        {
            Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, "Puzzels"));
        }
        LoadPuzzles(Path.Combine(Application.streamingAssetsPath, "PuzzelGame/Puzzels"), "original");
        LoadPuzzles(Path.Combine(Application.persistentDataPath, "Puzzels"), "custom");

        GameObject pokemonPuzzles = GameObject.FindWithTag("Pokemon");
        GameObject pokemonPuzzlesContent = Instantiate(pokemonPuzzles, PokemonPuzzles);
        pokemonPuzzlesContent.transform.SetAsFirstSibling();
        PokemonPuzzles.GetComponent<ScrollRect>().content = pokemonPuzzlesContent.GetComponent<RectTransform>();
    }

    private void Start()
    {
        StartCoroutine(ResetScrollRect());
    }

    public void NextPuzzleTypeSelection()
    {
        puzzleTypeMenus[currentPuzzleTypeSelection].SetActive(false);

        currentPuzzleTypeSelection = (currentPuzzleTypeSelection + 1) % puzzleTypeMenus.Length;
        puzzleTypeMenus[currentPuzzleTypeSelection].SetActive(true);

        //zet puzzle switch button icoon sprite tot de volgende soort (in dit geval ook het vorige)
        puzzleSwitchTypeButtonIcon.sprite = puzzleTypeButtonSprites[(currentPuzzleTypeSelection + 1) % puzzleTypeButtonSprites.Length];
    }

    public void OpenDifficultySelection() //open difficulty menu met puzzel
    {
        StartButton.interactable = true;
        Menus[1].SetActive(true);
        Menus[CurrentMenu].SetActive(false);
        Image preview = GameObject.FindWithTag("Preview").GetComponent<Image>();
        preview.sprite = PuzzleImage;
        preview.preserveAspect = true;
        CalculatePieces(preview);
        DeleteButton.SetActive(PuzzleName.Contains("custom-"));
    }

    public void CalculatePieces(Image imageSprite)
    {
        Texture2D image = imageSprite.sprite.texture;
        float imageAspect = (float)image.width / image.height;

        int columns, rows;

        if (image.width > image.height) //landscape
        {
            columns = Mathf.Min(Difficulty * 3, image.width);
            rows = Mathf.RoundToInt(columns / imageAspect);
        }
        else //portrait
        {
            rows = Mathf.Min(Difficulty * 3, image.height);
            columns = Mathf.RoundToInt(rows * imageAspect);
        }
        PiecesText.text = "Aantal stukken: " + rows * columns;
    }

    public void CloseDifficultySelection() //sluit difficulty menu
    {
        StartButton.interactable = false;
        Menus[1].SetActive(false);
        Menus[CurrentMenu].SetActive(true);
    }

    private void AddPuzzle(Sprite puzzleImage, string name) //puzzelknop toevoegen met sprite en naam
    {
        PuzzleButton puzzleButton = Instantiate(PzButton, Vector3.zero, Quaternion.identity, Puzzles.transform).GetComponent<PuzzleButton>();
        puzzleButton.CreatePuzzle(puzzleImage);
        puzzleButton.name = name;
    }

    private void LoadPuzzles(string path, string name) //alle puzzels op gegeven pad laden
    {
        string[] pngFiles = Directory.GetFiles(path, "*.png");
        string[] jpgFiles = Directory.GetFiles(path, "*.jpg");
        string[] jpegFiles = Directory.GetFiles(path, "*.jpeg");
        string[] allImageFiles = pngFiles.Concat(jpgFiles).Concat(jpegFiles).ToArray();


        foreach (string imageFile in allImageFiles)
        {
            byte[] fileData = File.ReadAllBytes(imageFile);
            Texture2D texture = new(2, 2);
            texture.LoadImage(fileData);

            AddPuzzle(Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f)), name + "-" + Path.GetFileName(imageFile));
        }
    }

    public void AddCustomPuzzle() //een puzzel vanuit de verkenner toevoegen
    {
        var extensions = new[] { new ExtensionFilter("Afbeeldingen", "png", "jpg", "jpeg") };
        string[] filePaths = StandaloneFileBrowser.OpenFilePanel("Kies een afbeelding", "", extensions, false);
        if (filePaths != null && filePaths.Length > 0 && !string.IsNullOrEmpty(filePaths[0]))
        {
            string imagePath = filePaths[0];
            byte[] fileData = File.ReadAllBytes(imagePath);
            Texture2D texture = new(2, 2);
            texture.LoadImage(fileData);

            AddPuzzle(Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f)), "custom-" + Path.GetFileName(imagePath));
            File.Copy(imagePath, Path.Combine(Application.persistentDataPath, "Puzzels", Path.GetFileName(imagePath)), true);
        }

        ScrollRect scrollRect = Puzzles.transform.parent.GetComponentInChildren<ScrollRect>();
        if (scrollRect != null)
        {
            scrollRect.normalizedPosition = new Vector2(0, 0);

            Scrollbar scrollbar = scrollRect.verticalScrollbar;
            if (scrollbar != null)
            {
                scrollbar.value = 0;
            }
        }
    }

    public void DeletePuzzle() //een puzzel die vanuit de verkenner toegevoegd is verwijderen
    {
        File.Delete(Path.Combine(Application.persistentDataPath, "Puzzels", PuzzleName.Substring(7)));
        Menus[0].SetActive(true);
        Destroy(GameObject.Find(PuzzleName));
        Menus[0].SetActive(false);
    }

    private IEnumerator ResetScrollRect()
    {
        yield return null;

        ScrollRect scrollRect = Puzzles.transform.parent.GetComponent<ScrollRect>();
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 1;

            Scrollbar scrollbar = scrollRect.verticalScrollbar;
            if (scrollbar != null)
            {
                scrollbar.value = 1;
            }
        }
    }
}
