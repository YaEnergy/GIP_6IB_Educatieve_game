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

    [Header("Rooster")]
    [SerializeField] private Vector2 _baseGridSize = Vector2.zero; //Basis Rooster grootte

    [Header("Cell")]
    [SerializeField] private bool _preserveCellAspectRatio = true; //Word de schaal van de cell behouden?
    [SerializeField] private Vector2 _baseCellSize = Vector2.zero; //Basis Cell grootte
    [SerializeField] private Vector2 _baseCellSpacing = Vector2.zero; //Basis Cell tussenruimte

    [Header("Padding")]
    [SerializeField] private bool _scalePadding = false; //Word de padding van de grid layout group ook aangepast?
    [SerializeField] private bool _preservePaddingAspectRatio = true; //Word de schaal van de padding behouden?
    [SerializeField] private RectOffset _basePadding; //Basis rooster padding

    public Vector2 BaseGridSize { get => _baseGridSize; set => _baseGridSize = value; } //Basis Rooster size

    public bool PreserveCellAspectRatio { get => _preserveCellAspectRatio; set => _preserveCellAspectRatio = value; } //Word de schaal van de cell behouden?
    public Vector2 BaseCellSize { get => _baseCellSize; set => _baseCellSize = value; } //Basis Cell grootte
    public Vector2 BaseCellSpacing { get => _baseCellSpacing; set => _baseCellSpacing = value; } //Basis Cell tussenruimte

    public bool ScalePadding { get => _scalePadding; set => _scalePadding = value; } //Word de padding van de grid layout group ook aangepast?
    public bool PreservePaddingAspectRatio { get => _preservePaddingAspectRatio; set => _preservePaddingAspectRatio = value; } //Word de schaal van de padding behouden?
    public RectOffset BasePadding { get => _basePadding; set => _basePadding = value; } //Basis rooster padding

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

        //X-schaal
        float scaleX = rectTransform.rect.width / BaseGridSize.x;

        //Y-schaal
        float scaleY = rectTransform.rect.height / BaseGridSize.y;

        if (PreserveCellAspectRatio)
        {
            //Schaal om te gebruiken, gebruik kleinste want de grootste kan buiten rooster grootte komen
            //er wordt één waarde gebruikt voor beide assen zodat de schaal van de cell grootte behouden wordt.
            float scale = Math.Min(scaleX, scaleY);

            gridLayoutGroup.cellSize = BaseCellSize * scale;
            gridLayoutGroup.spacing = BaseCellSpacing * scale;
        }
        else
        {
            //Verhouding van de cell grootte en tussenruimte wordt niet behouden.
            //De cell wordt uitgestrekt.

            gridLayoutGroup.cellSize = new(BaseCellSize.x * scaleX, BaseCellSize.y * scaleY);
            gridLayoutGroup.spacing = new(BaseCellSpacing.x * scaleX, BaseCellSpacing.y * scaleY);
        }

        if (PreservePaddingAspectRatio)
        {
            //Schaal om te gebruiken, gebruik kleinste want de grootste kan buiten rooster grootte komen
            //er wordt één waarde gebruikt voor beide assen zodat de schaal van de padding behouden wordt.
            float scale = Math.Min(scaleX, scaleY);

            gridLayoutGroup.padding.left = (int)((float)BasePadding.left * scale);
            gridLayoutGroup.padding.right = (int)((float)BasePadding.right * scale);
            gridLayoutGroup.padding.top = (int)((float)BasePadding.top * scale);
            gridLayoutGroup.padding.bottom = (int)((float)BasePadding.bottom * scale);
        }
        else
        {
            //Verhouding van de padding wordt niet behouden.
            //Padding wordt uitgestrekt.

            gridLayoutGroup.padding.left = (int)((float)BasePadding.left * scaleX);
            gridLayoutGroup.padding.right = (int)((float)BasePadding.right * scaleX);
            gridLayoutGroup.padding.top = (int)((float)BasePadding.top * scaleY);
            gridLayoutGroup.padding.bottom = (int)((float)BasePadding.bottom * scaleY);
        }
    }
}
