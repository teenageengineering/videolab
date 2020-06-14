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
        float _delayToOff = 0.0f;

        [SerializeField]
        bool _discrete = false;

        #endregion

        #region Node I/O

        [Inlet]
        public float input {
            set {
                if (!enabled) return;

                if (_discrete) _currentState = State.Dormant;

                _currentValue = value;

                _invokeValue = _currentValue + 1;                               //Makes sure _invokeValue != _currentValue as soon as new input comes in
                                                                                //so that two equal consecutive input values < _threshold both invoke _offEvent in "discrete mode"

                if (_currentValue >= _threshold &&
                    _currentState != State.Enabled)
                {
                    _onEvent.Invoke();

                    if (!_discrete) _currentState = State.Enabled;              //Makes sure state stays Dormant in "discrete mode" so that 
                                                                                //consecutive inputs >= _threshold keep invoking _onEvent
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
        float _delayTimer;
        float _invokeValue;

        #endregion

        #region MonoBehaviour functions

        void Update()
        {
            if (_currentValue >= _threshold)
            {
                _delayTimer = 0;
            }
            else if (_currentValue < _threshold &&
                     _invokeValue != _currentValue &&                 
                     _currentState != State.Disabled)
            {
                _delayTimer += Time.deltaTime;
                if (_delayTimer >= _delayToOff)
                {
                    _offEvent.Invoke();
                    _invokeValue = _currentValue;                               //Makes sure that in "discrete mode" the offEvent does not keep 
                                                                                //getting invoked every frame by single input < _threshold

                    if (!_discrete) _currentState = State.Disabled;             //Makes sure state stays Dormant in "discrete mode" so that 
                                                                                //consecutive inputs < _threshold keep invoking _onEvent                    
                }
            }
        }
        #endregion
    }
}
