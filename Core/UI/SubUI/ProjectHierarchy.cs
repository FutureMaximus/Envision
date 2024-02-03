using Envision.Core.Projects;
using Envision.Core.Projects.ProjectGroups;
using Envision.Graphics.Models;
using Envision.Graphics.Models.Generic;
using Envision.Graphics.Render;
using Envision.Util;
using ImGuiNET;
using System.Numerics;

namespace Envision.Core.UI.SubUI;

public static class ProjectHierarchy
{
    public static readonly Dictionary<DraggableComponentData, IIdentifiable> draggableDatas = new();

    #region Project Group
    public static void Show(ref Engine engine)
    {
        if (ProjectManager.ActiveProject is null) return;
        ImGui.Begin(ProjectManager.ActiveProject.Config.Name);
        ImGui.SetWindowFontScale(1.3f);
        foreach (ProjectGroup projectGroup in ProjectManager.ActiveProject.ProjectGroups)
        {
            SearchForMissingDraggables(projectGroup, false);
            ImGui.Separator();
            ShowProjectGroup(projectGroup, true);
            ImGui.Separator();
        }
        ImGui.End();
    }

    private static void ShowProjectGroup(ProjectGroup projectGroup, bool source)
    {
        bool collapsed = true;
        bool collapsedHeader = ImGui.CollapsingHeader(projectGroup.Name);
        if (collapsedHeader)
        {
            SearchForMissingDraggables(projectGroup);
        }
        ImGuiDragDropFlags flags = ImGuiDragDropFlags.SourceNoHoldToOpenOthers | ImGuiDragDropFlags.SourceNoPreviewTooltip;
        if (ImGui.BeginDragDropSource(flags))
        {
            Guid? parentID = (!source) ? projectGroup.Parent?.ID : null;
            DraggableComponentOp op = new(projectGroup.ID, parentID);
            ImGuiCond condition = ImGui.IsMouseDragging(ImGuiMouseButton.Left) ? ImGuiCond.Once : ImGuiCond.Always;
            UIUtil.SetDragAndDropPayload("Project Hierarchy", op, condition);
            ImGui.EndDragDropSource();
        }
        if (ImGui.BeginDragDropTarget())
        {
            DraggableComponentOp? componentOp = UIUtil.AcceptPayLoad<DraggableComponentOp>("Project Hierarchy");
            if (componentOp is DraggableComponentOp op)
            {
                object? dropData = GetDraggableFromID(op.ID);
                // If the project group is the same type as the drop data, then we can drop it.
                if (dropData != null && dropData.GetType() == projectGroup.GetType())
                {
                    /*if (dropData is ModelProjectGroup modelGroup)
                    {
                        modelGroup.Parent?.RemoveChild(modelGroup);
                        projectGroup.AddChild(modelGroup);
                    }
                    else if (dropData is ProjectGroup group)
                    {
                        group.Parent?.RemoveChild(group);
                        projectGroup.AddChild(group);   
                    }*/
                }
            }
        }
        if (collapsedHeader)
        {
            collapsed = false;
        }
        if (!collapsed)
        {
            ImGui.Indent();
            foreach (ProjectGroup child in projectGroup.Children)
            {
                ShowProjectGroup(child, false);
            }
            if (projectGroup is ModelProjectGroup modelProjectGroup)
            {
                foreach (IModel model in modelProjectGroup.Models)
                {
                    ShowModel(model);
                }
            }
            ImGui.Unindent();
        }
    }

    private static object? GetDraggableFromID(Guid id)
    {
        foreach (DraggableComponentData data in draggableDatas.Keys)
        {
            if (data.ID == id)
            {
                return draggableDatas[data];
            }
        }
        return null;
    }

    private static void SearchForMissingDraggables(ProjectGroup projectGroup, bool isChild = true)
    {
        CheckDraggable(projectGroup, isChild);

        if (projectGroup is ModelProjectGroup modelProjectGroup)
        {
            foreach (IModel model in modelProjectGroup.Models)
            {
                CheckDraggable(model);

                if (model is GenericModel genericModel)
                {
                    foreach (GenericModelPart part in genericModel.Parts)
                    {
                        CheckDraggable(part);
                        foreach (GenericMesh mesh in part.Meshes)
                        {
                            CheckDraggable(mesh);
                        }
                    }
                }
            }
        }
    }

    private static void CheckDraggable(IIdentifiable identifiable, bool isChild = true)
    {
        bool found = false;
        foreach (DraggableComponentData dragKey in draggableDatas.Keys)
        {
            if (dragKey.ID == identifiable.ID)
            {
                found = true;
            }
        }
        if (!found)
        {
            DraggableComponentData data = new(identifiable.ID, isChild);
            draggableDatas.Add(data, identifiable);
        }
    }
    #endregion

    #region Model
    private static void ShowModel(IModel model)
    {
        bool collapsed = true;
        ImGui.PushStyleColor(ImGuiCol.Header, new Vector4(0.25f, 0.25f, 0.25f, 1.0f));
        bool collapsedHeader = ImGui.CollapsingHeader(model.Name);
        ImGuiDragDropFlags flags = ImGuiDragDropFlags.SourceNoHoldToOpenOthers | ImGuiDragDropFlags.SourceNoPreviewTooltip;
        if (ImGui.BeginDragDropSource(flags))
        {
            // TODO: Drag to model project group logic.
            ImGui.EndDragDropSource();
        }
        if (ImGui.BeginDragDropTarget())
        {
            // TODO: Drop to model project group logic.
            ImGui.EndDragDropTarget();
        }
        if (collapsedHeader)
        {
            // TODO: Model modification logic.
            collapsed = false;
        }
        if (!collapsed)
        {
            ImGui.Indent();
            
            ImGui.Unindent();
        }
    }

    private static void ShowModelChildHierarchy(DraggableComponentData draggable, ref List<GenericModelPart> parts)
    {

    }
    #endregion
}
