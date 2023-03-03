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
using System.Reflection;

namespace Klak.Wiring
{
    [AddComponentMenu("Klak/Wiring/Output/Generic/Float Out")]
    public class FloatOut : NodeBase
    {
        #region Editable properties

        [SerializeField]
        Component _target;

        [SerializeField]
        string _propertyName;

        [SerializeField]
        string _particleSystemModuleName = "<none>";

        #endregion

        #region Node I/O

        [Inlet]
        public float input 
        {
            set 
            {
                if (!enabled || _target == null || _propertyInfo == null) return;

                if (_target.GetType() == typeof(ParticleSystem) && _particleSystemModuleName != "<none>")
                {
                    _propertyInfo.SetValue(_boxedStruct, value, null);
                }
                    
                else _propertyInfo.SetValue(_target, value, null);
            }
        }

        #endregion

        #region Private members

        PropertyInfo _propertyInfo;
        PropertyInfo _moduleInfo;
        object _boxedStruct;

        void OnEnable()
        {
            if (_target == null || string.IsNullOrEmpty(_propertyName)) return;

            else
            {
                if (_target.GetType() == typeof(ParticleSystem) && _particleSystemModuleName != "<none>")
                {
                    _moduleInfo = _target.GetType().GetProperty(_particleSystemModuleName);
                    _propertyInfo = _moduleInfo.PropertyType.GetProperty(_propertyName);
                    _boxedStruct = _moduleInfo.GetValue(_target);
                }

                else
                {
                    _propertyInfo = _target.GetType().GetProperty(_propertyName);
                }
            }
        }

        #endregion
    }
}
