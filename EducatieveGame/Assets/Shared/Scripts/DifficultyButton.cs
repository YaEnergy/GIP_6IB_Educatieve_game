using System;
using UnityEngine;

public class DifficultyButton : MonoBehaviour
{
    [SerializeField] private GameObject _check; //check van deze difficulty button

    public GameObject Check { get => _check; set => _check = value; } //check van deze difficulty button

    public void SetDifficulty(int difficulty) //moeilijkheidsgraad instellen
    {
        GameObject[] checks = GameObject.FindGameObjectsWithTag("Check");
        foreach (GameObject check in checks)
        {
            check.SetActive(false);
        }
        MenuLogic.SetDifficulty(difficulty);

        if (Check != null)
            Check.SetActive(true);
    }

    public void SetCustomDifficulty(string difficulty) //moeilijkheidsgraad instellen
    {
        GameObject[] checks = GameObject.FindGameObjectsWithTag("Check");

        foreach (GameObject check in checks)
        {
            //check of this checklist
            if (check.transform.parent.parent == transform.parent)
            {
                check.SetActive(false);
            }
        }
        string[] parts = difficulty.Split('-');
        PlayerPrefs.SetInt(parts[0], Convert.ToInt32(parts[1]));
        
        if (Check != null)
            Check.SetActive(true);
    }
}
