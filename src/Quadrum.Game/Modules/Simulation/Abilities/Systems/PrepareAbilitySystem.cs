using System;
using DefaultEcs;
using GameHost.Native.Fixed;
using Quadrum.Game.Modules.Simulation.Abilities.Components;
using Quadrum.Game.Modules.Simulation.Application;
using Quadrum.Game.Modules.Simulation.Common.Systems;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Components;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Utility;
using Quadrum.Game.Utilities;
using revecs;
using revecs.Core;
using revecs.Systems.Generator;
using revghost;

namespace Quadrum.Game.Modules.Simulation.Abilities.Systems;

public partial class PrepareAbilitySystem : SimulationSystem
{
    public PrepareAbilitySystem(Scope scope) : base(scope)
    {
        SubscribeTo<ISimulationUpdateLoopSubscriber>(
            OnUpdate,
            b => b
                .SetGroup<AbilitySystemGroup>()
                // Update before all conditions were updated
                .BeforeGroup<AbilityConditionSystemGroup>()
                // Update before abilities are executed
                .BeforeGroup<AbilityExecutionSystemGroup>()
        );
    }

    private Commands _cmd;
    private OwnerQuery _ownerQuery;

    protected override void OnInit()
    {
        _cmd = new Commands(Simulation);
        _ownerQuery = new OwnerQuery(Simulation);
    }

    private void OnUpdate(Entity _)
    {
        _ownerQuery.QueueAndComplete(Runner, (state, entities) =>
        {
            var cmd = state.Data;

            /*var rhythmRelativeAccessor = state.World.AccessEntityComponent(RhythmEngineDescription.Relative.Type.GetOrCreate(state.World));
            var activeAccessor = state.World.AccessSparseSet(OwnerActiveAbility.Type.GetOrCreate(state.World));
            var abilityBufferAccessor = state.World.AccessEntityComponent(AbilityOwnerDescription.Type.GetOrCreate(state.World));*/
            //while (entities._enumerator.MoveNext())
            foreach (var entity in entities)
            {
                /*var entity = entities._enumerator.Current;
                ref readonly var rhythmRelative = ref rhythmRelativeAccessor[entity][0];
                ref var active = ref activeAccessor[entity];
                var abilityBuffer = abilityBufferAccessor[entity];*/
                var rhythmRelative = entity.rhythmRelative;
                ref var active = ref entity.active;
                var abilityBuffer = entity.abilityBuffer;

                ref readonly var engineState = ref cmd.ReadRhythmEngineState(rhythmRelative);
                ref readonly var engineSettings = ref cmd.ReadRhythmEngineSettings(rhythmRelative);
                ref readonly var executingCommand = ref cmd.ReadRhythmEngineExecutingCommand(rhythmRelative);
                ref readonly var comboState = ref cmd.ReadGameComboState(rhythmRelative);
                ref readonly var comboSettings = ref cmd.ReadGameComboSettings(rhythmRelative);
                ref readonly var commandState = ref cmd.ReadGameCommandState(rhythmRelative);

                UpdateCombo(comboState, commandState, executingCommand, ref active);

                {
                    var previousCommand = default(UEntityHandle);
                    var offset = 1;
                    if (executingCommand.ActivationBeatStart >
                        RhythmUtility.GetActivationBeat(engineState, engineSettings))
                    {
                        offset += 1;
                    }

                    var cmdIdx = active.Combo.GetLength() - 1 - offset;
                    if (cmdIdx >= 0 && active.Combo.GetLength() >= cmdIdx + 1)
                        previousCommand = active.Combo.Span[cmdIdx];

                    foreach (var abilityHandle in abilityBuffer)
                    {
                        cmd.UpdateAbilityRhythmEngineSet(abilityHandle) = new AbilityRhythmEngineSet
                        {
                            Engine = rhythmRelative,
                            State = engineState,
                            Settings = engineSettings,
                            Executing = executingCommand,
                            ComboState = comboState,
                            ComboSettings = comboSettings,
                            CommandState = commandState,
                            Command = executingCommand.CommandTarget.Handle,
                            PreviousCommand = previousCommand
                        };
                    }
                }
            }
        }, _cmd);
    }

    private static void UpdateCombo(in GameComboState gameCombo, in GameCommandState commandState,
        in RhythmEngineExecutingCommand executingCommand,
        ref OwnerActiveAbility activeSelf)
    {
        if (gameCombo.Count <= 0)
        {
            activeSelf.LastActivationTime = -1;
            activeSelf.Combo.Clear();
            return;
        }

        if (activeSelf.LastCommandActiveTime == commandState.StartTimeMs
            || executingCommand.CommandTarget.Equals(default))
            return;

        activeSelf.LastCommandActiveTime = commandState.StartTimeMs;
        activeSelf.LastActivationTime = -1;
        activeSelf.AddCombo(executingCommand.CommandTarget.Handle);
    }
    
    private partial record struct OwnerQuery : IQuery<(
        Read<AbilityOwnerDescription> abilityBuffer,
        Write<OwnerActiveAbility> active,
        
        Read<RhythmEngineDescription.Relative> rhythmRelative)>;

    private partial record struct Commands :
        RhythmEngineState.Cmd.IRead,
        RhythmEngineSettings.Cmd.IRead,
        RhythmEngineExecutingCommand.Cmd.IRead,
        GameComboState.Cmd.IRead,
        GameComboSettings.Cmd.IRead,
        GameCommandState.Cmd.IRead,
        
        AbilityRhythmEngineSet.Cmd.IAdmin;
}