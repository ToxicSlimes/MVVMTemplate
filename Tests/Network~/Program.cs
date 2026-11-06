using System;
using Reflex.Core;

// Diagnostic probe: same factory capture pattern as FusionNetworkInstaller.
sealed class CapturingSingleton
{
    public readonly Container Container;
    public CapturingSingleton(Container container) => Container = container;
}

static class Program
{
    static void Require(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }

    static void Main()
    {
        var root = new ContainerBuilder().AddSingleton(c => new CapturingSingleton(c)).Build();
        var scene = root.Scope();
        var singleton = scene.Resolve<CapturingSingleton>();
        Require(ReferenceEquals(singleton.Container, scene), "Factory did not capture scene container");
        scene.Dispose();
        Require(ReferenceEquals(singleton, root.Resolve<CapturingSingleton>()), "Singleton did not survive scene disposal");
        Require(!singleton.Container.HasBinding<CapturingSingleton>(), "Captured scene container was not cleared");
        root.Dispose();
        Console.WriteLine("PASS: singleton retains disposed child container after scene unload (3 assertions)");
    }
}
