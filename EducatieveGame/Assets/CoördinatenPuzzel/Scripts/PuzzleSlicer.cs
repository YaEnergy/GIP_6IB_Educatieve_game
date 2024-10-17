using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PuzzleSlicer : MonoBehaviour, IDropHandler
{
    [SerializeField] private GameObject _puzzlePiece; //prefab voor een puzzelstuk

    private GameObject PuzzlePiece { get => _puzzlePiece; set => _puzzlePiece = value; }

    public (int, int) SliceImage(Texture2D image, int maxColumns, int maxRows) //afbeelding snijden in puzzelstukken
    {
        float imageAspect = (float)image.width / image.height;

        int columns, rows;

        if (image.width > image.height) //landscape
        {
            columns = Mathf.Min(maxColumns, image.width);
            rows = Mathf.RoundToInt(columns / imageAspect);
        }
        else //portrait
        {
            rows = Mathf.Min(maxRows, image.height);
            columns = Mathf.RoundToInt(rows * imageAspect);
        }

        //breedte en hoogte van 1 stuk
        int width = image.width / columns;
        int height = image.height / rows;

        //voor elke rij
        for (int row = 0; row < rows; row++)
        {
            //voor elke kolom
            for (int col = 0; col < columns; col++)
            {
                Texture2D texture = new(width, height);
                Color[] pixels = image.GetPixels(col * width, row * height, width, height);

                //voor elke pixel nakijken of hij niet transparant is
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        if (pixels[y * width + x].a < 0.5f)
                        {
                            pixels[y * width + x] = Color.white;
                        }
                    }
                }

                texture.SetPixels(pixels);
                texture.Apply();
                texture.filterMode = FilterMode.Point;

                //het puzzelstuk aanmaken
                GameObject imagePart = Instantiate(PuzzlePiece, Vector3.zero, Quaternion.identity, transform.GetChild(0).transform);

                //de schaal en sprite van het puzzelstuk instellen
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100);
                imagePart.GetComponent<Image>().sprite = sprite;
                imagePart.name = $"{IntToChar(col + 1)}-{rows - row}"; //naam van puzzelstuk instellen met coordinaten
            }
        }
        
        return (columns, rows);
    }

    public char IntToChar(int num)
    {
        return (char)('A' + num - 1);
    }

    public void OnDrop(PointerEventData eventData)
    {
        GameObject dropped = eventData.pointerDrag;
        if (dropped.GetComponent<PuzzlePiece>() != null)
        {
            PuzzlePiece piece = dropped.GetComponent<PuzzlePiece>();
            piece.ParentAfterDrag = transform.GetChild(0).transform;
        }
    }
}
