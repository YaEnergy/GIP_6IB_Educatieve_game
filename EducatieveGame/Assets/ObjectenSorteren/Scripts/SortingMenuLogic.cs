using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SortingMenuLogic : MenuLogic
{
    [SerializeField] private GameObject[] _gameColors; //de kleuren voor het spel
    [SerializeField] private GameObject _colorsWarning; //tekst waarschuwing te weinig geselecteerde kleuren
    [SerializeField] private Button _startButton; //knop om spel te starten
    private int _enabledColors = 3; //aantal geselecteerde kleuren

    private static Color[] _sortingColors; //de kleuren voor het spel
    private static Color[] _selectedSortingColors; //de geselecteerde kleuren voor het spel
    private static string[] _sortingTexts; //de kleur teksten voor het spel
    private static string[] _selectedSortingTexts; //de geselecteerde kleur teksten voor het spel

    private GameObject[] GameColors { get => _gameColors; set => _gameColors = value; }
    private GameObject ColorsWarning { get => _colorsWarning; set => _colorsWarning = value; }
    private Button StartButton { get => _startButton; set => _startButton = value; }
    private int EnabledColors { get => _enabledColors; set => _enabledColors = value; }
    public static Color[] SortingColors { get => _sortingColors; private set => _sortingColors = value; }
    public static Color[] SelectedSortingColors { get => _selectedSortingColors; set => _selectedSortingColors = value; }
    public static string[] SortingTexts { get => _sortingTexts; private set => _sortingTexts = value; }
    public static string[] SelectedSortingTexts { get => _selectedSortingTexts; private set => _selectedSortingTexts = value; }

    private void Awake() //menu instellen
    {
        AwakeBase();
    }

    private void Update() //nakijken of er genoeg kleuren zijn geselecteerd
    {
        if (ColorsWarning != null && StartButton != null)
        {
            if (Difficulty + 1 > EnabledColors)
            {
                ColorsWarning.SetActive(true);
                StartButton.interactable = false;
            }
            else
            {
                ColorsWarning.SetActive(false);
                StartButton.interactable = true;
            }
        }
    }

    public void ToggleColor(int index) //kleur aan/uit zetten
    {
        if (GameColors[index].transform.GetChild(1).GetComponent<Image>().enabled)
        {
            GameColors[index].transform.GetChild(1).GetComponent<Image>().enabled = false;
            EnabledColors--;
        }
        else
        {
            GameColors[index].transform.GetChild(1).GetComponent<Image>().enabled = true;
            EnabledColors++;
        }
    }

    public void LoadColors() //kleuren laden
    {
        List<Color> sortingColors = new();
        List<Color> selectedSortingColors = new();
        List<string> sortingTexts = new();
        List<string> selectedSortingTexts = new();
        for (int i = 0; i < GameColors.Length; i++)
        {
            if (GameColors[i].transform.GetChild(1).GetComponent<Image>().enabled)
            {
                Transform colorTransform = GameColors[i].transform.GetChild(2);
                Image colorImage = colorTransform.GetComponent<Image>();

                sortingColors.Add(colorImage.color);
                selectedSortingColors.Add(colorImage.color);
                sortingTexts.Add(GameColors[i].transform.name);
                selectedSortingTexts.Add(GameColors[i].transform.name);
            }
            else
            {
                sortingColors.Add(GameColors[i].transform.GetComponent<Image>().color);
                sortingTexts.Add(GameColors[i].transform.name);
            }
        }

        SortingColors = sortingColors.ToArray();
        SelectedSortingColors = selectedSortingColors.ToArray();
        SortingTexts = sortingTexts.ToArray();
        SelectedSortingTexts = selectedSortingTexts.ToArray();
    }
}
