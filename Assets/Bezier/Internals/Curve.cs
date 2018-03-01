using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Bezier
{
    [Serializable]
    public class Point {

        public Vector2 p;
        public Vector2[] c = new Vector2[2];

        public Point() {}

        public Point(Vector2 p, Vector2 c1, Vector2 c2)
        {
            this.p = p;
            this.c[0] = c1;
            this.c[1] = c2;
        }
    }

    public static class Curve
    {
        public static float precision = 0.1f;

        public static Vector2 Eval(Point pt0, Point pt1, float t)
        {
            float oneMinusT = 1f - t;

            return oneMinusT * oneMinusT * oneMinusT * pt0.p +
                3f * oneMinusT * oneMinusT * t * pt0.c[1] +
                3f * oneMinusT * t * t * pt1.c[0] +
                t * t * t * pt1.p;
        }

        public static Vector2 Deriv(Point pt0, Point pt1, float t)
        {
            float oneMinusT = 1f - t;

            return 3f * oneMinusT * oneMinusT * (pt0.c[1] - pt0.p) +
                6f * oneMinusT * t * (pt1.c[0] - pt0.c[1]) +
                3f * t * t * (pt1.p - pt1.c[0]);
        }
    }
}
