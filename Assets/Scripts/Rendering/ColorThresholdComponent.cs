using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Rendering
{
    [Serializable,
     VolumeComponentMenuForRenderPipeline("Custom/CustomEffectComponent", typeof(UniversalRenderPipeline))]
    public class ColorThresholdComponent : VolumeComponent, IPostProcessComponent
    {
        // A color that is constant even when the weight changes
        public UnityEngine.Rendering.NoInterpColorParameter colorThresold = new NoInterpColorParameter(Color.black);

        // Other 'Parameter' variables you might have

        // Tells when our effect should be rendered
        public bool IsActive() => true;

        // I have no idea what this does yet but I'll update the post once I find an usage
        public bool IsTileCompatible() => true;
    }
}