using OpenTK.Mathematics;

namespace CubeWay.Entities.Base
{
    public abstract class Entity
    {
        public Vector3 Position = Vector3.Zero;
        public Vector3 Rotation = Vector3.Zero;
        public Vector3 Velocity = Vector3.Zero;
        public bool IsActive { get; set; } = true;

        public Entity(Vector3 position)
        {
            Position = position;
            Velocity = Vector3.Zero;
        }

        public virtual void Update(float deltaTime)
        {
            Position += Velocity * deltaTime;
        }
    }
}
