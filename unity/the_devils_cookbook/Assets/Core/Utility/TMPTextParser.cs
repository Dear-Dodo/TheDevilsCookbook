using System.Text.RegularExpressions;
using UnityEngine;

namespace TDC.Core.Utility
{
    public static class TMPTextParser
    {
        public class ParserSettings
        {
            public Color32 HyperlinkNormal = new Color32(0, 127, 238, 255);
            public Color32 HyperlinkHover = new Color32(0, 182, 238, 255);
        }

        private static Regex _LinkPattern = new Regex("<link=\"(http.+|mailto:.+)\">(.*)</link>"); 
        
        public static string Parse(string text, ParserSettings settings)
        {
            return _LinkPattern.Replace(text, (m) => ReplaceLinkMatch(m, settings)).Replace("\\t", "\t");
        }

        private static string ReplaceLinkMatch(Match link, ParserSettings settings)
        {
            string hex = ColorUtility.ToHtmlStringRGBA(settings.HyperlinkNormal);
            return
                $"<color=#{hex}>" +
                $"<u color=#{hex}>" +
                $"<link=\"{link.Groups[1].Value}\">" +
                $"<sprite=0 color=#{hex}>{link.Groups[2].Value}" +
                $"</link>" +
                $"</u>" +
                $"</color>";
        }
    }
}