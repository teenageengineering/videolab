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
    [AddComponentMenu("Klak/Wiring/Convertion/Accumulator")]
    public class Accumulator : NodeBase
    {
        [SerializeField]
        float _floatValue;
        public float floatValue {
            get { return _floatValue; }
            set { _floatValue = value; }
        }

        #region Node I/O

        [Inlet]
        public float reset {
            set {
                if (!enabled) return;
                _floatValue = 0;
            }
        }

        [Inlet]
        public float delta {
            set {
                if (!enabled) return;
                _floatValue += value;
            }
        }

        [SerializeField, Outlet]
        FloatEvent _valueEvent = new FloatEvent();

        #endregion

        float _prevValue;

        #region Monobehaviour

        void Update()
        {
            if (floatValue != _prevValue)
			{
				_valueEvent.Invoke(_floatValue); 
                _prevValue = _floatValue;
            }
        }

        #endregion
    }
}