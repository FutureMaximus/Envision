using Envision.Graphics.Models.Generic;
using Envision.Util;

namespace Envision.Core.Projects;

/// <summary> A group in a project that contains a list of objects. </summary>
public abstract class ProjectGroup : IDisposable, IIdentifiable
{
    public ProjectGroup? Parent;
    public List<ProjectGroup> Children = new();
    public Guid ID => _guid;

    private readonly Guid _guid = Guid.NewGuid();
    private readonly List<IIdentifiable> _projectObjects = new();

    public ProjectGroup(ProjectGroup? parent = null)
    {
        Parent = parent;
    }

    public abstract string Name { get; set; }

    public void AddSceneObject<T>(object sceneObject) where T : class, IIdentifiable
    {
        if (sceneObject is ProjectGroup)
        {
            _projectObjects.Add((IIdentifiable)sceneObject);
        }
        else
        {
            throw new Exception("Invalid scene object type.");
        }
    }

    public void RemoveSceneObject<T>(object sceneObject) where T : class, IIdentifiable
    {
        if (sceneObject is ProjectGroup)
        {
            _projectObjects.Remove((IIdentifiable)sceneObject);
        }
        else
        {
            throw new Exception("Invalid scene object type.");
        }
    }

    public void Dispose()
    {
        foreach (ProjectGroup child in Children)
        {
            child.Dispose();
        }
        GC.SuppressFinalize(this);
    }

    public override string ToString()
    {
        return $"Name: {Name}, ID: {ID}, Parent: {Parent?.Name ?? "None"}, Children: {Children.Count}";
    }

    public override bool Equals(object? obj)
    {
        if (obj is ProjectGroup group)
        {
            return group.ID == ID;
        }
        return false;
    }

    public override int GetHashCode() => HashCode.Combine(ID);
}
