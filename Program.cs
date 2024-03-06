// Envision is a modeling software for the creation of low poly models with high quality graphics.

using Envision;
using Envision.Util;
using OpenTK.Windowing.Common;

ContextFlags flags;
    string debugPath = "C:\\VisualStudioProjects\\Envision\\Util\\DebugData";
    string shaderPath = "C:\\VisualStudioProjects\\Envision\\Graphics\\Shaders\\InternalShaders";
    Config.LoadFromCustomFile(debugPath, shaderPath);
    flags = ContextFlags.Debug | ContextFlags.ForwardCompatible;
    //Config.LoadFromRelative();
    //flags = ContextFlags.ForwardCompatible;
Window window = new((int)Config.Settings.Resolution.X, (int)Config.Settings.Resolution.Y, flags)
{
    UpdateFrequency = 60,
};
window.Run();