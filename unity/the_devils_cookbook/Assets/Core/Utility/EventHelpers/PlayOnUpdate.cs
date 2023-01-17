/*
 *  Author: James Greensill
 *  Usage:  For use in the inspector, to execute code on Update.
 */

namespace TDC.Core.EventHelpers
{
    /// <summary>
    /// This helper will execute the event on Update.
    /// </summary>
    public class PlayOnUpdate : EventHelper
    {
        public void Update() => Execute();
    }
}