using System;
using System.Collections.Generic;
using System.IO;
using CubeWay.Engine.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public class VoxelMesh
{

    private VoxelMeshRenderer meshRenderer;

    public Color[,,] data;
    public Vector3 meshSize;
    public Vector3 localPosition = Vector3.Zero;
    public Vector3 position;
    public Vector3 rotation  = Vector3.Zero; // В градусах (Euler)

    public VoxelMesh(string filepath)
    {
        var (width, height, depth, voxels) = ReadCub(filepath);

        Console.WriteLine($"Чанк загружен: {width}x{depth}x{height}, {voxels.Count} вокселей.");

        data = new Color[width, height, depth];
        meshSize = new Vector3(width, height, depth);

        foreach (Voxel voxel in voxels)
        {
            data[voxel.X, voxel.Y, voxel.Z] = new Color(voxel.R, voxel.G, voxel.B);
        }
        meshRenderer = new VoxelMeshRenderer(this);
    }
    public struct Color
    {
        public byte R, G, B;

        public Color(byte r, byte g, byte b)
        {
            R = r; G = g; B = b;
        }

        public override bool Equals(object obj)
        {
            return obj is Color color && this == color;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(R, G, B);
        }

        public bool IsBlack() => R == 0 && G == 0 && B == 0;

        public static bool operator ==(Color left, Color right)
        {
            return left.R == right.R && left.G == right.G && left.B == right.B;
        }

        public static bool operator !=(Color left, Color right)
        {
            return !(left == right);
        }
    }


    struct Voxel
    {
        public int X, Y, Z;
        public byte R, G, B;

        public Voxel(int x, int y, int z, byte r, byte g, byte b)
        {
            X = x; Y = y; Z = z;
            R = r; G = g; B = b;
        }
    }

    static (int width, int depth, int height, List<Voxel> voxels) ReadCub(string filename)
    {
        using (var fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
        using (var br = new BinaryReader(fs))
        {
            // Читаем размеры сетки (3 числа по 4 байта, little-endian)
            int width = br.ReadInt32();
            int depth = br.ReadInt32();
            int height = br.ReadInt32();

            var voxels = new List<Voxel>();

            for (int z = 0; z < height; z++)
            {
                for (int y = 0; y < depth; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        byte r = br.ReadByte();
                        byte g = br.ReadByte();
                        byte b = br.ReadByte();

                        if (r != 0 || g != 0 || b != 0) // Пропускаем пустые воксели
                        {
                            voxels.Add(new Voxel(x, y, z, r, g, b));
                        }
                    }
                }
            }

            return (width, depth, height, voxels);
        }
    }

    public Matrix4 GetModelMatrix()
    {
        // Вычисляем геометрический центр модели
        Vector3 centerOffset = new Vector3(meshSize.X / 2f, meshSize.Y / 2f, meshSize.Z / 2f);

        // Создаем матрицу трансформации с учетом локальной позиции 
        return Matrix4.CreateTranslation(-centerOffset) *  // Сначала смещаем к центру
               Matrix4.CreateRotationY(MathHelper.DegreesToRadians(rotation.Y)) *
               Matrix4.CreateRotationX(MathHelper.DegreesToRadians(rotation.X)) *
               Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(rotation.Z)) *
               Matrix4.CreateTranslation(position);  // Применяем только позицию без дополнительного смещения
    }



    public bool IsVoxelTransparent(int x, int y, int z)
    {
        // Если внутри меша — проверяем обычным способом
        if (x >= 0 && x < meshSize.X && y >= 0 && y < meshSize.Y && z >= 0 && z < meshSize.Z)
            return data[x, y, z].IsBlack();

        return true;
    }

    public void Render(Shader shader)
    {
        shader.SetMatrix4("model", GetModelMatrix());
        meshRenderer.Render(shader);
    }
}