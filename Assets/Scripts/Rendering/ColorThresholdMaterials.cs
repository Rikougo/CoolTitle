using UnityEngine;

namespace Rendering
{
    [System.Serializable, CreateAssetMenu(fileName = "ColorThresholdMaterials", menuName = "Game/ColorThresholdMaterials")]
    public class ColorThresholdMaterials : ScriptableObject
    {
        public Material customEffect;
        
        static ColorThresholdMaterials _instance;

        public static ColorThresholdMaterials Instance
        {
            get
            {
                if (_instance != null) return _instance;

                _instance = UnityEngine.Resources.Load("ColorThresoldMaterials") as ColorThresholdMaterials;
                return _instance;
            }
        }
    }
}
