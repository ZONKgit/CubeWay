using OpenTK.Mathematics;

namespace CubeWay.Entities.Player
{
    public class PlayerData
    {
        public int Health { get; set; }
        public int MaxHealth { get; set; } = 100;
        public float Speed { get; set; } = 5.0f;
        public Vector3 Front = new Vector3();
        public Vector3 Right = new Vector3();

        public PlayerData()
        {
            Health = MaxHealth;
        }
    }
}
