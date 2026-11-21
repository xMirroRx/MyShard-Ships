using System.Collections.Generic;

namespace ClassicUO.Game.Managers
{
    /// <summary>
    /// Store-purchased SHIP DYE. A player buys a UO hue in the Store and applies it; while they PILOT any ship, the
    /// WHOLE ship (multi tiles + the ship's own tile + every component: tiller, hatch, sail, hold, planks…) is
    /// recoloured to that hue. Like the ship GLOW, the state is keyed per SHIP-PART SERIAL — the server (which owns
    /// the pilot state) tells every client "part S is dyed hue H", and each client renders that serial with the hue.
    /// This manager holds serial -> hue (0 = no dye). Mirrors <see cref="ShipGlowManager"/> but carries a hue instead
    /// of a single on/off bit.
    /// </summary>
    public static class ShipDyeManager
    {
        // ship-part (multi tile / component item) serial -> applied dye hue.
        private static readonly Dictionary<uint, ushort> _dye = new();

        /// <summary>Set/clear a ship part's dye hue (server-driven, by serial). hue 0 = clear.</summary>
        public static void SetDye(uint serial, ushort hue)
        {
            if (hue == 0)
                _dye.Remove(serial);
            else
                _dye[serial] = hue;
        }

        /// <summary>Applied dye hue for this serial, or 0 if none.</summary>
        public static ushort GetDye(uint serial) => _dye.TryGetValue(serial, out ushort h) ? h : (ushort)0;

        public static void Clear() => _dye.Clear();
    }
}
