using TDC.Affectables;
using UnityEngine;

namespace TDC.Spellcasting
{
    public class Target
    {
        public Vector3 Position;
        public AffectableStats AffectableUnit;

        public Target(Vector3 pos)
        {
            Position = pos;
            AffectableUnit = null;
        }

        public Target(AffectableStats unit)
        {
            Position = Vector3.zero;
            AffectableUnit = unit;
        }
    }
}
