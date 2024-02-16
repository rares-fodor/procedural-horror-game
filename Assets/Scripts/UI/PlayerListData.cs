using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public struct PlayerListData : IEquatable<PlayerListData>, INetworkSerializable
{
    public ulong clientId;
    public bool monster;
    public bool ready;

    public bool Equals(PlayerListData other)
    {
        return clientId == other.clientId;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref monster);
        serializer.SerializeValue(ref ready);
    }
}
