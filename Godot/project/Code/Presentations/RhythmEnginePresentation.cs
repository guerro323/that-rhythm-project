using Collections.Pooled;
using Godot;
using PataNext.Export.Godot.Presentation;
using PataNext.Game.Client.Core.Inputs;
using Quadrum.Game.Modules.Simulation;
using Quadrum.Game.Modules.Simulation.Application;
using Quadrum.Game.Modules.Simulation.Players;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Components;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Utility;
using revecs.Core;
using revghost;
using revtask.Core;

namespace QuadrumPrototype.Code.Presentations;

public class RhythmEnginePresentation : PresentationGodotBaseSystem
{
    private const string Path = "res://scenes/rhythm_engine_interface.tscn";

    private GameTimeQuery _gameTimeQuery;

    public RhythmEnginePresentation(Scope scope) : base(scope)
    {
        ResourceLoader.LoadThreadedRequest(Path);
    }

    protected override void GetMatchedComponents(PooledList<ComponentType> all, PooledList<ComponentType> or, PooledList<ComponentType> none)
    {
        _gameTimeQuery = new GameTimeQuery(GameWorld);
        
        all.Add(RhythmEngineSettings.Type.GetOrCreate(GameWorld));
        all.Add(RhythmEngineState.Type.GetOrCreate(GameWorld));
        all.Add(RhythmEngineExecutingCommand.Type.GetOrCreate(GameWorld));
    }

    protected override bool EntityMatch(in UEntityHandle entity)
    {
        return true;
    }

    protected override bool OnSetPresentation(in UEntitySafe entity, out JobRequest job)
    {
        job = NewInstantiateJob(entity, (PackedScene) ResourceLoader.LoadThreadedGet(Path));
        Console.WriteLine("YES");
        return true;
    }

    protected override bool OnRemovePresentation(in UEntitySafe entity, in Node node)
    {
        return true;
    }

    private GameTime _previousGameTime;

    protected override void OnPresentationLoop()
    {
        base.OnPresentationLoop();

        var gameTime = _gameTimeQuery.First().GameTime;
        var dt = (float) gameTime.Delta.TotalSeconds;

        var executingCommandAccessor = GameWorld.AccessSparseSet(RhythmEngineExecutingCommand.Type.GetOrCreate(GameWorld));
        var stateAccessor = GameWorld.AccessSparseSet(RhythmEngineState.Type.GetOrCreate(GameWorld));
        var settingsAccessor = GameWorld.AccessSparseSet(RhythmEngineSettings.Type.GetOrCreate(GameWorld));
        
        foreach (var entity in QueryWithPresentation)
        {
            if (!TryGetNode(entity, out var node))
                continue;

            var executingCommand = executingCommandAccessor[entity];
            var state = stateAccessor[entity];
            var settings = settingsAccessor[entity];
            
            var eoc = executingCommand.ActivationBeatEnd - state.CurrentBeat;
            node.CallDeferred("play_metronome", state.CurrentBeat, eoc);

            var player = GameWorld.GetPlayerDescriptionRelative(entity);
            if (!GameWorld.HasGameRhythmInput(player))
                continue;

            var input = GameWorld.GetGameRhythmInput(player);
            for (var i = 0; i < input.Actions.Length; i++)
            {
                var action = input.Actions[i];
                if (!action.InterFrame.IsPressed((gameTime with {Frame = gameTime.Frame - 1}).FrameRange))
                    continue;
                    
                var score = Math.Abs(RhythmUtility.GetScore(state, settings));

                node.CallDeferred("play_drum", i, score);
            }

            var comboState = GameWorld.GetGameComboState(entity);
            var powerState = GameWorld.GetPowerGaugeState(entity);
            
            var comboLabel = (Label) node.GetNode("%ComboLabel");
            comboLabel.Text = comboState.Count.ToString();

            var feverGauge = node.GetNode("%FeverGauge");
            {
                // Lerp
                var prevProgress = feverGauge.Get("progress").AsDouble();
                var lerped = Mathf.Lerp(prevProgress, comboState.Score, dt * 2f);
                lerped = Mathf.MoveToward(lerped, comboState.Score, dt);
                feverGauge.Set("progress", lerped);
            }
            
            var powerLabel = node.GetNode("%PowerLabel");
            {
                var prevLevel = powerLabel.Get("power_level").AsInt32();
                var prevProgress = powerLabel.Get("power_progress").AsDouble();

                var target = ((float) powerState.Tick) / powerState.MaxTick;
                
                var lerped = Mathf.Lerp(prevProgress, target, dt);
                lerped = Mathf.MoveToward(lerped, target, dt * 0.5f);

                if (prevLevel != powerState.Level)
                    lerped = target;
                
                powerLabel.Set("power_level", powerState.Level);
                powerLabel.Set("power_progress", lerped);
                powerLabel.Set("power_max_level", powerState.MaxLevel);
            }
        }

        _previousGameTime = gameTime;
    }
}