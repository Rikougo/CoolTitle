using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Rendering
{
    [System.Serializable]
    public class ColorThresholdPass : ScriptableRenderPass
    {
        // Used to render from camera to post processings
        // back and forth, until we render the final image to
        // the camera
        RenderTargetIdentifier m_source;
        RenderTargetIdentifier m_destinationA;
        RenderTargetIdentifier m_destinationB;
        RenderTargetIdentifier m_latestDest;

        readonly int m_temporaryRTIdA = Shader.PropertyToID("_TempRT");
        readonly int m_temporaryRTIdB = Shader.PropertyToID("_TempRTB");

        public ColorThresholdPass()
        {
            // Set the render pass event
            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        }

        public override void OnCameraSetup(CommandBuffer p_command, ref RenderingData p_renderingData)
        {
            // Grab the camera target descriptor. We will use this when creating a temporary render texture.
            RenderTextureDescriptor l_descriptor = p_renderingData.cameraData.cameraTargetDescriptor;
            l_descriptor.depthBufferBits = 0;

            var l_renderer = p_renderingData.cameraData.renderer;
            m_source = l_renderer.cameraColorTarget;

            // Create a temporary render texture using the descriptor from above.
            p_command.GetTemporaryRT(m_temporaryRTIdA, l_descriptor, FilterMode.Bilinear);
            m_destinationA = new RenderTargetIdentifier(m_temporaryRTIdA);
            p_command.GetTemporaryRT(m_temporaryRTIdB, l_descriptor, FilterMode.Bilinear);
            m_destinationB = new RenderTargetIdentifier(m_temporaryRTIdB);
        }

        // The actual execution of the pass. This is where custom rendering occurs.
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            // Here you get your materials from your custom class
            // (It's up to you! But here is how I did it)
            var l_materials = ColorThresholdMaterials.Instance;
            if (l_materials == null)
            {
                Debug.LogError("Custom Post Processing Materials instance is null");
                return;
            }

            CommandBuffer l_command = CommandBufferPool.Get("Custom Post Processing");
            l_command.Clear();

            // This holds all the current Volumes information
            // which we will need later
            VolumeStack l_stack = VolumeManager.instance.stack;

            #region Local Methods

            // Swaps render destinations back and forth, so that
            // we can have multiple passes and similar with only a few textures
            void BlitTo(Material p_mat, int p_pass = 0)
            {
                RenderTargetIdentifier l_first = m_latestDest;
                RenderTargetIdentifier l_last = l_first == m_destinationA ? m_destinationB : m_destinationA;
                Blit(l_command, l_first, l_last, p_mat, p_pass);

                m_latestDest = l_last;
            }

            #endregion

            // Starts with the camera source
            m_latestDest = m_source;

            //---Custom effect here---
            var l_customEffect = l_stack.GetComponent<ColorThresholdComponent>();
            
            // Only process if the effect is active
            if (l_customEffect.IsActive())
            {
                var l_material = l_materials.customEffect;
                // P.s. optimize by caching the property ID somewhere else
                l_material.SetColor(Shader.PropertyToID("_ColorThresold"), l_customEffect.colorThresold.value);

                BlitTo(l_material);
            }

            // Add any other custom effect/component you want, in your preferred order
            // Custom effect 2, 3 , ...


            // DONE! Now that we have processed all our custom effects, applies the final result to camera
            Blit(l_command, m_latestDest, m_source);

            context.ExecuteCommandBuffer(l_command);
            CommandBufferPool.Release(l_command);
        }
        
        public override void OnCameraCleanup(CommandBuffer p_command)
        {
            p_command.ReleaseTemporaryRT(m_temporaryRTIdA);
            p_command.ReleaseTemporaryRT(m_temporaryRTIdB);
        }
    }
}