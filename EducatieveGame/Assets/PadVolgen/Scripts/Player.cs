using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private PathManager _pad;
    private Vector2 _currentPosition; //huidige positie speler
    private int _steps = 0;
    private int _wrongSteps = 0;
    private int _targetLocation = 1;

    private PathManager Pad { get => _pad; set => _pad = value; }
    public Vector2 CurrentPosition { get => _currentPosition; set => _currentPosition = value; }
    public int Steps { get => _steps; private set => _steps = value; }
    public int WrongSteps { get => _wrongSteps; private set => _wrongSteps = value; }
    public int TargetLocation { get => _targetLocation; set => _targetLocation = value; }

    private void Awake()
    {
        Pad = GameObject.FindWithTag("GameView").GetComponent<PathManager>();
    }

    public bool TryMove(PathTile tile, Vector2 tilePosition) //speler laten bewegen naar tile als dit mogelijk is
    {
        //lijst met posities van tiles rond de huidige tile
        List<Vector2> adjacentPositions = new List<Vector2> 
        {
        new Vector2(CurrentPosition.x + 1, CurrentPosition.y),
        new Vector2(CurrentPosition.x - 1, CurrentPosition.y),
        new Vector2(CurrentPosition.x, CurrentPosition.y + 1),
        new Vector2(CurrentPosition.x, CurrentPosition.y - 1)
        };

        //als het een tile is die naast de tile van de speler ligt en dit geen obstakel is, dan de speler verplaatsen
        if (Pad.Generator.Arrows && Pad.Finder.NextTile.Equals(tile))
        {
            transform.parent = tile.transform;
            StartCoroutine(MoveToParentPosition());
            CurrentPosition = tilePosition;
            Steps++;
            Pad.Finder.ShowNextArrow();
            return true;
        }
        else if (adjacentPositions.Contains(tilePosition) && !tile.IsObstacle && !Pad.Generator.Arrows)
        {
            transform.parent = tile.transform;
            StartCoroutine(MoveToParentPosition());
            CurrentPosition = tilePosition;
            Steps++;
            return true;
        }
        else if (Pad.Grid.GetTileAtPosition(CurrentPosition).Equals(tile) || !adjacentPositions.Contains(tilePosition))
        {
            return false;
        }
        else
        {
            WrongSteps++;
            return false;
        }
    }

    private IEnumerator MoveToParentPosition()
    {
        float timeElapsed = 0f;
        float duration = 0.4f;
        Vector3 startPos = transform.position;
        Vector3 targetPos = transform.parent.position;

        while (timeElapsed < duration)
        {
            float t = timeElapsed / duration;
            transform.position = Vector2.Lerp(startPos, targetPos, t);

            timeElapsed += Time.deltaTime;

            yield return null;
        }

        transform.position = targetPos;
    }
}
