﻿namespace Dialog.Runtime.Assets
{
    [System.Serializable]
    public class ExposedProperty
    {
        public static ExposedProperty CreateInstance()
        {
            return new ExposedProperty();
        }

        public string PropertyName = "New String";
        public string PropertyValue = "New Value";
    }
}