using CubeWay.Entities.Base;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using CubeWay.Engine.Graphics;

namespace CubeWay.Entities.Player
{
    public class Player : Character
    {
        public PlayerData Data { get; private set; }
        public PlayerController playerController;

        Game game;
        public Camera camera;

        private VoxelMesh headMesh = new VoxelMesh("Assets/head.cub");
        private VoxelMesh bodyMesh = new VoxelMesh("Assets/body.cub");
        private VoxelMesh footMesh = new VoxelMesh("Assets/foot.cub");
        private VoxelMesh handMesh = new VoxelMesh("Assets/hand.cub");


        public Player(Vector3 position, Game _game) : base(position) {
            Data = new PlayerData();
            game = _game;
            camera = new Camera(position, _game.Size.X / (float)_game.Size.Y);
            playerController = new PlayerController(this);
        }

        public override void TakeDamage(int amount)
        {
            Data.Health -= amount;
            if (Data.Health <= 0)
            {
                Die();
            }
        }

        public void Die()
        {
            // Логика смерти игрока
        }

        public Vector3 headOffset = new Vector3(0, -1, 1.1f);
        public Vector3 bodyOffset = new Vector3(0, -1.5f, 1);

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
            camera._position = Position;// + -Data.Front;
            camera._yaw = Rotation.Y;

            headMesh.rotation = Rotation + new Vector3(-90, 90, 0);
            headMesh.position = Position + headOffset;

            bodyMesh.rotation = Rotation + new Vector3(-90, 90, 0);
            bodyMesh.position = Position + bodyOffset;
        }

        public void Render(Shader shader)
        {
            headMesh.Render(shader);
            bodyMesh.Render(shader);
        }

        public void ProcessKeyboard(KeyboardState input, float deltaTime)
        {
            playerController.ProcessKeyboard(input, deltaTime);
            if (input.IsKeyPressed(Keys.Right)) { headOffset.X += 0.0625f; }
            if (input.IsKeyPressed(Keys.Left)) { headOffset.X -= 0.0625f; }
            if (input.IsKeyPressed(Keys.Up)) { headOffset.Z += 0.0625f; }
            if (input.IsKeyPressed(Keys.Down)) { headOffset.Z -= 0.0625f; }

        }

        public void ProcessMouseMovement(float xOffset, float yOffset)
        {
            xOffset *= camera._mouseSensitivity;
            Rotation.Y += xOffset;

            yOffset *= camera._mouseSensitivity;
            camera._pitch -= yOffset;
            camera._pitch = MathHelper.Clamp(camera._pitch, -89f, 89f);

            camera.ProcessMouseMovement(xOffset, yOffset);
        }
    }
}
