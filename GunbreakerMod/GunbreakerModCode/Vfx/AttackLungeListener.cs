using GunbreakerMod.GunbreakerModCode.Characters;
using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib.Cards;

namespace GunbreakerMod.GunbreakerModCode.Vfx;

// No Spine skeleton for our character (plain-texture body, see Gunbreaker.cs), so the vanilla
// SetAnimationTrigger("Attack") lunge (NCreature.SetAnimationTrigger -> _spineAnimator?.SetTrigger,
// confirmed via decompile) is a no-op for us. This reproduces the STS1-style forward-and-back bump
// via CreatureBumpAnimator. Fire-and-forget: the tween is not awaited, so it plays alongside the
// attack's own damage/vfx instead of delaying card resolution.
public sealed class AttackLungeListener : ICardOnPlayHookListener
{
    private const float ForwardOffsetX = 40f;
    private const float ForwardDuration = 0.12f;
    private const float ReturnDuration = 0.18f;

    public Task<bool> BeforeCardOnPlay(BeforeCardOnPlayContext context)
    {
        PlayLunge(context.CardPlay);
        return Task.FromResult(false);
    }

    private static void PlayLunge(CardPlay cardPlay)
    {
        if (cardPlay.Card.Type != CardType.Attack || cardPlay.Player.Character is not Gunbreaker)
        {
            return;
        }

        CreatureBumpAnimator.PlayBump(cardPlay.Player.Creature, ForwardOffsetX, ForwardDuration, ReturnDuration);
    }
}
