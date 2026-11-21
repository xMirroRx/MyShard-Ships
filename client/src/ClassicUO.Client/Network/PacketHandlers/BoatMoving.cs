using ClassicUO.Configuration;
using ClassicUO.Game;
using ClassicUO.Game.Data;
using ClassicUO.Game.GameObjects;
using ClassicUO.IO;
using ClassicUO.Network.PacketHandlers.Helpers;

namespace ClassicUO.Network.PacketHandlers;

internal static class BoatMoving
{
    public static void Receive(World world, ref StackDataReader p)
    {
        if (!world.InGame)
            return;

        uint serial = p.ReadUInt32BE();
        byte boatSpeed = p.ReadUInt8();
        Direction movingDirection = (Direction)p.ReadUInt8() & Direction.Mask;
        Direction facingDirection = (Direction)p.ReadUInt8() & Direction.Mask;
        ushort x = p.ReadUInt16BE();
        ushort y = p.ReadUInt16BE();
        ushort z = p.ReadUInt16BE();

        Item multi = world.Items.Get(serial);

        if (multi == null)
            return;

        bool smooth =
            ProfileManager.CurrentProfile != null
            && ProfileManager.CurrentProfile.UseSmoothBoatMovement;

        // Boat delta for this packet (non-smooth mode carries the local player by it instead of snapping him).
        int boatDx = x - multi.X;
        int boatDy = y - multi.Y;
        int boatDz = (sbyte)z - multi.Z;
        if (smooth)
            world.BoatMovingManager.AddStep(
                serial,
                boatSpeed,
                movingDirection,
                facingDirection,
                x,
                y,
                (sbyte)z
            );
        else
        {
            multi.SetInWorldTile(x, y, (sbyte)z);

            if (world.HouseManager.TryGetHouse(serial, out House house))
                house.Generate(true, true, true);
        }

        int count = p.ReadUInt16BE();

        for (int i = 0; i < count; i++)
        {
            uint cSerial = p.ReadUInt32BE();
            ushort cx = p.ReadUInt16BE();
            ushort cy = p.ReadUInt16BE();
            ushort cz = p.ReadUInt16BE();

            if (cSerial == world.Player)
            {
                world.RangeSize.X = cx;
                world.RangeSize.Y = cy;
            }

            Entity ent = world.Get(cSerial);

            if (ent == null)
                continue;

            if (smooth)
                world.BoatMovingManager.PushItemToList(
                    serial,
                    cSerial,
                    x - cx,
                    y - cy,
                    (sbyte)(z - cz)
                );
            else
            {
                // MyShard: the local player is moved by the boat's delta (steps walked on deck but not yet confirmed
                // stay valid). UpdatePlayer here snapped him to the server's older position and reset the walker,
                // so walking on a moving ship stuttered on every boat step.
                if (cSerial == world.Player)
                    world.BoatMovingManager.ShiftPlayer(boatDx, boatDy, boatDz);
                else
                    ObjectHelpers.UpdateGameObject(
                        world,
                        cSerial,
                        ent.Graphic,
                        0,
                        (ushort)(ent.Graphic == 0x2006 ? ((Item)ent).Amount : 0),
                        cx,
                        cy,
                        (sbyte)cz,
                        SerialHelper.IsMobile(ent) ? ent.Direction : 0,
                        ent.Hue,
                        ent.Flags,
                        0,
                        0,
                        1
                    );
            }
        }
    }
}
