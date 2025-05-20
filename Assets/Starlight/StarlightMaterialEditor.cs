using UnityEngine;
using UnityEditor;

namespace Starlight
{
    public class StarlightMaterialEditor : ShaderGUI
    {
        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            Material material = materialEditor.target as Material;
            
            // Enable instancing on this material
            material.enableInstancing = true;
            
            // Draw the default inspector
            base.OnGUI(materialEditor, properties);
        }
    }
}