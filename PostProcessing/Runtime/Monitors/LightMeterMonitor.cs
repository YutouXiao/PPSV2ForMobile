using System;

namespace UnityEngine.Rendering.PPSMobile
{
    /// <summary>
    /// This class holds settings for the Light Meter monitor.
    /// </summary>
    [Serializable]
    public sealed class LightMeterMonitor : Monitor
    {
        /// <summary>
        /// The width of the rendered light meter.
        /// </summary>
        public int width = 512;

        /// <summary>
        /// The height of the rendered light meter.
        /// </summary>
        public int height = 256;

        /// <summary>
        /// Should we display grading and tonemapping curves on top?
        /// </summary>
        /// <remarks>
        /// This only works when <see cref="GradingMode.HighDefinitionRange"/> is active.
        /// </remarks>
        public bool showCurves = true;

        internal override bool ShaderResourcesAvailable(PostProcessRenderContext context)
        {
            return context.resources.shaders.lightMeter && context.resources.shaders.lightMeter.isSupported;
        }

        internal override void Render(PostProcessRenderContext context)
        {
            CheckOutput(width, height);

            var histogram = context.logHistogram;

            var sheet = context.propertySheets.Get(context.resources.shaders.lightMeter);
            sheet.ClearKeywords();
            sheet.properties.SetBuffer(ShaderIDs.HistogramBuffer, histogram.data);

            var scaleOffsetRes = histogram.GetHistogramScaleOffsetRes(context);
            scaleOffsetRes.z = 1f / width;
            scaleOffsetRes.w = 1f / height;

            sheet.properties.SetVector(ShaderIDs.ScaleOffsetRes, scaleOffsetRes);

            if (context.logLut != null && showCurves)
            {
                sheet.EnableKeyword("COLOR_GRADING_HDR");
                sheet.properties.SetTexture(ShaderIDs.Lut3D, context.logLut);
            }

            var cmd = context.command;
            cmd.BeginSample("LightMeter");
            cmd.BlitFullscreenTriangle(BuiltinRenderTextureType.None, output, sheet, 0);
            cmd.EndSample("LightMeter");
        }
    }
}
