using CubeWay.Entities.Base;
using OpenTK.Mathematics;

namespace CubeWay.Entities.Base
{
    public class Character : Entity
    {
        public Vector3 Position { get; set; }
        public Vector3 Velocity { get; set; }
        public float Speed { get; set; } = 5.0f;
        public bool IsGrounded { get; set; } = false;

        public Character(Vector3 position) : base(position) { }


        public virtual void TakeDamage(int damage)
        {
            // Базовая логика получения урона
        }

        public virtual void Update(float deltaTime)
        {
            // Простая гравитация
            if (!IsGrounded)
            {
                //SetVelocity(new Vector3(Velocity.X, Velocity.Y - 9.81f * deltaTime, Velocity.Z));
            }

            // Обновляем позицию
            Position += Velocity * deltaTime;
        }

        public void SetVelocity(Vector3 newVelocity)
        {
            Velocity = newVelocity;
        }

    }
}
