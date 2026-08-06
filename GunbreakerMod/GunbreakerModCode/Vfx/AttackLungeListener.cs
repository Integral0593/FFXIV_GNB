using Godot;
using GunbreakerMod.GunbreakerModCode.Characters;
using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib.Cards;

namespace GunbreakerMod.GunbreakerModCode.Vfx;

// No Spine skeleton for our character (plain-texture body, see Gunbreaker.cs), so the vanilla
// SetAnimationTrigger("Attack") lunge (NCreature.SetAnimationTrigger -> _spineAnimator?.SetTrigger,
// confirmed via decompile) is a no-op for us. This reproduces the STS1-style forward-and-back bump
// with a plain position Tween on NCreature.Visuals (the Node2D holding just the character art -
// NOT the whole NCreature Control, which also holds the health bar/UI overlay that shouldn't move).
// Fire-and-forget: the tween is not awaited, so it plays alongside the attack's own damage/vfx
// instead of delaying card resolution.
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

        var visuals = cardPlay.Player.Creature.GetCreatureNode()?.Visuals;
        if (visuals == null)
        {
            return;
        }

        var basePosition = visuals.Position;
        var tween = visuals.CreateTween();
        tween.TweenProperty(visuals, "position:x", basePosition.X + ForwardOffsetX, ForwardDuration)
            .SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);
        tween.TweenProperty(visuals, "position:x", basePosition.X, ReturnDuration)
            .SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.In);
    }
}
