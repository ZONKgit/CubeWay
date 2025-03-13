using System;
using System.IO;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public class Shader
{
    public int Handle { get; private set; }

    public Shader(string vertexPath, string fragmentPath)
    {
        string vertexCode = File.ReadAllText(vertexPath);
        string fragmentCode = File.ReadAllText(fragmentPath);

        Console.WriteLine("Vertex Shader Code:\n" + vertexCode);
        Console.WriteLine("Fragment Shader Code:\n" + fragmentCode);


        int vertex = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertex, vertexCode);
        GL.CompileShader(vertex);
        CheckShaderErrors(vertex, "VERTEX");

        int fragment = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragment, fragmentCode);
        GL.CompileShader(fragment);
        CheckShaderErrors(fragment, "FRAGMENT");

        Handle = GL.CreateProgram();
        GL.AttachShader(Handle, vertex);
        GL.AttachShader(Handle, fragment);
        GL.LinkProgram(Handle);
        CheckProgramErrors(Handle);

        GL.DetachShader(Handle, vertex);
        GL.DetachShader(Handle, fragment);
        GL.DeleteShader(vertex);
        GL.DeleteShader(fragment);
    }

    public void Use()
    {
        GL.UseProgram(Handle);
    }

    private void CheckShaderErrors(int shader, string type)
    {
        GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
        if (success == 0)
        {
            string log = GL.GetShaderInfoLog(shader);
            Console.WriteLine($"ERROR::SHADER::{type}::COMPILATION_FAILED\n{log}");
        }
    }

    private void CheckProgramErrors(int program)
    {
        GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int success);
        if (success == 0)
        {
            string log = GL.GetProgramInfoLog(program);
            Console.WriteLine($"ERROR::PROGRAM::LINKING_FAILED\n{log}");
        }
    }

    public void SetMatrix4(string name, Matrix4 matrix)
    {
        int location = GL.GetUniformLocation(Handle, name);
        GL.UniformMatrix4(location, false, ref matrix);
    }

    public void SetVector3(string name, Vector3 vector)
    {
        int location = GL.GetUniformLocation(Handle, name);
        GL.Uniform3(location, vector);
    }

}
