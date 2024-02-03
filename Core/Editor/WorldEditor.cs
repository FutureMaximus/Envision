using Envision.Graphics.Models;
using Envision.Graphics.Shaders;
using Envision.Util;
using System.Collections.Concurrent;

namespace Envision.Core.Editor;

public class WorldEditor
{
    public ConcurrentDictionary<IModel, byte> Models = new();

    public WorldEditor()
    {

    }

    public IModel? GetModel(string otherName)
    {
        foreach (IModel model in Models.Keys)
        {
            if (model.Name == otherName)
            {
                return model;
            }
        }
        return null;
    }

    public void AddModel(IModel model) => Models.TryAdd(model, 0);

    /// <summary>
    /// Removes the model from the engine and disposes it. 
    /// Optionally, you can choose to log the removal.
    /// </summary>
    public void RemoveModel(string modelInternalName, bool silent = true)
    {
        foreach (IModel model in Models.Keys)
        {
            if (model.Name == modelInternalName)
            {
                Models.TryRemove(model, out byte _);
                model.Dispose();
                if (!silent)
                {
                    DebugLogger.Log($"<aqua>Removed model <white>{model.Name} <aqua>from the engine.");
                }
                return;
            }
        }
    }
    /// <summary>
    /// Removes the model from the engine and disposes it. 
    /// Optionally, you can choose to log the removal.
    /// </summary>
    public void RemoveModel(IModel model, bool silent = true)
    {
        if (Models.TryRemove(model, out byte _))
        {
            if (!silent)
            {
                DebugLogger.Log($"<aqua>Removed model <white>{model.Name} <aqua>from the engine.");
            }
            model.Dispose();
        }
    }

    /// <summary>
    /// Returns a model that is of the specified IModel type
    /// and has the specified internal name.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="internalName"></param>
    /// <returns>
    /// The model that is of the specified type and has the specified internal name.
    /// </returns>
    public T? GetModelFromType<T>(string internalName) where T : IModel
    {
        foreach (IModel model in Models.Keys)
        {
            if (model.GetType() == typeof(T) && model.Name == internalName)
            {
                return (T)model;
            }
        }
        return default;
    }

    /// <summary>
    /// Returns a list of models that are of the specified IModel type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns>
    /// The list of models that are of the specified type.
    /// </returns>
    public List<T> GetModelsFromType<T>() where T : IModel
    {
        List<T> models = new();
        foreach (IModel model in Models.Keys)
        {
            if (model.GetType() == typeof(T))
            {
                models.Add((T)model);
            }
        }
        return models;
    }
}
