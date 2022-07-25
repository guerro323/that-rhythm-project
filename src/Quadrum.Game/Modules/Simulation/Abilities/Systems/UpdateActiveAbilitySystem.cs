using GameHost.Native.Fixed;
using Quadrum.Game.Modules.Simulation.Abilities.Components;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Components;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Utility;
using Quadrum.Game.Utilities;
using revecs.Core;
using revecs.Systems.Generator;

namespace Quadrum.Game.Modules.Simulation.Abilities.Systems;

public partial struct UpdateActiveAbilitySystem : IRevolutionSystem,
    RhythmEngineState.Cmd.IRead,
    RhythmEngineSettings.Cmd.IRead,
    RhythmEngineExecutingCommand.Cmd.IRead,
    GameComboState.Cmd.IRead,
    GameComboSettings.Cmd.IRead,
    GameCommandState.Cmd.IRead,
    
    AbilityStateBeforeActivationTag.Cmd.IAdmin,
    AbilityStateActiveTag.Cmd.IAdmin,
    AbilityStateChainingTag.Cmd.IAdmin,
    
    AbilityRhythmEngineSet.Cmd.IAdmin
{
    public void Constraints(in SystemObject sys)
    {
        sys.SetGroup<AbilitySystemGroup>();
        // Update after all conditions were updated
        sys.DependOn<AbilityConditionSystemGroup.End>();
        // Update before abilities are updated
        sys.AddForeignDependency<AbilityExecutionSystemGroup.Begin>();
    }

    public void Body()
    {
        foreach (var entity in RequiredQuery(
                     Read<AbilityOwnerDescription>("abilityBuffer"),
                     Write<OwnerActiveAbility>("active"),

                     Read<RhythmEngineDescription.Relative>("rhythmRelative")
                 ))
        {
            ref readonly var engineState = ref Cmd.ReadRhythmEngineState(entity.rhythmRelative);
            ref readonly var engineSettings = ref Cmd.ReadRhythmEngineSettings(entity.rhythmRelative);
            ref readonly var executingCommand = ref Cmd.ReadRhythmEngineExecutingCommand(entity.rhythmRelative);
            ref readonly var comboState = ref Cmd.ReadGameComboState(entity.rhythmRelative);
            ref readonly var comboSettings = ref Cmd.ReadGameComboSettings(entity.rhythmRelative);
            ref readonly var commandState = ref Cmd.ReadGameCommandState(entity.rhythmRelative);

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
                    Cmd.RemoveAbilityStateActiveTag(abilityHandle);
                    Cmd.RemoveAbilityStateChainingTag(abilityHandle);
                    Cmd.RemoveAbilityStateBeforeActivationTag(abilityHandle);

                    Cmd.UpdateAbilityRhythmEngineSet(abilityHandle) = new AbilityRhythmEngineSet
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
}