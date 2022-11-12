using System.Collections.Generic;
using UnityEngine;

namespace Unfoundry
{
    public class BatchRenderingGroup
    {
        private Dictionary<Mesh, RenderBatchWithColors> batchesByMesh = new Dictionary<Mesh, RenderBatchWithColors>(new MeshComparer());

        public BatchRenderingGroup()
        {
        }

        public void Render(Material prepassMaterial, Material material)
        {
            foreach (var kv in batchesByMesh) kv.Value.Render(kv.Key, prepassMaterial, material);
        }

        public void Clear()
        {
            foreach (var kv in batchesByMesh) kv.Value.Clear();
        }

        public void Move(Vector3 offset)
        {
            foreach (var kv in batchesByMesh) kv.Value.Move(offset);
        }

        public void SetAlpha(float alpha)
        {
            foreach (var kv in batchesByMesh) kv.Value.SetAlpha(alpha);
        }

        public BatchRenderingHandle AddSimplePlaceholderTransform(Mesh mesh, Matrix4x4 transform, Color color)
        {
            RenderBatchWithColors batch;
            if (!batchesByMesh.TryGetValue(mesh, out batch)) batchesByMesh[mesh] = batch = new RenderBatchWithColors();

            var id = batch.AddEntry(transform, color);
            return new BatchRenderingHandle(batch, id);
        }

        private class MeshComparer : IEqualityComparer<Mesh>
        {
            public bool Equals(Mesh x, Mesh y)
            {
                return x.GetInstanceID() == y.GetInstanceID();
            }

            public int GetHashCode(Mesh obj)
            {
                return obj.GetInstanceID();
            }
        }
    }
}
