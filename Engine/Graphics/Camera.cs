using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;


public class Camera
{
    public Vector3 _position;
    public Vector3 localPosition = new Vector3(0, 0, 0);
    private Vector3 _front = -Vector3.UnitZ;
    private Vector3 _up = Vector3.UnitY;
    private Vector3 _right;
    private float _aspectRatio;
    public float _yaw = -90f;
    public float _pitch = 0f;
    public float _mouseSensitivity = 0.1f;
    private float _speed = 2.5f;

    public Vector3 Position => _position;

    public Camera(Vector3 localPosition, float aspectRatio)
    {
        this.localPosition = localPosition;
        _aspectRatio = aspectRatio;
        UpdateVectors();
    }

    private void UpdateVectors()
    {
        Vector3 front;
        front.X = MathF.Cos(MathHelper.DegreesToRadians(_yaw)) * MathF.Cos(MathHelper.DegreesToRadians(_pitch));
        front.Y = MathF.Sin(MathHelper.DegreesToRadians(_pitch));
        front.Z = MathF.Sin(MathHelper.DegreesToRadians(_yaw)) * MathF.Cos(MathHelper.DegreesToRadians(_pitch));
        _front = Vector3.Normalize(front);
        _right = Vector3.Normalize(Vector3.Cross(_front, Vector3.UnitY));
        _up = Vector3.Normalize(Vector3.Cross(_right, _front));
    }

    public Matrix4 GetViewMatrix() => Matrix4.LookAt(_position, _position + _front, _up);

    public Matrix4 GetProjectionMatrix() => Matrix4.CreatePerspectiveFieldOfView(
        MathHelper.DegreesToRadians(70f), _aspectRatio, 0.5f, 5000f);

    public void Update()
    {
        _position = localPosition;
    }

    public void ProcessKeyboard(KeyboardState input, float deltaTime)
    {

    }

    public void ProcessMouseMovement(float xOffset, float yOffset)
    {
        UpdateVectors();
    }
}
