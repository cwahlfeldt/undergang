using System;
using Game;
using Godot;
using Microsoft.Extensions.DependencyInjection;

public partial class DependencyInjection : Node
{
    private static IServiceProvider _serviceProvider;

    public override void _Ready()
    {
        var services = new ServiceCollection();
        services.AddSingleton<EntityManager>();
        // Add other systems...

        _serviceProvider = services.BuildServiceProvider();
    }
}