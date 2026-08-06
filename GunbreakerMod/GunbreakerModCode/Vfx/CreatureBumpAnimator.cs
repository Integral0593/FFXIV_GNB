using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace GunbreakerMod.GunbreakerModCode.Vfx;

// Shared position-bump tween used for both the attack-lunge (forward, toward the enemy) and the
// hurt-reaction (backward, away from the enemy) since our character has no Spine rig for the
// vanilla trigger animations to drive (see AttackLungeListener.cs). Operates on
// NCreature.Visuals - the Node2D holding just the character art, not the whole NCreature Control
// which also carries the health bar/UI overlay that shouldn't move.
public static class CreatureBumpAnimator
{
    public static void PlayBump(Creature creature, float offsetX, float outDuration, float returnDuration)
    {
        var visuals = creature.GetCreatureNode()?.Visuals;
        if (visuals == null)
        {
            return;
        }

        var basePosition = visuals.Position;
        var tween = visuals.CreateTween();
        tween.TweenProperty(visuals, "position:x", basePosition.X + offsetX, outDuration)
            .SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);
        tween.TweenProperty(visuals, "position:x", basePosition.X, returnDuration)
            .SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.In);
    }
}
