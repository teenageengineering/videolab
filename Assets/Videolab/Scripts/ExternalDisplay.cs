using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ExternalDisplay : MonoBehaviour 
{
	void Start()
	{
		StartCoroutine(CheckExternalDisplays());
	}

	int currentDisplayCount = 1;

	IEnumerator CheckExternalDisplays()
	{
		while (true)
		{
			if (currentDisplayCount != Display.displays.Length)
			{
				Camera camera = gameObject.GetComponent<Camera>();
				if (Display.displays.Length > 1)
				{
					Display extDisplay = Display.displays[1];
					Screen.SetResolution(extDisplay.renderingWidth, extDisplay.renderingHeight, true);
					extDisplay.Activate();
					camera.SetTargetBuffers(extDisplay.colorBuffer, extDisplay.depthBuffer);
				}
				else
				{
					Display intDisplay = Display.displays[0];
					camera.SetTargetBuffers(intDisplay.colorBuffer, intDisplay.depthBuffer);
					camera.depth = 0; // camera loses depth setting when switching buffers
				}

				currentDisplayCount = Display.displays.Length;
			}

			yield return new WaitForSeconds(1f);
		}
	}
}
