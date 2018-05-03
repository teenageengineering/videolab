// source: https://github.com/fontello/svgpath/blob/master/lib/a2c.js

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SVGHelper
{
    const float TAU = Mathf.PI * 2;

    // Calculate an angle between two unit vectors
    //
    // Since we measure angle between radii of circular arcs,
    // we can use simplified math (without length normalization)
    //
    static float unit_vector_angle(float ux, float uy, float vx, float vy) {
        var sign = (ux * vy - uy * vx < 0) ? -1 : 1;
        var dot  = ux * vx + uy * vy;

        // Add this to work with arbitrary vectors:
        // dot /= Math.sqrt(ux * ux + uy * uy) * Math.sqrt(vx * vx + vy * vy);

        // rounding errors, e.g. -1.0000000000000002 can screw up this
        if (dot >  1.0) { dot = 1.0f; }
        if (dot < -1.0) { dot = -1.0f; }

        return sign * Mathf.Acos(dot);
    }


    // Convert from endpoint to center parameterization,
    // see http://www.w3.org/TR/SVG11/implnote.html{","#ArcImp"},
    //
    // Return [cx, cy, theta1, delta_theta]
    //
    static float[] get_arc_center(float x1, float y1, float x2, float y2, float fa, float fs, float rx, float ry, float sin_phi, float cos_phi) {
        // Step 1.
        //
        // Moving an ellipse so origin will be the middlepoint between our two
        // points. After that, rotate it to line up ellipse axes with coordinate
        // axes.
        //
        var x1p =  cos_phi*(x1-x2)/2 + sin_phi*(y1-y2)/2;
        var y1p = -sin_phi*(x1-x2)/2 + cos_phi*(y1-y2)/2;

        var rx_sq  =  rx * rx;
        var ry_sq  =  ry * ry;
        var x1p_sq = x1p * x1p;
        var y1p_sq = y1p * y1p;

        // Step 2.
        //
        // Compute coordinates of the centre of this ellipse (cx', cy')
        // in the new coordinate system.
        //
        var radicant = (rx_sq * ry_sq) - (rx_sq * y1p_sq) - (ry_sq * x1p_sq);

        if (radicant < 0) {
            // due to rounding errors it might be e.g. -1.3877787807814457e-17
            radicant = 0;
        }

        radicant /=   (rx_sq * y1p_sq) + (ry_sq * x1p_sq);
        radicant = Mathf.Sqrt(radicant) * (fa == fs ? -1 : 1);

        var cxp = radicant *  rx/ry * y1p;
        var cyp = radicant * -ry/rx * x1p;

        // Step 3.
        //
        // Transform back to get centre coordinates (cx, cy) in the original
        // coordinate system.
        //
        var cx = cos_phi*cxp - sin_phi*cyp + (x1+x2)/2;
        var cy = sin_phi*cxp + cos_phi*cyp + (y1+y2)/2;

        // Step 4.
        //
        // Compute angles (theta1, delta_theta).
        //
        var v1x =  (x1p - cxp) / rx;
        var v1y =  (y1p - cyp) / ry;
        var v2x = (-x1p - cxp) / rx;
        var v2y = (-y1p - cyp) / ry;

        var theta1 = unit_vector_angle(1, 0, v1x, v1y);
        var delta_theta = unit_vector_angle(v1x, v1y, v2x, v2y);

        if (fs == 0 && delta_theta > 0) {
            delta_theta -= TAU;
        }
        if (fs == 1 && delta_theta < 0) {
            delta_theta += TAU;
        }

        return new float[] { cx, cy, theta1, delta_theta };
    }

    //
    // Approximate one unit arc segment with bézier curves,
    // see http://math.stackexchange.com/questions/873224
    //
    public static float[] approximate_unit_arc(float theta1, float delta_theta) {
        var alpha = 4f / 3 * Mathf.Tan(delta_theta/4);

        var x1 = Mathf.Cos(theta1);
        var y1 = Mathf.Sin(theta1);
        var x2 = Mathf.Cos(theta1 + delta_theta);
        var y2 = Mathf.Sin(theta1 + delta_theta);

        return new float[] { x1 - y1*alpha, y1 + x1*alpha, x2 + y2*alpha, y2 - x2*alpha, x2, y2 };
    }

    public static float[] a2c(float x1, float y1, float x2, float y2, float fa, float fs, float rx, float ry, float phi) {
        var sin_phi = Mathf.Sin(phi * TAU / 360);
        var cos_phi = Mathf.Cos(phi * TAU / 360);

        // Make sure radii are valid
        //
        var x1p =  cos_phi*(x1-x2)/2 + sin_phi*(y1-y2)/2;
        var y1p = -sin_phi*(x1-x2)/2 + cos_phi*(y1-y2)/2;

        if (x1p == 0 && y1p == 0) {
            // we're asked to draw line to itself
            return new float[0];
        }

        if (rx == 0 || ry == 0) {
            // one of the radii is zero
            return new float[0];
        }


        // Compensate out-of-range radii
        //
        rx = Mathf.Abs(rx);
        ry = Mathf.Abs(ry);

        var lambda = (x1p * x1p) / (rx * rx) + (y1p * y1p) / (ry * ry);
        if (lambda > 1) {
            rx *= Mathf.Sqrt(lambda);
            ry *= Mathf.Sqrt(lambda);
        }


        // Get center parameters (cx, cy, theta1, delta_theta)
        //
        var cc = get_arc_center(x1, y1, x2, y2, fa, fs, rx, ry, sin_phi, cos_phi);

        var result = new List<float>();
        var theta1 = cc[2];
        var delta_theta = cc[3];

        // Split an arc to multiple segments, so each segment
        // will be less than τ/4 (= 90°)
        //
        var segments = Mathf.Max(Mathf.Ceil(Mathf.Abs(delta_theta) / (TAU / 4)), 1);
        delta_theta /= segments;

        for (var i = 0; i < segments; i++) {
            result.AddRange(approximate_unit_arc(theta1, delta_theta));
            theta1 += delta_theta;
        }

        // We have a bezier approximation of a unit circle,
        // now need to transform back to the original ellipse
        //
        float[] curve = new float[result.Count];

        for (var i = 0; i < result.Count; i += 2)
        {
            var x = result[i + 0];
            var y = result[i + 1];

            // scale
            x *= rx;
            y *= ry;

            // rotate
            var xp = cos_phi*x - sin_phi*y;
            var yp = sin_phi*x + cos_phi*y;

            // translate
            curve[i + 0] = xp + cc[0];
            curve[i + 1] = yp + cc[1];
        }

        return curve;
    }

    public static Dictionary<string, string> colorCode = new Dictionary<string, string>()
    {
        {"aliceblue",           "#f0f8ff"},
        {"antiquewhite",        "#faebd7"},
        {"aqua",                "#00ffff"},
        {"aquamarine",          "#7fffd4"},
        {"azure",               "#f0ffff"},
        {"beige",               "#f5f5dc"},
        {"bisque",              "#ffe4c4"},
        {"black",               "#000000"},
        {"blanchedalmond",      "#ffebcd"},
        {"blue",                "#0000ff"},
        {"blueviolet",          "#8a2be2"},
        {"brown",               "#a52a2a"},
        {"burlywood",           "#deb887"},
        {"cadetblue",           "#5f9ea0"},
        {"chartreuse",          "#7fff00"},
        {"chocolate",           "#d2691e"},
        {"coral",               "#ff7f50"},
        {"cornflower",          "#6495ed"},
        {"cornsilk",            "#fff8dc"},
        {"crimson",             "#dc143c"},
        {"cyan",                "#00ffff"},
        {"darkblue",            "#00008b"},
        {"darkcyan",            "#008b8b"},
        {"darkgoldenrod",       "#b8860b"},
        {"darkgray",            "#a9a9a9"},
        {"darkgreen",           "#006400"},
        {"darkkhaki",           "#bdb76b"},
        {"darkmagenta",         "#8b008b"},
        {"darkolivegreen",      "#556b2f"},
        {"darkorange",          "#ff8c00"},
        {"darkorchid",          "#9932cc"},
        {"darkred",             "#8b0000"},
        {"darksalmon",          "#e9967a"},
        {"darkseagreen",        "#8fbc8f"},
        {"darkslateblue",       "#483d8b"},
        {"darkslategray",       "#2f4f4f"},
        {"darkturquoise",       "#00ced1"},
        {"darkviolet",          "#9400d3"},
        {"deeppink",            "#ff1493"},
        {"deepskyblue",         "#00bfff"},
        {"dimgray",             "#696969"},
        {"dodgerblue",          "#1e90ff"},
        {"firebrick",           "#b22222"},
        {"floralwhite",         "#fffaf0"},
        {"forestgreen",         "#228b22"},
        {"fuchsia",             "#ff00ff"},
        {"gainsboro",           "#dcdcdc"},
        {"ghostwhite",          "#f8f8ff"},
        {"gold",                "#ffd700"},
        {"goldenrod",           "#daa520"},
        {"gray",                "#808080"},
        {"green",               "#00ff00"},
        {"webgreen",            "#008000"},
        {"greenyellow",         "#adff2f"},
        {"honeydew",            "#f0fff0"},
        {"hotpink",             "#ff69b4"},
        {"indianred",           "#cd5c5c"},
        {"indigo",              "#4b0082"},
        {"ivory",               "#fffff0"},
        {"khaki",               "#f0e68c"},
        {"lavender",            "#e6e6fa"},
        {"lavenderblush",       "#fff0f5"},
        {"lawngreen",           "#7cfc00"},
        {"lemonchiffon",        "#fffacd"},
        {"lightblue",           "#add8e6"},
        {"lightcoral",          "#f08080"},
        {"lightcyan",           "#e0ffff"},
        {"lightgoldenrod",      "#fafad2"},
        {"lightgray",           "#d3d3d3"},
        {"lightgreen",          "#90ee90"},
        {"lightpink",           "#ffb6c1"},
        {"lightsalmon",         "#ffa07a"},
        {"lightseagreen",       "#20b2aa"},
        {"lightskyblue",        "#87cefa"},
        {"lightslategray",      "#778899"},
        {"lightsteelblue",      "#b0c4de"},
        {"lightyellow",         "#ffffe0"},
        {"lime",                "#00ff00"},
        {"limegreen",           "#32cd32"},
        {"linen",               "#faf0e6"},
        {"magenta",             "#ff00ff"},
        {"maroon",              "#b03060"},
        {"webmaroon",           "#7f0000"},
        {"mediumaquamarine",    "#66cdaa"},
        {"mediumblue",          "#0000cd"},
        {"mediumorchid",        "#ba55d3"},
        {"mediumpurple",        "#9370db"},
        {"mediumseagreen",      "#3cb371"},
        {"mediumslateblue",     "#7b68ee"},
        {"mediumspringgreen",   "#00fa9a"},
        {"mediumturquoise",     "#48d1cc"},
        {"mediumvioletred",     "#c71585"},
        {"midnightblue",        "#191970"},
        {"mintcream",           "#f5fffa"},
        {"mistyrose",           "#ffe4e1"},
        {"moccasin",            "#ffe4b5"},
        {"navajowhite",         "#ffdead"},
        {"navyblue",            "#000080"},
        {"oldlace",             "#fdf5e6"},
        {"olive",               "#808000"},
        {"olivedrab",           "#6b8e23"},
        {"orange",              "#ffa500"},
        {"orangered",           "#ff4500"},
        {"orchid",              "#da70d6"},
        {"palegoldenrod",       "#eee8aa"},
        {"palegreen",           "#98fb98"},
        {"paleturquoise",       "#afeeee"},
        {"palevioletred",       "#db7093"},
        {"papayawhip",          "#ffefd5"},
        {"peachpuff",           "#ffdab9"},
        {"peru",                "#cd853f"},
        {"pink",                "#ffc0cb"},
        {"plum",                "#dda0dd"},
        {"powderblue",          "#b0e0e6"},
        {"purple",              "#a020f0"},
        {"webpurple",           "#7f007f"},
        {"rebeccapurple",       "#663399"},
        {"red",                 "#ff0000"},
        {"rosybrown",           "#bc8f8f"},
        {"royalblue",           "#4169e1"},
        {"saddlebrown",         "#8b4513"},
        {"salmon",              "#fa8072"},
        {"sandybrown",          "#f4a460"},
        {"seagreen",            "#2e8b57"},
        {"seashell",            "#fff5ee"},
        {"sienna",              "#a0522d"},
        {"silver",              "#c0c0c0"},
        {"skyblue",             "#87ceeb"},
        {"slateblue",           "#6a5acd"},
        {"slategray",           "#708090"},
        {"snow",                "#fffafa"},
        {"springgreen",         "#00ff7f"},
        {"steelblue",           "#4682b4"},
        {"tan",                 "#d2b48c"},
        {"teal",                "#008080"},
        {"thistle",             "#d8bfd8"},
        {"tomato",              "#ff6347"},
        {"turquoise",           "#40e0d0"},
        {"violet",              "#ee82ee"},
        {"wheat",               "#f5deb3"},
        {"white",               "#ffffff"},
        {"whitesmoke",          "#f5f5f5"},
        {"yellow",              "#ffff00"},
        {"yellowgreen",         "#9acd32"},
        {"none",                "#00000000"}
    };
}
