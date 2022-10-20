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
        public NoInterpColorParameter colorThresold = new NoInterpColorParameter(Color.black);
        
        public bool IsActive() => true;
        
        public bool IsTileCompatible() => true;
    }
}