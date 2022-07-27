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

public partial class UpdateActiveAbilitySystem : SimulationSystem
{
    public UpdateActiveAbilitySystem(Scope scope) : base(scope)
    {
        SubscribeTo<ISimulationUpdateLoopSubscriber>(
            OnUpdate,
            b => b
                .SetGroup<AbilitySystemGroup>()
                // Update after all conditions were updated
                .AfterGroup<AbilityConditionSystemGroup>()
                // Update before abilities are executed
                .BeforeGroup<AbilityExecutionSystemGroup>()
        );
    }

    private Commands _cmd;
    private OwnerQuery _ownerQuery;

    protected override void OnInit()
    {
        _cmd = new Commands(Simulation);
    }

    private void OnUpdate(Entity _)
    {
        foreach (var entity in _ownerQuery)
        {
            ref readonly var engineState = ref _cmd.ReadRhythmEngineState(entity.rhythmRelative);
            ref readonly var engineSettings = ref _cmd.ReadRhythmEngineSettings(entity.rhythmRelative);
            ref readonly var executingCommand = ref _cmd.ReadRhythmEngineExecutingCommand(entity.rhythmRelative);
            ref readonly var comboState = ref _cmd.ReadGameComboState(entity.rhythmRelative);
            ref readonly var comboSettings = ref _cmd.ReadGameComboSettings(entity.rhythmRelative);
            ref readonly var commandState = ref _cmd.ReadGameCommandState(entity.rhythmRelative);

            var isSimulationOwned = true;
            var isRhythmEngineOwned = true;

            var isNewIncomingCommand =
                updateAndCheckNewIncomingCommand(comboState, commandState, executingCommand, ref entity.active);
            var isActivation = entity.active.LastActivationTime == -1
                               && comboState.Count > 0
                               && commandState.StartTime <= engineState.Elapsed;

            entity.active.Incoming = default;

            {
                var previousCommand = default(UEntityHandle);
                var offset = 1;
                if (executingCommand.ActivationBeatStart > RhythmUtility.GetActivationBeat(engineState, engineSettings))
                {
                    offset += 1;
                }

                var cmdIdx = entity.active.Combo.GetLength() - 1 - offset;
                if (cmdIdx >= 0 && entity.active.Combo.GetLength() >= cmdIdx + 1)
                    previousCommand = entity.active.Combo.Span[cmdIdx];

                foreach (var abilityHandle in entity.abilityBuffer)
                {
                    _cmd.RemoveAbilityStateActiveTag(abilityHandle);
                    _cmd.RemoveAbilityStateChainingTag(abilityHandle);
                    _cmd.RemoveAbilityStateBeforeActivationTag(abilityHandle);

                    _cmd.UpdateAbilityRhythmEngineSet(abilityHandle) = new AbilityRhythmEngineSet
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
    }
    
    private static bool updateAndCheckNewIncomingCommand(in GameComboState gameCombo, in GameCommandState commandState,
        in RhythmEngineExecutingCommand executingCommand,
        ref OwnerActiveAbility activeSelf)
    {
        if (gameCombo.Count <= 0)
        {
            activeSelf.LastActivationTime = -1;
            activeSelf.Combo.Clear();
            return false;
        }

        if (activeSelf.LastCommandActiveTime == commandState.StartTimeMs
            || executingCommand.CommandTarget.Equals(default))
            return false;

        activeSelf.LastCommandActiveTime = commandState.StartTimeMs;
        activeSelf.LastActivationTime = -1;
        activeSelf.AddCombo(executingCommand.CommandTarget.Handle);
        return true;

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

        AbilityStateBeforeActivationTag.Cmd.IAdmin,
        AbilityStateActiveTag.Cmd.IAdmin,
        AbilityStateChainingTag.Cmd.IAdmin,

        AbilityRhythmEngineSet.Cmd.IAdmin;
}