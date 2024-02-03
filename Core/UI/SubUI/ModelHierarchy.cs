using Envision.Core.Editor;
using Envision.Core.Projects;
using Envision.Graphics.Models.Generic;
using Envision.Graphics.Render;
using Envision.Graphics.Render.RenderPasses.SubPasses;
using Envision.Util;
using ImGuiNET;
using System.Numerics;

namespace Envision.Core.UI.SubUI;

public static class ModelHierarchy
{
    public static readonly List<DraggableComponentData> DraggableComponentDatas = new();
    public static readonly HashSet<object> OutlinedObjects = new();

    public static void Show(ref Engine engine)
    {
        /*if (ProjectManager.ActiveProject is  && modelEditor.Model is GenericModel model)
        {
            // Check if draggable component data exists for each part and mesh and if not, add it.
            foreach (GenericModelPart part in model.Parts)
            {
                DraggableComponentData? data = DraggableComponentDatas.FirstOrDefault(x => x.ID == part.ID);
                if (data.Value.ID != part.ID || DraggableComponentDatas.Count == 0)
                {
                    DraggableComponentData newData = new(part.ID);
                    DraggableComponentDatas.Add(newData);
                }
                foreach (GenericMesh mesh in part.Meshes)
                {
                    DraggableComponentData? meshData = DraggableComponentDatas.FirstOrDefault(x => x.ID == mesh.ID);
                    if (meshData.Value.ID != mesh.ID)
                    {
                        DraggableComponentData newData = new(mesh.ID)
                        {
                            IsChild = true
                        };
                        DraggableComponentDatas.Add(newData);
                    }
                }
            }

            ImGui.Begin(model.Name);
            ImGui.SetWindowFontScale(1.3f);
            foreach (DraggableComponentData draggable in DraggableComponentDatas.ToList())
            {
                if (draggable.IsChild) continue;
                ImGui.Separator();
                ShowChildHierarchy(draggable, ref model.Parts);
                ImGui.Separator();

                // Parent space for model parts.
                ImGui.InvisibleButton("Hierarchy Parent Space", new Vector2(ImGui.GetWindowSize().X, 10));
                ImGuiDragDropFlags flags = ImGuiDragDropFlags.SourceNoHoldToOpenOthers | ImGuiDragDropFlags.SourceNoPreviewTooltip;
                if (ImGui.BeginDragDropSource(flags))
                {
                    DraggableComponentOp op = new(draggable.ID, null);
                    UIUtil.SetDragAndDropPayload("Model Hierarchy", op);
                    ImGui.EndDragDropSource();
                }
                if (ImGui.BeginDragDropTarget())
                {
                    DraggableComponentOp? componentOp = UIUtil.AcceptPayLoad<DraggableComponentOp>("Model Hierarchy");
                    if (componentOp is DraggableComponentOp op)
                    {
                        GenericModelPart? dropSourcePart = model.Parts.FirstOrDefault(x => x.ID == op.ID);
                        if (dropSourcePart is not null)
                        {
                            dropSourcePart.Parent?.RemoveChild(dropSourcePart);
                            dropSourcePart.Parent = null;
                            DraggableComponentData? dropSourceData = DraggableComponentDatas.ToList().FirstOrDefault(x => x.ID == op.ID);
                            if (dropSourceData is DraggableComponentData data)
                            {
                                // TODO: Set at index.
                                data.IsChild = false;
                                DraggableComponentDatas.Remove(data);
                                DraggableComponentDatas.Add(data);
                            }
                        }
                    }
                }
            }
            ImGui.End();
        }
        else
        {
            DraggableComponentDatas.Clear();
        }*/
    }

