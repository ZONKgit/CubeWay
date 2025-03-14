using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace CubeWay
{
    public class ChunkMesh
    {
        public List<float> vertices = new();
        public List<uint> indices = new();
        private Chunk chunk;
        public object meshLock = new(); // Добавляем объект блокировки

        public ChunkMesh(Chunk _chunk) { chunk = _chunk; }

        public void GenerateMesh()
        {
            lock (meshLock)
            {
                vertices.Clear();
                indices.Clear();
                uint index = 0;

                for (int x = 0; x < chunk.chunkSize; x++)
                {
                    for (int y = 0; y < chunk.chunkHeight; y++)
                    {
                        for (int z = 0; z < chunk.chunkSize; z++)
                        {
                            if (chunk.data[x, y, z] == 0) continue; // Пропуск пустых блоков

                            Vector3 pos = new(x + chunk.position.X * chunk.chunkSize, y, z + chunk.position.Y * chunk.chunkSize);

                            if (chunk.IsBlockTransparent(x - 1, y, z)) AddFace(pos, new Vector3(-1, 0, 0), x, y, z, ref index); // Левая грань
                            if (chunk.IsBlockTransparent(x + 1, y, z)) AddFace(pos, new Vector3(1, 0, 0), x, y, z, ref index);  // Правая грань
                            if (chunk.IsBlockTransparent(x, y, z - 1)) AddFace(pos, new Vector3(0, 0, -1), x, y, z, ref index); // Задняя грань
                            if (chunk.IsBlockTransparent(x, y, z + 1)) AddFace(pos, new Vector3(0, 0, 1), x, y, z, ref index);  // Передняя грань
                            if (chunk.IsBlockTransparent(x, y + 1, z)) AddFace(pos, new Vector3(0, 1, 0), x, y, z, ref index);  // Верхняя грань
                            if (chunk.IsBlockTransparent(x, y - 1, z)) AddFace(pos, new Vector3(0, -1, 0), x, y, z, ref index); // Нижняя грань
                        }
                    }
                }
                chunk.chunkManager.readyToRenderChunks.Enqueue(chunk);
            }
        }

        // Вычисление значения AO для вершины на основе соседних блоков
        private float CalculateAO(bool side1, bool side2, bool corner)
        {

            // Minecraft использует более сложную схему расчета AO
            if (side1 && side2)
                return 0.0f; // Максимальное затенение в углу

            // Счетчик блокирующих блоков
            int blockers = 0;
            if (side1) blockers++;
            if (side2) blockers++;
            if (corner && !side1 && !side2) blockers++;

            // Более плавная градация - как в Minecraft
            switch (blockers)
            {
                case 0: return 1.0f;//1  // Нет блокирующих блоков - полная яркость
                case 1: return 0.9f;//0.7  // Один блокирующий блок - легкое затенение
                case 2: return 0.8f;//0.5  // Два блокирующих блока - среднее затенение
                case 3: return 0.7f;//0.2  // Три блокирующих блока - сильное затенение
                default: return 1.0f;// 0.0 // На всякий случай
            }
        }


        // Добавляем метод для определения яркости стороны блока
        private float GetFaceBrightness(Vector3 normal)
        {
            // В Minecraft разные стороны имеют разную базовую яркость
            if (normal == new Vector3(0, 1, 0)) // Верх
                return 1.0f;
            else if (normal == new Vector3(0, -1, 0)) // Низ
                return 0.6f;
            else // Боковые стороны
                return 0.8f;
        }

        private void AddFace(Vector3 pos, Vector3 normal, int blockX, int blockY, int blockZ, ref uint index)
        {
            Vector3[] offsets = new Vector3[4];
            float[] aoValues = new float[4];

            // Базовая яркость стороны блока
            float faceBrightness = GetFaceBrightness(normal);

            // Базовый цвет блока (в будущем может зависеть от типа блока)
            Vector3 baseColor = GetBlockColor(chunk.data[blockX, blockY, blockZ]);

            // Код позиционирования вершин остается тем же...
            // ... (код определения offsets для разных сторон блока)

            if (normal == new Vector3(0, 1, 0)) // Верх
            {
                offsets = new Vector3[]
                {
                    new(-0.5f, 0.5f, 0.5f),   // Левый верхний ближний
                    new(0.5f, 0.5f, 0.5f),    // Правый верхний ближний
                    new(0.5f, 0.5f, -0.5f),   // Правый верхний дальний
                    new(-0.5f, 0.5f, -0.5f)   // Левый верхний дальний
                };

                // Проверка блоков вокруг вершины для вычисления AO
                // Расширенная проверка для верхней грани (больше блоков проверяется)

                // Непосредственные соседи
                bool left = !chunk.IsBlockTransparent(blockX - 1, blockY + 1, blockZ);
                bool right = !chunk.IsBlockTransparent(blockX + 1, blockY + 1, blockZ);
                bool front = !chunk.IsBlockTransparent(blockX, blockY + 1, blockZ + 1);
                bool back = !chunk.IsBlockTransparent(blockX, blockY + 1, blockZ - 1);

                // Диагональные соседи
                bool leftFront = !chunk.IsBlockTransparent(blockX - 1, blockY + 1, blockZ + 1);
                bool rightFront = !chunk.IsBlockTransparent(blockX + 1, blockY + 1, blockZ + 1);
                bool leftBack = !chunk.IsBlockTransparent(blockX - 1, blockY + 1, blockZ - 1);
                bool rightBack = !chunk.IsBlockTransparent(blockX + 1, blockY + 1, blockZ - 1);

                // Проверяем блоки на уровень выше для более точного расчета теней
                bool leftUp = !chunk.IsBlockTransparent(blockX - 1, blockY + 2, blockZ);
                bool rightUp = !chunk.IsBlockTransparent(blockX + 1, blockY + 2, blockZ);
                bool frontUp = !chunk.IsBlockTransparent(blockX, blockY + 2, blockZ + 1);
                bool backUp = !chunk.IsBlockTransparent(blockX, blockY + 2, blockZ - 1);

                // Расчет AO для каждой вершины с учетом всех окружающих блоков
                aoValues[0] = VertexAOCalculation(left, front, leftFront, leftUp, frontUp); // Левый верхний ближний
                aoValues[1] = VertexAOCalculation(right, front, rightFront, rightUp, frontUp); // Правый верхний ближний
                aoValues[2] = VertexAOCalculation(right, back, rightBack, rightUp, backUp); // Правый верхний дальний
                aoValues[3] = VertexAOCalculation(left, back, leftBack, leftUp, backUp); // Левый верхний дальний
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

                // Непосредственные соседи
                bool left = !chunk.IsBlockTransparent(blockX - 1, blockY - 1, blockZ);
                bool right = !chunk.IsBlockTransparent(blockX + 1, blockY - 1, blockZ);
                bool front = !chunk.IsBlockTransparent(blockX, blockY - 1, blockZ + 1);
                bool back = !chunk.IsBlockTransparent(blockX, blockY - 1, blockZ - 1);

                // Диагональные соседи
                bool leftFront = !chunk.IsBlockTransparent(blockX - 1, blockY - 1, blockZ + 1);
                bool rightFront = !chunk.IsBlockTransparent(blockX + 1, blockY - 1, blockZ + 1);
                bool leftBack = !chunk.IsBlockTransparent(blockX - 1, blockY - 1, blockZ - 1);
                bool rightBack = !chunk.IsBlockTransparent(blockX + 1, blockY - 1, blockZ - 1);

                // Проверяем блоки на уровень ниже для более точного расчета теней
                bool leftDown = !chunk.IsBlockTransparent(blockX - 1, blockY - 2, blockZ);
                bool rightDown = !chunk.IsBlockTransparent(blockX + 1, blockY - 2, blockZ);
                bool frontDown = !chunk.IsBlockTransparent(blockX, blockY - 2, blockZ + 1);
                bool backDown = !chunk.IsBlockTransparent(blockX, blockY - 2, blockZ - 1);

                // Расчет AO для каждой вершины
                aoValues[0] = VertexAOCalculation(right, front, rightFront, rightDown, frontDown); // Правый нижний ближний
                aoValues[1] = VertexAOCalculation(left, front, leftFront, leftDown, frontDown);    // Левый нижний ближний
                aoValues[2] = VertexAOCalculation(left, back, leftBack, leftDown, backDown);       // Левый нижний дальний
                aoValues[3] = VertexAOCalculation(right, back, rightBack, rightDown, backDown);    // Правый нижний дальний
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

                // Непосредственные соседи
                bool bottom = !chunk.IsBlockTransparent(blockX + 1, blockY - 1, blockZ);
                bool top = !chunk.IsBlockTransparent(blockX + 1, blockY + 1, blockZ);
                bool front = !chunk.IsBlockTransparent(blockX + 1, blockY, blockZ + 1);
                bool back = !chunk.IsBlockTransparent(blockX + 1, blockY, blockZ - 1);

                // Диагональные соседи
                bool bottomFront = !chunk.IsBlockTransparent(blockX + 1, blockY - 1, blockZ + 1);
                bool topFront = !chunk.IsBlockTransparent(blockX + 1, blockY + 1, blockZ + 1);
                bool bottomBack = !chunk.IsBlockTransparent(blockX + 1, blockY - 1, blockZ - 1);
                bool topBack = !chunk.IsBlockTransparent(blockX + 1, blockY + 1, blockZ - 1);

                // Дополнительные блоки для более точного расчета теней
                bool rightMore = !chunk.IsBlockTransparent(blockX + 2, blockY, blockZ);
                bool rightMoreTop = !chunk.IsBlockTransparent(blockX + 2, blockY + 1, blockZ);
                bool rightMoreBottom = !chunk.IsBlockTransparent(blockX + 2, blockY - 1, blockZ);
                bool rightMoreFront = !chunk.IsBlockTransparent(blockX + 2, blockY, blockZ + 1);
                bool rightMoreBack = !chunk.IsBlockTransparent(blockX + 2, blockY, blockZ - 1);

                // Расчет AO для каждой вершины с дополнительным влиянием от блоков дальше
                aoValues[0] = VertexAOCalculation(bottom, front, bottomFront, rightMoreBottom, rightMoreFront); // Правый нижний ближний
                aoValues[1] = VertexAOCalculation(bottom, back, bottomBack, rightMoreBottom, rightMoreBack);    // Правый нижний дальний
                aoValues[2] = VertexAOCalculation(top, back, topBack, rightMoreTop, rightMoreBack);             // Правый верхний дальний
                aoValues[3] = VertexAOCalculation(top, front, topFront, rightMoreTop, rightMoreFront);          // Правый верхний ближний
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

                // Непосредственные соседи
                bool bottom = !chunk.IsBlockTransparent(blockX - 1, blockY - 1, blockZ);
                bool top = !chunk.IsBlockTransparent(blockX - 1, blockY + 1, blockZ);
                bool front = !chunk.IsBlockTransparent(blockX - 1, blockY, blockZ + 1);
                bool back = !chunk.IsBlockTransparent(blockX - 1, blockY, blockZ - 1);

                // Диагональные соседи
                bool bottomFront = !chunk.IsBlockTransparent(blockX - 1, blockY - 1, blockZ + 1);
                bool topFront = !chunk.IsBlockTransparent(blockX - 1, blockY + 1, blockZ + 1);
                bool bottomBack = !chunk.IsBlockTransparent(blockX - 1, blockY - 1, blockZ - 1);
                bool topBack = !chunk.IsBlockTransparent(blockX - 1, blockY + 1, blockZ - 1);

                // Дополнительные блоки для более точного расчета теней
                bool leftMore = !chunk.IsBlockTransparent(blockX - 2, blockY, blockZ);
                bool leftMoreTop = !chunk.IsBlockTransparent(blockX - 2, blockY + 1, blockZ);
                bool leftMoreBottom = !chunk.IsBlockTransparent(blockX - 2, blockY - 1, blockZ);
                bool leftMoreFront = !chunk.IsBlockTransparent(blockX - 2, blockY, blockZ + 1);
                bool leftMoreBack = !chunk.IsBlockTransparent(blockX - 2, blockY, blockZ - 1);

                // Расчет AO для каждой вершины
                aoValues[0] = VertexAOCalculation(top, back, topBack, leftMoreTop, leftMoreBack);         // Левый верхний дальний
                aoValues[1] = VertexAOCalculation(bottom, back, bottomBack, leftMoreBottom, leftMoreBack);  // Левый нижний дальний
                aoValues[2] = VertexAOCalculation(bottom, front, bottomFront, leftMoreBottom, leftMoreFront); // Левый нижний ближний
                aoValues[3] = VertexAOCalculation(top, front, topFront, leftMoreTop, leftMoreFront);       // Левый верхний ближний
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

                // Непосредственные соседи
                bool left = !chunk.IsBlockTransparent(blockX - 1, blockY, blockZ + 1);
                bool right = !chunk.IsBlockTransparent(blockX + 1, blockY, blockZ + 1);
                bool top = !chunk.IsBlockTransparent(blockX, blockY + 1, blockZ + 1);
                bool bottom = !chunk.IsBlockTransparent(blockX, blockY - 1, blockZ + 1);

                // Диагональные соседи
                bool topLeft = !chunk.IsBlockTransparent(blockX - 1, blockY + 1, blockZ + 1);
                bool topRight = !chunk.IsBlockTransparent(blockX + 1, blockY + 1, blockZ + 1);
                bool bottomLeft = !chunk.IsBlockTransparent(blockX - 1, blockY - 1, blockZ + 1);
                bool bottomRight = !chunk.IsBlockTransparent(blockX + 1, blockY - 1, blockZ + 1);

                // Дополнительные блоки для более точного расчета теней
                bool frontMore = !chunk.IsBlockTransparent(blockX, blockY, blockZ + 2);
                bool frontMoreLeft = !chunk.IsBlockTransparent(blockX - 1, blockY, blockZ + 2);
                bool frontMoreRight = !chunk.IsBlockTransparent(blockX + 1, blockY, blockZ + 2);
                bool frontMoreTop = !chunk.IsBlockTransparent(blockX, blockY + 1, blockZ + 2);
                bool frontMoreBottom = !chunk.IsBlockTransparent(blockX, blockY - 1, blockZ + 2);

                // Расчет AO для каждой вершины
                aoValues[0] = VertexAOCalculation(left, top, topLeft, frontMoreLeft, frontMoreTop);         // Левый верхний ближний
                aoValues[1] = VertexAOCalculation(left, bottom, bottomLeft, frontMoreLeft, frontMoreBottom);   // Левый нижний ближний
                aoValues[2] = VertexAOCalculation(right, bottom, bottomRight, frontMoreRight, frontMoreBottom); // Правый нижний ближний
                aoValues[3] = VertexAOCalculation(right, top, topRight, frontMoreRight, frontMoreTop);       // Правый верхний ближний
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

                // Непосредственные соседи
                bool left = !chunk.IsBlockTransparent(blockX - 1, blockY, blockZ - 1);
                bool right = !chunk.IsBlockTransparent(blockX + 1, blockY, blockZ - 1);
                bool top = !chunk.IsBlockTransparent(blockX, blockY + 1, blockZ - 1);
                bool bottom = !chunk.IsBlockTransparent(blockX, blockY - 1, blockZ - 1);

                // Диагональные соседи
                bool topLeft = !chunk.IsBlockTransparent(blockX - 1, blockY + 1, blockZ - 1);
                bool topRight = !chunk.IsBlockTransparent(blockX + 1, blockY + 1, blockZ - 1);
                bool bottomLeft = !chunk.IsBlockTransparent(blockX - 1, blockY - 1, blockZ - 1);
                bool bottomRight = !chunk.IsBlockTransparent(blockX + 1, blockY - 1, blockZ - 1);

                // Дополнительные блоки для более точного расчета теней
                bool backMore = !chunk.IsBlockTransparent(blockX, blockY, blockZ - 2);
                bool backMoreLeft = !chunk.IsBlockTransparent(blockX - 1, blockY, blockZ - 2);
                bool backMoreRight = !chunk.IsBlockTransparent(blockX + 1, blockY, blockZ - 2);
                bool backMoreTop = !chunk.IsBlockTransparent(blockX, blockY + 1, blockZ - 2);
                bool backMoreBottom = !chunk.IsBlockTransparent(blockX, blockY - 1, blockZ - 2);

                // Расчет AO для каждой вершины
                aoValues[0] = VertexAOCalculation(right, top, topRight, backMoreRight, backMoreTop);       // Правый верхний дальний
                aoValues[1] = VertexAOCalculation(right, bottom, bottomRight, backMoreRight, backMoreBottom); // Правый нижний дальний
                aoValues[2] = VertexAOCalculation(left, bottom, bottomLeft, backMoreLeft, backMoreBottom);   // Левый нижний дальний
                aoValues[3] = VertexAOCalculation(left, top, topLeft, backMoreLeft, backMoreTop);         // Левый верхний дальний
            }

            // Создание вершин с расчитанными AO значениями
            float[] faceVertices = new float[28];
            for (int i = 0; i < 4; i++)
            {
                // Применяем AO и базовую яркость стороны к цвету
                Vector3 color = baseColor * aoValues[i] * faceBrightness;

                // Ограничиваем значения цвета, чтобы избежать переполнения
                color.X = Math.Clamp(color.X, 0.0f, 1.0f);
                color.Y = Math.Clamp(color.Y, 0.0f, 1.0f);
                color.Z = Math.Clamp(color.Z, 0.0f, 1.0f);

                faceVertices[i * 7] = pos.X + offsets[i].X;
                faceVertices[i * 7 + 1] = pos.Y + offsets[i].Y;
                faceVertices[i * 7 + 2] = pos.Z + offsets[i].Z;
                faceVertices[i * 7 + 3] = color.X; // R
                faceVertices[i * 7 + 4] = color.Y; // G
                faceVertices[i * 7 + 5] = color.Z; // B
                faceVertices[i * 7 + 6] = aoValues[i]; // Сохраняем AO для возможного использования в шейдере
            }

            // Индексы для создания двух треугольников
            uint[] faceIndices = {
                index, index + 1, index + 2,
                index + 2, index + 3, index
            };

            vertices.AddRange(faceVertices);
            indices.AddRange(faceIndices);
            index += 4;
        }

        // Улучшенный расчет AO для вершины с учетом большего количества блоков
        private float VertexAOCalculation(bool side1, bool side2, bool corner, bool extraSide1, bool extraSide2)
        {
            // Базовый расчет AO
            float ao = CalculateAO(side1, side2, corner);

            // Дополнительное затенение от блоков на уровень выше
            if (extraSide1) ao *= 0.9f;
            if (extraSide2) ao *= 0.9f;

            return Math.Max(ao, 0.05f); // Минимальный уровень яркости
        }

        // Получение цвета блока в зависимости от его типа
        private Vector3 GetBlockColor(byte blockType)
        {
            // Пример разных цветов для разных типов блоков
            switch (blockType)
            {
                case 1: return new Vector3(145, 209, 67) / 255; // Трава
                case 2: return new Vector3(150, 108, 74) / 255; // Земля
                case 3: return new Vector3(180, 180, 180) / 255; // Камень
                default: return new Vector3(145, 209, 67) / 255; // По умолчанию
            }
        }

    }
}
