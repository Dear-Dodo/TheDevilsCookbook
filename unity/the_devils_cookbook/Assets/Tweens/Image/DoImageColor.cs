using DG.Tweening;
using UnityEngine;

namespace TDC.Tweens
{
    public class DoImageColor : ImageTweener<Color>
    {
        public override void Play(Color target) => imageRenderer.DOColor(target, Duration).SetEase(Ease);
    }
}