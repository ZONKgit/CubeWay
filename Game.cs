using CubeWay.Entities.Player;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using System.Drawing;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System.Diagnostics;
using CubeWay.UI.Core;
using CubeWay.Gui.Hud;

namespace CubeWay
{
    public class Game : GameWindow
    {
        public static Game Instance { get; private set; }

        private Shader _shader; // Хранить объект Shader вместо только его ID
        public Player _player;
        public ChunkManager _chunkManager;
        private bool _firstLoad = true;
        VoxelMesh voxelMesh;

        // Debug переменные
        public double _deltaTime = 0;
        public double Fps { get; private set; }
        public float ChunkMeshGenerationTime = 0;
        public float ChunkDataGenerationTime = 0;



        private GuiManager _guiManager;
        private DebugWindow _debugWindow;




        public Game() : base(GameWindowSettings.Default, new NativeWindowSettings() { Size = new Vector2i(1600, 900), APIVersion = new Version(3, 3), Vsync = VSyncMode.Off })
        {
            _player = new Player(new Vector3(0, 16, 0), this);
            CursorState = CursorState.Grabbed;

            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                throw new Exception("Game instance already exists!");
            }
        }

        protected override void OnLoad()
        {
            base.OnLoad();
            GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            GL.Enable(EnableCap.DepthTest);

            _chunkManager = new ChunkManager();
            _shader = new Shader("shader.vert", "shader.frag");
            voxelMesh = new VoxelMesh("arrow.cub");

            _guiManager = new GuiManager(ClientSize.X, ClientSize.Y);
            _debugWindow = new DebugWindow();
            _guiManager.RegisterWindow(_debugWindow);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            _guiManager.Update(this, (float)e.Time);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            _shader.Use();

            Matrix4 rotation = Matrix4.CreateRotationY(0);
            Matrix4 translation = Matrix4.CreateTranslation(1.0f, 0.0f, 0.0f);
            Matrix4 model = rotation * translation;
            Matrix4 view = _player.camera.GetViewMatrix();
            Matrix4 projection = _player.camera.GetProjectionMatrix();

            int modelLoc = GL.GetUniformLocation(_shader.Handle, "model");
            int viewLoc = GL.GetUniformLocation(_shader.Handle, "view");
            int projLoc = GL.GetUniformLocation(_shader.Handle, "projection");

            GL.UniformMatrix4(modelLoc, false, ref model);
            GL.UniformMatrix4(viewLoc, false, ref view);
            GL.UniformMatrix4(projLoc, false, ref projection);

            _chunkManager.RenderChunks(_shader);
            _player.Render(_shader);
            voxelMesh.rotation.Y += 0.1f;
            voxelMesh.Render(_shader);

            // Обновление FPS
            _deltaTime = e.Time;
            if (_deltaTime > 0){ Fps = 1.0 / _deltaTime; }

            _guiManager.Render();

            SwapBuffers();
        }


        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            _player.camera = new Camera(_player.camera.Position, e.Width / (float)e.Height);
            GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);
            _guiManager.Resize(ClientSize.X, ClientSize.Y);
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            HandleInput(); // Обрабатываем ввод

            //_camera.ProcessKeyboard(KeyboardState, (float)args.Time);
            _player.ProcessKeyboard(KeyboardState, (float)args.Time);
            _player.Update((float)args.Time);
            _chunkManager.Update(_player.Position);
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            if (CursorState == CursorState.Grabbed)
            {
                _player.ProcessMouseMovement(e.DeltaX, e.DeltaY);
            }
        }

 

        protected override void OnUnload()
        {
            // Освобождаем ресурсы
            //_shader.Dispose();
            //_chunk.Dispose();

            base.OnUnload();
        }

        private bool _wireframeEnabled = false;
        private void HandleInput()
        {
            if (KeyboardState.IsKeyPressed(Keys.F3))
            {
                _debugWindow.IsVisible = !_debugWindow.IsVisible;
            }

            if (KeyboardState.IsKeyPressed(Keys.F4))
            {
                _wireframeEnabled = !_wireframeEnabled;
                GL.PolygonMode(MaterialFace.FrontAndBack, _wireframeEnabled ? PolygonMode.Line : PolygonMode.Fill);
            }

            if (KeyboardState.IsKeyPressed(Keys.Escape))
            {
                CursorState = CursorState == CursorState.Grabbed ? CursorState.Normal : CursorState.Grabbed;
            }
        }

    }
}