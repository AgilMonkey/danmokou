﻿using System;
using DMK.DMath;
using JetBrains.Annotations;
using UnityEngine;


namespace DMK.Scriptables {
[CreateAssetMenu(menuName = "Colors/Palette")]
public class Palette : ScriptableObject, INamedGradient, ISerializationCallbackReceiver {
    public enum Shade {
        WHITE,
        HIGHLIGHT,
        LIGHT,
        PURE,
        DARK,
        OUTLINE,
        BLACK
    }

    public string colorName;
    public string Name => colorName;
    public Color highlight;
    public Color light;
    public Color pure;
    public Color dark;
    public Color outline;
    /// <summary>
    /// Set to true for palettes that enable FT_RECOLOR in shaders.
    /// </summary>
    public bool recolorizable;
    private static readonly Color BLACK = Color.black;
    private static readonly Color WHITE = Color.white;

    [NonSerialized] [CanBeNull] private DGradient cachedGrad;

    public void OnAfterDeserialize() {
        cachedGrad = CalculateGradient();
    }

    private DGradient CalculateGradient() =>
        ColorHelpers.FromKeys(new[] {
            new GradientColorKey(Color.black, 0f),
            new GradientColorKey(outline, 0.1f),
            new GradientColorKey(dark, 0.3f),
            new GradientColorKey(pure, 0.5f),
            new GradientColorKey(light, 0.7f),
            new GradientColorKey(highlight, 0.9f),
            new GradientColorKey(Color.white, 1f),
        }, ColorHelpers.fullAlphaKeys);

    public DGradient Mix(Palette target) =>
        ColorHelpers.FromKeys(new[] {
            new GradientColorKey(Color.black, 0f),
            new GradientColorKey(outline, 0.1f),
            new GradientColorKey(dark, 0.3f),
            new GradientColorKey(pure, 0.5f),
            new GradientColorKey(Color.Lerp(pure, target.pure, 0.6f), .65f),
            new GradientColorKey(target.light, 0.8f),
            new GradientColorKey(target.highlight, 1f),
        }, ColorHelpers.fullAlphaKeys);

    public void OnBeforeSerialize() { }

    public IGradient Gradient => cachedGrad ?? (cachedGrad = CalculateGradient());

    public Color GetColor(Shade shade) {
        if (shade == Shade.WHITE) {
            return WHITE;
        } else if (shade == Shade.HIGHLIGHT) {
            return highlight;
        } else if (shade == Shade.LIGHT) {
            return light;
        } else if (shade == Shade.PURE) {
            return pure;
        } else if (shade == Shade.DARK) {
            return dark;
        } else if (shade == Shade.OUTLINE) {
            return outline;
        }
        return BLACK;
    }
}
}

