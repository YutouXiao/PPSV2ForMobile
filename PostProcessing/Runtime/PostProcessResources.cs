using System;

namespace UnityEngine.Rendering.PPSMobile
{
    /// <summary>
    /// This asset is used to store references to shaders and other resources we might need at
    /// runtime without having to use a `Resources` folder. This allows for better memory management,
    /// better dependency tracking and better interoperability with asset bundles.
    /// </summary>
    public sealed class PostProcessResources : ScriptableObject
    {
        [Serializable]
        public sealed class Shaders
        {
            public Shader bloom;
            public Shader copy;
            public Shader copyStd;
            public Shader copyStdFromTexArray;
            public Shader copyStdFromDoubleWide;
            public Shader discardAlpha;
            public Shader finalPass;
            public Shader texture2dLerp;
            public Shader uber;
            public Shader lut2DBaker;
            public Shader lightMeter;
            public Shader gammaHistogram;
            public Shader waveform;
            public Shader vectorscope;
            public Shader debugOverlays;

            public Shaders Clone()
            {
                return (Shaders)MemberwiseClone();
            }
        }

        [Serializable]
        public sealed class ComputeShaders
        {
            public ComputeShader exposureHistogram;
            public ComputeShader texture3dLerp;
            public ComputeShader gammaHistogram;
            public ComputeShader waveform;
            public ComputeShader vectorscope;
            public ComputeShader gaussianDownsample;

            public ComputeShaders Clone()
            {
                return (ComputeShaders)MemberwiseClone();
            }
        }

        [Serializable]
        public sealed class SMAALuts
        {
            public Texture2D area;
            public Texture2D search;
        }

        public Texture2D[] blueNoise64;
        public Texture2D[] blueNoise256;
        public SMAALuts smaaLuts;
        public Shaders shaders;
        public ComputeShaders computeShaders;

#if UNITY_EDITOR
        public delegate void ChangeHandler();
        public ChangeHandler changeHandler;

        void OnValidate()
        {
            if (changeHandler != null)
                changeHandler();
        }
#endif
    }
}
