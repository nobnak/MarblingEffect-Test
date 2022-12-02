using MarblingEffectSys;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class TestOffset : MonoBehaviour {

    public Dependency dep = new Dependency();

    public Texture offset_tex;

    #region unity
    private void Update() {
        if (dep != null && dep.effect != null) {
            dep.effect.OffsetTex = offset_tex;
        }        
    }
    #endregion

    #region declarations
    [System.Serializable]
    public class Dependency {
        public MarblingPostEffect effect;
    }
    #endregion
}