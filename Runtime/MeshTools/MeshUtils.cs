using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace com.Plapamaru.Shared.MeshTools
{
    public static class MeshUtils
    {
        private const int UInt16VertexLimit = 65535;

        public static GameObject Combine(Transform root, bool includeInactive = true)
        {
            var instancesByMaterial = new Dictionary<Material, List<CombineInstance>>();
            var materials = new List<Material>();
            var temporaryMeshes = new List<Mesh>();
            Matrix4x4 worldToRoot = root.worldToLocalMatrix;

            foreach (MeshFilter filter in root.GetComponentsInChildren<MeshFilter>(includeInactive))
            {
                Mesh mesh = filter.sharedMesh;
                if (mesh == null || !filter.TryGetComponent(out MeshRenderer renderer))
                    continue;

                Material[] rendererMaterials = renderer.sharedMaterials;
                if (rendererMaterials.Length == 0)
                    continue;

                Matrix4x4 matrix = worldToRoot * filter.transform.localToWorldMatrix;
                for (int sub = 0; sub < mesh.subMeshCount; sub++)
                {
                    Material material = rendererMaterials[Mathf.Min(sub, rendererMaterials.Length - 1)];
                    if (material == null)
                        continue;

                    Mesh part = mesh;
                    if (mesh.subMeshCount > 1)
                    {
                        part = ExtractSubMesh(mesh, sub);
                        temporaryMeshes.Add(part);
                    }

                    if (!instancesByMaterial.TryGetValue(material, out List<CombineInstance> instances))
                    {
                        instances = new List<CombineInstance>();
                        instancesByMaterial.Add(material, instances);
                        materials.Add(material);
                    }
                    instances.Add(new CombineInstance { mesh = part, transform = matrix });
                }
            }

            var perMaterial = new CombineInstance[materials.Count];
            int totalVertices = 0;
            for (int i = 0; i < materials.Count; i++)
            {
                List<CombineInstance> instances = instancesByMaterial[materials[i]];
                var mesh = new Mesh { indexFormat = IndexFormatFor(CountVertices(instances)) };
                mesh.CombineMeshes(instances.ToArray(), true, true);
                temporaryMeshes.Add(mesh);
                perMaterial[i] = new CombineInstance { mesh = mesh, transform = Matrix4x4.identity };
                totalVertices += mesh.vertexCount;
            }

            var combined = new Mesh { name = root.name, indexFormat = IndexFormatFor(totalVertices) };
            combined.CombineMeshes(perMaterial, false, true);
            combined.Optimize();

            foreach (Mesh mesh in temporaryMeshes)
                DestroySafe(mesh);

            var result = new GameObject(root.name + "Combined");
            result.transform.SetPositionAndRotation(root.position, root.rotation);
            result.transform.localScale = root.lossyScale;
            result.AddComponent<MeshFilter>().sharedMesh = combined;
            result.AddComponent<MeshRenderer>().sharedMaterials = materials.ToArray();
            return result;
        }

        public static Mesh ExtractSubMesh(Mesh mesh, int subMeshIndex)
        {
            int[] triangles = mesh.GetTriangles(subMeshIndex);
            var remap = new Dictionary<int, int>();
            var used = new List<int>();
            var newTriangles = new int[triangles.Length];

            for (int i = 0; i < triangles.Length; i++)
            {
                int oldIndex = triangles[i];
                if (!remap.TryGetValue(oldIndex, out int newIndex))
                {
                    newIndex = used.Count;
                    remap.Add(oldIndex, newIndex);
                    used.Add(oldIndex);
                }
                newTriangles[i] = newIndex;
            }

            var result = new Mesh { name = $"{mesh.name}_{subMeshIndex}", indexFormat = IndexFormatFor(used.Count) };
            result.SetVertices(Pick(mesh.vertices, used));
            CopyIfPresent(mesh.normals, used, v => result.SetNormals(v));
            CopyIfPresent(mesh.tangents, used, v => result.SetTangents(v));
            CopyIfPresent(mesh.colors, used, v => result.SetColors(v));
            CopyIfPresent(mesh.uv, used, v => result.SetUVs(0, v));
            CopyIfPresent(mesh.uv2, used, v => result.SetUVs(1, v));
            result.SetTriangles(newTriangles, 0);
            result.RecalculateBounds();
            return result;
        }

        public static Mesh WithPivot(Mesh mesh, Vector3 normalizedPivot, out Vector3 offset)
        {
            Bounds bounds = mesh.bounds;
            offset = new Vector3(
                Mathf.Lerp(bounds.min.x, bounds.max.x, normalizedPivot.x),
                Mathf.Lerp(bounds.min.y, bounds.max.y, normalizedPivot.y),
                Mathf.Lerp(bounds.min.z, bounds.max.z, normalizedPivot.z));

            Mesh result = Object.Instantiate(mesh);
            result.name = mesh.name;
            Vector3[] vertices = result.vertices;
            for (int i = 0; i < vertices.Length; i++)
                vertices[i] -= offset;
            result.vertices = vertices;
            result.RecalculateBounds();
            return result;
        }

        public static GameObject ReduceToSingleMaterial(MeshRenderer renderer, string colorProperty = "_BaseColor", string textureProperty = "_BaseMap")
        {
            Mesh sourceMesh = renderer.GetComponent<MeshFilter>().sharedMesh;
            Material[] materials = renderer.sharedMaterials;
            if (materials.Length <= 1)
            {
                Debug.LogError($"'{renderer.name}' already has a single material.", renderer);
                return null;
            }

            int atlasSize = Mathf.NextPowerOfTwo(Mathf.CeilToInt(Mathf.Sqrt(materials.Length)));
            var atlas = new Texture2D(atlasSize, atlasSize, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            var pixels = new Color[atlasSize * atlasSize];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = Color.magenta;

            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i] == null || !materials[i].HasProperty(colorProperty))
                {
                    Debug.LogError($"Material {i} on '{renderer.name}' has no {colorProperty}.", renderer);
                    continue;
                }
                pixels[i / atlasSize * atlasSize + i % atlasSize] = materials[i].GetColor(colorProperty);
            }
            atlas.SetPixels(pixels);
            atlas.Apply();

            Mesh mesh = Object.Instantiate(sourceMesh);
            var uvs = new Vector2[mesh.vertexCount];
            var triangles = new List<int>();
            float cell = 1f / atlasSize;
            for (int sub = 0; sub < sourceMesh.subMeshCount; sub++)
            {
                int materialIndex = Mathf.Min(sub, materials.Length - 1);
                var uv = new Vector2((materialIndex % atlasSize + 0.5f) * cell, (materialIndex / atlasSize + 0.5f) * cell);
                foreach (int index in sourceMesh.GetTriangles(sub))
                {
                    triangles.Add(index);
                    uvs[index] = uv;
                }
            }
            mesh.subMeshCount = 1;
            mesh.SetTriangles(triangles, 0);
            mesh.uv = uvs;
            mesh.RecalculateBounds();

            var material = new Material(materials[0]);
            material.SetTexture(textureProperty, atlas);
            if (material.HasProperty(colorProperty))
                material.SetColor(colorProperty, Color.white);

            var result = new GameObject(renderer.name + "SingleMaterial");
            result.transform.SetPositionAndRotation(renderer.transform.position, renderer.transform.rotation);
            result.transform.localScale = renderer.transform.lossyScale;
            result.AddComponent<MeshFilter>().sharedMesh = mesh;
            result.AddComponent<MeshRenderer>().sharedMaterial = material;
            return result;
        }

        private static IndexFormat IndexFormatFor(int vertexCount)
        {
            return vertexCount > UInt16VertexLimit ? IndexFormat.UInt32 : IndexFormat.UInt16;
        }

        private static int CountVertices(List<CombineInstance> instances)
        {
            int count = 0;
            foreach (CombineInstance instance in instances)
                count += instance.mesh.vertexCount;
            return count;
        }

        private static List<T> Pick<T>(T[] source, List<int> indices)
        {
            var result = new List<T>(indices.Count);
            foreach (int index in indices)
                result.Add(source[index]);
            return result;
        }

        private static void CopyIfPresent<T>(T[] source, List<int> indices, System.Action<List<T>> apply)
        {
            if (source != null && source.Length > 0)
                apply(Pick(source, indices));
        }

        private static void DestroySafe(Object obj)
        {
            if (Application.isPlaying)
                Object.Destroy(obj);
            else
                Object.DestroyImmediate(obj);
        }
    }
}
