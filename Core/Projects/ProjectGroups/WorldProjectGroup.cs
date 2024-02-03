namespace Envision.Core.Projects.ProjectGroups;

/// <summary>
/// World project group contains a list of 3D representable objects such as models, lights, and effects.
/// </summary>
public sealed class WorldProjectGroup : ProjectGroup
{
    public List<ProjectWorld> ProjectWorlds = new();

    public override string Name { get => _name; set => _name = value; }
    private string _name = "World";

    public WorldProjectGroup(ProjectGroup? parent = null) : base(parent)
    {

    }
}
