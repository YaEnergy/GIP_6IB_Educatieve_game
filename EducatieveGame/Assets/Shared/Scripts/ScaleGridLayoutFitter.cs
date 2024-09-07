using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Een monobehaviour voor GridLayoutGroup game objecten om een standaard rooster grootte, cell grootte en tussenruimte, te schalen voor elk rooster grootte.
/// </summary>
[RequireComponent(typeof(GridLayoutGroup))]
[RequireComponent(typeof(RectTransform))]
[DisallowMultipleComponent]
public class ScaleGridLayoutFitter : MonoBehaviour
{
    private GridLayoutGroup gridLayoutGroup;
    private RectTransform rectTransform;

    [SerializeField] private Vector2 _baseGridSize = Vector2.zero; //Basis Rooster grootte

    [SerializeField] private bool _preserveCellAspectRatio = true; //Word de schaal van de cell behouden?
    [SerializeField] private Vector2 _baseCellSize = Vector2.zero; //Basis Cell grootte
    [SerializeField] private Vector2 _baseCellSpacing = Vector2.zero; //Basis Cell tussenruimte

    public Vector2 BaseGridSize { get => _baseGridSize; set => _baseGridSize = value; } //Basis Rooster size

    public bool PreserveCellAspectRatio { get => _preserveCellAspectRatio; set => _preserveCellAspectRatio = value; } //Word de schaal van de cell behouden?
    public Vector2 BaseCellSize { get => _baseCellSize; set => _baseCellSize = value; } //Basis Cell grootte
    public Vector2 BaseCellSpacing { get => _baseCellSpacing; set => _baseCellSpacing = value; } //Basis Cell tussenruimte

    // Start is called before the first frame update
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        gridLayoutGroup = GetComponent<GridLayoutGroup>();
        UpdateGridLayoutGroup();
    }

    // Wordt gecalled wanneer de rect transform grootte wordt veranderd, en dus ook bij scherm grootte verandering doordat de rect transform grootte ook verandert (enkel bij stretch)
    void OnRectTransformDimensionsChange()
    {
        if (gridLayoutGroup != null)
            UpdateGridLayoutGroup();
    }

    private void UpdateGridLayoutGroup()
    {
        if (gridLayoutGroup == null)
            throw new NullReferenceException(nameof(gridLayoutGroup) + " is null!");
        
        if (PreserveCellAspectRatio)
        {
            //Schaal om te gebruiken, gebruik kleinste want de grootste kan buiten rooster grootte komen
            //er wordt één waarde gebruikt voor beide assen zodat de schaal van de cell grootte behouden wordt.
            float scale = Math.Min(rectTransform.rect.width / BaseGridSize.x, rectTransform.rect.height / BaseGridSize.y);

            gridLayoutGroup.cellSize = BaseCellSize * scale;
            gridLayoutGroup.spacing = BaseCellSpacing * scale;
        }
        else
        {
            //Verhouding van de cell grootte en tussenruimte wordt niet behouden.
            //De cell wordt uitgestrekt.

            //X-schaal
            float scaleX = rectTransform.rect.width / BaseGridSize.x;

            //Y-schaal
            float scaleY = rectTransform.rect.height / BaseGridSize.y;

            gridLayoutGroup.cellSize = new(BaseCellSize.x * scaleX, BaseCellSize.y * scaleY);
            gridLayoutGroup.spacing = new(BaseCellSpacing.x * scaleX, BaseCellSpacing.y * scaleY);
        }
    }
}
