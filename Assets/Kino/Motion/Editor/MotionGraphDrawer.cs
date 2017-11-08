//
// Kino/Motion - Motion blur effect
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
using UnityEditor;

namespace Kino
{
    public class MotionGraphDrawer
    {
        #region Public methods

        public MotionGraphDrawer(Texture blendingIcon)
        {
            _blendingIcon = blendingIcon;

            _lowerCenterStyle = new GUIStyle(EditorStyles.miniLabel);
            _lowerCenterStyle.alignment = TextAnchor.LowerCenter;

            _middleCenterStyle = new GUIStyle(EditorStyles.miniLabel);
            _middleCenterStyle.alignment = TextAnchor.MiddleCenter;

            if (EditorGUIUtility.isProSkin)
            {
                _colorDark = new Color(0.18f, 0.18f, 0.18f);
                _colorGray = new Color(0.43f, 0.43f, 0.43f);
            }
            else
            {
                _colorDark = new Color(0.64f, 0.64f, 0.64f);
                _colorGray = new Color(0.92f, 0.92f, 0.92f);
            }
        }

        public void DrawShutterGraph(float angle)
        {
            var center = GUILayoutUtility.GetRect(128, kHeight).center;

            // Parameters used to make transitions smooth.
            var zeroWhenOff = Mathf.Min(1.0f, angle * 0.1f);
            var zeroWhenFull = Mathf.Min(1.0f, (360 - angle) * 0.02f);

            // Shutter angle graph
            var discCenter = center - new Vector2(kHeight * 2.4f, 0);
            // - exposure duration indicator
            DrawDisc(discCenter, kHeight * Mathf.Lerp(0.5f, 0.38f, zeroWhenFull), _colorGray);
            // - shutter disc
            DrawDisc(discCenter, kHeight * 0.16f * zeroWhenFull, _colorDark);
            // - shutter blade
            DrawArc(discCenter, kHeight * 0.5f, 360 - angle, _colorDark);
            // - shutter axis
            DrawDisc(discCenter, zeroWhenOff, _colorGray);

            // Shutter label (off/full)
            var labelSize = new Vector2(kHeight, kHeight);
            var labelOrigin = discCenter - labelSize * 0.5f;
            var labelRect = new Rect(labelOrigin, labelSize);

            if (angle == 0)
                GUI.Label(labelRect, "Off", _middleCenterStyle);
            else if (angle == 360)
                GUI.Label(labelRect, "Full", _middleCenterStyle);

            // Exposure time bar graph
            var outerBarSize = new Vector2(4.75f, 0.5f) * kHeight;
            var innerBarSize = outerBarSize;
            innerBarSize.x *= angle / 360;

            var barCenter = center + new Vector2(kHeight * 0.9f, 0);
            var barOrigin = barCenter - outerBarSize * 0.5f;

            DrawRect(barOrigin, outerBarSize, _colorDark);
            DrawRect(barOrigin, innerBarSize, _colorGray);

            var barText = "Exposure time = " + (angle / 3.6f).ToString("0") + "% of ΔT";
            GUI.Label(new Rect(barOrigin, outerBarSize), barText, _middleCenterStyle);
        }

        public void DrawBlendingGraph(float strength)
        {
            var center = GUILayoutUtility.GetRect(128, kHeight).center;

            var iconSize = new Vector2(kHeight, kHeight);
            var iconStride = new Vector2(kHeight * 0.9f, 0);
            var iconOrigin = center - iconSize * 0.5f - iconStride * 2;

            for (var i = 0; i < 5; i++)
            {
                var weight = BlendingWeight(strength, i / 60.0f);
                var rect = new Rect(iconOrigin + iconStride * i, iconSize);

                var color = _colorGray;
                color.a = weight;

                GUI.color = color;
                GUI.Label(rect, _blendingIcon);

                GUI.color = Color.white;
                GUI.Label(rect, (weight * 100).ToString("0") + "%", _lowerCenterStyle);
            }
            // EditorGUIUtility.isProSkin
        }

        #endregion

        #region Private members

        const float kHeight = 32;

        Texture _blendingIcon;

        GUIStyle _lowerCenterStyle;
        GUIStyle _middleCenterStyle;

        Color _colorDark;
        Color _colorGray;

        Vector3[] _rectVertices = new Vector3[4];

        // Weight function for multi frame blending
        float BlendingWeight(float strength, float time)
        {
            if (strength > 0 || time == 0)
                return Mathf.Exp(-time * Mathf.Lerp(80.0f, 16.0f, strength));
            else
                return 0;
        }

        // Draw a solid disc in the graph rect.
        void DrawDisc(Vector2 center, float radius, Color fill)
        {
            Handles.color = fill;
            Handles.DrawSolidDisc(center, Vector3.forward, radius);
        }

        // Draw an arc in the graph rect.
        void DrawArc(Vector2 center, float radius, float angle, Color fill)
        {
            var start = new Vector2(
                -Mathf.Cos(Mathf.Deg2Rad * angle / 2),
                 Mathf.Sin(Mathf.Deg2Rad * angle / 2)
            );

            Handles.color = fill;
            Handles.DrawSolidArc(center, Vector3.forward, start, angle, radius);
        }

        // Draw a rectangle in the graph rect.
        void DrawRect(Vector2 origin, Vector2 size, Color color)
        {
            var p0 = origin;
            var p1 = origin + size;

            _rectVertices[0] = p0;
            _rectVertices[1] = new Vector2(p1.x, p0.y);
            _rectVertices[2] = p1;
            _rectVertices[3] = new Vector2(p0.x, p1.y);

            Handles.color = Color.white;
            Handles.DrawSolidRectangleWithOutline(_rectVertices, color, Color.clear);
        }

        #endregion
    }
}
