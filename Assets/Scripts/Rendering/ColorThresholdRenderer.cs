using UnityEngine.Rendering.Universal;

namespace Rendering
{
    [System.Serializable]
    public class ColorThresholdRenderer : ScriptableRendererFeature
    {
        private ColorThresholdPass m_colorThresoldPass;

        public override void Create()
        {
            m_colorThresoldPass = new ColorThresholdPass();
        }
        
        public override void AddRenderPasses(ScriptableRenderer p_renderer, ref RenderingData p_renderingData)
        {
            p_renderer.EnqueuePass(m_colorThresoldPass);
        }
    }
}
