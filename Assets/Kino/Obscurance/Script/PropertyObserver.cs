//
// Kino/Obscurance - Screen space ambient obscurance image effect
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

namespace Kino
{
    public partial class Obscurance : MonoBehaviour
    {
        // Observer class that detects changes on properties
        class PropertyObserver
        {
            // Obscurance properties
            bool _downsampling;
            OcclusionSource _occlusionSource;
            bool _ambientOnly;

            // Camera properties
            int _pixelWidth;
            int _pixelHeight;

            // Check if it has to reset itself for property changes.
            public bool CheckNeedsReset(Obscurance target, Camera camera)
            {
                return
                    _downsampling != target.downsampling ||
                    _occlusionSource != target.occlusionSource ||
                    _ambientOnly != target.ambientOnly ||
                    _pixelWidth != camera.pixelWidth ||
                    _pixelHeight != camera.pixelHeight;
            }

            // Update the internal state.
            public void Update(Obscurance target, Camera camera)
            {
                _downsampling = target.downsampling;
                _occlusionSource = target.occlusionSource;
                _ambientOnly = target.ambientOnly;
                _pixelWidth = camera.pixelWidth;
                _pixelHeight = camera.pixelHeight;
            }
        }
    }
}
