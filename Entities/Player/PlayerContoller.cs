using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;

namespace CubeWay.Entities.Player
{
    public class PlayerController
    {
        private Player player;

        public PlayerController(Player _player)
        {
            player = _player;
        }

        public void ProcessKeyboard(KeyboardState input, float deltaTime)
        {
            float velocity = player.Data.Speed * deltaTime;
            if (input.IsKeyDown(Keys.LeftShift)) velocity *= 5.0f; // Ускорение

            player.Data.Front.X = MathF.Cos(MathHelper.DegreesToRadians(player.Rotation.Y));
            player.Data.Front.Z = MathF.Sin(MathHelper.DegreesToRadians(player.Rotation.Y));
            player.Data.Right = Vector3.Normalize(Vector3.Cross(player.Data.Front, Vector3.UnitY));


            if (input.IsKeyDown(Keys.W)) player.Position += player.Data.Front * velocity;
            if (input.IsKeyDown(Keys.S)) player.Position -= player.Data.Front * velocity;
            if (input.IsKeyDown(Keys.A)) player.Position -= player.Data.Right * velocity;
            if (input.IsKeyDown(Keys.D)) player.Position += player.Data.Right * velocity;
            if (input.IsKeyDown(Keys.Space)) player.Position += Vector3.UnitY * velocity;
            if (input.IsKeyDown(Keys.LeftControl)) player.Position -= Vector3.UnitY * velocity;
        }
    }
}
