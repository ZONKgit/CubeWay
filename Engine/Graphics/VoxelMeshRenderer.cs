using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using CubeWay.Engine.Graphics;

namespace CubeWay.Engine.Graphics;
public class VoxelMeshRenderer
{
    private VoxelMesh mesh;
    private List<float> vertices = new();
    private List<uint> indices = new();
    private int vao, vbo, ebo;

    public VoxelMeshRenderer(VoxelMesh mesh)
    {
        this.mesh = mesh;
        GenerateMesh();
        SetupOpenGL();
    }

    private void GenerateMesh()
    {
        vertices.Clear();
        indices.Clear();
        uint index = 0;
        for (int x = 0; x < mesh.meshSize.X; x++)
        {
            for (int y = 0; y < mesh.meshSize.Y; y++)
            {
                for (int z = 0; z < mesh.meshSize.Z; z++)
                {
                if (mesh.data[x, y, z] == new VoxelMesh.Color(0, 0, 0)) continue; // Пропуск пустых блоков

                    Vector3 pos = new(x * 0.0625f, y * 0.0625f, z * 0.0625f);
                        
                    if (mesh.IsVoxelTransparent(x - 1, y, z)) AddFace(pos, new Vector3(-1, 0, 0), x, y, z, ref index); // Левая грань
                    if (mesh.IsVoxelTransparent(x + 1, y, z)) AddFace(pos, new Vector3(1, 0, 0), x, y, z, ref index);  // Правая грань
                    if (mesh.IsVoxelTransparent(x, y, z - 1)) AddFace(pos, new Vector3(0, 0, -1), x, y, z, ref index); // Задняя грань
                    if (mesh.IsVoxelTransparent(x, y, z + 1)) AddFace(pos, new Vector3(0, 0, 1), x, y, z, ref index);  // Передняя грань
                    if (mesh.IsVoxelTransparent(x, y + 1, z)) AddFace(pos, new Vector3(0, 1, 0), x, y, z, ref index);  // Верхняя грань
                    if (mesh.IsVoxelTransparent(x, y - 1, z)) AddFace(pos, new Vector3(0, -1, 0), x, y, z, ref index); // Нижняя грань
                }
            }
        }
    }

    // Вычисление значения AO для вершины на основе соседних блоков
    private float CalculateAO(bool side1, bool side2, bool corner)
    {
        if (side1 && side2)
            return 0.0f;

        float ao = 1.0f;

        if (side1)
            ao -= 0.4f;
        if (side2)
            ao -= 0.4f;
        if (corner && !side1 && !side2)
            ao -= 0.4f;

        return Math.Max(ao, 0.3f); // Минимальное значение AO
    }

