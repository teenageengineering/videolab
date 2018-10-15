using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Xml;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

namespace Bezier
{
    public class SVGParser
    {
        #region Public

        // non re-entrant
        public GameObject Parse(string svgStr, bool stripGroups = false)
        {
            _stripGroups = stripGroups;

            // Ignore links to external resources.
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.XmlResolver = null;
            settings.ProhibitDtd = false;

            GameObject go = new GameObject("SVG");
            RectTransform svgElement = go.AddComponent<RectTransform>();

            int maskLayer = 0;

            using (XmlReader reader = XmlReader.Create(new StringReader(svgStr)))
            {
                while (reader.Read() && reader.Name != "svg");

                string viewBoxStr = reader.GetAttribute("viewBox");
                if (viewBoxStr != null)
                {
                    float[] floatArgs = FloatsFromString(viewBoxStr);
                    if (floatArgs.Count() == 4)
                    {
                        svgElement.anchoredPosition = new Vector2(floatArgs[0], -floatArgs[1]);
                        svgElement.sizeDelta = new Vector2(floatArgs[2], floatArgs[3]);

                        _origin = new Vector2(-floatArgs[2] / 2, floatArgs[3] / 2);
                    }
                }

                ParseElementContent(svgElement, reader, ref maskLayer);
            }

            return go;
        }

        #endregion

        #region XML parser

        void ParseElementContent(Transform parentElement, XmlReader reader, ref int maskLayer)
        {
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.EndElement)
                    continue;

