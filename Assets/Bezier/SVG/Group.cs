using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bezier
{  
    [RequireComponent(typeof(RectTransform))]
    public class Group : MonoBehaviour 
    {
        public void EnvelopeChildren()
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

        Vector2 Div(Vector2 num, Vector2 denum)
        {
            return new Vector2(num.x / denum.x, num.y / denum.y);
        }

        public void StretchChildren()
        {
            RectTransform rt = transform as RectTransform;

            foreach (RectTransform child in transform)
            {
                if (child.GetComponent<Handle>())
                    break;

                Vector2 pos = (Vector2)child.anchoredPosition + rt.rect.size / 2;
                Vector2 halfsize = child.rect.size / 2;

                child.anchorMin = Div(pos - halfsize, rt.rect.size);
                child.anchorMax = Div(pos + halfsize, rt.rect.size);
                child.offsetMin = Vector2.zero;
                child.offsetMax = Vector2.zero;

                Group childGroup = child.GetComponent<Group>();
                if (childGroup)
                    childGroup.StretchChildren();
            }
        }

        public void FreeChildren()
        {
            RectTransform rt = transform as RectTransform;

            foreach (RectTransform child in transform)
            {
                if (child.GetComponent<Handle>())
                    break;

                Vector2 min = Vector2.Scale(child.anchorMin, rt.rect.size);
                Vector2 max = Vector2.Scale(child.anchorMax, rt.rect.size);

                child.anchorMin = child.anchorMax = Vector2.one / 2;
                child.sizeDelta = max - min;
                child.anchoredPosition = ((max + min) - rt.rect.size) / 2;

                Group childGroup = child.GetComponent<Group>();
                if (childGroup)
                    childGroup.FreeChildren();
            }
        }
    }
}
