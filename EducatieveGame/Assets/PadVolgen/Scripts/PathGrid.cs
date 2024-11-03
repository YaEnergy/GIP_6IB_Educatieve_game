using System.Collections.Generic;
using UnityEngine;

//dit script houd het grid van het pad bij
public class PathGrid : MonoBehaviour
{
    [SerializeField] private int _width; //breedte grid
    [SerializeField] private int _height; //hoogte grid
    [SerializeField] private GameObject _tile; //een tile
    private Dictionary<Vector2, PathTile> _tiles = new(); //de tiles van het grid met hun posities
    [SerializeField] private Sprite _waterSprite;
    public int Width { get => _width; set => _width = value; }
    public int Height { get => _height; set => _height = value; }
    private GameObject Tile { get => _tile; set => _tile = value; }
    private Dictionary<Vector2, PathTile> Tiles { get => _tiles; set => _tiles = value; }

    private void Awake()
    {
        Width = MenuLogic.Difficulty * 5;
        Height = MenuLogic.Difficulty * 3;
    }

    public void GenerateGrid() //grid genereren
    {
        //oud grid verwijderen indien nodig
        Tiles.Clear();

        for (int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        //nieuw grid maken
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                GameObject spawnedTile = Instantiate(Tile, new Vector2(x, y), Quaternion.identity, transform);
                spawnedTile.name = x + "-" + y;

                Tiles[new Vector2(x, y)] = spawnedTile.GetComponent<PathTile>();
                spawnedTile.GetComponent<PathTile>().SetTile(Color.white, false, false);
            }
        }
    }

    public PathTile GetTileAtPosition(Vector2 pos) //tile op positie vinden
    {
        if (Tiles.TryGetValue(pos, out PathTile tile))
        {
            return tile;
        }

        return null;
    }

    public Vector2 GetTilePosition(PathTile tile) //positie van tile vinden
    {
        foreach (KeyValuePair<Vector2, PathTile> kvp in Tiles)
        {
            if (kvp.Value == tile)
            {
                return kvp.Key;
            }
        }

        return Vector2.zero;
    }

    public void DisableAllTiles()
    {
        foreach (KeyValuePair<Vector2, PathTile> kvp in Tiles)
        {
            kvp.Value.enabled = false;
        }
    }

    public void EnableAllTiles()
    {
        foreach (KeyValuePair<Vector2, PathTile> kvp in Tiles)
        {
            kvp.Value.enabled = true;
        }
    }
}
