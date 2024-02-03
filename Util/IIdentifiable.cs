namespace Envision.Util;

/// <summary>
/// Interface for objects that have an ID.
/// Utilized extensively in the project system for draggable items.
/// It is recommended to use the ID when overriding the Equals method.
/// </summary>
public interface IIdentifiable
{
    public Guid ID { get; }
}
