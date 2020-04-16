using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
public class PalutteEffect : MonoBehaviour {

	public Material material;

	public Texture LUTTexture;

	public int gridWidth = 16;
	public int gridHeight = 16;

	public int pixelsWidth = 200;
	public int pixelsHeight = 150;

	private int activeWidth = 200;
	private int activeHeight = 200;

	public bool autoSetWidth = false;
	public bool autoSetHeight = false;
	public bool matchCamSize = false;

	[Range(0f, 0.5f)]
	public float ditherAmount = 0.1f;

	[Range(0.5f, 2f)]
	public float pixelStretch = 1f;

	public bool jaggiesAreGood = true;

	private Camera cam;

	void OnRenderImage (RenderTexture source, RenderTexture destination) {
		
		if (autoSetWidth) {
			if (cam == null) cam = GetComponent<Camera>();
			float bestFraction = (float)pixelsHeight / (float)cam.pixelHeight;
			pixelsWidth = Mathf.RoundToInt(cam.pixelWidth * bestFraction * pixelStretch);
		}else if (autoSetHeight) {
			if (cam == null) cam = GetComponent<Camera>();
			float bestFraction = (float)pixelsWidth / (float)cam.pixelWidth;
			pixelsHeight = Mathf.RoundToInt(cam.pixelHeight * bestFraction * pixelStretch);
		}
		activeWidth = pixelsWidth;
		activeHeight = pixelsHeight;

		if (matchCamSize) {
			if (cam == null) cam = GetComponent<Camera>();
			activeWidth = cam.pixelWidth;
			activeHeight = cam.pixelHeight;
		}

		if (LUTTexture == null) {
			Graphics.Blit(source, destination);
			return;
		}

		material.SetFloat("_PWidth", activeWidth);
		material.SetFloat("_PHeight", activeHeight);
		material.SetFloat("_DitherRange", ditherAmount);
		material.SetTexture("_LUTTex", LUTTexture);
		material.SetFloat("_LUTBlueTilesX", gridWidth);
		material.SetFloat("_LUTBlueTilesY", gridHeight);
		material.SetFloat("_GridFractionX", 1f / (float)gridWidth);
		material.SetFloat("_GridFractionY", 1f / (float)gridHeight);

		if (jaggiesAreGood) {
			source.filterMode = FilterMode.Point;
		}

		Graphics.Blit(source, destination, material);
	}
}