    private static void ShowChildHierarchy(DraggableComponentData draggable, ref List<GenericModelPart> parts)
    {
        /*bool collapsed = true;
        GenericModelPart? targetPart = parts.FirstOrDefault(x => x.ID == draggable.ID);
        GenericMesh? mesh = parts.SelectMany(x => x.Meshes).FirstOrDefault(x => x.ID == draggable.ID);
        bool collapsedHeader;
        if (targetPart is not null)
        {
            collapsedHeader = ImGui.CollapsingHeader(targetPart.Name);
            OutlinedObjects.Add(targetPart);
        }
        else if (mesh is not null)
        {
            ImGui.PushStyleColor(ImGuiCol.Header, new Vector4(0.15f, 0.15f, 0.15f, 1f));
            collapsedHeader = ImGui.CollapsingHeader(mesh.Name);
            ImGui.PopStyleColor();
        }
        else
        {
            collapsedHeader = ImGui.CollapsingHeader(draggable.ID.ToString());
        }
        ImGuiDragDropFlags flags = ImGuiDragDropFlags.SourceNoHoldToOpenOthers | ImGuiDragDropFlags.SourceNoPreviewTooltip;
        if (ImGui.BeginDragDropSource(flags))
        {
            if (targetPart is not null)
            {
                Guid? parentID = targetPart.Parent?.ID;
                DraggableComponentOp op = new(draggable.ID, parentID);
                ImGuiCond condition = ImGui.IsMouseDragging(ImGuiMouseButton.Left) ? ImGuiCond.Once : ImGuiCond.Always;
                UIUtil.SetDragAndDropPayload("Model Hierarchy", op, condition);
            }
            else if (mesh is not null)
            {
                Guid? parentID = mesh.ParentPart.ID;
                DraggableComponentOp op = new(draggable.ID, parentID);
                ImGuiCond condition = ImGui.IsMouseDragging(ImGuiMouseButton.Left) ? ImGuiCond.Once : ImGuiCond.Always;
                UIUtil.SetDragAndDropPayload("Model Hierarchy", op, condition);
            }
            ImGui.EndDragDropSource();
        }
        if (ImGui.BeginDragDropTarget())
        {
            DraggableComponentOp? componentOp = UIUtil.AcceptPayLoad<DraggableComponentOp>("Model Hierarchy");
            if (componentOp is DraggableComponentOp op)
            {
                GenericModelPart? dropSourcePart = parts.FirstOrDefault(x => x.ID == op.ID);
                GenericMesh? dropSourceMesh = parts.SelectMany(x => x.Meshes).FirstOrDefault(x => x.ID == op.ID);
                // Add the part to the target part.
                if (dropSourcePart is not null && targetPart is not null)
                {
                    dropSourcePart.Parent?.RemoveChild(dropSourcePart);
                    dropSourcePart.Parent = targetPart;
                    targetPart.AddChild(dropSourcePart);
                    DraggableComponentData? dropSourceData = DraggableComponentDatas.ToList().FirstOrDefault(x => x.ID == op.ID);
                    if (dropSourceData is DraggableComponentData data)
                    {
                        data.IsChild = true;
                        DraggableComponentDatas.Remove(data);
                        DraggableComponentDatas.Add(data);
                    }
                }
                // Add the mesh to the target part.
                else if (dropSourceMesh is not null && targetPart is not null)
                {
                    GenericModelPart parentPart = dropSourceMesh.ParentPart;
                    parentPart.Meshes.Remove(dropSourceMesh);
                    dropSourceMesh.ParentPart = targetPart;
                    if (!targetPart.Meshes.Contains(dropSourceMesh))
                    {
                        targetPart.Meshes.Add(dropSourceMesh);
                    }
                }
            }
            ImGui.EndDragDropTarget();
        }
        // Modification Area
        if (collapsedHeader)
        {
            ImGui.Indent();
            if (targetPart is not null)
            {
                ImGui.Text("Transformation");
                if (ImGui.BeginChild(targetPart.Name, new Vector2(ImGui.GetWindowSize().X / 2, ImGui.GetWindowSize().Y / 8), true))
                {
                    ImGui.PushStyleColor(ImGuiCol.ChildBg, new Vector4(0.15f, 0.15f, 0.15f, 1.0f));
                    Vector3 position = new(targetPart.Position.X, targetPart.Position.Y, targetPart.Position.Z);
                    ImGui.DragFloat3("Position", ref position, 0.01f);
                    targetPart.Position = new(position.X, position.Y, position.Z);
                    Vector3 rotation = new(targetPart.RotationEuler.X, targetPart.RotationEuler.Y, targetPart.RotationEuler.Z);
                    ImGui.DragFloat3("Rotation", ref rotation, 0.05f);
                    targetPart.RotationEuler = new(rotation.X, rotation.Y, rotation.Z);
                    Vector3 scale = new(targetPart.Scale.X, targetPart.Scale.Y, targetPart.Scale.Z);
                    ImGui.DragFloat3("Scale", ref scale, 0.01f);
                    targetPart.Scale = new(scale.X, scale.Y, scale.Z);
                    ImGui.EndChild();
                }
            }
            else if (mesh is not null)
            {
                if (ImGui.BeginChild(mesh.Name, new Vector2(ImGui.GetWindowSize().X / 2, ImGui.GetWindowSize().Y / 6), true))
                {
                    ImGui.PushStyleColor(ImGuiCol.ChildBg, new Vector4(0.15f, 0.15f, 0.15f, 1.0f));
                    ImGui.BulletText($"Vertices {mesh.VerticesLength}");
                    ImGui.BulletText($"Indices {mesh.IndicesLength}");
                    ImGui.BulletText($"Normals {mesh.NormalsLength}");
                    ImGui.BulletText($"UVs {mesh.TextureCoordsLength}");
                    ImGui.Text("Transformation");
                    ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.1f, 0.1f, 0.1f, 1.0f));
                    Vector3 position = new(mesh.Position.X, mesh.Position.Y, mesh.Position.Z);
                    ImGui.DragFloat3("Position", ref position, 0.01f);
                    mesh.Position = new(position.X, position.Y, position.Z);
                    Vector3 rotation = new(mesh.RotationEuler.X, mesh.RotationEuler.Y, mesh.RotationEuler.Z);
                    ImGui.DragFloat3("Rotation", ref rotation, 0.05f);
                    mesh.RotationEuler = new(rotation.X, rotation.Y, rotation.Z);
                    Vector3 scale = new(mesh.Scale.X, mesh.Scale.Y, mesh.Scale.Z);
                    ImGui.DragFloat3("Scale", ref scale, 0.01f);
                    mesh.Scale = new(scale.X, scale.Y, scale.Z);
                    mesh.UpdateMesh(false);
                    ImGui.EndChild();
                }
            }
            ImGui.Unindent();
            collapsed = false;
        }
        if (!collapsed)
        {
            ImGui.Indent();
            if (targetPart is not null)
            {
                foreach (GenericModelPart childPart in targetPart.GetChildren())
                {
                    DraggableComponentData? data = DraggableComponentDatas.FirstOrDefault(x => x.ID == childPart.ID);
                    if (data is DraggableComponentData draggableData)
                    {
                        ShowChildHierarchy(draggableData, ref parts);
                    }
                }
                ImGui.Text("Meshes");
                foreach (GenericMesh genericMesh in targetPart.Meshes)
                {
                    DraggableComponentData? data = DraggableComponentDatas.FirstOrDefault(x => x.ID == genericMesh.ID);
                    if (data is DraggableComponentData draggableData)
                    {
                        ShowChildHierarchy(draggableData, ref parts);
                    }
                }
            }
            ImGui.Unindent();
        }*/
    }
}
