using UnityEngine;
using UnityEditor;

[RequireComponent(typeof(MeshFilter))]
public class MeshDebugger : MonoBehaviour
{
  public MeshFilter meshFilter;

  public bool showVertices = true;
  public float vertexSize = 0.01f;

  public bool showVertexIndexes = true;
  public int indexLabelSize = 16;

  /// <summary>
  /// If enabled by the Unity Inspector. It draw somehelp debug UI in the Scene view.
  /// </summary>
  private void OnDrawGizmos()
  {
    if (showVertices) 
    {            
      if (meshFilter != null && meshFilter.sharedMesh != null)
      {
        Gizmos.color = Color.blue;
        GUI.contentColor = Color.blue;

        for (int i = 0; i < meshFilter.sharedMesh.vertexCount; i++)
        {
          //Covert Vertex position from Local to World Space.
          var point = transform.TransformPoint(meshFilter.sharedMesh.vertices[i]);

          Gizmos.DrawSphere(point, vertexSize);

          //Draw Vertex Index?
          if (showVertexIndexes)
            Handles.Label(point, i.ToString(), new GUIStyle(GUI.skin.label) { fontSize = indexLabelSize });
        }
      }
    }        
  }
}