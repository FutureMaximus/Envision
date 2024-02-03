using Envision.Core.World;

namespace Envision.Core.Projects.ProjectGroups;

/// <summary> 
/// Project worlds contains 3D representable objects such as models, lights, and effects
/// that can be modified and rendered.
/// </summary>
public class ProjectWorld
{
    public readonly List<WorldEntity> Entities = new();

    public ProjectWorld()
    {

    }

}
