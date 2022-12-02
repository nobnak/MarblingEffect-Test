using Gist2.Extensions.RectExt;
using Gist2.Extensions.ScreenExt;
using Gist2.Extensions.SizeExt;
using Gist2.Wrappers;
using LLGraphicsUnity;
using LLGraphicsUnity.Shapes;
using MarblingEffectSys;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

public class TestBrush : MonoBehaviour {

    public Dependency dep = new Dependency();
    public Events events = new Events();
    public Tuner tuner = new Tuner();

    protected GLMaterial gl;
    protected MarblingShader shader;

    protected bool isDown;
    protected float2 uv;
    protected Stroke stroke;

    protected RenderTextureWrapper offset0, offset1;

    #region unity
    private void OnEnable() {
        gl = new GLMaterial();
        shader = new MarblingShader();

        offset0 = new RenderTextureWrapper(GenerateOffsetTexture);
        offset1 = new RenderTextureWrapper(GenerateOffsetTexture);
    }
    private void OnDisable() {
        if (gl != null) {
            gl.Dispose();
            gl = null;
        }
        if (shader != null) {
            shader.Dispose();
            shader = null;
        }

        events.OnUpdate?.Invoke(null);
        if (offset0 != null) {
            offset0.Dispose();
            offset0 = null;
        }
        if (offset1 != null) {
            offset1.Dispose();
            offset1 = null;
        }
    }
    private void OnRenderObject() {
        var c = Camera.current;
        if (c == null || (c.cullingMask & (1 << gameObject.layer)) == 0 || !isActiveAndEnabled) return;

        if (!tuner.show_boundary || dep.brush == null || !stroke.enabled) return;

        var of = stroke.offset;
        of = math.normalize(of);
        var color = new float4(math.abs(of), math.dot(math.max(-of, 0), 1), 1f);
        var prop = new GLProperty() {
            Color = (Vector4)color,
            ZTestMode = GLProperty.ZTestEnum.ALWAYS,
            ZWriteMode = false,
            LineThickness = 5f,
        };

        using (new GLMatrixScope())
        using (gl.GetScope(prop, ShaderPass.Line)) {
            GL.LoadOrtho();

            var model = Matrix4x4.TRS(
                new float3(stroke.rect.CenterAsRect(), 0f),
                Quaternion.identity,
                new float3(stroke.rect.SizeAsRect(), 1f));
            using (new GLModelViewScope(model))
                Quad.LineStrip();
        }
    }
    private void Update() {
        stroke = default;
        if (Input.GetMouseButton(0)) {
            var screen = ScreenExtension.ScreenSize();
            var curr_uv = screen.UV(Input.mousePosition);
            var offset = uv - curr_uv;

            if (isDown && dep.brush != null && math.lengthsq(offset) > 0) {
                var cross_aspect = dep.brush.Size().Aspect() / screen.Aspect();
                var brush_size = new float2(tuner.size * cross_aspect, tuner.size);
                var rect = RectExtension.CreateAsRect(curr_uv - 0.5f * brush_size, brush_size);
                stroke = new Stroke(dep.brush, rect, offset * tuner.strength);
            }

            isDown = true;
            uv = curr_uv;
        } else {
            isDown = false;
        }

        if (dep.main != null) offset0.Size = dep.main.Size();
        if (stroke.enabled) {
            Graphics.Blit(offset0, offset1);
            shader.Add(offset1, offset0, stroke.brush, stroke.rect, stroke.offset);
            Swap(ref offset0, ref offset1);
        }
        events.OnUpdate?.Invoke(offset0);
    }
    #endregion

    #region methods
    public static void Swap<T>(ref T offset0, ref T offset1) {
        var tmp = offset1;
        offset1 = offset0;
        offset0 = tmp;
    }
    protected RenderTexture GenerateOffsetTexture(int2 s) {
        var tex = new RenderTexture(s.x, s.y, 0, RenderTextureFormat.ARGBFloat);
        tex.hideFlags = HideFlags.DontSave;
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.useMipMap = false;
        tex.Create();
        shader.Reset(tex);
        return tex;
    }
    #endregion

    #region declarations
    [System.Serializable]
    public class Events {
        public TextureEvent OnUpdate = new TextureEvent();

        [System.Serializable]
        public class TextureEvent : UnityEvent<Texture> { }
    }
    [System.Serializable]
    public struct Stroke {
        public bool enabled;
        public Texture brush;
        public float4 rect;
        public float2 offset;

        public Stroke(Texture brush, float4 rect, float2 offset) {
            this.brush = brush;
            this.enabled = true;
            this.rect = rect;
            this.offset = offset;
        }

        public override string ToString() {
            return $"<{GetType().Name}: v=({offset.x:e2},{offset.y:e2}) rect={rect}>";
        }
    }
    [System.Serializable]
    public class Dependency {
        public Texture brush;
        public Camera main;
    }
    [System.Serializable]
    public class Tuner {
        public bool show_boundary;
        public float size = 0.2f;
        public float strength = 1f;
    }
    #endregion
}