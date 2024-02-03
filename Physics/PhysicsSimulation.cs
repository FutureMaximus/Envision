using BepuPhysics;
using BepuUtilities;
using BepuUtilities.Memory;

namespace Envision.Physics;

/// Library by https://github.com/bepu/bepuphysics2
public class PhysicsSimulation : IDisposable
{
    public Simulation Simulation { get; private set; }

    public BufferPool BufferPool { get; private set; }

    public ThreadDispatcher ThreadDispatcher { get; private set; }

    public PhysicsSimulation()
    {
        int targetThreadCount = int.Max(1, Environment.ProcessorCount > 4 ? Environment.ProcessorCount - 2 : Environment.ProcessorCount - 1);
        ThreadDispatcher = new ThreadDispatcher(targetThreadCount);
        BufferPool = new BufferPool();
        Simulation = Simulation.Create(BufferPool, new NarrowPhaseCallbacks(), new IntegratorCallbacks(), new SolveDescription(velocityIterationCount: 1, substepCount: 8));
    }

    public void Dispose()
    {
        Simulation.Dispose();
        BufferPool.Clear();
        ThreadDispatcher.Dispose();
        GC.SuppressFinalize(this);
    }
}
