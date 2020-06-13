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
    [AddComponentMenu("Klak/Wiring/Switching/Threshold")]
    public class Threshold : NodeBase
    {
        #region Editable properties

        [SerializeField]
        float _threshold = 0.01f;

        [SerializeField]
        bool _discrete = false;

        #endregion

        #region Node I/O

        [Inlet]
        public float input {
            set {
                if (!enabled) return;

                _currentValue = value;

                if (_discrete)
                {
                    _currentState = State.Dormant;
                }

                if (_currentValue >= _threshold &&
                    _currentState != State.Enabled)
                {
                    _onEvent.Invoke();
                    
                    if (!_discrete)         //Makes sure _currentState stays Dormant in discrete mode so that
                                            //consecutive input values >= _threshold keep invoking _onEvent
                    {
                        _currentState = State.Enabled;
                    }
                }

                else if (_currentValue < _threshold &&
                     _currentState != State.Disabled)
                {
                    _offEvent.Invoke();

                    if (!_discrete)         //Same but for consecutive input values < _threshold to keep invoking _offEvent
                    {      
                        _currentState = State.Disabled;
                    }
                    
                }
            }
        }

        [SerializeField, Outlet]
        VoidEvent _onEvent = new VoidEvent();

        [SerializeField, Outlet]
        VoidEvent _offEvent = new VoidEvent();

        #endregion

        #region Private members

        enum State { Dormant, Enabled, Disabled }

        State _currentState;
        float _currentValue;

        #endregion
    }
}
