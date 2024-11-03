using System;
using UnityEngine;

/// <summary>
/// Beeldverhoudingshulpmethodes voor camera's,
/// geeft duidelijke(r) namen aan bepaalde wiskundige berekeningen voor beeldverhoudingen, die vaak onduidelijk worden.
/// </summary>
public static class CameraAspectRatioHelper
{
    /// <summary>
    /// Geeft beste envelopping orthographic size voor bepaalde camera beeldverhoudingen om fitWidth & fitHeight in te passen
    /// </summary>
    /// <param name="fitWidth"></param>
    /// <param name="fitHeight"></param>
    /// <param name="cameraAspect"></param>
    /// <returns>Camera orthographic size</returns>
    public static float OrthographicSizeEnveloppeRect(float fitWidth, float fitHeight, float cameraAspect)
    {
        //(ortho * 2) = height => ortho = height / 2
        //(ortho * aspect * 2) = width => ortho = width / aspect / 2
        return Math.Max(fitWidth / cameraAspect, fitHeight) / 2.0f;
    }

    /// <summary>
    /// Geeft beste envelopping orthographic size voor bepaalde camera beeldverhoudingen om een camera met een standaard orthographic size en beeldverhouding in te passen
    /// </summary>
    /// <param name="baseOrthographicSize"></param>
    /// <param name="baseAspect"></param>
    /// <param name="newAspect"></param>
    /// <returns>Camera orthographic size</returns>
    public static float OrthographicSizeEnveloppeBase(float baseOrthographicSize, float baseAspect, float newAspect)
    {
        //maak de camera niet kleiner, dan past de standaard hoogte niet meer
        float aspectMultiplier = Mathf.Max(baseAspect / newAspect, 1.0f);
        return baseOrthographicSize * aspectMultiplier;
    }
}
