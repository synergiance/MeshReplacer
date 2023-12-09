using UnityEngine;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using System;
using UnityEditor;
#endif

namespace Synergiance.Utils {

	public enum MeshRendererType {
		MeshRenderer, SkinnedMeshRenderer
	}

	public class ReplaceMeshes : MonoBehaviour {
		public MeshRendererType meshTypeToSearchFor = MeshRendererType.SkinnedMeshRenderer;
		public Mesh meshToSearchFor;
		public MeshRendererType meshTypeToReplaceWith = MeshRendererType.MeshRenderer;
		public Mesh meshToReplaceWith;
	}

	#if UNITY_EDITOR
	[CustomEditor(typeof(ReplaceMeshes))]
	public class ReplaceMeshesEditor : Editor {
		public override void OnInspectorGUI() {
			base.OnInspectorGUI();
			EditorGUILayout.Space();
			EditorGUILayout.HelpBox("Make sure you save before clicking \"Replace\"", MessageType.Warning);
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
				Material[] oldMats;

				MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();

				switch (t.meshTypeToSearchFor) {
					case MeshRendererType.SkinnedMeshRenderer:
						SkinnedMeshRenderer skinnedMeshRenderer = gameObject.GetComponent<SkinnedMeshRenderer>();

						if (!skinnedMeshRenderer) continue;
						if (skinnedMeshRenderer.sharedMesh != t.meshToSearchFor) continue;

						oldMats = skinnedMeshRenderer.sharedMaterials;

						Undo.RecordObject(gameObject, "Deleted skinned mesh renderer");
						DestroyImmediate(skinnedMeshRenderer);

						break;
					case MeshRendererType.MeshRenderer:
						MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();

						if (!meshRenderer || !meshFilter) continue;
						if (meshFilter.sharedMesh != t.meshToSearchFor) continue;

						oldMats = meshRenderer.sharedMaterials;

						Undo.RecordObject(gameObject, "Deleted mesh renderer and mesh filter");
						DestroyImmediate(meshRenderer);

						break;
					default:
						continue;
				}

				int numMats = t.meshToReplaceWith.subMeshCount;
				Material[] newMats = new Material[numMats];
				Array.Copy(oldMats, newMats, Mathf.Min(numMats, oldMats.Length));

				switch (t.meshTypeToReplaceWith) {
					case MeshRendererType.SkinnedMeshRenderer:
						if (meshFilter) DestroyImmediate(meshFilter);
						SkinnedMeshRenderer skinnedMeshRenderer = gameObject.AddComponent<SkinnedMeshRenderer>();
						skinnedMeshRenderer.sharedMesh = t.meshToReplaceWith;
						skinnedMeshRenderer.sharedMaterials = newMats;
						break;
					case MeshRendererType.MeshRenderer:
						if (!meshFilter) meshFilter = gameObject.AddComponent<MeshFilter>();
						meshFilter.sharedMesh = t.meshToReplaceWith;
						MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
						meshRenderer.sharedMaterials = newMats;
						break;
					default:
						continue;
				}

				numReplacedMeshes++;

				EditorUtility.SetDirty(gameObject);
			}

			Debug.Log($"Replaced {numReplacedMeshes} meshes!");
		}
	}
	#endif

}
