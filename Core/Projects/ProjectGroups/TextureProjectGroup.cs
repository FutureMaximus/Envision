namespace Envision.Core.Projects.ProjectGroups;

public class TextureProjectGroup : ProjectGroup
{
    public override string Name { get => _name; set => _name = value; }
    private string _name = "Textures";
}
