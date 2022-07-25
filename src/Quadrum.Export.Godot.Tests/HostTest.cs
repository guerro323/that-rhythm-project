using System;
using System.Threading;
using revghost;
using revghost.Module;
using revghost.Utility;
using Xunit;
using Xunit.Abstractions;

namespace Quadrum.Export.Godot.Tests;

public abstract class HostTest
{
    public HostTest(ITestOutputHelper testOutput)
    {
        HostLogger.Output = (level, line, source, theme) =>
        {
            if (string.IsNullOrEmpty(source))
                source = "global";
            if (string.IsNullOrEmpty(theme))
                theme = ":";
            testOutput.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}|{level}|{source}|{theme}: {line}");
        };
    }
    
    public abstract GhostRunner CreateRunner();

    [Fact]
    public void Run()
    {
        var runner = CreateRunner();
        while (runner.Loop() && !runner.HostEntity.World.Has<ExitApplication>())
        {
            Thread.Sleep(16);
        }
        runner.Dispose();
    }

    public struct ExitApplication
    {
    }
}