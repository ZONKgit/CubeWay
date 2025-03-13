using System;
using System.Diagnostics;
using OpenTK.Mathematics;
using static FastNoiseLite;

namespace CubeWay
{
    public class Chunk
    {
        public int chunkSize = 16;
        public int chunkHeight = 128;
        public byte[,,] data;
        public Vector2i position;
        public ChunkRenderer chunkRenderer;
        private ChunkManager chunkManager;


        // Конструктор
        public Chunk(Vector2i _position, ChunkManager _chunkManager)
        {
            data = new byte[chunkSize, chunkHeight, chunkSize]; // Выделяем память
            chunkRenderer = new ChunkRenderer(this);
            position = _position;
            chunkManager = _chunkManager;

            // Засекаем время генерации ландшафта
            Stopwatch chunkTimer = Stopwatch.StartNew();
            GenerateTerrain();
            chunkTimer.Stop();
  
            Game.Instance.ChunkDataGenerationTime = chunkTimer.ElapsedMilliseconds; // Время в миллисекундах
        }

        private void GenerateTerrain()
        {
            for (int x = 0; x < chunkSize; x++)
            {
                for (int z = 0; z < chunkSize; z++)
                {
                    float height = chunkManager.noise.GetNoise(x + position.X * chunkSize, z + position.Y * chunkSize) * 10f + 10;

                    for (int y = 0; y < chunkHeight; y++)
                    {
                        data[x, y, z] = (byte)(y < height ? 1 : 0);
                    }
                }
            }
        }

        // Проверка, является ли блок прозрачным (возвращает true для воздуха)
        public bool IsBlockTransparent(int x, int y, int z)
        {
            // Если координаты в пределах текущего чанка
            if (x >= 0 && x < chunkSize && y >= 0 && y < chunkHeight && z >= 0 && z < chunkSize)
                return data[x, y, z] == 0;

            // Если координата Y вне допустимого диапазона
            if (y < 0 || y >= chunkHeight)
                return true;

            // Преобразуем локальные координаты в глобальные мировые координаты
            int worldX = position.X * chunkSize + x;
            int worldZ = position.Y * chunkSize + z;

            // Используем ChunkManager для проверки соседних чанков
            if (chunkManager != null)
            {
                // Вызов метода ChunkManager для получения прозрачности блока в мировых координатах
                return chunkManager.IsBlockTransparent(worldX, y, worldZ);
            }

            return false; // Если chunkManager не инициализирован
        }


        public void Render(Shader shader)
        {
            chunkRenderer.Render(shader);
        }

        public void Dispose()
        {
            chunkRenderer.Dispose(); // Освобождаем GPU-ресурсы
        }

    }
}