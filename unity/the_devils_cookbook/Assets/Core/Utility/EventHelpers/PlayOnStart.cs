/*
 *  Author: James Greensill
 *  Usage:  For use in the inspector, to execute code on Start.
 */

namespace TDC.Core.EventHelpers
{
    /// <summary>
    /// This helper will execute the event on Start.
    /// </summary>
    public class PlayOnStart : EventHelper
    {
        public void Start() => Execute();
    }
}