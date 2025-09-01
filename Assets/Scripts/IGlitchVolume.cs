using UnityEngine;
using UnityEngine.Rendering;

public interface IGlitchVolume
{
    void SetupGlitch(KinoGlitchPass renderPass, ScriptableRenderContext context, Material mat);
}