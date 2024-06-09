using HarmonyLib;
using UnityEngine;
using UnityEngine.Rendering;

namespace Unfoundry
{
    public abstract class CustomHandheldMode
    {
        public abstract void UpdateBehavoir();
        public abstract void Enter();
        public abstract void Exit();
        public abstract void ShowMenu();
        public abstract bool OnRotateY();

        protected static void DrawBox(Vector3 from, Vector3 to, Material material)
        {
            var matrix = Matrix4x4.TRS((from + to) * 0.5f, Quaternion.identity, to - from);
            Graphics.DrawMesh(ResourceDB.mesh_cubeCenterPivot, matrix, material, GlobalStaticCache.s_Layer_DragHelper, Camera.main, 0, null, false, false, false);
        }

        protected static void DrawBox(Vector3 from, Vector3 to, Vector3 expand, Material material)
        {
            DrawBox(from - expand, to + expand, material);
        }

        private static Matrix4x4[] edgeMatrices = new Matrix4x4[12];

        protected static void DrawBoxWithEdges(Vector3 from, Vector3 to, float faceOffset, float edgeSize, Material faceMaterial, Material edgeMaterial)
        {
            DrawBox(from - new Vector3(faceOffset, faceOffset, faceOffset), to + new Vector3(faceOffset, faceOffset, faceOffset), faceMaterial);

            var expand = Vector3.one * edgeSize;
            Matrix4x4 buildEdgeMatrix(Vector3 min, Vector3 max)
            {
                return Matrix4x4.TRS((min + max) * 0.5f, Quaternion.identity, max - min + expand);
            }

            Vector3 v1 = new Vector3(from.x, from.y, from.z);
            Vector3 v2 = new Vector3(to.x, from.y, from.z);
            Vector3 v3 = new Vector3(from.x, to.y, from.z);
            Vector3 v4 = new Vector3(to.x, to.y, from.z);
            Vector3 v5 = new Vector3(from.x, from.y, to.z);
            Vector3 v6 = new Vector3(to.x, from.y, to.z);
            Vector3 v7 = new Vector3(from.x, to.y, to.z);
            Vector3 v8 = new Vector3(to.x, to.y, to.z);
            edgeMatrices[0] = buildEdgeMatrix(v1, v2);
            edgeMatrices[1] = buildEdgeMatrix(v3, v4);
            edgeMatrices[2] = buildEdgeMatrix(v5, v6);
            edgeMatrices[3] = buildEdgeMatrix(v7, v8);
            edgeMatrices[4] = buildEdgeMatrix(v1, v3);
            edgeMatrices[5] = buildEdgeMatrix(v2, v4);
            edgeMatrices[6] = buildEdgeMatrix(v5, v7);
            edgeMatrices[7] = buildEdgeMatrix(v6, v8);
            edgeMatrices[8] = buildEdgeMatrix(v1, v5);
            edgeMatrices[9] = buildEdgeMatrix(v2, v6);
            edgeMatrices[10] = buildEdgeMatrix(v3, v7);
            edgeMatrices[11] = buildEdgeMatrix(v4, v8);
            Graphics.DrawMeshInstanced(ResourceDB.mesh_cubeCenterPivot, 0, edgeMaterial, edgeMatrices, 12, null, ShadowCastingMode.Off, false, GlobalStaticCache.s_Layer_DragHelper, Camera.main, LightProbeUsage.Off, null);
        }

        protected static void DrawArrow(Vector3 origin, Vector3 direction, Material material, float scale = 1.0f, float offset = 0.5f)
        {
            Matrix4x4 matrix = Matrix4x4.TRS(origin + direction * offset, Quaternion.LookRotation(direction) * Quaternion.Euler(0.0f, 90.0f, 0.0f), Vector3.one * scale);
            Graphics.DrawMesh(ResourceDB.resourceLinker.mesh_arrow_centerPivot, matrix, material, GlobalStaticCache.s_Layer_DragHelper, Camera.main, 0, null, false, false, false);
        }

        public static Ray GetLookRay()
        {
            return GameRoot.getClientRenderCharacter().getLookRay();
        }

        public static bool GetTargetCube(float offset, out Vector3 targetPoint, out Vector3Int targetCoord, out Vector3Int targetNormal)
        {
            var lookRay = GetLookRay();
            RaycastHit hitInfo;
            if (Physics.Raycast(lookRay, out hitInfo, 300.0f, GlobalStaticCache.s_LayerMask_Terrain | GlobalStaticCache.s_LayerMask_TerrainTileCollider | GlobalStaticCache.s_LayerMask_BuildableObjectFullSize | GlobalStaticCache.s_LayerMask_BuildableObjectPartialSize))
            {
                targetPoint = hitInfo.point;
                var normal = Plugin.SnappedToNearestAxis(hitInfo.normal);
                targetCoord = new Vector3Int(Mathf.FloorToInt(hitInfo.point.x + normal.x * offset), Mathf.FloorToInt(hitInfo.point.y + normal.y * offset), Mathf.FloorToInt(hitInfo.point.z + normal.z * offset));
                targetNormal = new Vector3Int(Mathf.RoundToInt(normal.x), Mathf.RoundToInt(normal.y), Mathf.RoundToInt(normal.z));
                return true;
            }

            targetPoint = Vector3.zero;
            targetCoord = Vector3Int.zero;
            targetNormal = new Vector3Int(0, 1, 0);
            return false;
        }

