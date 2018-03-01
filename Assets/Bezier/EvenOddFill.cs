using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

namespace Bezier 
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Graphic))]
    public class EvenOddFill : MonoBehaviour {

        #region Public

        public bool isMask = true;

        [Range(0, 7)]
        public int layer = 0;

        #endregion

        #region Private

        [SerializeField, HideInInspector]
        Shader _shader;

        Material _material;

        #endregion

        #region Monobehaviour

        void Update()
        {
            if (!_material)
            {
                _material = new Material(_shader);

                Graphic graphic = GetComponent<Graphic>();
                graphic.material = _material;
            }

            if (isMask)
            {
                _material.SetFloat("_StencilComp", (float)CompareFunction.Always);
                _material.SetFloat("_StencilOp", (float)StencilOp.Invert);
                _material.SetFloat("_StencilWriteMask", (float)(1 << layer));
                _material.SetFloat("_StencilReadMask", 0);
                _material.SetFloat("_ColorMask", 0);
            }
            else
            {
                _material.SetFloat("_StencilComp", (float)CompareFunction.NotEqual);
                _material.SetFloat("_StencilOp", (float)StencilOp.Keep);
                _material.SetFloat("_StencilWriteMask", 0);
                _material.SetFloat("_StencilReadMask", (float)(1 << layer));
                _material.SetFloat("_ColorMask", 15);
            }
        }

        #endregion
    }
}