                switch (reader.Name)
                {
                    case "g":
                        ParseGroupElement(parentElement, reader);
                        break;
                    case "rect":
                        ParseRectElement(parentElement, reader);
                        break;
                    case "circle":
                        ParseCircleElement(parentElement, reader);
                        break;
                    case "polygon":
                        ParsePolygonElement(parentElement, reader);
                        break;
                    case "path":
                        ParsePathElement(parentElement, reader, ref maskLayer);
                        break;
                }
            }
        }

        void ParseGroupElement(Transform parentElement, XmlReader reader)
        {
            int maskLayer = 0;

            if (_stripGroups)
            {
                using (XmlReader innerReader = reader.ReadSubtree())
                {
                    innerReader.Read();
                    ParseElementContent(parentElement, innerReader, ref maskLayer);
                }
            }
            else
            {
                string id = reader.GetAttribute("id") ?? "Group";

                GameObject go = new GameObject(GameObjectUtility.GetUniqueNameForSibling(parentElement, id));
                RectTransform groupElement = go.AddComponent<RectTransform>();
                groupElement.SetParent(parentElement, false);

                using (XmlReader innerReader = reader.ReadSubtree())
                {
                    innerReader.Read();
                    ParseElementContent(groupElement, innerReader, ref maskLayer);
                }

                EnvelopeChildren(go.transform);
            }
        }

        void ParseRectElement(Transform parentElement, XmlReader reader)
        {
            string id = reader.GetAttribute("id") ?? "Rect";

            string s;
            float x = ((s = reader.GetAttribute("x")) != null) ? _origin.x + float.Parse(s) : _origin.x;
            float y = ((s = reader.GetAttribute("y")) != null) ? _origin.y - float.Parse(s) : _origin.y;
            float w = ((s = reader.GetAttribute("width")) != null) ? float.Parse(s) : 0;
            float h = ((s = reader.GetAttribute("height")) != null) ? float.Parse(s) : 0;
            float rx = ((s = reader.GetAttribute("rx")) != null) ? float.Parse(s) : 0;

            GameObject go = new GameObject(GameObjectUtility.GetUniqueNameForSibling(parentElement, id));
            go.transform.SetParent(parentElement, false);

            Image image = go.AddComponent<Image>();
            image.color = ParseFillColor(reader);

            Shape shape = go.AddComponent<Shape>();
            shape.AddHandle(GameObjectUtility.GetUniqueNameForSibling(shape.transform, "Handle"), new Vector2(x, y), rx).gameObject.SetActive(false);
            shape.AddHandle(GameObjectUtility.GetUniqueNameForSibling(shape.transform, "Handle"), new Vector2(x + w, y), rx).gameObject.SetActive(false);
            shape.AddHandle(GameObjectUtility.GetUniqueNameForSibling(shape.transform, "Handle"), new Vector2(x + w, y - h)).gameObject.SetActive(false);
            shape.AddHandle(GameObjectUtility.GetUniqueNameForSibling(shape.transform, "Handle"), new Vector2(x, y - h), rx).gameObject.SetActive(false);
            EnvelopeChildren(go.transform);
        }

        void ParseCircleElement(Transform parentElement, XmlReader reader)
        {
            string id = reader.GetAttribute("id") ?? "Circle";

            string s;
            float cx = ((s = reader.GetAttribute("cx")) != null) ? _origin.x + float.Parse(s) : _origin.x;
            float cy = ((s = reader.GetAttribute("cy")) != null) ? _origin.y - float.Parse(s) : _origin.y;
            float r = ((s = reader.GetAttribute("r")) != null) ? float.Parse(s) : 0;

            GameObject go = new GameObject(GameObjectUtility.GetUniqueNameForSibling(parentElement, id));
            go.transform.SetParent(parentElement, false);

            Image image = go.AddComponent<Image>();
            image.color = ParseFillColor(reader);

            Shape shape = go.AddComponent<Shape>();
            shape.AddHandle(GameObjectUtility.GetUniqueNameForSibling(shape.transform, "Handle"), new Vector2(cx - r, cy - r), r).gameObject.SetActive(false);
            shape.AddHandle(GameObjectUtility.GetUniqueNameForSibling(shape.transform, "Handle"), new Vector2(cx + r, cy - r), r).gameObject.SetActive(false);
            shape.AddHandle(GameObjectUtility.GetUniqueNameForSibling(shape.transform, "Handle"), new Vector2(cx + r, cy + r), r).gameObject.SetActive(false);
            shape.AddHandle(GameObjectUtility.GetUniqueNameForSibling(shape.transform, "Handle"), new Vector2(cx - r, cy + r), r).gameObject.SetActive(false);
            EnvelopeChildren(go.transform);
        }

        void ParsePolygonElement(Transform parentElement, XmlReader reader)
        {
            string id = reader.GetAttribute("id") ?? "Polygon";
            string pointsStr = reader.GetAttribute("points");

            if (pointsStr != null)
            {
                float[] floatArgs = FloatsFromString(pointsStr);
                ParseMove(parentElement, floatArgs, false);
            }

            _curSegment.gameObject.name = GameObjectUtility.GetUniqueNameForSibling(parentElement, id);

            Graphic graphic = _curSegment.GetComponent<Graphic>();
            graphic.color = ParseFillColor(reader);

            ParseClose();
            EnvelopeChildren(_curSegment.transform);
        }

        void ParsePathElement(Transform parentElement, XmlReader reader, ref int maskLayer)
        {
            string id = reader.GetAttribute("id") ?? "Path";
            string dStr = reader.GetAttribute("d");

            if (dStr != null)
            {
                string separators = @"(?=[A-Za-z])";
                var tokens = Regex.Split(dStr, separators).Where(t => !string.IsNullOrEmpty(t));

                GameObject pathObj = new GameObject(GameObjectUtility.GetUniqueNameForSibling(parentElement, id));
                RectTransform pathElement = pathObj.AddComponent<RectTransform>();
                pathElement.SetParent(parentElement, false);

                foreach (string token in tokens)
                    ParsePathCommand(pathElement, token);

                Color fillColor = ParseFillColor(reader);

                foreach (Transform segment in pathElement)
                    EnvelopeChildren(segment);

                if (pathElement.childCount > 1)
                {
                    EnvelopeChildren(pathElement);

                    if (pathObj.name.StartsWith("Path"))
                        pathObj.name = "Compound " + pathObj.name;

                    foreach (Transform segment in pathElement)
                    {
                        EvenOddFill mask = segment.gameObject.AddComponent<EvenOddFill>();
                        mask.layer = maskLayer;
                    }

                    GameObject fillObj = new GameObject("Segment Fill");
                    RectTransform fill = fillObj.AddComponent<RectTransform>();
                    fill.SetParent(pathElement, false);

                    fill.anchorMin = Vector2.zero;
                    fill.anchorMax = Vector2.one;
                    fill.sizeDelta = Vector2.zero;

                    Image fillImage = fillObj.AddComponent<Image>();
                    fillImage.color = fillColor;

                    EvenOddFill fillEvenOdd = fillObj.AddComponent<EvenOddFill>();
                    fillEvenOdd.isMask = false;
                    fillEvenOdd.layer = maskLayer;

                    maskLayer = (maskLayer + 1) % 8;
                }
                // prune segment container
                else if (pathElement.childCount > 0)
                {
                    _curSegment.gameObject.name = pathObj.name;
                    _curSegment.transform.SetParent(parentElement, false);

                    Graphic graphic = _curSegment.GetComponent<Graphic>();
                    graphic.color = fillColor;

                    DestroyObject(pathElement.gameObject);
                }
            }
        }

        Color ParseFillColor(XmlReader reader)
        {
            Color color = Color.black;

            string styleStr = reader.GetAttribute("style");
            if (styleStr != null)
            {
                var m = Regex.Match(styleStr, @"fill:\s*([^;]*)");
                if (m.Success)
                {
                    string colorStr = m.Groups[1].Captures[0].Value;
                    if (colorStr.Substring(0, 1) != "#")
                        colorStr = SVGHelper.colorCode[colorStr.ToLower()];

                    ColorUtility.TryParseHtmlString(colorStr, out color);
                }
            }

            return color;
        }

        #endregion

        #region Path data parser

        void ParsePathCommand(Transform parentElement, string commandStr)
        {
            char c = commandStr.Substring(0, 1).Single();
            float[] floatArgs = FloatsFromString(commandStr.Substring(1));

            switch (c)
            {
                case 'M':
                case 'm':
                    ParseMove(parentElement, floatArgs, (c == 'm'));
                    break;
                case 'L':
                case 'l':
                    ParseLine(floatArgs, (c == 'l'));
                    break;
                case 'H':
                case 'h':
                    ParseAxisAligned(floatArgs, false, (c == 'h'));
                    break;
                case 'V':
                case 'v':
                    ParseAxisAligned(floatArgs, true, (c == 'v'));
                    break;
                case 'C':
                case 'c':
                case 'S':
                case 's':
                    ParseCubic(floatArgs, (c == 'S' || c == 's'), (c == 'c' || c == 's'));
                    break;
                case 'Q':
                case 'q':
                    ParseQuad(floatArgs, (c == 'q'));
                    break;
                case 'A':
                case 'a':
                    ParseArc(floatArgs, (c == 'a'));
                    break;
                case 'Z':
                case 'z':
                    ParseClose();
                    break;
                default:
                    Debug.LogError("Unsupported command: " + c);
                    break;
            }
        }

        void ParseMove(Transform parentElement, float[] floatArgs, bool isRel)
        {
            // endpoint
            Handle endHandle = null;
            if (_curSegment)
                endHandle = _curSegment.GetHandles()[0];

            // start new segment
            GameObject segmentObj = new GameObject(GameObjectUtility.GetUniqueNameForSibling(parentElement, "Segment"));
            RectTransform segment = segmentObj.AddComponent<RectTransform>();
            segment.SetParent(parentElement, false);

            segmentObj.AddComponent<Image>();
            _curSegment = segmentObj.AddComponent<Shape>();

            // first handle
            int i = 0;
            Vector2 pos = new Vector2(floatArgs[i++], -floatArgs[i++]);
            pos += (isRel && endHandle) ? endHandle.pos : _origin;

            _curHandle = _curSegment.AddHandle(GameObjectUtility.GetUniqueNameForSibling(_curSegment.transform, "Handle"), pos);
            _curHandle.gameObject.SetActive(false);

            if (i < floatArgs.Length)
                ParseLine(floatArgs.Skip(i).Take(floatArgs.Length - i).ToArray(), isRel);
        }

        void ParseLine(float[] floatArgs, bool isRel)
        {
            int i = 0;
            while (i < floatArgs.Length)
            {
                Vector2 pos = new Vector2(floatArgs[i++], -floatArgs[i++]);
                pos += (isRel) ? _curHandle.pos : _origin;

                _curHandle = _curSegment.AddHandle(GameObjectUtility.GetUniqueNameForSibling(_curSegment.transform, "Handle"), pos);
                _curHandle.gameObject.SetActive(false);
            }
        }

        void ParseAxisAligned(float[] floatArgs, bool isVertical, bool isRel)
        {
            int i = 0;
            while (i < floatArgs.Length)
            {
                float a = floatArgs[i++];
                Vector2 pos = _curHandle.pos;
                if (isVertical)
                    pos.y = (isRel) ? pos.y - a : _origin.y - a;
                else
                    pos.x = (isRel) ? pos.x + a : _origin.x + a;

                _curHandle = _curSegment.AddHandle(GameObjectUtility.GetUniqueNameForSibling(_curSegment.transform, "Handle"), pos);
                _curHandle.gameObject.SetActive(false);
            }
        }

        void ParseCubic(float[] floatArgs, bool isSmooth, bool isRel)
        {
            int i = 0;
            while (i < floatArgs.Length)
            {
                Vector2 c1 = _curHandle.pos - (Vector2)_curHandle.control1;
                if (!isSmooth) c1 = new Vector2(floatArgs[i++], -floatArgs[i++]);
                Vector2 c2 = new Vector2(floatArgs[i++], -floatArgs[i++]);
                Vector2 pos = new Vector2(floatArgs[i++], -floatArgs[i++]);

                Vector2 offset = (isRel) ? _curHandle.pos : _origin;
                pos += offset;
                c1 += offset;
                c2 += offset;

                _curHandle.control2 = c1 - _curHandle.pos;
                if (isSmooth)
                    _curHandle.mode = Handle.Mode.Mirrored;

                _curHandle = _curSegment.AddHandle(GameObjectUtility.GetUniqueNameForSibling(_curSegment.transform, "Handle"), pos);
                _curHandle.control1 = c2 - _curHandle.pos;
                _curHandle.gameObject.SetActive(false);
            }
        }

        void ParseQuad(float[] floatArgs, bool isRel)
        {
            // in quad coordinate system
            Vector2 prevPos = new Vector2(_curHandle.pos.x, -_curHandle.pos.y) - _origin;
            prevPos.y = -prevPos.y;

            List<float> cubicArgs = new List<float>();
            int i = 0;
            while (i < floatArgs.Length)
            {
                Vector2 c0 = new Vector2(floatArgs[i++], floatArgs[i++]);
                Vector2 pos = new Vector2(floatArgs[i++], floatArgs[i++]);

                Vector2 c1 = Vector2.Lerp(prevPos, c0, 2f / 3f);
                Vector2 c2 = Vector2.Lerp(pos, c0, 2f / 3f);
                cubicArgs.AddRange(new float[] { c1.x, c1.y, c2.x, c2.y, pos.x, pos.y });

                prevPos = pos;
            }

            ParseCubic(cubicArgs.ToArray(), false, isRel);
        }

        void ParseArc(float[] floatArgs, bool isRel)
        {
            // in arc coordinate system
            Vector2 prevPos = new Vector2(_curHandle.pos.x, _curHandle.pos.y) - _origin;
            prevPos.y = -prevPos.y;

            List<float> cubicArgs = new List<float>();
            int i = 0;
            while (i < floatArgs.Length)
            {
                Vector2 r = new Vector2(floatArgs[i++], floatArgs[i++]);
                float phi = -floatArgs[i++];
                float arcFlag = floatArgs[i++];
                float sweepFlag = floatArgs[i++];

                Vector2 pos = new Vector2(floatArgs[i++], floatArgs[i++]);
                if (isRel) pos += prevPos;

                float[] c = SVGHelper.a2c(prevPos.x, prevPos.y, pos.x, pos.y, arcFlag, sweepFlag, r.x, r.y, phi);
                cubicArgs.AddRange(c);

                prevPos = pos;
            }

            ParseCubic(cubicArgs.ToArray(), false, false);
        }

        void ParseClose()
        {
            Handle startHandle = _curSegment.GetHandles()[0];

            if ((_curHandle.pos - startHandle.pos).magnitude < Curve.precision)
            {
                startHandle.control1 = _curHandle.control1;

                DestroyObject(_curHandle.gameObject);
            }

            _curSegment.closedPath = true;
        }

        #endregion

        #region Helpers

        bool _stripGroups;

        Vector2 _origin;
        Shape _curSegment;
        Handle _curHandle;

        float[] FloatsFromString(string args)
        {
            // fix for adobe terseness
            args = Regex.Replace(args, @"(\.\d+)\.", "$1 .");

            string argSeparators = @"[\s,]|(?=-)";
            var splitArgs = Regex.Split(args, argSeparators).Where(t => !string.IsNullOrEmpty(t));

            return splitArgs.Select(arg => float.Parse(arg)).ToArray();
        }

        void EnvelopeChildren(Transform transform)
        {
            Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 max = new Vector2(float.MinValue, float.MinValue);

            foreach (RectTransform child in transform)
            {
                Vector2 pos = child.anchoredPosition;
                min = Vector2.Min(pos + child.rect.min, min);
                max = Vector2.Max(pos + child.rect.max, max);
            }

            RectTransform rt = transform as RectTransform;
            rt.sizeDelta = max - min;
            rt.anchoredPosition = (max + min) / 2;
            foreach (RectTransform child in transform)
                child.anchoredPosition -= rt.anchoredPosition;
        }

        void DestroyObject(GameObject go)
        {
            if (Application.isEditor)
                GameObject.DestroyImmediate(go);
            else
                GameObject.Destroy(go);
        }

        #endregion
    }
}
