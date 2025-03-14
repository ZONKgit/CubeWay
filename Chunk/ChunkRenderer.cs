using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;


namespace CubeWay
{
    public class ChunkRenderer
    {
        private int vao;
        private int vbo;
        private int ebo;
        private int indicesCount;
        private bool isReady = false;

        private List<float> vertices = new List<float>();
        private List<uint> indices = new List<uint>();

        public ChunkRenderer()
        {
            vao = GL.GenVertexArray();
            vbo = GL.GenBuffer();
            ebo = GL.GenBuffer();
        }

        public void UpdateMesh(List<float> newVertices, List<uint> newIndices)
        {
            vertices = new List<float>(newVertices);
            indices = new List<uint>(newIndices);
            indicesCount = indices.Count;
            isReady = vertices.Count > 0 && indices.Count > 0;

            if (!isReady) return;

            GL.BindVertexArray(vao);

            // Заполняем VBO (буфер вершин)
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Count * sizeof(float), vertices.ToArray(), BufferUsageHint.DynamicDraw);

            // Заполняем EBO (буфер индексов)
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Count * sizeof(uint), indices.ToArray(), BufferUsageHint.DynamicDraw);

            // Позиция (3 float)
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 7 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            // Цвет (3 float)
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 7 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            // AO значение (1 float) - если нужно использовать в шейдере
            GL.VertexAttribPointer(2, 1, VertexAttribPointerType.Float, false, 7 * sizeof(float), 6 * sizeof(float));
            GL.EnableVertexAttribArray(2);

            GL.BindVertexArray(0);
        }

        public void Render(Shader shader)
        {
            if (!isReady) return;

            shader.Use();
            GL.BindVertexArray(vao);
            GL.DrawElements(PrimitiveType.Triangles, indicesCount, DrawElementsType.UnsignedInt, 0);
            GL.BindVertexArray(0);
        }

        public void Dispose()
        {
            GL.DeleteVertexArray(vao);
            GL.DeleteBuffer(vbo);
            GL.DeleteBuffer(ebo);
        }

    }
}