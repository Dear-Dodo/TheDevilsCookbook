using DG.Tweening;
using UnityEngine;

namespace TDC.Tweens
{
    public class DoSpriteColor : SpriteTweener<Color>
    {
        public override void Play(Color target) => SpriteRenderer.DOColor(target, Duration).SetEase(Ease);
    }
}