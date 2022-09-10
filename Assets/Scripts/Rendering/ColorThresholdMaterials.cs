using UnityEngine;

namespace Rendering
{
    [System.Serializable, CreateAssetMenu(fileName = "ColorThresholdMaterials", menuName = "Game/ColorThresholdMaterials")]
    public class ColorThresholdMaterials : ScriptableObject
    {
        //---Your Materials---
        public Material customEffect;
        
        //---Accessing the data from the Pass---
        static ColorThresholdMaterials _instance;

        public static ColorThresholdMaterials Instance
        {
            get
            {
                if (_instance != null) return _instance;
                // TODO check if application is quitting
                // and avoid loading if that is the case

                _instance = UnityEngine.Resources.Load("ColorThresoldMaterials") as ColorThresholdMaterials;
                return _instance;
            }
        }
    }
}
