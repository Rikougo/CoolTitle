using UnityEngine.Rendering.Universal;

namespace Rendering
{
    [System.Serializable]
    public class ColorThresholdRenderer : ScriptableRendererFeature
    {
        ColorThresholdPass pass;

        public override void Create()
        {
            pass = new ColorThresholdPass();
        }
        
        public override void AddRenderPasses(ScriptableRenderer p_renderer, ref RenderingData p_renderingData)
        {
            p_renderer.EnqueuePass(pass);
        }
    }
}
