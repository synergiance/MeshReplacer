using UnityEngine;

#if UNITY_EDITOR
using System;
using UnityEditor;
#endif

namespace Synergiance.Utils {

	public class ReplaceMeshes : MonoBehaviour {
		public Mesh meshToSearchFor;
		public Mesh meshToReplaceWith;
	}

	#if UNITY_EDITOR
	[CustomEditor(typeof(ReplaceMeshes))]
	public class ReplaceMeshesEditor : Editor {
		public override void OnInspectorGUI() {
			base.OnInspectorGUI();
			if (GUI.Button(EditorGUILayout.GetControlRect(), "Replace")) Replace();
		}

		// ReSharper disable Unity.PerformanceAnalysis
		private void Replace() {
			ReplaceMeshes t = (ReplaceMeshes)target;

			if (t.meshToSearchFor == null) {
				Debug.LogError("Cannot search for null meshes!");
				return;
			}

			if (t.meshToReplaceWith == null) {
				Debug.LogError("Cannot replace with a null mesh!");
				return;
			}

			int numReplacedMeshes = 0;

			GameObject[] allObjects = FindObjectsOfType<GameObject>();
			foreach (GameObject gameObject in allObjects) {
				SkinnedMeshRenderer skinnedMeshRenderer = gameObject.GetComponent<SkinnedMeshRenderer>();
				if (!skinnedMeshRenderer) continue;
				if (skinnedMeshRenderer.sharedMesh != t.meshToSearchFor) continue;

				Undo.RecordObject(target, "Replaced skinned mesh renderers with mesh renderers of a new mesh");

				Material[] mats = skinnedMeshRenderer.sharedMaterials;

				DestroyImmediate(skinnedMeshRenderer);

				MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
				if (!meshFilter) meshFilter = gameObject.AddComponent<MeshFilter>();
				meshFilter.sharedMesh = t.meshToReplaceWith;

				int numMats = t.meshToReplaceWith.subMeshCount;
				Material[] newMats = new Material[numMats];
				Array.Copy(mats, newMats, Mathf.Min(numMats, mats.Length));

				MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
				meshRenderer.sharedMaterials = mats;

				numReplacedMeshes++;

				EditorUtility.SetDirty(gameObject);
			}

			Debug.Log($"Replaced {numReplacedMeshes} meshes!");
		}
	}
	#endif

}
