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

public class TestCollisionBrush : MarblingUVBase {

    public Dependency dep = new Dependency();
    public Tuner tuner = new Tuner();

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

        if (dep.skin != null && dep.surface != null && dep.main != null) {
            var screen = dep.skin.Size();
            var ray = dep.main.ScreenPointToRay(Input.mousePosition);
            if (Input.GetMouseButton(0)
                && dep.surface.Raycast(ray, out var hit, float.MaxValue)) {

                var curr_uv = (float2)hit.textureCoord;
                var offset = uv - curr_uv;
                Debug.Log($"uv={curr_uv}");

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
    #endregion
    #region methods
    #endregion

    #region declarations
    [System.Serializable]
    public class Dependency {
        public Texture brush;
        public Collider surface;
        public Texture skin;
        public Camera main;
    }
    #endregion
}