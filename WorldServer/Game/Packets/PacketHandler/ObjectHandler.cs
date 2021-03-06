﻿/*
 * Copyright (C) 2012-2014 Arctium <http://arctium.org>
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using Framework.Constants;
using Framework.Constants.NetMessage;
using Framework.Logging;
using Framework.Network.Packets;
using WorldServer.Game.WorldEntities;
using WorldServer.Network;

namespace WorldServer.Game.Packets.PacketHandler
{
    public class ObjectHandler : Globals
    {
        public static void HandleUpdateObjectCreate(ref WorldClass session)
        {
            WorldObject character = session.Character;
            PacketWriter updateObject = new PacketWriter(ServerMessage.ObjectUpdate);

            updateObject.WriteUInt16((ushort)character.Map);
            updateObject.WriteUInt32(1);
            updateObject.WriteUInt8((byte)UpdateType.CreateObject);
            updateObject.WriteGuid(character.Guid);
            updateObject.WriteUInt8((byte)ObjectType.Player);

            UpdateFlag updateFlags = UpdateFlag.Alive | UpdateFlag.Rotation | UpdateFlag.Self;
            WorldMgr.WriteUpdateObjectMovement(ref updateObject, ref character, updateFlags);

            character.SetUpdateFields();

            character.WriteUpdateFields(ref updateObject);
            character.WriteDynamicUpdateFields(ref updateObject);

            session.Send(ref updateObject);
        }

        public static void HandleUpdateObjectValues(ref WorldClass session)
        {
            WorldObject character = session.Character;
            PacketWriter updateObject = new PacketWriter(ServerMessage.ObjectUpdate);

            updateObject.WriteUInt16((ushort)character.Map);
            updateObject.WriteUInt32(1);
            updateObject.WriteUInt8((byte)UpdateType.Values);
            updateObject.WriteGuid(character.Guid);

            character.WriteUpdateFields(ref updateObject);
            character.WriteDynamicUpdateFields(ref updateObject);

            session.Send(ref updateObject);
        }

        public static PacketWriter HandleDestroyObject(ref WorldClass session, ulong guid, bool animation = false)
        {
            PacketWriter destroyObject = new PacketWriter(ServerMessage.DestroyObject);
            BitPack BitPack = new BitPack(destroyObject, guid);

            BitPack.WriteGuidMask(5, 6, 1, 7, 2, 0, 3);
            BitPack.Write(animation);
            BitPack.WriteGuidMask(4);

            BitPack.Flush();

            BitPack.WriteGuidBytes(1, 5, 3, 0, 2, 6, 7, 4);

            return destroyObject;
        }

        [Opcode(ClientMessage.ObjectUpdateFailed, "17658")]
        public static void HandleObjectUpdateFailed(ref PacketReader packet, WorldClass session)
        {
            byte[] guidMask = { 5, 6, 4, 3, 7, 1, 2, 0 };
            byte[] guidBytes = {6, 3, 0, 7, 1, 4, 2, 5 };

            BitUnpack GuidUnpacker = new BitUnpack(packet);

            ulong guid = GuidUnpacker.GetPackedValue(guidMask, guidBytes);
            Log.Message(LogType.Debug, "ObjectUpdate failed for object with Guid {0}", guid);
        }
    }
}
