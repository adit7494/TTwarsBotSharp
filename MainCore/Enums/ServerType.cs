namespace MainCore.Enums
{
    /// <summary>
    /// Represents the type of Travian server.
    /// TTWars is a speed version of Travian with different HTML structure and faster gameplay.
    /// </summary>
    public enum ServerType
    {
        /// <summary>
        /// Standard Travian server (international, national, etc.)
        /// </summary>
        Travian = 0,

        /// <summary>
        /// TTWars speed server - uses similar HTML structure but with some differences
        /// in element IDs, class names, and timer formats (millisecond precision).
        /// </summary>
        TTWars = 1,
    }
}
