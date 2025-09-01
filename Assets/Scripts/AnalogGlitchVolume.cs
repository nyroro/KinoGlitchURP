using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable, VolumeComponentMenu("KinoGlitch/Analog Glitch")]
public class AnalogGlitchVolume : VolumeComponent, IPostProcessComponent, IGlitchVolume
{
    public static readonly int k_ScanLineJitterId = Shader.PropertyToID("_ScanLineJitter");
    public static readonly int k_VerticalJumpId = Shader.PropertyToID("_VerticalJump");
    public static readonly int k_HorizontalShakeId = Shader.PropertyToID("_HorizontalShake");
    public static readonly int k_ColorDriftId = Shader.PropertyToID("_ColorDrift");

    [Tooltip("扫描线抖动强度")]
    public ClampedFloatParameter scanLineJitter = new ClampedFloatParameter(0f, 0f, 1f);

    [Tooltip("扫描线抖动阈值")]
    public ClampedFloatParameter scanLineThreshold = new ClampedFloatParameter(0.1f, 0f, 1f);

    [Tooltip("垂直跳跃强度")]
    public ClampedFloatParameter verticalJump = new ClampedFloatParameter(0f, 0f, 1f);

    [Tooltip("水平抖动强度")]
    public ClampedFloatParameter horizontalShake = new ClampedFloatParameter(0f, 0f, 1f);

    [Tooltip("颜色漂移强度")]
    public ClampedFloatParameter colorDrift = new ClampedFloatParameter(0f, 0f, 1f);

    [Tooltip("颜色漂移速度")]
    public ClampedFloatParameter colorDriftSpeed = new ClampedFloatParameter(1f, 0f, 5f);

    public bool IsActive() => scanLineJitter.value > 0f || verticalJump.value > 0f ||
                            horizontalShake.value > 0f || colorDrift.value > 0f;

    public bool IsTileCompatible() => false;

    public void SetupGlitch(KinoGlitchPass renderPass, ScriptableRenderContext context, Material mat)
    {
        mat.SetVector(k_ScanLineJitterId, new Vector2(
            scanLineJitter.value,
            scanLineThreshold.value));

        mat.SetVector(k_VerticalJumpId, new Vector2(
            verticalJump.value,
            Time.time * verticalJump.value));

        mat.SetFloat(k_HorizontalShakeId, horizontalShake.value);

        mat.SetVector(k_ColorDriftId, new Vector2(
            colorDrift.value,
            Time.time * colorDriftSpeed.value));
    }
}