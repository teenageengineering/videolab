//
// KinoIsoline - Isoline effect
//
// Copyright (C) 2015 Keijiro Takahashi
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

namespace Kino
{
    [RequireComponent(typeof(Isoline))]
    [AddComponentMenu("Kino Image Effects/Isoline Scroller")]
    public class IsolineScroller : MonoBehaviour
    {
        [SerializeField]
        Vector3 _direction = Vector3.one * 0.577f;

        public Vector3 direction {
            get { return _direction; }
            set { _direction = value; }
        }

        [SerializeField]
        float _speed = 0.2f;

        public float speed {
            get { return _speed; }
            set { _speed = value; }
        }

        float _time;

        void Update()
        {
            var target = GetComponent<Isoline>();
            var delta = _direction.normalized * _speed * Time.deltaTime;
            target.offset += delta;
        }
    }
}
