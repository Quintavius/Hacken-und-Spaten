using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PalutteEffect))]
public class PalutteEffectEditor : Editor {

	public override void OnInspectorGUI() {
		PalutteEffect effect = (PalutteEffect)target;

		EditorGUI.BeginChangeCheck();
		

		//Palutte material
		effect.material = (Material)EditorGUILayout.ObjectField("Material:", effect.material, typeof(Material));

		//LUT texture
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("LUT Texture:");
		effect.LUTTexture = (Texture)EditorGUILayout.ObjectField(effect.LUTTexture, typeof(Texture),false);
		EditorGUILayout.EndHorizontal();

		//LUT grid properties
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("LUT Grid Dimensions:");
		effect.gridWidth = EditorGUILayout.IntField(effect.gridWidth);
		effect.gridHeight = EditorGUILayout.IntField(effect.gridHeight);
		EditorGUILayout.EndHorizontal();

		
		EditorGUILayout.LabelField("________________________________________________________________________");
		EditorGUILayout.Space();
		

		effect.matchCamSize = EditorGUILayout.Toggle("Match Camera:", effect.matchCamSize);
		

		if (!effect.matchCamSize) {
			EditorGUILayout.Space();

			EditorGUILayout.BeginHorizontal();
			//toggle auto width
			EditorGUILayout.BeginVertical(GUILayout.Width(20));
			effect.autoSetWidth = EditorGUILayout.Toggle(effect.autoSetWidth);
			if (effect.autoSetWidth) effect.autoSetHeight = false;
			EditorGUILayout.EndVertical();
			
			//set width
			if (effect.autoSetWidth) {
				effect.pixelStretch = EditorGUILayout.Slider("Auto-Width:",effect.pixelStretch, 0.5f, 2f);
			} else {
				effect.pixelsWidth = EditorGUILayout.IntField("Width:", effect.pixelsWidth);
			}
			EditorGUILayout.EndHorizontal();

			
			EditorGUILayout.BeginHorizontal();
			//toggle auto height
			EditorGUILayout.BeginVertical(GUILayout.Width(20));
			effect.autoSetHeight = EditorGUILayout.Toggle(effect.autoSetHeight);
			if (effect.autoSetHeight) effect.autoSetWidth = false;
			EditorGUILayout.EndVertical();
			//set height
			if (effect.autoSetHeight) {
				effect.pixelStretch = EditorGUILayout.Slider("Auto-Height:", effect.pixelStretch, 0.5f, 2f);
			} else {
				effect.pixelsHeight = EditorGUILayout.IntField("Height:", effect.pixelsHeight);
			}
			EditorGUILayout.EndHorizontal();


		}
		
		
		EditorGUILayout.LabelField("________________________________________________________________________");
		EditorGUILayout.Space();

		effect.ditherAmount = EditorGUILayout.Slider("Dither Amount: ", effect.ditherAmount, 0f, 0.5f);

		//EditorGUILayout.LabelField("________________________________________________________________________");
		EditorGUILayout.Space();

		effect.jaggiesAreGood = EditorGUILayout.Toggle("Jaggies are good:", effect.jaggiesAreGood);

		EditorGUILayout.Space();

		if (EditorGUI.EndChangeCheck()) {
			EditorUtility.SetDirty(effect);
		}
	}
}
