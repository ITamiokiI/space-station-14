using Content.Server.Atmos.EntitySystems;
using Content.Shared.Atmos;
using JetBrains.Annotations;

namespace Content.Server.Atmos.Reactions;

[UsedImplicitly]
public sealed class ProtoNitrateHydrogenConversionReaction : IGasReactionEffect
{
    public ReactionResult React(GasMixture mixture, IGasMixtureHolder? holder, AtmosphereSystem atmosphereSystem)
    {
        var initialHyperNoblium = mixture.GetMoles(Gas.HyperNoblium);
        if (initialHyperNoblium >= 5.0f && mixture.Temperature > 20f)
            return ReactionResult.NoReaction;

        var initialProtoNitrate = mixture.GetMoles(Gas.ProtoNitrate);
        var initialHydrogen = mixture.GetMoles(Gas.Hydrogen);

        var producedAmount = Math.Min(Atmospherics.ProtoNitrateHydrogenConversionMaxRate, Math.Min(initialHydrogen, initialProtoNitrate));

        if (producedAmount <= 0 || initialHydrogen-producedAmount < 0f)
            return ReactionResult.NoReaction;

        var oldHeatCapacity = atmosphereSystem.GetHeatCapacity(mixture);

        mixture.AdjustMoles(Gas.Hydrogen, -producedAmount);
        mixture.AdjustMoles(Gas.ProtoNitrate, producedAmount*0.5f);

        var energyUsed = producedAmount * Atmospherics.ProtoNitrateHydrogenConversionEnergy;

        var newHeatCapacity = atmosphereSystem.GetHeatCapacity(mixture);
        if (newHeatCapacity > Atmospherics.MinimumHeatCapacity)
            mixture.Temperature = Math.Max((mixture.Temperature * oldHeatCapacity - energyUsed) / newHeatCapacity, Atmospherics.TCMB);

        return ReactionResult.Reacting;
    }
}
