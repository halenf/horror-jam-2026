using UnityEditor;
using UnityEngine;

namespace Train.Tools
{
    public class MaterialReplacer : MonoBehaviour
    {
        [SerializeField] private string m_newMatPath;

        [ContextMenu("Replace All")]
        public void Replace()
        {
            Transform[] all = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            
            foreach (Transform t in all)
            {
                // do skinned mesh renderers
                if (t.TryGetComponent(out SkinnedMeshRenderer r))
                {
                    int matCount = r.sharedMaterials.Length;
                    Material[] newMaterials = new Material[matCount];
                    for (int i = 0; i < matCount; i++)
                    {
                        string matName = r.sharedMaterials[i].name + ".mat";

                        Material newMat = AssetDatabase.LoadAssetAtPath<Material>(m_newMatPath + matName);
                        if (newMat == null)
                        {
                            Debug.LogError("Could not find material with path " + m_newMatPath + matName);
                            return;
                        }

                        newMaterials[i] = newMat;
                    }

                    r.sharedMaterials = newMaterials;
                }

                // do lod groups
                if (t.TryGetComponent(out LODGroup l))
                {
                    foreach (LOD lod in l.GetLODs())
                    {
                        foreach (Renderer ren in lod.renderers)
                        {
                            int matCount = ren.sharedMaterials.Length;
                            Material[] newMaterials = new Material[matCount];
                            for (int i = 0; i < matCount; i++)
                            {
                                string matName = ren.sharedMaterials[i].name + ".mat";

                                Material newMat = AssetDatabase.LoadAssetAtPath<Material>(m_newMatPath + matName);
                                if (newMat == null)
                                {
                                    Debug.LogError("Could not find material with path " + m_newMatPath + matName);
                                    return;
                                }

                                newMaterials[i] = newMat;
                            }

                            ren.sharedMaterials = newMaterials;
                        }
                    }
                }
            }
        }
    }
}