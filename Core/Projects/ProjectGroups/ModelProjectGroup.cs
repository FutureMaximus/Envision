using Envision.Core.Projects;
using Envision.Graphics.Models;

namespace Envision.Core.Projects.ProjectGroups;

/// <summary>
/// The model project group contains a list of models that can be placed into a world project group.
/// </summary>
public sealed class ModelProjectGroup : ProjectGroup
{
    public List<IModel> Models;
    public IModel? SelectedModel;

    public override string Name { get => _name; set => _name = value; }
    private string _name = "Models";

    public ModelProjectGroup(List<IModel>? models = null, ProjectGroup? parent = null) : base(parent)
    {
        Models = models ?? new List<IModel>();
    }
}
