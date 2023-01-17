using System;

namespace TDC.Spellcasting
{
    [AttributeUsage(AttributeTargets.Field)]
    public class SpellStatAttribute : Attribute
    {
        public string Name;
        // TODO: Add conditional visibility
        // TODO: Add value remapping (bool -> string, etc.)

        public SpellStatAttribute(string name)
        {
            Name = name;
        }
    }
}