    private void AddFace(Vector3 pos, Vector3 normal, int blockX, int blockY, int blockZ, ref uint index)
    {
        Vector3[] offsets = new Vector3[4];
        float[] aoValues = new float[4];

        VoxelMesh.Color voxelColor = mesh.data[blockX, blockY, blockZ];
        Vector3 color = new Vector3(voxelColor.R, voxelColor.G, voxelColor.B) / 255.0f;


        if (normal == new Vector3(0, 1, 0)) // Верх
        {
            offsets = new Vector3[]
            {
                new(-0.5f, 0.5f, 0.5f),   // Левый верхний ближний
                new(0.5f, 0.5f, 0.5f),    // Правый верхний ближний
                new(0.5f, 0.5f, -0.5f),   // Правый верхний дальний
                new(-0.5f, 0.5f, -0.5f)   // Левый верхний дальний
            };

            // Вычисляем AO для каждой вершины верхней грани
            bool left = !mesh.IsVoxelTransparent(blockX - 1, blockY + 1, blockZ);
            bool right = !mesh.IsVoxelTransparent(blockX + 1, blockY + 1, blockZ);
            bool front = !mesh.IsVoxelTransparent(blockX, blockY + 1, blockZ + 1);
            bool back = !mesh.IsVoxelTransparent(blockX, blockY + 1, blockZ - 1);
            bool leftFront = !mesh.IsVoxelTransparent(blockX - 1, blockY + 1, blockZ + 1);
            bool rightFront = !mesh.IsVoxelTransparent(blockX + 1, blockY + 1, blockZ + 1);
            bool leftBack = !mesh.IsVoxelTransparent(blockX - 1, blockY + 1, blockZ - 1);
            bool rightBack = !mesh.IsVoxelTransparent(blockX + 1, blockY + 1, blockZ - 1);

            aoValues[0] = CalculateAO(left, front, leftFront);   // Левый верхний ближний
            aoValues[1] = CalculateAO(right, front, rightFront); // Правый верхний ближний
            aoValues[2] = CalculateAO(right, back, rightBack);   // Правый верхний дальний
            aoValues[3] = CalculateAO(left, back, leftBack);     // Левый верхний дальний
        }
        else if (normal == new Vector3(0, -1, 0)) // Низ
        {
            offsets = new Vector3[]
            {
                new(0.5f, -0.5f, 0.5f),   // Правый нижний ближний
                new(-0.5f, -0.5f, 0.5f),  // Левый нижний ближний
                new(-0.5f, -0.5f, -0.5f), // Левый нижний дальний
                new(0.5f, -0.5f, -0.5f),  // Правый нижний дальний
            };

            bool left = !mesh.IsVoxelTransparent(blockX - 1, blockY - 1, blockZ);
            bool right = !mesh.IsVoxelTransparent(blockX + 1, blockY - 1, blockZ);
            bool front = !mesh.IsVoxelTransparent(blockX, blockY - 1, blockZ + 1);
            bool back = !mesh.IsVoxelTransparent(blockX, blockY - 1, blockZ - 1);
            bool leftFront = !mesh.IsVoxelTransparent(blockX - 1, blockY - 1, blockZ + 1);
            bool rightFront = !mesh.IsVoxelTransparent(blockX + 1, blockY - 1, blockZ + 1);
            bool leftBack = !mesh.IsVoxelTransparent(blockX - 1, blockY - 1, blockZ - 1);
            bool rightBack = !mesh.IsVoxelTransparent(blockX + 1, blockY - 1, blockZ - 1);

            aoValues[0] = CalculateAO(right, front, rightFront); // Правый нижний ближний
            aoValues[1] = CalculateAO(left, front, leftFront);   // Левый нижний ближний
            aoValues[2] = CalculateAO(left, back, leftBack);     // Левый нижний дальний
            aoValues[3] = CalculateAO(right, back, rightBack);   // Правый нижний дальний
        }
        else if (normal == new Vector3(1, 0, 0)) // Правая
        {
            offsets = new Vector3[]
            {
                new(0.5f, -0.5f, 0.5f),   // Правый нижний ближний
                new(0.5f, -0.5f, -0.5f),  // Правый нижний дальний
                new(0.5f, 0.5f, -0.5f),   // Правый верхний дальний
                new(0.5f, 0.5f, 0.5f),    // Правый верхний ближний
            };

            bool bottom = !mesh.IsVoxelTransparent(blockX + 1, blockY - 1, blockZ);
            bool top = !mesh.IsVoxelTransparent(blockX + 1, blockY + 1, blockZ);
            bool front = !mesh.IsVoxelTransparent(blockX + 1, blockY, blockZ + 1);
            bool back = !mesh.IsVoxelTransparent(blockX + 1, blockY, blockZ - 1);
            bool bottomFront = !mesh.IsVoxelTransparent(blockX + 1, blockY - 1, blockZ + 1);
            bool topFront = !mesh.IsVoxelTransparent(blockX + 1, blockY + 1, blockZ + 1);
            bool bottomBack = !mesh.IsVoxelTransparent(blockX + 1, blockY - 1, blockZ - 1);
            bool topBack = !mesh.IsVoxelTransparent(blockX + 1, blockY + 1, blockZ - 1);

            aoValues[0] = CalculateAO(bottom, front, bottomFront); // Правый нижний ближний
            aoValues[1] = CalculateAO(bottom, back, bottomBack);   // Правый нижний дальний
            aoValues[2] = CalculateAO(top, back, topBack);         // Правый верхний дальний
            aoValues[3] = CalculateAO(top, front, topFront);       // Правый верхний ближний
        }
        else if (normal == new Vector3(-1, 0, 0)) // Левая
        {
            offsets = new Vector3[]
            {
                new(-0.5f, 0.5f, -0.5f),  // Левый верхний дальний
                new(-0.5f, -0.5f, -0.5f), // Левый нижний дальний
                new(-0.5f, -0.5f, 0.5f),  // Левый нижний ближний
                new(-0.5f, 0.5f, 0.5f),   // Левый верхний ближний
            };

            bool bottom = !mesh.IsVoxelTransparent(blockX - 1, blockY - 1, blockZ);
            bool top = !mesh.IsVoxelTransparent(blockX - 1, blockY + 1, blockZ);
            bool front = !mesh.IsVoxelTransparent(blockX - 1, blockY, blockZ + 1);
            bool back = !mesh.IsVoxelTransparent(blockX - 1, blockY, blockZ - 1);
            bool bottomFront = !mesh.IsVoxelTransparent(blockX - 1, blockY - 1, blockZ + 1);
            bool topFront = !mesh.IsVoxelTransparent(blockX - 1, blockY + 1, blockZ + 1);
            bool bottomBack = !mesh.IsVoxelTransparent(blockX - 1, blockY - 1, blockZ - 1);
            bool topBack = !mesh.IsVoxelTransparent(blockX - 1, blockY + 1, blockZ - 1);

            aoValues[0] = CalculateAO(top, back, topBack);         // Левый верхний дальний
            aoValues[1] = CalculateAO(bottom, back, bottomBack);   // Левый нижний дальний
            aoValues[2] = CalculateAO(bottom, front, bottomFront); // Левый нижний ближний
            aoValues[3] = CalculateAO(top, front, topFront);       // Левый верхний ближний
        }
        else if (normal == new Vector3(0, 0, 1)) // Передняя
        {
            offsets = new Vector3[]
            {
                new(-0.5f, 0.5f, 0.5f),   // Левый верхний ближний
                new(-0.5f, -0.5f, 0.5f),  // Левый нижний ближний
                new(0.5f, -0.5f, 0.5f),   // Правый нижний ближний
                new(0.5f, 0.5f, 0.5f),    // Правый верхний ближний
            };

            bool left = !mesh.IsVoxelTransparent(blockX - 1, blockY, blockZ + 1);
            bool right = !mesh.IsVoxelTransparent(blockX + 1, blockY, blockZ + 1);
            bool top = !mesh.IsVoxelTransparent(blockX, blockY + 1, blockZ + 1);
            bool bottom = !mesh.IsVoxelTransparent(blockX, blockY - 1, blockZ + 1);
            bool topLeft = !mesh.IsVoxelTransparent(blockX - 1, blockY + 1, blockZ + 1);
            bool topRight = !mesh.IsVoxelTransparent(blockX + 1, blockY + 1, blockZ + 1);
            bool bottomLeft = !mesh.IsVoxelTransparent(blockX - 1, blockY - 1, blockZ + 1);
            bool bottomRight = !mesh.IsVoxelTransparent(blockX + 1, blockY - 1, blockZ + 1);

            aoValues[0] = CalculateAO(left, top, topLeft);         // Левый верхний ближний
            aoValues[1] = CalculateAO(left, bottom, bottomLeft);   // Левый нижний ближний
            aoValues[2] = CalculateAO(right, bottom, bottomRight); // Правый нижний ближний
            aoValues[3] = CalculateAO(right, top, topRight);       // Правый верхний ближний
        }
        else if (normal == new Vector3(0, 0, -1)) // Задняя
        {
            offsets = new Vector3[]
            {
                new(0.5f, 0.5f, -0.5f),   // Правый верхний дальний
                new(0.5f, -0.5f, -0.5f),  // Правый нижний дальний
                new(-0.5f, -0.5f, -0.5f), // Левый нижний дальний
                new(-0.5f, 0.5f, -0.5f),  // Левый верхний дальний
            };

            bool left = !mesh.IsVoxelTransparent(blockX - 1, blockY, blockZ - 1);
            bool right = !mesh.IsVoxelTransparent(blockX + 1, blockY, blockZ - 1);
            bool top = !mesh.IsVoxelTransparent(blockX, blockY + 1, blockZ - 1);
            bool bottom = !mesh.IsVoxelTransparent(blockX, blockY - 1, blockZ - 1);
            bool topLeft = !mesh.IsVoxelTransparent(blockX - 1, blockY + 1, blockZ - 1);
            bool topRight = !mesh.IsVoxelTransparent(blockX + 1, blockY + 1, blockZ - 1);
            bool bottomLeft = !mesh.IsVoxelTransparent(blockX - 1, blockY - 1, blockZ - 1);
            bool bottomRight = !mesh.IsVoxelTransparent(blockX + 1, blockY - 1, blockZ - 1);

            aoValues[0] = CalculateAO(right, top, topRight);       // Правый верхний дальний
            aoValues[1] = CalculateAO(right, bottom, bottomRight); // Правый нижний дальний
            aoValues[2] = CalculateAO(left, bottom, bottomLeft);   // Левый нижний дальний
            aoValues[3] = CalculateAO(left, top, topLeft);         // Левый верхний дальний
        }

        float[] faceVertices = new float[28];
        for (int i = 0; i < 4; i++)
        {
            // Вычисляем затемненный цвет на основе базового цвета и AO
            //color *= aoValues[i];

            faceVertices[i * 7] = pos.X + offsets[i].X * 0.0625f;
            faceVertices[i * 7 + 1] = pos.Y + offsets[i].Y * 0.0625f;
            faceVertices[i * 7 + 2] = pos.Z + offsets[i].Z * 0.0625f;
            faceVertices[i * 7 + 3] = color.X; // R
            faceVertices[i * 7 + 4] = color.Y; // G
            faceVertices[i * 7 + 5] = color.Z; // B
            faceVertices[i * 7 + 6] = aoValues[i]; // AO значение (можно использовать в шейдере)
        }

        uint[] faceIndices =
        {
            index, index + 1, index + 2,
            index + 2, index + 3, index
        };

        vertices.AddRange(faceVertices);
        indices.AddRange(faceIndices);
        index += 4;
    }

