using System;
using System.Collections.Generic;
using UnityEngine;
using static Il2CppSystem.Uri;

namespace Unfoundry
{
    public class RenderBatchWithColors
    {
        private const int batchSize = 1023;

        private List<MaterialPropertyBlock> materialPropertyBlocks = new List<MaterialPropertyBlock>();
        private List<Matrix4x4[]> transformBatches = new List<Matrix4x4[]>();
        private List<Vector4[]> colorBatches = new List<Vector4[]>();

        private int lastBatchIndex = 0;
        private int nextSubIndex = 0;

        private static int colorNameId = 0;

        public RenderBatchWithColors()
        {
        }

        public void Render(Mesh mesh, Material prepassMaterial, Material material)
        {
            if (colorNameId == 0) colorNameId = Shader.PropertyToID("_Color");

            var total = 0;
            for (int i = 0; i <= lastBatchIndex; i++)
            {
                int thisBatchSize = (i == lastBatchIndex) ? nextSubIndex : batchSize;
                total += thisBatchSize;

                materialPropertyBlocks[i].SetVectorArray(colorNameId, colorBatches[i], batchSize); // ToDo: use dirty flags
                for (int subMeshIndex = 0; subMeshIndex < mesh.subMeshCount; subMeshIndex++)
                {
                    if (prepassMaterial != null)
                    {
                        Graphics.DrawMeshInstanced(
                            mesh, subMeshIndex, prepassMaterial,
                            transformBatches[i], thisBatchSize, materialPropertyBlocks[i],
                            UnityEngine.Rendering.ShadowCastingMode.Off, false,
                            GlobalStaticCache.s_Layer_BuildableObjectFullSize);
                    }

                    Graphics.DrawMeshInstanced(
                        mesh, subMeshIndex, material,
                        transformBatches[i], thisBatchSize, materialPropertyBlocks[i],
                        UnityEngine.Rendering.ShadowCastingMode.Off, false,
                        GlobalStaticCache.s_Layer_BuildableObjectFullSize);
                }
            }
        }

        public void Clear()
        {
            lastBatchIndex = nextSubIndex = 0;
        }

        public uint AddEntry(Matrix4x4 transform, Color color)
        {
            if (nextSubIndex >= batchSize)
            {
                lastBatchIndex++;
                nextSubIndex = 0;
            }

            while (transformBatches.Count <= lastBatchIndex)
            {
                transformBatches.Add(new Matrix4x4[batchSize]);
                colorBatches.Add(new Vector4[batchSize]);
                materialPropertyBlocks.Add(new MaterialPropertyBlock());
            }

            transformBatches[lastBatchIndex][nextSubIndex] = transform;
            colorBatches[lastBatchIndex][nextSubIndex] = color;

            return MakeId(lastBatchIndex, nextSubIndex++);
        }

        public void SetTransform(uint id, Matrix4x4 transform)
        {
            int index, subIndex;
            SplitId(id, out index, out subIndex);

            transformBatches[index][subIndex] = transform;
        }

        public void Move(uint id, Vector3 offset)
        {
            int index, subIndex;
            SplitId(id, out index, out subIndex);

            var matrix = transformBatches[index][subIndex];
            var positionColumn = matrix.GetColumn(3);
            positionColumn.x += offset.x;
            positionColumn.y += offset.y;
            positionColumn.z += offset.z;
            matrix.SetColumn(3, positionColumn);

            transformBatches[index][subIndex] = matrix;
        }

        public void Move(Vector3 offset)
        {
            for (int i = 0; i <= lastBatchIndex; i++)
            {
                int thisBatchSize = (i == lastBatchIndex) ? nextSubIndex : batchSize;
                for (int j = 0; j < thisBatchSize; j++)
                {
                    var matrix = transformBatches[i][j];
                    var positionColumn = matrix.GetColumn(3);
                    positionColumn.x += offset.x;
                    positionColumn.y += offset.y;
                    positionColumn.z += offset.z;
                    matrix.SetColumn(3, positionColumn);

                    transformBatches[i][j] = matrix;
                }
            }
        }

        public void SetAlpha(float alpha)
        {
            for (int i = 0; i <= lastBatchIndex; i++)
            {
                int thisBatchSize = (i == lastBatchIndex) ? nextSubIndex : batchSize;
                for (int j = 0; j < thisBatchSize; j++)
                {
                    var color = colorBatches[i][j];
                    color.w = alpha;

                    colorBatches[i][j] = color;
                }
            }
        }

        public void SetColor(uint id, Color color)
        {
            int index, subIndex;
            SplitId(id, out index, out subIndex);

            colorBatches[index][subIndex] = color;
        }

        private static uint MakeId(int index, int subIndex) => ((uint)subIndex & 0xFFFF) | (((uint)index & 0xFFFF) << 16);

        private static void SplitId(uint id, out int index, out int subIndex)
        {
            index = (int)((id >> 16) & 0xFFFF);
            subIndex = (int)(id & 0xFFFF);
        }

        public static uint NextId(uint id)
        {
            int index, subIndex;
            SplitId(id, out index, out subIndex);

            return (subIndex + 1 < batchSize) ? MakeId(index, subIndex + 1) : MakeId(index + 1, 0);
        }
    }
}
