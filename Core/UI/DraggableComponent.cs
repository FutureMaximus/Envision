namespace Envision.Core.UI;

/// <summary> 
/// Operation data for a draggable component
/// where you iterate through a list of draggable components
/// and determine what occurs if the ID matches the ID of the
/// draggable component. 
/// </summary>
public struct DraggableComponentOp
{
    public Guid? ParentID;
    public Guid ID;
    public DraggableComponentOp(Guid id, Guid? parentID = null)
    {
        ParentID = parentID;
        ID = id;
    }

    public readonly override bool Equals(object? obj)
    {
        if (obj is DraggableComponentOp op)
        {
            return op.ID == ID;
        }
        return false;
    }

    public readonly override int GetHashCode()
    {
        return HashCode.Combine(ID);
    }

    public static bool operator ==(DraggableComponentOp left, DraggableComponentOp right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(DraggableComponentOp left, DraggableComponentOp right)
    {
        return !(left == right);
    }
}

/// <summary>
/// Data for a draggable component that contains a GUID
/// to determine what occurs when the draggable component is dropped.
/// </summary>
/// <typeparam name="T"></typeparam>
public struct DraggableComponentData
{
    public Guid ID;
    public bool IsChild;

    public DraggableComponentData(Guid id, bool isChild = false)
    {
        ID = id;
        IsChild = isChild;
    }

    public readonly override bool Equals(object? obj)
    {
        if (obj is DraggableComponentData data)
        {
            return data.ID == ID;
        }
        return false;
    }

    public readonly override int GetHashCode()
    {
        return HashCode.Combine(ID);
    }

    public static bool operator ==(DraggableComponentData left, DraggableComponentData right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(DraggableComponentData left, DraggableComponentData right)
    {
        return !(left == right);
    }
}
