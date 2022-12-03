using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Rotater : MonoBehaviour {

    public Tuner tuner = new Tuner();

    #region unity
    private void Update() {
        var dt = Time.deltaTime;
        var q = quaternion.EulerXYZ(dt * math.radians(tuner.rotation_speed));
        transform.localRotation *= q;
    }
    #endregion

    #region declarations
    [System.Serializable]
    public class Tuner {
        public float3 rotation_speed;
    }
    #endregion
}

