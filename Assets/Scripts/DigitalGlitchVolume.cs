using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable, VolumeComponentMenu("Kino/Glitch/Digital")]
public class DigitalGlitchVolume : VolumeComponent, IPostProcessComponent, IGlitchVolume
{
    public static readonly int k_IntensityId = Shader.PropertyToID("_Intensity");

    [Tooltip("Glitch effect intensity")]
    public ClampedFloatParameter intensity = new ClampedFloatParameter(0f, 0f, 1f);

    [Tooltip("Glitch noise size")]
    public Vector2Int noiseSize = new Vector2Int(64, 32);
    public bool IsActive() => intensity.value > 0f && active;

    public bool IsTileCompatible() => false;

    private Texture2D _noiseTexture;
    private RenderTexture _trashFrame1;
    private RenderTexture _trashFrame2;
    static Color RandomColor()
    {
        return new Color(Random.value, Random.value, Random.value, Random.value);
    }
    public void SetupGlitch(KinoGlitchPass renderPass, ScriptableRenderContext context, Material mat)
    {
        mat.hideFlags = HideFlags.DontSave;

        if (_noiseTexture == null)
        {
            _noiseTexture = new Texture2D(noiseSize.x, noiseSize.y, TextureFormat.ARGB32, false);
            _noiseTexture.hideFlags = HideFlags.DontSave;
            _noiseTexture.wrapMode = TextureWrapMode.Clamp;
            _noiseTexture.filterMode = FilterMode.Point;
        }

        if (_trashFrame1 == null)
        {
            _trashFrame1 = new RenderTexture(Screen.width, Screen.height, 0);
            _trashFrame1.hideFlags = HideFlags.DontSave;
        }
        if (_trashFrame2 == null)
        {
            _trashFrame2 = new RenderTexture(Screen.width, Screen.height, 0);
            _trashFrame2.hideFlags = HideFlags.DontSave;
        }
        var color = RandomColor();

        for (var y = 0; y < _noiseTexture.height; y++)
        {
            for (var x = 0; x < _noiseTexture.width; x++)
            {
                if (Random.value > 0.89f) color = RandomColor();
                _noiseTexture.SetPixel(x, y, color);
            }
        }

        _noiseTexture.Apply();

        var fcount = Time.frameCount;

        if (fcount % 13 == 0) renderPass.BlitToRenderTexture(context, _trashFrame1);
        if (fcount % 73 == 0) renderPass.BlitToRenderTexture(context, _trashFrame2);


        mat.SetFloat(k_IntensityId, intensity.value);
        mat.SetTexture("_NoiseTex", _noiseTexture);
        var trashFrame = Random.value > 0.5f ? _trashFrame1 : _trashFrame2;
        mat.SetTexture("_TrashTex", trashFrame);
    }
}