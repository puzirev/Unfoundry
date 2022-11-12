using System;
using UnityEngine;

namespace Unfoundry
{
    public struct BatchRenderingHandle
    {
        private readonly RenderBatchWithColors batch;
        private readonly uint id;

        public BatchRenderingHandle(RenderBatchWithColors batch, uint id)
        {
            this.batch = batch;
            this.id = id;
        }

        public BatchRenderingHandle Next => new BatchRenderingHandle(batch, RenderBatchWithColors.NextId(id));

        public void SetTransform(Matrix4x4 transform)
        {
            batch.SetTransform(id, transform);
        }

        public void Move(Vector3 offset)
        {
            batch.Move(id, offset);
        }

        public void SetColor(Color color)
        {
            batch.SetColor(id, color);
        }

        public void SetTransform(int count, Matrix4x4 transform)
        {
            var handle = this;
            for (int i = 0; i < count; i++)
            {
                handle.SetTransform(transform);
                handle = handle.Next;
            }
        }

        public void SetColor(int count, Color color)
        {
            var handle = this;
            for (int i = 0; i < count; i++)
            {
                handle.SetColor(color);
                handle = handle.Next;
            }
        }

        public void SetTransformAndColor(int count, Matrix4x4 transform, Color color)
        {
            var handle = this;
            for (int i = 0; i < count; i++)
            {
                handle.SetTransform(transform);
                handle.SetColor(color);
                handle = handle.Next;
            }
        }
    }
}
