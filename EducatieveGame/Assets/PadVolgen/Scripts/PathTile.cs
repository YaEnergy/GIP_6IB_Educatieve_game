using System;
using System.Collections;
using System.Net.NetworkInformation;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PathTile : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRend; //spriterenderer
    [SerializeField] private GameObject _mouseHighlight; //highlight
    [SerializeField] private GameObject _pathHighlight; //highlight debug
    [SerializeField] private GameObject _playerHighlight; //highlight debug
    [SerializeField] public GameObject _finishParticle;
    [SerializeField] public GameObject _locationParticle;
    private bool _isFinish = false; //of de tile de finish is
    private bool _isObstacle = false; //of de tile een obstakel is
    private bool _isCheckpoint = false; //of de tile een locatie is
    private bool _visited = false; //of de tile al bezocht is
    private PathManager _pad; //het pad

    private static bool _cooldown = false;

    public SpriteRenderer SpriteRend { get => _spriteRend; set => _spriteRend = value; }
    public GameObject MouseHighlight { get => _mouseHighlight; set => _mouseHighlight = value; }
    public GameObject PathHighlight { get => _pathHighlight; set => _pathHighlight = value; }
    private GameObject PlayerHighlight { get => _playerHighlight; set => _playerHighlight = value; }
    public bool IsFinish { get => _isFinish; set => _isFinish = value; }
    public bool IsObstacle { get => _isObstacle; set => _isObstacle = value; }
    public bool IsCheckpoint { get => _isCheckpoint; set => _isCheckpoint = value; }
    public bool Visited { get => _visited; private set => _visited = value; }
    private PathManager Pad { get => _pad; set => _pad = value; }
    private static bool Cooldown { get => _cooldown; set => _cooldown = value; }

    private void Awake() //highlight uitzetten
    {
        Pad = GameObject.FindWithTag("GameView").GetComponent<PathManager>();
        MouseHighlight.SetActive(false);
    }

    private void OnMouseEnter() //highlight aanzetten
    {
        if (enabled)
        {
            MouseHighlight.SetActive(true);
        }
    }

    private void OnMouseExit() //highlight uitzetten
    {
        if (enabled)
        {
            MouseHighlight.SetActive(false);
        }
    }

    private void OnMouseDown() //speler zoeken en hem proberen verplaatsen naar deze tile als dat mogelijk is
    {
        if (enabled && ! Cooldown)
        {
            bool success = GameObject.FindWithTag("Player").GetComponent<Player>().TryMove(this, Pad.Grid.GetTilePosition(this));

            if (success)
            {
                StartCoroutine(StartCooldown());
                int totalSteps = GameObject.FindWithTag("Player").GetComponent<Player>().Steps + GameObject.FindWithTag("Player").GetComponent<Player>().WrongSteps;
                GameObject playerHighlight = Instantiate(PlayerHighlight, transform.position, Quaternion.identity, transform);
                PlayerHighlight.transform.SetAsLastSibling();
                playerHighlight.GetComponent<SpriteRenderer>().color = new(1, 0.9f, 0f, 0.44f);
                playerHighlight.transform.localScale = new(0.1f + totalSteps / (20f * MenuLogic.Difficulty), 0.1f + totalSteps/ (20f * MenuLogic.Difficulty), 0);
            }

            if (!Pad.Generator.RandomOrder)
            {
                if (IsCheckpoint && success && !Visited && GetComponentInChildren<TextMeshPro>().text.Equals(GameObject.FindWithTag("Player").GetComponent<Player>().TargetLocation.ToString()))
                {
                    Visited = true;
                    GameObject.FindWithTag("Player").GetComponent<Player>().TargetLocation++;
                    StartCooldown();
                    Instantiate(_locationParticle, transform.position, transform.rotation);
                    Destroy(transform.GetChild(4).gameObject);
                }
            }
            else
            {
                if (IsCheckpoint && success && !Visited)
                {
                    Visited = true;
                    GameObject.FindWithTag("Player").GetComponent<Player>().TargetLocation++;
                    StartCooldown();
                    Instantiate(_locationParticle, transform.position, transform.rotation);
                    Destroy(transform.GetChild(4).gameObject);
                    Pad.Checkpoints.Insert(Pad.Checkpoints.Count - 1, this);
                }
            }

            if (IsFinish && success)
            {
                bool everyLocationVisited = true;

                foreach (PathTile checkpoint in Pad.Checkpoints)
                {
                    // kijken of de locatie bezocht is
                    if (checkpoint.IsCheckpoint && !checkpoint.Visited)
                    {
                        everyLocationVisited = false;
                        break;
                    }
                }
                if (Pad.Generator.RandomOrder)
                {
                    foreach (PathTile checkpoint in Pad.RandomPath)
                    {
                        // kijken of de locatie bezocht is
                        if (checkpoint.IsCheckpoint && !checkpoint.Visited)
                        {
                            everyLocationVisited = false;
                            break;
                        }
                    }
                }
                // kijken of alle locaties bezocht zijn
                if (everyLocationVisited)
                {
                    Pad.Generator.ShowAStarPath(new(0.9f, 0f, 0f, 0.44f));
                    if (Pad.Generator.Arrows && !Pad.Generator.ScrambledOrder)
                    {
                        StartCoroutine(EndGame());
                    }
                    else
                    {
                        StartCoroutine(EndGame());
                    }
                }
            }
        }
    }

    public void SetTile(Color color, bool isObstacle, bool isLocation) //tile instellen met kleur, of het een obstakel is, of het een locatie is en de locatienaam
    {
        SpriteRend.color = color;
        IsObstacle = isObstacle;
        IsCheckpoint = isLocation;
    }

    private IEnumerator EndGame()
    {
        Pad.Grid.DisableAllTiles();
        yield return new WaitForSeconds(0.4f);
        Instantiate(_finishParticle, transform.position, transform.rotation);
        yield return new WaitForSeconds(0.4f);
        Pad.WinGame();
    }

    private IEnumerator StartCooldown()
    {
        Cooldown = true;
        yield return new WaitForSeconds(0.4f);
        Cooldown = false;
    }
}
