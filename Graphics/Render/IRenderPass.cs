namespace Envision.Graphics.Render;

public interface IRenderPass : IDisposable
{
    void Load();

    void Render();

    bool IsEnabled { get; set; }
}
