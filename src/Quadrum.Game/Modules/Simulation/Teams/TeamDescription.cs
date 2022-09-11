using revecs.Extensions.RelativeEntity.Generator;

namespace Quadrum.Game.Modules.Simulation.Teams;

public partial record struct TeamDescription : IDescriptionComponent;
public partial record struct TeamHostileDescription : IDescriptionComponent;
public partial record struct TeamFriendlyDescription : IDescriptionComponent;