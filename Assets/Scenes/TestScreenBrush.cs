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

public class TestScreenBrush : MarblingUVBase {

    public Dependency dep = new Dependency();
    public ScreenTuner tuner = new ScreenTuner();

    protected GLMaterial gl;

    protected bool isDown;
    protected float2 uv;

    #region unity
    protected override void OnEnable() {
        CurrTuner = tuner;
        base.OnEnable();
        gl = new GLMaterial();
    }
    protected override void OnDisable() {
        base.OnDisable();
        if (gl != null) {
            gl.Dispose();
            gl = null;
        }
    }
    protected override void Update() {
        base.Update();

        if (dep.main != null) {
            var screen = dep.main.Size();
            if (Input.GetMouseButton(0)) {
                var curr_uv = screen.UV(Input.mousePosition);
                var offset = uv - curr_uv;

                if (isDown && dep.brush != null && math.lengthsq(offset) > 0) {
                    var cross_aspect = dep.brush.Size().Aspect() / screen.Aspect();
                    var brush_size = new float2(tuner.size * cross_aspect, tuner.size);
                    var rect = RectExtension.CreateAsRect(curr_uv - 0.5f * brush_size, brush_size);
                    var stroke = new Stroke(dep.brush, rect, offset * tuner.strength);
                    Add(stroke);
                }

                isDown = true;
                uv = curr_uv;
            } else {
                isDown = false;
            }

            Render(screen);
        }
    }
    protected virtual void OnRenderObject() {
        var c = Camera.current;
        if (c == null || (c.cullingMask & (1 << gameObject.layer)) == 0 || !isActiveAndEnabled) return;

        if (!tuner.show_boundary || dep.brush == null || strokes.Count <= 0) return;

        var prop = new GLProperty() {
            ZTestMode = GLProperty.ZTestEnum.ALWAYS,
            ZWriteMode = false,
            LineThickness = 5f,
        };

        using (new GLMatrixScope())
        using (gl.GetScope(prop, ShaderPass.Line)) {
            GL.LoadOrtho();

            foreach (var stroke in strokes) {
                var of = stroke.offset;
                of = math.normalize(of);
                var color = new float4(math.abs(of), math.dot(math.max(-of, 0), 1), 1f);

                var model = Matrix4x4.TRS(
                    new float3(stroke.rect.CenterAsRect(), 0f),
                    Quaternion.identity,
                    new float3(stroke.rect.SizeAsRect(), 1f));
                using (new GLModelViewScope(model))
                    Quad.LineStrip();
            }
        }
    }
    #endregion
    #region methods
    #endregion

    #region declarations
    [System.Serializable]
    public class Dependency {
        public Texture brush;
        public Camera main;
    }
    [System.Serializable]
    public class ScreenTuner : Tuner {
        public bool show_boundary;
    }
    #endregion
}