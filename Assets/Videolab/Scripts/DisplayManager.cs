using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayManager : MonoBehaviour {

    public string tagName = "MainCamera";

	[HideInInspector]
	public ExternalDisplay externalDisplay;

	GameObject _externalCamera;

	void Update()
	{
        GameObject mainCamera = GameObject.FindGameObjectWithTag(tagName);

		if (mainCamera != _externalCamera)
		{
			if (mainCamera != null)
			{
                if (!mainCamera.GetComponent<ExternalDisplay>())
				    mainCamera.AddComponent<ExternalDisplay>();
			}
		
			_externalCamera = mainCamera;
		}
	}
}
