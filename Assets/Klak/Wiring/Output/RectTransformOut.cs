//
// Klak - Utilities for creative coding with Unity
//
// Copyright (C) 2016 Keijiro Takahashi
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
using UnityEngine;

namespace Klak.Wiring
{
    [AddComponentMenu("Klak/Wiring/Output/Component/RectTransform Out")]
    public class RectTransformOut : NodeBase
    {
        #region Editable properties

        [SerializeField]
        RectTransform _targetTransform;

        [SerializeField]
        bool _addToOriginal = true;

        #endregion

        #region Node I/O

        [Inlet]
        public Vector3 anchoredPosition {
            set {
                if (!enabled || _targetTransform == null) return;
                _targetTransform.anchoredPosition = 
                    _addToOriginal ? _originalAnchoredPosition + (Vector2)value : (Vector2)value;
            }
        }

        [Inlet]
        public Vector3 sizeDelta {
            set {
                if (!enabled || _targetTransform == null) return;
                _targetTransform.sizeDelta =
                    _addToOriginal ? _originalSizeDelta + (Vector2)value : (Vector2)value;
            }
        }

        [Inlet]
        public float uniformScale {
            set {
                if (!enabled || _targetTransform == null) return;
                var s = Vector2.one * value;
                if (_addToOriginal) s += _originalSizeDelta;
                _targetTransform.sizeDelta = s;
            }
        }

        #endregion

        #region Private members

        Vector2 _originalAnchoredPosition;
        Vector2 _originalSizeDelta;

        void OnEnable()
        {
            if (_targetTransform != null)
            {
                _originalAnchoredPosition = _targetTransform.anchoredPosition;
                _originalSizeDelta = _targetTransform.sizeDelta;
            }
        }

        void OnDisable()
        {
            if (_targetTransform != null)
            {
                _targetTransform.anchoredPosition = _originalAnchoredPosition;
                _targetTransform.sizeDelta = _originalSizeDelta;
            }
        }

        #endregion
    }
}
