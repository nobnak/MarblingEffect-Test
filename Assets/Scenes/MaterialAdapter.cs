using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialAdapter : MonoBehaviour {

    public string prop;
    public Material target;

    public Texture CurrTexture {
        set {
            if (target != null) target.SetTexture(prop, value);
        }
        get => (target != null) ? target.GetTexture(prop) : null;
    }
}