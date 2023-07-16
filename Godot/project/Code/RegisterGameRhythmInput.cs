using DefaultEcs;
using Godot;
using PataNext.Game.Client.Core.Inputs;
using Quadrum.Game.Modules.Simulation;
using Quadrum.Game.Modules.Simulation.Application;
using Quadrum.Game.Modules.Simulation.Common.Systems;
using Quadrum.Game.Modules.Simulation.Players;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Systems;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Utility;
using Quadrum.Game.Utilities;
using revecs;
using revghost;
using revghost.Ecs;
using revghost.Injection;
using revghost.Loop.EventSubscriber;

namespace QuadrumPrototype.Client;

public partial class RegisterGameRhythmInput : SimulationSystem
{
    public RegisterGameRhythmInput(Scope scope) : base(scope)
    {
        SubscribeTo<ISimulationUpdateLoopSubscriber>(
            OnUpdate,
            builder => 
                builder.BeforeGroup<RhythmEngineExecutionGroup>()
        );
    }

    private GameTimeQuery _timeQuery;
    private PlayerQuery _players;

    protected override void OnInit()
    {
        _timeQuery = new GameTimeQuery(Simulation);
        _players = new PlayerQuery(Simulation);
    }

    private bool[] _previousInputs = new[] { false, false, false, false };
    
    private void OnUpdate(Entity _)
    {
        var time = _timeQuery.First().GameTime;
        foreach (var player in _players)
        {
            ref var playerInput = ref player.Input;
            for (var i = 0; i < playerInput.Actions.Length; i++)
            {
                var name = (DefaultCommandKeys) (i + 1) switch
                {
                    DefaultCommandKeys.Up => "r_up",
                    DefaultCommandKeys.Down => "r_down",
                    DefaultCommandKeys.Left => "r_left",
                    DefaultCommandKeys.Right => "r_right"
                };

                var isActive = Input.IsActionPressed(name);
                var gdInput = (
                    active: isActive,
                    down: isActive && !_previousInputs[i],
                    up: !isActive && _previousInputs[i]
                );
                
                _previousInputs[i] = isActive;

                ref var action = ref playerInput.Actions[i];

                if (gdInput.active)
                    action.ActiveTime += time.Delta;
                else
                    action.ActiveTime = default;
            
                action.IsSliding = (action.IsSliding && gdInput.up) || (gdInput.active && action.ActiveTime > TimeSpan.FromMilliseconds(350));

                if (gdInput.down)
                {
                    action.InterFrame.Pressed = time.Frame;
                }

                if (gdInput.up)
                    action.InterFrame.Released = time.Frame;
                
            }
        }
    }
    
    private partial struct PlayerQuery : IQuery<(All<PlayerDescription>, Write<GameRhythmInput> Input)> {}
}