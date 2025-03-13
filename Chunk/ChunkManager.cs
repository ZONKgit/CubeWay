using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using OpenTK.Mathematics;
using System.Collections.Concurrent;

namespace CubeWay
{
    public class ChunkManager
    {
        public const int ChunkSize = 16;
        public const int ChunkHeight = 128;
        public FastNoiseLite noise;
        private Dictionary<Vector2i, Chunk> chunks = new(); // Храним чанки по их координатам (2D)
        public int renderDistance = 32; // Радиус прогрузки чанков в чанках (например, 4 = 9x9 чанков)
        public ConcurrentQueue<Chunk> readyToRenderChunks = new();





        public ChunkManager()
        {
            noise = new FastNoiseLite();
        }

        public void  Update(Vector3 playerPosition)
        {
            Vector2i playerChunkPos = new(
                (int)Math.Floor(playerPosition.X / 16f),
                (int)Math.Floor(playerPosition.Z / 16f)
            );

            LoadChunksAround(playerChunkPos);
            UnloadDistantChunks(playerChunkPos);

            while (readyToRenderChunks.TryDequeue(out Chunk chunk))
            {
                List<float> verticesCopy;
                List<uint> indicesCopy;

                lock (chunk.chunkMesh.meshLock) // Блокируем доступ, пока копируем данные
                {
                    verticesCopy = new List<float>(chunk.chunkMesh.vertices);
                    indicesCopy = new List<uint>(chunk.chunkMesh.indices);
                    chunk.chunkRenderer.UpdateMesh(verticesCopy, indicesCopy);
                }
            }
        }


        private void LoadChunksAround(Vector2i playerChunkPos)
        {
            // Засекаем время генерации меша
            Stopwatch chunkTimer = Stopwatch.StartNew();

            List<Vector2i> newChunks = new List<Vector2i>();

            for (int x = -renderDistance; x <= renderDistance; x++)
            {
                for (int z = -renderDistance; z <= renderDistance; z++)
                {
                    Vector2i chunkCoord = new(playerChunkPos.X + x, playerChunkPos.Y + z);

                    if (!chunks.ContainsKey(chunkCoord))
                    {
                        chunks[chunkCoord] = new Chunk(chunkCoord, this);
                        newChunks.Add(chunkCoord);
                    }
                }
            }

            chunkTimer.Stop();
            if (chunkTimer.ElapsedMilliseconds != 0) { Console.WriteLine($"part1: {chunkTimer.ElapsedMilliseconds}"); }
            chunkTimer.Restart();

            // Затем обновляем меши для всех новых чанков и их соседей
            foreach (Vector2i newChunkCoord in newChunks)
            {
                // Обновляем меш для нового чанка
                chunks[newChunkCoord].UpdateMesh();

                // Обновляем меши для соседних чанков (если они существуют)
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dz = -1; dz <= 1; dz++)
                    {
                        if (dx == 0 && dz == 0) continue; // Пропускаем сам чанк

                        Vector2i neighborCoord = new Vector2i(newChunkCoord.X + dx, newChunkCoord.Y + dz);
                        if (chunks.ContainsKey(neighborCoord))
                        {
                            chunks[neighborCoord].UpdateMesh();
                        }
                    }
                }
            }
            chunkTimer.Stop();
            if (chunkTimer.ElapsedMilliseconds != 0) { Console.WriteLine($"part2: {chunkTimer.ElapsedMilliseconds}"); }
        }

        private void UnloadDistantChunks(Vector2i playerChunkPos)
        {
            List<Vector2i> chunksToRemove = new();

            foreach (var chunkCoord in chunks.Keys.ToList())
            {
                if (Math.Abs(chunkCoord.X - playerChunkPos.X) > renderDistance ||
                    Math.Abs(chunkCoord.Y - playerChunkPos.Y) > renderDistance)
                {
                    chunks[chunkCoord].Dispose();
                    chunksToRemove.Add(chunkCoord);
                }
            }

            foreach (var chunkCoord in chunksToRemove)
            {
                chunks.Remove(chunkCoord);
            }
        }

        public bool TryGetChunk(int x, int z, out Chunk chunk)
        {
            return chunks.TryGetValue(new Vector2i(x, z), out chunk);
        }


        public bool IsBlockTransparent(int x, int y, int z)
        {
            // Если координата Y вне допустимого диапазона
            if (y < 0 || y >= ChunkHeight)
                return true;

            // Вычисляем индексы чанка для данных мировых координат
            int chunkX = (int)Math.Floor((double)x / ChunkSize);
            int chunkZ = (int)Math.Floor((double)z / ChunkSize);

            // Вычисляем локальные координаты внутри чанка
            // Для отрицательных значений требуется особая обработка
            int localX = x - chunkX * ChunkSize;
            int localZ = z - chunkZ * ChunkSize;

            // Убедимся, что локальные координаты в пределах [0, ChunkSize-1]
            localX = (localX + ChunkSize) % ChunkSize;
            localZ = (localZ + ChunkSize) % ChunkSize;

            // Пытаемся получить чанк по его координатам
            Chunk chunk = null;
            if (chunks.TryGetValue(new Vector2i(chunkX, chunkZ), out chunk) && chunk != null)
            {
                // Проверяем прозрачность блока в найденном чанке
                return chunk.data[localX, y, localZ] == 0;
            }

            return false; // Если чанк не найден, считаем блок не прозрачным
        }


        public void RenderChunks(Shader shader)
        {
            foreach (var chunk in chunks.Values){chunk.Render(shader);}
        }
    }
}
