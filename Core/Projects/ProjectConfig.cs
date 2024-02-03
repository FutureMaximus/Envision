using OpenTK.Mathematics;

namespace Envision.Core.Projects;

/// <summary> The internal data for a scene. </summary>
public struct ProjectConfig
{
    public string Name;

    public ProjectConfig(string name, Vector3 cameraPosition)
    {
        Name = name;
    }
}
