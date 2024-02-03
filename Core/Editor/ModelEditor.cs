using Envision.Graphics.Models;
using Envision.Graphics.Render;

namespace Envision.Core.Editor;

public class ModelEditor
{
    /// <summary> The model that is being modified. </summary>
    public IModel Model;

    /// <summary> The engine that owns this editor. </summary>
    public Engine Engine;

    public ModelEditor(IModel model, Engine engine)
    {
        Model = model;
        Engine = engine;
    }
}
