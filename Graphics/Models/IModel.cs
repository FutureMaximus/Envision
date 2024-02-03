using Envision.Util;

namespace Envision.Graphics.Models;

public interface IModel : IIdentifiable, IDisposable
{
    string Name { get; set; }

    void Load();

    void Render();

    void Unload();
}
