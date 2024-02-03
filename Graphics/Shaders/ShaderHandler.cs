namespace Envision.Graphics.Shaders;

public class ShaderHandler : IDisposable
{
    public Dictionary<string, Shader> Shaders = new();
    public string[] ShaderFiles;
    public string ShaderPath;
    
    public ShaderHandler(string shaderPath)
    {
        ShaderPath = shaderPath;
        ShaderFiles = Directory.GetFiles(shaderPath);
    }

    public void AddShader(Shader shader)
    {
        if (ShaderPath is null)
        {
            throw new NullReferenceException("Shader path is not set.");
        }
        if (Shaders.ContainsKey(shader.Name))
        {
            throw new ArgumentNullException($"Shader with name {shader.Name} already exists.");
        }
        Shaders.Add(shader.Name, shader);
    }

    public Shader? GetShader(string name)
    {
        if (ShaderPath is null)
        {
            throw new NullReferenceException("Shader path is not set.");
        }

        if (Shaders.TryGetValue(name, out Shader? shader))
        {
            return shader;
        }

        return null;
    }

    public void Dispose()
    {
        foreach (Shader shader in Shaders.Values)
        {
            shader.Dispose();
        }
        GC.SuppressFinalize(this);
    }
}
