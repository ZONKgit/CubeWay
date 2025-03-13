using OpenTK.Windowing.Desktop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;

namespace CubeWay.UI.Core
{
    public class GuiManager
    {
        private ImGuiController _controller;
        private List<IGuiWindow> _windows = new();

        public GuiManager(int width, int height)
        {
            _controller = new ImGuiController(width, height);
        }

        public void RegisterWindow(IGuiWindow window)
        {
            _windows.Add(window);
        }

        public void Update(GameWindow game, float deltaTime)
        {
            _controller.Update(game, deltaTime);
        }

        public void Render()
        {
            foreach (var window in _windows)
            {
                if (window.IsVisible)
                {
                    window.Render();
                }
            }

            _controller.Render();
        }

        public void Resize(int width, int height)
        {
            _controller.WindowResized(width, height);
        }
    }

}
