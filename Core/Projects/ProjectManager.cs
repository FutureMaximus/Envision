namespace Envision.Core.Projects;

public static class ProjectManager
{
    public static readonly List<Project> Projects = new();

    public static Project? ActiveProject { get; set; }

    /// <summary>
    /// Removes the current active scene from the scene manager
    /// and sets the active scene to the first scene in the list.
    /// </summary>
    public static void RemoveActiveScene()
    {
        if (ActiveProject != null)
        {
            ActiveProject.Dispose();
            Projects.Remove(ActiveProject);
        }
        ActiveProject = Projects.Count > 0 ? Projects[0] : null;
    }
}
