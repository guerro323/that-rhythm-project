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
            foreach (var entity in entities)
            {
                ref readonly var engineState = ref cmd.ReadRhythmEngineState(entity.rhythmRelative);
                ref readonly var engineSettings = ref cmd.ReadRhythmEngineSettings(entity.rhythmRelative);
                ref readonly var executingCommand = ref cmd.ReadRhythmEngineExecutingCommand(entity.rhythmRelative);
                ref readonly var comboState = ref cmd.ReadGameComboState(entity.rhythmRelative);
                ref readonly var comboSettings = ref cmd.ReadGameComboSettings(entity.rhythmRelative);
                ref readonly var commandState = ref cmd.ReadGameCommandState(entity.rhythmRelative);

                UpdateCombo(comboState, commandState, executingCommand, ref entity.active);

                {
                    var previousCommand = default(UEntityHandle);
                    var offset = 1;
                    if (executingCommand.ActivationBeatStart >
                        RhythmUtility.GetActivationBeat(engineState, engineSettings))
                    {
                        offset += 1;
                    }

                    var cmdIdx = entity.active.Combo.GetLength() - 1 - offset;
                    if (cmdIdx >= 0 && entity.active.Combo.GetLength() >= cmdIdx + 1)
                        previousCommand = entity.active.Combo.Span[cmdIdx];

                    foreach (var abilityHandle in entity.abilityBuffer)
                    {
                        cmd.UpdateAbilityRhythmEngineSet(abilityHandle) = new AbilityRhythmEngineSet
                        {
                            Engine = entity.rhythmRelative,
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