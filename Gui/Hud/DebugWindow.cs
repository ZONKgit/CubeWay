using CubeWay.UI.Core;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubeWay.Gui.Hud
{
    public class DebugWindow : IGuiWindow
    {
        public bool IsVisible { get; set; } = true;

        public void Render()
        {
            if (!IsVisible) return;

            ImGui.SetNextWindowPos(new System.Numerics.Vector2(10, 10), ImGuiCond.Always);
            ImGui.SetNextWindowBgAlpha(0.5f);
            ImGui.Begin("Debug Info", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.AlwaysAutoResize);

            ImGui.Text($"FPS: {Game.Instance.Fps:F2}");
            ImGui.Text($"Position: {Game.Instance._player.Position.X:F2}, {Game.Instance._player.Position.Y:F2}, {Game.Instance._player.Position.Z:F2}");
            ImGui.Text($"Render Distance: {Game.Instance._chunkManager.renderDistance:F2}");
            ImGui.Text($"Terrain generation time: {Game.Instance.ChunkDataGenerationTime}");
            ImGui.Text($"Chunk mesh generation time: {Game.Instance.ChunkMeshGenerationTime}");



            ImGui.End();
        }
    }

}
