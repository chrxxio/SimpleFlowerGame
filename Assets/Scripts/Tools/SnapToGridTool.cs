#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class SnapToParentVertexTool
{
    [MenuItem("Tools/Snap Selected To Parent Vertex")]
    static void SnapSelected()
    {
        foreach (GameObject obj in Selection.gameObjects)
        {
            Transform parent = obj.transform.parent;
            if (parent == null)
            {
                Debug.LogWarning($"{obj.name} has no parent.");
                continue;
            }

            MeshFilter mf = parent.GetComponent<MeshFilter>();
            if (mf == null || mf.sharedMesh == null)
            {
                Debug.LogWarning($"{parent.name} has no mesh.");
                continue;
            }

            Mesh mesh = mf.sharedMesh;
            Vector3[] verts = mesh.vertices;

            Vector3 closest = Vector3.zero;
            float minDist = float.MaxValue;

            foreach (Vector3 v in verts)
            {
                Vector3 worldV = parent.TransformPoint(v);
                float dist = Vector3.Distance(obj.transform.position, worldV);

                if (dist < minDist)
                {
                    minDist = dist;
                    closest = worldV;
                }
            }

            Undo.RecordObject(obj.transform, "Snap To Parent Vertex");
            obj.transform.position = closest;
        }

        Debug.Log("Snapped selected objects to parent vertices.");
    }
}
#endif