        protected static bool GetTargetCube(float offset, out Vector3Int targetCoord)
        {
            var lookRay = GetLookRay();
            RaycastHit hitInfo;
            if (Physics.Raycast(lookRay, out hitInfo, 30.0f, GlobalStaticCache.s_LayerMask_Terrain | GlobalStaticCache.s_LayerMask_TerrainTileCollider | GlobalStaticCache.s_LayerMask_BuildableObjectFullSize | GlobalStaticCache.s_LayerMask_BuildableObjectPartialSize))
            {
                var normal = Plugin.SnappedToNearestAxis(hitInfo.normal);
                targetCoord = new Vector3Int(Mathf.FloorToInt(hitInfo.point.x + normal.x * offset), Mathf.FloorToInt(hitInfo.point.y + normal.y * offset), Mathf.FloorToInt(hitInfo.point.z + normal.z * offset));
                return true;
            }

            targetCoord = Vector3Int.zero;
            return false;
        }

        public static readonly Vector3Int[] faceNormals = new Vector3Int[6]
        {
            new Vector3Int(1, 0, 0),
            new Vector3Int(-1, 0, 0),
            new Vector3Int(0, 1, 0),
            new Vector3Int(0, -1, 0),
            new Vector3Int(0, 0, 1),
            new Vector3Int(0, 0, -1)
        };
        public static float BoxRayIntersection(Vector3 from, Vector3 to, Ray ray, out Vector3Int normal, out int faceIndex)
        {
            if (new Bounds((from + to) * 0.5f, to - from).IntersectRay(ray, out _))
            {
                var maxDistance = float.MinValue;
                normal = Vector3Int.zero;
                faceIndex = -1;
                for (int i = 0; i < 6; ++i)
                {
                    if (Vector3.Dot(faceNormals[i], ray.direction) < 0.0f)
                    {
                        float distance;
                        if (new Plane(faceNormals[i], ((i & 1) == 0) ? to : from).Raycast(ray, out distance))
                        {
                            if (distance > maxDistance)
                            {
                                maxDistance = distance;
                                normal = faceNormals[i];
                                faceIndex = i;
                            }
                        }
                    }
                }

                return maxDistance;
            }

            faceIndex = -1;
            normal = new Vector3Int(0, 1, 0);
            return -1.0f;
        }

        public static float BoxRayIntersection(Vector3 from, Vector3 to, Ray ray, out Vector3Int normal, out int faceIndex, out bool isInternal)
        {
            var bounds = new Bounds((from + to) * 0.5f, to - from);
            if (bounds.Contains(ray.origin))
            {
                var minDistance = float.MaxValue;
                normal = Vector3Int.zero;
                faceIndex = -1;
                for (int i = 0; i < 6; ++i)
                {
                    if (Vector3.Dot(faceNormals[i], ray.direction) > 0.0f)
                    {
                        float distance;
                        if (new Plane(faceNormals[i], ((i & 1) == 0) ? to : from).Raycast(ray, out distance))
                        {
                            if (distance < minDistance)
                            {
                                minDistance = distance;
                                normal = faceNormals[i];
                                faceIndex = i;
                            }
                        }
                    }
                }

                isInternal = true;
                return minDistance;
            }
            else if (bounds.IntersectRay(ray, out var _))
            {
                var maxDistance = float.MinValue;
                normal = Vector3Int.zero;
                faceIndex = -1;
                for (int i = 0; i < 6; ++i)
                {
                    if (Vector3.Dot(faceNormals[i], ray.direction) < 0.0f)
                    {
                        float distance;
                        if (new Plane(faceNormals[i], ((i & 1) == 0) ? to : from).Raycast(ray, out distance))
                        {
                            if (distance > maxDistance)
                            {
                                maxDistance = distance;
                                normal = faceNormals[i];
                                faceIndex = i;
                            }
                        }
                    }
                }

                isInternal = false;
                return maxDistance;
            }

            faceIndex = -1;
            normal = new Vector3Int(0, 1, 0);
            isInternal = false;
            return -1.0f;
        }

        public static bool TryGetAxialDragOffset(Ray axisRay, Ray lookRay, out float offset)
        {
            offset = 0.0f;
            if (Vector3.Dot(lookRay.direction, axisRay.origin - lookRay.origin) < 0.0f) return false;

            Vector3 right = Vector3.Cross(lookRay.direction, axisRay.direction);
            Vector3 planeNormal = Vector3.Cross(right, axisRay.direction);
            var plane = new Plane(planeNormal, axisRay.origin);

            float distance;
            if (!plane.Raycast(lookRay, out distance)) return false;

            offset = Vector3.Dot(axisRay.direction, lookRay.GetPoint(distance) - axisRay.origin);

            return true;
        }

        protected static void DumpMaterial(Material material)
        {
            var shader = material.shader;
            int count = shader.GetPropertyCount();
            Debug.Log((string)$"============== {shader.name} ===============");
            for (int i = 0; i < count; ++i)
            {
                string value;
                switch (shader.GetPropertyType(i))
                {
                    case ShaderPropertyType.Color:
                        value = material.GetColor(shader.GetPropertyName(i)).ToString();
                        break;
                    case ShaderPropertyType.Vector:
                        value = material.GetVector(Shader.PropertyToID(shader.GetPropertyName(i))).ToString();
                        break;
                    case ShaderPropertyType.Float:
                    case ShaderPropertyType.Range:
                        value = material.GetFloat(shader.GetPropertyName(i)).ToString();
                        break;
                    case ShaderPropertyType.Texture:
                        var texture = material.GetTexture(shader.GetPropertyName(i));
                        value = texture == null ? "null" : texture.dimension.ToString();
                        break;
                    default:
                        value = "<undefined>";
                        break;
                }
                Debug.Log((string)$"{shader.GetPropertyType(i)} {shader.GetPropertyName(i)} = {value}");
            }
        }
    }
}
