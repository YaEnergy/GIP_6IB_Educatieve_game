using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Een monobehaviour voor HorizontalLayoutGroup game objecten om een standaard rooster grootte, en cell tussenruimte, te schalen voor elk rooster grootte.
/// </summary>
[RequireComponent(typeof(HorizontalLayoutGroup))]
[RequireComponent(typeof(RectTransform))]
[DisallowMultipleComponent]
public class ScaleHorizontalLayoutFitter : MonoBehaviour
{
    private HorizontalLayoutGroup horizontalLayoutGroup;
    private RectTransform rectTransform;

    [Header("Rooster")]
    [SerializeField] private Vector2 _baseGridSize = Vector2.zero; //Basis Rooster grootte

    [Header("Cell")]
    [SerializeField] private bool _preserveCellAspectRatio = true; //Word de schaal van de cell behouden?
    [SerializeField] private float _baseCellSpacing = 0.0f; //Basis Cell tussenruimte

    [Header("Padding")]
    [SerializeField] private bool _scalePadding = false; //Word de padding van de grid layout group ook aangepast?
    [SerializeField] private bool _preservePaddingAspectRatio = true; //Word de schaal van de padding behouden?
    [SerializeField] private RectOffset _basePadding; //Basis rooster padding

    public Vector2 BaseGridSize { get => _baseGridSize; set => _baseGridSize = value; } //Basis Rooster size

    public bool PreserveCellAspectRatio { get => _preserveCellAspectRatio; set => _preserveCellAspectRatio = value; } //Word de schaal van de cell behouden?
    public float BaseCellSpacing { get => _baseCellSpacing; set => _baseCellSpacing = value; } //Basis Cell tussenruimte

    public bool ScalePadding { get => _scalePadding; set => _scalePadding = value; } //Word de padding van de grid layout group ook aangepast?
    public bool PreservePaddingAspectRatio { get => _preservePaddingAspectRatio; set => _preservePaddingAspectRatio = value; } //Word de schaal van de padding behouden?
    public RectOffset BasePadding { get => _basePadding; set => _basePadding = value; } //Basis rooster padding

    // Start is called before the first frame update
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        horizontalLayoutGroup = GetComponent<HorizontalLayoutGroup>();
        UpdateHorizontalLayoutGroup();
    }

    // Wordt gecalled wanneer de rect transform grootte wordt veranderd, en dus ook bij scherm grootte verandering doordat de rect transform grootte ook verandert (enkel bij stretch)
    void OnRectTransformDimensionsChange()
    {
        if (horizontalLayoutGroup != null)
            UpdateHorizontalLayoutGroup();
    }

    private void UpdateHorizontalLayoutGroup()
    {
        if (horizontalLayoutGroup == null)
            throw new NullReferenceException(nameof(horizontalLayoutGroup) + " is null!");

        //X-schaal
        float scaleX = rectTransform.rect.width / BaseGridSize.x;

        //Y-schaal
        float scaleY = rectTransform.rect.height / BaseGridSize.y;

        if (PreserveCellAspectRatio)
        {
            //Schaal om te gebruiken, gebruik kleinste want de grootste kan buiten rooster grootte komen
            //er wordt één waarde gebruikt voor beide assen zodat de schaal van de cell behouden wordt.
            float scale = Math.Min(scaleX, scaleY);

            horizontalLayoutGroup.spacing = BaseCellSpacing * scale;
        }
        else
        {
            //Verhouding van de cell tussenruimte wordt niet behouden.

            horizontalLayoutGroup.spacing = BaseCellSpacing * scaleX;
        }

        if (PreservePaddingAspectRatio)
        {
            //Schaal om te gebruiken, gebruik kleinste want de grootste kan buiten rooster grootte komen
            //er wordt één waarde gebruikt voor beide assen zodat de schaal van de padding behouden wordt.
            float scale = Math.Min(scaleX, scaleY);

            horizontalLayoutGroup.padding.left = (int)((float)BasePadding.left * scale);
            horizontalLayoutGroup.padding.right = (int)((float)BasePadding.right * scale);
            horizontalLayoutGroup.padding.top = (int)((float)BasePadding.top * scale);
            horizontalLayoutGroup.padding.bottom = (int)((float)BasePadding.bottom * scale);
        }
        else
        {
            //Verhouding van de padding wordt niet behouden.
            //Padding wordt uitgestrekt.

            horizontalLayoutGroup.padding.left = (int)((float)BasePadding.left * scaleX);
            horizontalLayoutGroup.padding.right = (int)((float)BasePadding.right * scaleX);
            horizontalLayoutGroup.padding.top = (int)((float)BasePadding.top * scaleY);
            horizontalLayoutGroup.padding.bottom = (int)((float)BasePadding.bottom * scaleY);
        }
    }
}
