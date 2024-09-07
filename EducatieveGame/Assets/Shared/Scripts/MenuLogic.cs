using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public abstract class MenuLogic : MonoBehaviour
{
    [SerializeField] private GameObject[] _menus; //de menus
    [SerializeField] private Toggle[] _settingsToggles; //toggles van de instellingen
    [SerializeField] private string[] _settings; //namen instellingen
    [SerializeField] private bool _resetDifficulty; //of dit menu de moeilijkheidsgraad reset
    [SerializeField] private GameObject _confirmQuitMenu;
    [SerializeField] private TextMeshProUGUI _tabCounter;
    [SerializeField] private GameObject[] _infoTabs;
    [SerializeField] private GameObject _infoMenu;
    private bool _fromScript = false; //of toggle uit script getoggled wordt
    private bool _cooldown = false;
    private int _currentMenu = 0; //index van huidig menu
    private int _currentInfoTab = 0;

    private static int _difficulty = 1; //de moeilijkheidsgraad

    protected GameObject[] Menus { get => _menus; set => _menus = value; }
    protected Toggle[] SettingsToggles { get => _settingsToggles; set => _settingsToggles = value; }
    protected string[] Settings { get => _settings; set => _settings = value; }
    protected bool ResetDifficulty { get => _resetDifficulty; set => _resetDifficulty = value; }
    protected GameObject ConfirmQuitMenu { get => _confirmQuitMenu; set => _confirmQuitMenu = value; }
    protected TextMeshProUGUI TabCounter { get => _tabCounter; set => _tabCounter = value; }
    protected GameObject[] InfoTabs { get => _infoTabs; set => _infoTabs = value; }
    protected GameObject InfoMenu { get => _infoMenu; set => _infoMenu = value; }
    protected bool FromScript { get => _fromScript; set => _fromScript = value; }
    protected bool Cooldown { get => _cooldown; set => _cooldown = value; }
    protected int CurrentMenu { get => _currentMenu; set => _currentMenu = value; }
    protected int CurrentInfoTab { get => _currentInfoTab; set => _currentInfoTab = value; }
    public static int Difficulty { get => _difficulty; private set => _difficulty = value; }

    private RectTransform infoMenuRect;

    protected void AwakeBase() //difficulty op standaardwaarde en settings toggles instellen, en pak RectTransform van de info menu voor animaties
    {
        if (ResetDifficulty)
        {
            Difficulty = 1;
        }

        FromScript = true;
        for (int i = 0; i < SettingsToggles.Length; i++)
        {
            if (PlayerPrefs.GetInt(Settings[i]) == 1)
            {
                SettingsToggles[i].isOn = true;
            }
        }
        FromScript = false;

        infoMenuRect = InfoMenu.GetComponent<RectTransform>();
    }

    public virtual void OpenMenu(int index) //open menu met index
    {
        Menus[CurrentMenu].SetActive(false);
        Menus[index].SetActive(true);
        CurrentMenu = index;
    }

    public void Close(int index) //sluit gegeven menu en toon het standaard menu
    {
        Menus[index].SetActive(false);
        Menus[0].SetActive(true);
    }

    public virtual void ToggleSetting(int index) //instelling bijwerken
    {
        if (!FromScript)
        {
            if (PlayerPrefs.GetInt(Settings[index]) == 1)
            {
                PlayerPrefs.SetInt(Settings[index], 0);
            }
            else
            {
                PlayerPrefs.SetInt(Settings[index], 1);
            }
        }
    }

    public void LoadScene(string sceneName) //laad een scene met gegeven naam
    {
        SceneManager.LoadScene(sceneName);
    }

    public void Quit(bool confirm) //sluit het spel af
    {
        if (!ConfirmQuitMenu.activeSelf)
        {
            ConfirmQuitMenu.SetActive(true);
            Time.timeScale = 0f;
        }
        else if (confirm)
        {
            Application.Quit();
        }
        else
        {
            ConfirmQuitMenu.SetActive(false);
            Time.timeScale = 1f;
        }
    }

    public static void SetDifficulty(int difficulty) //difficulty instellen
    {
        Difficulty = difficulty;
    }

    public void OpenInfoMenu()
    {
        InfoMenu.SetActive(true);
    }

    public void CloseInfoMenu()
    {
        InfoMenu.SetActive(false);
    }

    public void ToggleInfoMenu()
    {
        InfoMenu.SetActive(!InfoMenu.activeSelf);
    }

    private void UpdateTabCounter(int value)
    {
        TabCounter.text = value.ToString();
    }

    private void TweenTabs(int enterIndex, int exitIndex, bool forward)
    {
        float infoMenuWidth = infoMenuRect.rect.width;
        
        if (forward)
        {
            //verplaatsen inkomende tab (van links naar recht)
            InfoTabs[enterIndex].transform.localPosition = new Vector3(infoMenuWidth, InfoTabs[enterIndex].transform.localPosition.y, InfoTabs[enterIndex].transform.localPosition.z);
            LeanTween.moveLocalX(InfoTabs[enterIndex], 0f, 1);

            //verplaatsen weggaande tab (van links naar rechts)
            InfoTabs[exitIndex].transform.localPosition = new Vector3(0, InfoTabs[exitIndex].transform.localPosition.y, InfoTabs[exitIndex].transform.localPosition.z);
            LeanTween.moveLocalX(InfoTabs[exitIndex], -infoMenuWidth, 1);
        }
        else
        {
            //verplaatsen inkomende tab (van rechts naar links)
            InfoTabs[enterIndex].transform.localPosition = new Vector3(-infoMenuWidth, InfoTabs[enterIndex].transform.localPosition.y, InfoTabs[enterIndex].transform.localPosition.z);
            LeanTween.moveLocalX(InfoTabs[enterIndex], 0f, 1);

            //verplaatsen weggaande tab (van rechts naar links)
            InfoTabs[exitIndex].transform.localPosition = new Vector3(0, InfoTabs[exitIndex].transform.localPosition.y, InfoTabs[exitIndex].transform.localPosition.z);
            LeanTween.moveLocalX(InfoTabs[exitIndex], infoMenuWidth, 1);
        }
    }

    public void ButtonForwardsClicked()
    {
        if (!Cooldown)
        {
            int _exitingTabIndex;

            //de actieve pagina-indexwaarde updaten
            if (CurrentInfoTab + 1 < InfoTabs.Length)
            {
                CurrentInfoTab += 1;
            }
            else
            {
                CurrentInfoTab = 0;
            }

            //het uitzetten van alle tabs niet nodig voor de animaties
            for (int i = 0; i < InfoTabs.Length; i++)
            {
                InfoTabs[i].SetActive(false);
            }

            //de actieve tab wordt aangezet
            InfoTabs[CurrentInfoTab].SetActive(true);

            //de vorige tab wordt aangezet (checken welke waarde voorop was zonder errors)
            if (CurrentInfoTab - 1 == -1)
            {
                InfoTabs[InfoTabs.Length - 1].SetActive(true);
                _exitingTabIndex = InfoTabs.Length - 1;
            }
            else
            {
                InfoTabs[CurrentInfoTab - 1].SetActive(true);
                _exitingTabIndex = CurrentInfoTab - 1;
            }

            TweenTabs(CurrentInfoTab, _exitingTabIndex, true);
            UpdateTabCounter(CurrentInfoTab + 1);
            Cooldown = true;
            StartCoroutine(ButtonCooldown());
        }
    }

    public void ButtonBackwardsClicked()
    {
        if (!Cooldown)
        {
            int _exitingTabIndex;

            if (CurrentInfoTab - 1 >= 0)
            {
                CurrentInfoTab -= 1;
            }
            else
            {
                CurrentInfoTab = InfoTabs.Length - 1;
            }

            //het uitzetten van alle tabs niet nodig voor de animaties
            for (int i = 0; i < InfoTabs.Length; i++)
            {
                InfoTabs[i].SetActive(false);
            }

            //de actieve tab wordt aangezet
            InfoTabs[CurrentInfoTab].SetActive(true);

            //de volgende tab wordt aangezet (checken welke waarde voorop was zonder errors)
            if (CurrentInfoTab + 1 == InfoTabs.Length)
            {
                InfoTabs[0].SetActive(true);
                _exitingTabIndex = 0;
            }
            else
            {
                InfoTabs[CurrentInfoTab + 1].SetActive(true);
                _exitingTabIndex = CurrentInfoTab + 1;
            }

            TweenTabs(CurrentInfoTab, _exitingTabIndex, false);
            UpdateTabCounter(CurrentInfoTab + 1);
            Cooldown = true;
            StartCoroutine(ButtonCooldown());
        }
    }

    private IEnumerator ButtonCooldown()
    {
        yield return new WaitForSecondsRealtime(1f);
        Cooldown = false;
    }
}