    private void SetupOpenGL()
    {
        vao = GL.GenVertexArray();
        vbo = GL.GenBuffer();
        ebo = GL.GenBuffer();

        GL.BindVertexArray(vao);

        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Count * sizeof(float), vertices.ToArray(), BufferUsageHint.StaticDraw);

        GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Count * sizeof(uint), indices.ToArray(), BufferUsageHint.StaticDraw);

        // Позиция (3 float)
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 7 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        // Цвет (3 float)
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 7 * sizeof(float), 3 * sizeof(float));
        GL.EnableVertexAttribArray(1);

        // AO значение (1 float) - если нужно использовать в шейдере
        GL.VertexAttribPointer(2, 1, VertexAttribPointerType.Float, false, 7 * sizeof(float), 6 * sizeof(float));
        GL.EnableVertexAttribArray(2);
    }

    public void Render(Shader shader)
    {
        Matrix4 translation = Matrix4.CreateTranslation(mesh.position);
        Matrix4 rotationX = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(mesh.rotation.X));
        Matrix4 rotationY = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(-mesh.rotation.Y));
        Matrix4 rotationZ = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(mesh.rotation.Z));

        Matrix4 model = rotationX * rotationY * rotationZ * translation; // Сначала поворот, потом позиция
        shader.SetMatrix4("model", model);
        shader.Use();

        GL.BindVertexArray(vao);

        //GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
        GL.DrawElements(PrimitiveType.Triangles, indices.Count, DrawElementsType.UnsignedInt, 0);
        //GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
    }

    // Метод для обновления меша после изменений в данных чанка
    public void UpdateMesh()
    {
        GenerateMesh();

        // Обновляем буферы
        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Count * sizeof(float), vertices.ToArray(), BufferUsageHint.StaticDraw);

        GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Count * sizeof(uint), indices.ToArray(), BufferUsageHint.StaticDraw);
    }
}
