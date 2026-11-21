using System.Collections.Generic;

namespace ClassicUO.Game.Managers
{
    /// <summary>
    /// Store-purchased SHIP glow. A player buys a per-ship-type glow in the Store and applies it; while they PILOT
    /// a matching ship, the whole ship glows. Unlike the weapon glow (which is keyed per player), the ship glow is
    /// keyed per SHIP SERIAL — the server (which owns the pilot state) tells every client "ship S is glowing" and
    /// each client makes that multi's components glow. This manager holds the set of currently-glowing ship serials.
    ///
    /// The bit map (key -> bit) is shared with the Store cards and the server's ship-type check.
    /// </summary>
    public static class ShipGlowManager
    {
        // Single fixed hue for the whole-ship glow (multi tiles + the ship's own tile + the tiller). Tunable.
        public const ushort GlowHue = 0x0481;

        // Canonical ship-glow types: key + bit index. Kept in sync with the Store cards and the server's ship
        // multi-id -> type mapping (Medium Dragon = MULTIDEF 0x0C-0x0F, Large Dragon = 0x14-0x17).
        public static readonly (string key, int bit)[] Types =
        {
            ("medium_dragon", 0),
            ("large_dragon",  1),
        };

        // ship (multi item) serials that are currently glowing.
        private static readonly HashSet<uint> _glow = new();

        public static int BitFor(string key)
        {
            foreach ((string k, int bit) in Types)
                if (k == key)
                    return bit;
            return -1;
        }

        /// <summary>Turn a ship's glow on/off (server-driven, by ship serial).</summary>
        public static void SetGlow(uint shipSerial, bool on)
        {
            if (on)
                _glow.Add(shipSerial);
            else
                _glow.Remove(shipSerial);
        }

        public static bool IsGlowing(uint shipSerial) => _glow.Contains(shipSerial);

        public static void Clear() => _glow.Clear();
    }
}
