using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using UnityEngine.Assertions;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.Rendering.PPSMobile
{
    using SceneManagement;
    using UnityObject = UnityEngine.Object;
    using LoadAction = RenderBufferLoadAction;
    using StoreAction = RenderBufferStoreAction;

    /// <summary>
    /// A set of runtime utilities used by the post-processing stack.
    /// </summary>
    public static class RuntimeUtilities
    {
        #region Textures        

        static Texture2D m_TransparentTexture;

        /// <summary>
        /// A 1x1 transparent texture.
        /// </summary>
        /// <remarks>
        /// This texture is only created once and recycled afterward. You shouldn't modify it.
        /// </remarks>
        public static Texture2D transparentTexture
        {
            get
            {
                if (m_TransparentTexture == null)
                {
                    m_TransparentTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false) { name = "Transparent Texture" };
                    m_TransparentTexture.SetPixel(0, 0, Color.clear);
                    m_TransparentTexture.Apply();
                }

                return m_TransparentTexture;
            }
        }        

        #endregion

        #region Rendering

        static PostProcessResources s_Resources;
        static Mesh s_FullscreenTriangle;

        /// <summary>
        /// A fullscreen triangle mesh.
        /// </summary>
        public static Mesh fullscreenTriangle
        {
            get
            {
                if (s_FullscreenTriangle != null)
                    return s_FullscreenTriangle;

                s_FullscreenTriangle = new Mesh { name = "Fullscreen Triangle" };

                // Because we have to support older platforms (GLES2/3, DX9 etc) we can't do all of
                // this directly in the vertex shader using vertex ids :(
                s_FullscreenTriangle.SetVertices(new List<Vector3>
                {
                    new Vector3(-1f, -1f, 0f),
                    new Vector3(-1f,  3f, 0f),
                    new Vector3( 3f, -1f, 0f)
                });
                s_FullscreenTriangle.SetIndices(new [] { 0, 1, 2 }, MeshTopology.Triangles, 0, false);
                s_FullscreenTriangle.UploadMeshData(false);

                return s_FullscreenTriangle;
            }
        }

        static Material s_CopyStdMaterial;

        /// <summary>
        /// A simple copy material to use with the builtin pipelines.
        /// </summary>
        public static Material copyStdMaterial
        {
            get
            {
                if (s_CopyStdMaterial != null)
                    return s_CopyStdMaterial;

                Assert.IsNotNull(s_Resources);
                var shader = s_Resources.shaders.copyStd;
                s_CopyStdMaterial = new Material(shader)
                {
                    name = "PostProcess - CopyStd",
                    hideFlags = HideFlags.HideAndDontSave
                };

                return s_CopyStdMaterial;
            }
        }        

        static Material s_CopyMaterial;

        /// <summary>
        /// A simple copy material independent from the rendering pipeline.
        /// </summary>
        public static Material copyMaterial
        {
            get
            {
                if (s_CopyMaterial != null)
                    return s_CopyMaterial;

                Assert.IsNotNull(s_Resources);
                var shader = s_Resources.shaders.copy;
                s_CopyMaterial = new Material(shader)
                {
                    name = "PostProcess - Copy",
                    hideFlags = HideFlags.HideAndDontSave
                };

                return s_CopyMaterial;
            }
        }        

        static PropertySheet s_CopySheet;

        /// <summary>
        /// A pre-configured <see cref="PropertySheet"/> for <see cref="copyMaterial"/>.
        /// </summary>
        public static PropertySheet copySheet
        {
            get
            {
                if (s_CopySheet == null)
                    s_CopySheet = new PropertySheet(copyMaterial);

                return s_CopySheet;
            }
        }

        internal static void UpdateResources(PostProcessResources resources)
        {
            Destroy(s_CopyMaterial);
            Destroy(s_CopyStdMaterial);
            s_CopyMaterial = null;
            s_CopyStdMaterial = null;
            s_CopySheet = null;
            s_Resources = resources;
        }

        /// <summary>
        /// Sets the current render target using specified <see cref="RenderBufferLoadAction"/>.
        /// </summary>
        /// <param name="cmd">The command buffer to set the render target on</param>
        /// <param name="rt">The render target to set</param>
        /// <param name="loadAction">The load action</param>
        /// <param name="storeAction">The store action</param>
        /// <remarks>
        /// <see cref="RenderBufferLoadAction"/> are only used on Unity 2018.2 or newer.
        /// </remarks>
        public static void SetRenderTargetWithLoadStoreAction(this CommandBuffer cmd, RenderTargetIdentifier rt, RenderBufferLoadAction loadAction, RenderBufferStoreAction storeAction)
        {
            #if UNITY_2018_2_OR_NEWER
            cmd.SetRenderTarget(rt, loadAction, storeAction);
            #else
            cmd.SetRenderTarget(rt);
            #endif
        }        

        /// <summary>
        /// Does a copy of source to destination using a fullscreen triangle.
        /// </summary>
        /// <param name="cmd">The command buffer to use</param>
        /// <param name="source">The source render target</param>
        /// <param name="destination">The destination render target</param>
        /// <param name="clear">Should the destination target be cleared?</param>
        /// <param name="viewport">An optional viewport to consider for the blit</param>
        public static void BlitFullscreenTriangle(this CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination, bool clear = false, Rect? viewport = null)
        {
            cmd.SetGlobalTexture(ShaderIDs.MainTex, source);
            cmd.SetRenderTargetWithLoadStoreAction(destination, viewport == null ? LoadAction.DontCare : LoadAction.Load, StoreAction.Store);

            if (viewport != null)
                cmd.SetViewport(viewport.Value);

            if (clear)
                cmd.ClearRenderTarget(true, true, Color.clear);

            cmd.DrawMesh(fullscreenTriangle, Matrix4x4.identity, copyMaterial, 0, 0);
        }

        /// <summary>
        /// Blits a fullscreen triangle using a given material.
        /// </summary>
        /// <param name="cmd">The command buffer to use</param>
        /// <param name="source">The source render target</param>
        /// <param name="destination">The destination render target</param>
        /// <param name="propertySheet">The property sheet to use</param>
        /// <param name="pass">The pass from the material to use</param>
        /// <param name="loadAction">The load action for this blit</param>
        /// <param name="viewport">An optional viewport to consider for the blit</param>
        public static void BlitFullscreenTriangle(this CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination, PropertySheet propertySheet, int pass, RenderBufferLoadAction loadAction, Rect? viewport = null)
        {
            cmd.SetGlobalTexture(ShaderIDs.MainTex, source);
            #if UNITY_2018_2_OR_NEWER
            bool clear = (loadAction == LoadAction.Clear);
            if(clear)
                loadAction = LoadAction.DontCare;
            #else
            bool clear = false;
            #endif
            cmd.SetRenderTargetWithLoadStoreAction(destination, viewport == null ? loadAction : LoadAction.Load, StoreAction.Store);

            if (viewport != null)
                cmd.SetViewport(viewport.Value);

            if (clear)
                cmd.ClearRenderTarget(true, true, Color.clear);

            cmd.DrawMesh(fullscreenTriangle, Matrix4x4.identity, propertySheet.material, 0, pass, propertySheet.properties);
        }

        /// <summary>
        /// Blits a fullscreen triangle using a given material.
        /// </summary>
        /// <param name="cmd">The command buffer to use</param>
        /// <param name="source">The source render target</param>
        /// <param name="destination">The destination render target</param>
        /// <param name="propertySheet">The property sheet to use</param>
        /// <param name="pass">The pass from the material to use</param>
        /// <param name="clear">Should the destination target be cleared?</param>
        /// <param name="viewport">An optional viewport to consider for the blit</param>
        public static void BlitFullscreenTriangle(this CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination, PropertySheet propertySheet, int pass, bool clear = false, Rect? viewport = null)
        {
            #if UNITY_2018_2_OR_NEWER
            cmd.BlitFullscreenTriangle(source, destination, propertySheet, pass, clear ? LoadAction.Clear : LoadAction.DontCare, viewport);
            #else
            cmd.SetGlobalTexture(ShaderIDs.MainTex, source);
            cmd.SetRenderTargetWithLoadStoreAction(destination, viewport == null ? LoadAction.DontCare : LoadAction.Load, StoreAction.Store);

            if (viewport != null)
                cmd.SetViewport(viewport.Value);

            if (clear)
                cmd.ClearRenderTarget(true, true, Color.clear);

            cmd.DrawMesh(fullscreenTriangle, Matrix4x4.identity, propertySheet.material, 0, pass, propertySheet.properties);
            #endif
        }        

        /// <summary>
        /// Blits a fullscreen quad using the builtin blit command and a given material.
        /// </summary>
        /// <param name="cmd">The command buffer to use</param>
        /// <param name="source">The source render target</param>
        /// <param name="destination">The destination render target</param>
        /// <param name="mat">The material to use for the blit</param>
        /// <param name="pass">The pass from the material to use</param>
        public static void BuiltinBlit(this CommandBuffer cmd, Rendering.RenderTargetIdentifier source, RenderTargetIdentifier destination, Material mat, int pass = 0)
        {
            #if UNITY_2018_2_OR_NEWER
            cmd.SetRenderTarget(destination, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
            destination = BuiltinRenderTextureType.CurrentActive;
            #endif
            cmd.Blit(source, destination, mat, pass);
        }        

        #endregion

        #region Unity specifics & misc methods

        /// <summary>
        /// Returns <c>true</c> if a scriptable render pipeline is currently in use, <c>false</c>
        /// otherwise.
        /// </summary>
        public static bool scriptableRenderPipelineActive
        {
            get { return GraphicsSettings.renderPipelineAsset != null; } // 5.6+ only
        }

        /// <summary>
        /// Returns <c>true</c> if the target platform is Android and the selected API is OpenGL,
        /// <c>false</c> otherwise.
        /// </summary>
        public static bool isAndroidOpenGL
        {
            get { return Application.platform == RuntimePlatform.Android && SystemInfo.graphicsDeviceType != GraphicsDeviceType.Vulkan; }
        }

        /// <summary>
        /// Gets the default HDR render texture format for the current target platform.
        /// </summary>
        public static RenderTextureFormat defaultHDRRenderTextureFormat
        {
            get
            {
#if UNITY_ANDROID || UNITY_IPHONE || UNITY_EDITOR
                RenderTextureFormat format = RenderTextureFormat.RGB111110Float;
#if UNITY_EDITOR
                var target = EditorUserBuildSettings.activeBuildTarget;
                if (target != BuildTarget.Android && target != BuildTarget.iOS && target != BuildTarget.tvOS && target != BuildTarget.Switch)
                    return RenderTextureFormat.DefaultHDR;
#endif // UNITY_EDITOR
                if (format.IsSupported())
                    return format;
#endif // UNITY_ANDROID || UNITY_IPHONE || UNITY_EDITOR
                return RenderTextureFormat.DefaultHDR;
            }
        }

        /// <summary>
        /// Checks if a given render texture format is a floating-point format.
        /// </summary>
        /// <param name="format">The format to test</param>
        /// <returns><c>true</c> if the format is floating-point, <c>false</c> otherwise</returns>
        public static bool isFloatingPointFormat(RenderTextureFormat format)
        {
            return format == RenderTextureFormat.DefaultHDR || format == RenderTextureFormat.ARGBHalf || format == RenderTextureFormat.ARGBFloat ||
                   format == RenderTextureFormat.RGFloat || format == RenderTextureFormat.RGHalf ||
                   format == RenderTextureFormat.RFloat || format == RenderTextureFormat.RHalf ||
                   format == RenderTextureFormat.RGB111110Float;
        }

        /// <summary>
        /// Properly destroys a given Unity object.
        /// </summary>
        /// <param name="obj">The object to destroy</param>
        public static void Destroy(UnityObject obj)
        {
            if (obj != null)
            {
#if UNITY_EDITOR
                if (Application.isPlaying)
                    UnityObject.Destroy(obj);
                else
                    UnityObject.DestroyImmediate(obj);
#else
                UnityObject.Destroy(obj);
#endif
            }
        }

        /// <summary>
        /// Returns <c>true</c> if the current color space setting is set to <c>Linear</c>,
        /// <c>false</c> otherwise.
        /// </summary>
        public static bool isLinearColorSpace
        {
            get { return QualitySettings.activeColorSpace == ColorSpace.Linear; }
        }

        /// <summary>
        /// Properly destroys a given profile.
        /// </summary>
        /// <param name="profile">The profile to destroy</param>
        /// <param name="destroyEffects">Should we destroy all the embedded settings?</param>
        public static void DestroyProfile(PostProcessProfile profile, bool destroyEffects)
        {
            if (destroyEffects)
            {
                foreach (var effect in profile.settings)
                    Destroy(effect);
            }

            Destroy(profile);
        }

        /// <summary>
        /// Properly destroys a volume.
        /// </summary>
        /// <param name="volume">The volume to destroy</param>
        /// <param name="destroyProfile">Should we destroy the attached profile?</param>
        /// <param name="destroyGameObject">Should we destroy the volume Game Object?</param>
        public static void DestroyVolume(PostProcessVolume volume, bool destroyProfile, bool destroyGameObject = false)
        {
            if (destroyProfile)
                DestroyProfile(volume.profileRef, true);

            var gameObject = volume.gameObject;
            Destroy(volume);

            if (destroyGameObject)
                Destroy(gameObject);
        }        

        /// <summary>
        /// Gets all scene objects in the hierarchy, including inactive objects. This method is slow
        /// on large scenes and should be used with extreme caution.
        /// </summary>
        /// <typeparam name="T">The component to look for</typeparam>
        /// <returns>A list of all components of type <c>T</c> in the scene</returns>
        public static IEnumerable<T> GetAllSceneObjects<T>()
            where T : Component
        {
            var queue = new Queue<Transform>();
            var roots = SceneManager.GetActiveScene().GetRootGameObjects();

            foreach (var root in roots)
            {
                queue.Enqueue(root.transform);
                var comp = root.GetComponent<T>();

                if (comp != null)
                    yield return comp;
            }

            while (queue.Count > 0)
            {
                foreach (Transform child in queue.Dequeue())
                {
                    queue.Enqueue(child);
                    var comp = child.GetComponent<T>();

                    if (comp != null)
                        yield return comp;
                }
            }
        }

        /// <summary>
        /// Creates an instance of a class if it's <c>null</c>.
        /// </summary>
        /// <typeparam name="T">The type to create</typeparam>
        /// <param name="obj">A reference to an instance to check and create if needed</param>
        public static void CreateIfNull<T>(ref T obj)
            where T : class, new()
        {
            if (obj == null)
                obj = new T();
        }

        #endregion

        #region Maths

        /// <summary>
        /// Returns the base-2 exponential function of <paramref name="x"/>, which is <c>2</c>
        /// raised to the power <paramref name="x"/>.
        /// </summary>
        /// <param name="x">Value of the exponent</param>
        /// <returns>The base-2 exponential function of <paramref name="x"/></returns>
        public static float Exp2(float x)
        {
            return Mathf.Exp(x * 0.69314718055994530941723212145818f);
        }       

        #endregion

        #region Reflection

        static IEnumerable<Type> m_AssemblyTypes;

        /// <summary>
        /// Gets all currently available assembly types.
        /// </summary>
        /// <returns>A list of all currently available assembly types</returns>
        /// <remarks>
        /// This method is slow and should be use with extreme caution.
        /// </remarks>
        public static IEnumerable<Type> GetAllAssemblyTypes()
        {
            if (m_AssemblyTypes == null)
            {
                m_AssemblyTypes = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(t =>
                    {
                        // Ugly hack to handle mis-versioned dlls
                        var innerTypes = new Type[0];
                        try
                        {
                            innerTypes = t.GetTypes();
                        }
                        catch { }
                        return innerTypes;
                    });
            }

            return m_AssemblyTypes;
        }

        /// <summary>
        /// Helper method to get the first attribute of type <c>T</c> on a given type.
        /// </summary>
        /// <typeparam name="T">The attribute type to look for</typeparam>
        /// <param name="type">The type to explore</param>
        /// <returns>The attribute found</returns>
        public static T GetAttribute<T>(this Type type) where T : Attribute
        {
            Assert.IsTrue(type.IsDefined(typeof(T), false), "Attribute not found");
            return (T)type.GetCustomAttributes(typeof(T), false)[0];
        }

        /// <summary>
        /// Returns all attributes set on a specific member.
        /// </summary>
        /// <typeparam name="TType">The class type where the member is defined</typeparam>
        /// <typeparam name="TValue">The member type</typeparam>
        /// <param name="expr">An expression path to the member</param>
        /// <returns>An array of attributes</returns>
        /// <remarks>
        /// This method doesn't return inherited attributes, only explicit ones.
        /// </remarks>
        public static Attribute[] GetMemberAttributes<TType, TValue>(Expression<Func<TType, TValue>> expr)
        {
            Expression body = expr;

            if (body is LambdaExpression)
                body = ((LambdaExpression)body).Body;

            switch (body.NodeType)
            {
                case ExpressionType.MemberAccess:
                    var fi = (FieldInfo)((MemberExpression)body).Member;
                    return fi.GetCustomAttributes(false).Cast<Attribute>().ToArray();
                default:
                    throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Returns a string path from an expression. This is mostly used to retrieve serialized
        /// properties without hardcoding the field path as a string and thus allowing proper
        /// refactoring features.
        /// </summary>
        /// <typeparam name="TType">The class type where the member is defined</typeparam>
        /// <typeparam name="TValue">The member type</typeparam>
        /// <param name="expr">An expression path fo the member</param>
        /// <returns>A string representation of the expression path</returns>
        public static string GetFieldPath<TType, TValue>(Expression<Func<TType, TValue>> expr)
        {
            MemberExpression me;
            switch (expr.Body.NodeType)
            {
                case ExpressionType.MemberAccess:
                    me = expr.Body as MemberExpression;
                    break;
                default:
                    throw new InvalidOperationException();
            }

            var members = new List<string>();
            while (me != null)
            {
                members.Add(me.Member.Name);
                me = me.Expression as MemberExpression;
            }

            var sb = new StringBuilder();
            for (int i = members.Count - 1; i >= 0; i--)
            {
                sb.Append(members[i]);
                if (i > 0) sb.Append('.');
            }

            return sb.ToString();
        }

        #endregion
    }
}
