using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Palutte {
	public class PalutteGenerator : EditorWindow {

		[MenuItem("Window/Palutte Generator")]
		static void Init() {

			PalutteGenerator window = (PalutteGenerator)EditorWindow.GetWindow<PalutteGenerator>();
			window.Show();
		}

		static Texture2D paletteSource;

		static Texture2D LUTtexture;
		static bool LUTGenerated = false;

		static bool generatePureLUT = false;

		static int RGDetail = 64;
		static int gridWidth = 16;
		static int gridHeight = 16;

		//static int tilePadding = 2;

		static float RGBInfluence = 1f;
		static float HInfluence = 0f;
		static float SInfluence = 0f;
		static float VInfluence = 0f;
		static float CIELabInfluence = 0f;//for when you wanna get SERIOUS

		static float progressBarYpos = 0;
		static string progressBarString = "";

		static bool working = false;

		static bool paletteObjectSelected = false;
		static bool paletteIsReadable = false;
		static bool paletteIsSmall = false;

		static Vector2 scrollPos;

		void OnGUI() {
			//GUILayout.Label(Random.Range(0, 100).ToString());
			scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
			GUILayout.Label("1) Select Palette image");
			EditorGUI.BeginChangeCheck();
			paletteSource = (Texture2D)EditorGUILayout.ObjectField(paletteSource, typeof(Texture2D), false);
			if (EditorGUI.EndChangeCheck()) {
				//just added a palette image, lets check it now
				paletteObjectSelected = paletteSource == true;
				CheckPalette();
			}
			if (paletteObjectSelected) {
				if (!paletteIsReadable) {
					GUILayout.Label("- Palette needs Read/write enabled in import settings.");
				}
				if (!paletteIsSmall) {
					GUILayout.Label("- Warning; larger palette images take longer.");
				}
			}
			GUILayout.Label("");


			GUILayout.Label("2) Choose LUT generation settings");
			RGDetail = EditorGUILayout.IntField("LUT tile size:", RGDetail);
			gridWidth = EditorGUILayout.IntField("LUT tiles X:", gridWidth);
			gridHeight = EditorGUILayout.IntField("LUT tiles Y:", gridHeight);
			//tilePadding = EditorGUILayout.IntField("LUT tiles padding:", tilePadding);

			GUILayout.Label("");
			generatePureLUT = EditorGUILayout.Toggle("Generate test LUT:", generatePureLUT);
			if (!generatePureLUT) {
				RGBInfluence = EditorGUILayout.FloatField("RGB Influence:", RGBInfluence);
				HInfluence = EditorGUILayout.FloatField("Hue Influence:", HInfluence);
				SInfluence = EditorGUILayout.FloatField("Saturation Influence:", SInfluence);
				VInfluence = EditorGUILayout.FloatField("Value Influence:", VInfluence);
				CIELabInfluence = EditorGUILayout.FloatField("CIE L*ab Influence:", CIELabInfluence);
			}
			GUILayout.Label("");
			GUILayout.Label("3) Go");
			if (paletteSource) {
				if (!working) {
					if (GUILayout.Button("Generate")) {
						BeginGeneration();
					}
				} else {
					if (GUILayout.Button("Cancel")) {
						CancelGeneration();
					}
				}
			}

			if (Event.current.type == EventType.Repaint) progressBarYpos = GUILayoutUtility.GetLastRect().y + GUILayoutUtility.GetLastRect().height;

			if (working || generationProgress != 0f) {
				EditorGUI.ProgressBar(new Rect(3, progressBarYpos + 3, position.width - 6, 20), generationProgress, progressBarString);
			}

			if (LUTGenerated && LUTtexture) {
				GUI.DrawTexture(new Rect(3, progressBarYpos + 26, position.width - 6, (position.width - 6) * heightScale), LUTtexture);

				if (GUI.Button(new Rect(3, progressBarYpos + 34 + (position.width - 6) * heightScale, position.width / 3, 20), "Save")) {
					string filename = EditorUtility.SaveFilePanel("Save LUT texture", Application.dataPath, "newPaletteLUT.png", "png");
					if (filename.Length != 0) SaveLUT(filename);
				}
			}

			GUILayout.Label("", GUILayout.Width(5), GUILayout.Height(54 + (position.width - 6) * heightScale));


			EditorGUILayout.EndScrollView();

			if (working) {
				DoWork();
			}


		}

		public void Update() {
			if (working) {
				Repaint();
			}
		}

		void OnInspectorUpdate() {
			Repaint();
		}

		static void CancelGeneration() {
			working = false;
			progressBarString = "";
			generationProgress = 0f;
		}

		static int LUTWidth = 1;
		static int LUTHeight = 1;
		static Color32[,] LUTcols;
		static float generationProgress = 0f;
		static void BeginGeneration() {


			//make sure we can work with current palette
			CheckPalette();
			if (paletteSource) {
				if (!paletteIsReadable) {
					//palette can't be read
					working = false;
					return;
				}
			} else {
				//no palette image
				working = false;
				return;
			}

			generationProgress = 0f;
			progressBarString = "Finding palette colours...";

			//clear existing palette cols
			paletteCols32 = new List<Color32>();
			paletteCols = new List<Color>();
			paletteColsCIELab = new List<Vector3>();

			paletteHues = new List<float>();
			paletteSaturations = new List<float>();
			paletteValues = new List<float>();


			//get palette colours
			Color32[] sourcePaletteVals = new Color32[paletteSource.width * paletteSource.height];
			sourcePaletteVals = paletteSource.GetPixels32();
			for (int i = 0; i < sourcePaletteVals.Length; i++) {
				AddColorToPalette(sourcePaletteVals[i]);
			}

			//Debug.Log("Palette Size: " + paletteCols32.Count.ToString());

			//build LUT from palette colours
			LUTWidth = RGDetail * gridWidth;
			LUTHeight = RGDetail * gridHeight;

			//fill actual palette LUTs
			LUTcols = new Color32[LUTWidth, LUTHeight];

			//prepare for work
			gy = gridHeight - 1;
			gx = 0;
			blueness = 0f;
			blueChange = 1f / ((float)(gridWidth * gridHeight));

			r = 0;

			progressBarString = "Working...";
			working = true;

			rFraction = 1f / (float)RGDetail;
			rFraction *= 1f / ((float)gridHeight * (float)gridWidth);
			//rProgress = 0f;
		}

		static int gy;
		static int gx;
		static float blueness;
		static float blueChange;

		static int r;
		static float rFraction;
		//static float rProgress = 0f;
		static void DoWork() {

			//for (int r = 0; r < RGDetail; r++) {
			for (int g = 0; g < RGDetail; g++) {
				LUTcols[r + (gx * RGDetail), g + (gy * RGDetail)] = GetNearestCol(r, g, blueness);// randCol;
			}
			//}
			r++;
			generationProgress += rFraction;
			if (r >= RGDetail) {
				r = 0;

				blueness += blueChange;

				generationProgress = blueness;

				gx++;
				if (gx >= gridWidth) {
					gx = 0;
					gy--;
					if (gy < 0) {
						CompleteGeneration();
					}
				}
			}
		}

		static void DoWorkOld() {

			for (int r = 0; r < RGDetail; r++) {
				for (int g = 0; g < RGDetail; g++) {
					LUTcols[r + (gx * RGDetail), g + (gy * RGDetail)] = GetNearestCol(r, g, blueness);// randCol;
				}
			}

			blueness += blueChange;

			generationProgress = blueness;

			gx++;
			if (gx >= gridWidth) {
				gx = 0;
				gy--;
				if (gy < 0) {
					CompleteGeneration();
				}
			}
		}

		static float heightScale;
		static void CompleteGeneration() {

			Color32[] finalCols = new Color32[LUTWidth * LUTHeight];
			//Debug.Log(finalCols.Length);
			//Debug.Log(LUTcols.Length);
			int pixIndex = 0;
			for (int py = 0; py < LUTHeight; py++) {
				//Debug.Log(LUTcols[0,py]);
				for (int px = 0; px < LUTWidth; px++) {
					finalCols[pixIndex] = LUTcols[px, py];
					pixIndex++;
				}
			}


			LUTtexture = new Texture2D(LUTWidth, LUTHeight, TextureFormat.ARGB32, false);
			LUTtexture.wrapMode = TextureWrapMode.Clamp;
			LUTtexture.SetPixels32(finalCols);
			LUTtexture.Apply();

			heightScale = (float)(RGDetail * gridHeight) / (float)(RGDetail * gridWidth);

			LUTGenerated = true;

			working = false;
			generationProgress = 1f;
			progressBarString = "Done!";

		}
		static void SaveLUT(string fileName) {
			if (!LUTGenerated) return;

			// Encode texture into PNG
			byte[] bytes = LUTtexture.EncodeToPNG();
			string filePath = fileName;// Application.dataPath + "/" + fileName + ".png";
			System.IO.File.WriteAllBytes(filePath, bytes);
			//Debug.Log("Saved " + LUTWidth.ToString() + "x" + LUTHeight.ToString() + " LUT texture to: " + filePath +"\n" + GetAssetPath(fileName));
			AssetDatabase.Refresh();

			string assetPath = GetAssetPath(fileName);
			if (assetPath != "") {
				//set import settings for LUT
				TextureImporter lutImporter = (TextureImporter)TextureImporter.GetAtPath(assetPath);
				if (!generatePureLUT) lutImporter.filterMode = FilterMode.Point;
				lutImporter.wrapMode = TextureWrapMode.Clamp;
				lutImporter.mipmapEnabled = false;
				lutImporter.textureCompression = TextureImporterCompression.Uncompressed;
				lutImporter.SaveAndReimport();

				AssetDatabase.Refresh();
			}
		}

		static string GetAssetPath(string fileName) {
			for (int i = 0; i < fileName.Length - 7; i++) {
				if (fileName.Substring(i, 7) == "/Assets" || fileName.Substring(i, 7) == "\\Assets") {
					return fileName.Substring(i + 1);
				}
			}

			return "";
		}


		static List<Color32> paletteCols32 = new List<Color32>();
		static List<Color> paletteCols = new List<Color>();
		static List<Vector3> paletteColsCIELab = new List<Vector3>();

		static List<float> paletteHues = new List<float>();
		static List<float> paletteSaturations = new List<float>();
		static List<float> paletteValues = new List<float>();
		static void AddColorToPalette(Color32 c) {
			for (int i = 0; i < paletteCols32.Count; i++) {
				if (paletteCols32[i].r == c.r && paletteCols32[i].g == c.g && paletteCols32[i].b == c.b) return;
			}
			paletteCols32.Add(c);
			paletteCols.Add(c);

			float h = 0f;
			float s = 0f;
			float v = 0f;
			Color.RGBToHSV(paletteCols[paletteCols.Count - 1], out h, out s, out v);
			paletteHues.Add(h);
			paletteSaturations.Add(s);
			paletteValues.Add(v);

			paletteColsCIELab.Add(ColorMath.RGBtoCIELab(paletteCols[paletteCols.Count - 1]));
		}

		static float HueDistance(float h1, float h2) {
			float d1 = Mathf.Abs(h1 - h2);
			float d2 = Mathf.Abs((1f + h1) - h2);
			if (d1 < d2) return d1;
			return d2;
		}

		static Color32 GetNearestCol(int r, int g, float b) {


			float cr = ((float)r) * (1f / ((float)RGDetail));
			float cg = ((float)g) * (1f / ((float)RGDetail));
			float cb = b;// ((float)b) * (1f / ((float)gridWidth));
			Color c = new Color(cr, 1f - cg, cb, 1f);

			if (generatePureLUT) return c;

			float h = 0f;
			float s = 0f;
			float v = 0f;
			Color.RGBToHSV(c, out h, out s, out v);

			int bestCol = 0;
			float closestValue = 100f;
			float valueDistance = 0f;
			for (int i = 0; i < paletteCols.Count; i++) {

				valueDistance = Mathf.Sqrt((HueDistance(paletteHues[i], h) * HueDistance(paletteHues[i], h)) * HInfluence
					+ (Mathf.Abs(paletteSaturations[i] - s) * Mathf.Abs(paletteSaturations[i] - s)) * SInfluence
					+ (Mathf.Abs(paletteValues[i] - v) * Mathf.Abs(paletteValues[i] - v)) * VInfluence);

				//add RGB distances
				valueDistance += Mathf.Sqrt(Mathf.Abs(paletteCols[i].r - c.r) * Mathf.Abs(paletteCols[i].r - c.r)
					+ Mathf.Abs(paletteCols[i].g - c.g) * Mathf.Abs(paletteCols[i].g - c.g)
					+ Mathf.Abs(paletteCols[i].b - c.b) * Mathf.Abs(paletteCols[i].b - c.b)) * RGBInfluence;

				if (CIELabInfluence > 0f) {
					valueDistance += ColorMath.DeltaECIE94(paletteColsCIELab[i], ColorMath.RGBtoCIELab(new Color(cr, 1f - cg, cb))) * CIELabInfluence;
				}

				//compare
				if (valueDistance < closestValue) {
					closestValue = valueDistance;
					bestCol = i;
				}
			}

			return paletteCols32[bestCol];
		}

		static void CheckPalette() {
			if (paletteSource != null) {
				string texturePath = UnityEditor.AssetDatabase.GetAssetPath(paletteSource);
				UnityEditor.TextureImporter textureImporter = (UnityEditor.TextureImporter)UnityEditor.AssetImporter.GetAtPath(texturePath);
				paletteIsReadable = textureImporter.isReadable;

				paletteIsSmall = paletteSource.width + paletteSource.height < 200;
			}
		}
	}
}