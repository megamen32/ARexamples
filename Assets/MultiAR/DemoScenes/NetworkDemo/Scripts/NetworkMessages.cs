﻿using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Network Message Types.
/// </summary>
public struct NetMsgType
{
    public const short GetGameAnchorRequest = 1001;

    public const short GetGameAnchorResponse = 1002;

    public const short CheckHostAnchorRequest = 1003;

    public const short CheckHostAnchorResponse = 1004;

    public const short SetGameAnchorRequest = 1005;

    public const short SetGameAnchorResponse = 1006;

    public const short HandleSyncTransform = 1007;

    public static short AttackRequest = 1008;
    public static short AttackResponse = 1009;
    public static short SetGameObjectRequest = 1010;
    public static short SetGameObjectResponse = 1011;
}


/// <summary>
/// Get-game-anchor request message.
/// </summary>
public class GetGameAnchorRequestMsg : MessageBase
{
    public string gameName;

    public override void Serialize(NetworkWriter writer)
    {
        base.Serialize(writer);
        writer.Write(gameName);
    }

    public override void Deserialize(NetworkReader reader)
    {
        base.Deserialize(reader);
        gameName = reader.ReadString();
    }
}


/// <summary>
/// Get-game-anchor response message.
/// </summary>
public class GetGameAnchorResponseMsg : MessageBase
{
    //public string apiKey;
    public byte[] anchorData;
    public string anchorId;
    public bool   found;

#pragma warning disable 618
    public override void Serialize(NetworkWriter writer)
#pragma warning restore 618
    {
        base.Serialize(writer);

        writer.Write(found);
        writer.Write(anchorId);
        //writer.Write(apiKey);

        if (anchorData == null)
        {
            anchorData = new byte[0];
        }

        writer.WriteBytesAndSize(anchorData, anchorData.Length);
    }

    public override void Deserialize(NetworkReader reader)
    {
        base.Deserialize(reader);

        found    = reader.ReadBoolean();
        anchorId = reader.ReadString();
        //apiKey = reader.ReadString();

        anchorData = reader.ReadBytesAndSize();
    }
}


/// <summary>
/// Check-host-anchor request message.
/// </summary>
public class CheckHostAnchorRequestMsg : MessageBase
{
    public string gameName;

    public override void Serialize(NetworkWriter writer)
    {
        base.Serialize(writer);
        writer.Write(gameName);
    }

    public override void Deserialize(NetworkReader reader)
    {
        base.Deserialize(reader);
        gameName = reader.ReadString();
    }
}


/// <summary>
/// Check-host-anchor response message.
/// </summary>
public class CheckHostAnchorResponseMsg : MessageBase
{
    public bool granted;
    //public string apiKey;

    public override void Serialize(NetworkWriter writer)
    {
        base.Serialize(writer);
        writer.Write(granted);
        //writer.Write(apiKey);
    }

    public override void Deserialize(NetworkReader reader)
    {
        base.Deserialize(reader);
        granted = reader.ReadBoolean();
        //apiKey = reader.ReadString();
    }
}

//class SpawnMessage : MessageBase

/// {
///    public uint netId;
///    public NetworkHash128 assetId;
///    public Vector3 position;
///    public byte[] payload;
///
///    // This method would be generated
///    public override void Deserialize(NetworkReader reader)
///    {
///        netId = reader.ReadPackedUInt32();
///        assetId = reader.ReadNetworkHash128();
///        position = reader.ReadVector3();
///        payload = reader.ReadBytesAndSize();
///    }
///
///    // This method would be generated
///    public override void Serialize(NetworkWriter writer)
///    {
///        writer.WritePackedUInt32(netId);
///        writer.Write(assetId);
///        writer.Write(position);
///        writer.WriteBytesFull(payload);
///    }
/// }

/// <summary>
/// Check-host-anchor response message.
/// </summary>
public class AttackMSG : MessageBase
{
    public enum AttackType : uint
    {
        Blast=1,
        Bullet=2,
        Shield=0
    }
    public AttackType attackType;
    public AttackMode attackMode;

    public enum AttackMode:uint
    {
        Primary=0,
        Secondary=1,
    }

    public uint netId;
    //public string apiKey;

    public override void Serialize(NetworkWriter writer)
    {
        base.Serialize(writer);
        writer.WritePackedUInt32(netId);
        writer.WritePackedUInt32( (uint) attackType);
        writer.WritePackedUInt32( (uint) attackMode);
        //writer.Write(apiKey);
    }

    public override void Deserialize(NetworkReader reader)
    {
        base.Deserialize(reader);
        netId = reader.ReadPackedUInt32();
        attackType = (AttackType) reader.ReadPackedUInt32();
        attackMode = (AttackMode) reader.ReadPackedUInt32();
        //apiKey = reader.ReadString();
    }
}
/// <summary>
/// Set-game-anchor request message.
/// </summary>
public class SetGameAnchorRequestMsg : MessageBase
{
    public byte[]     anchorData;
    public string     anchorId;
    public Vector3    anchorPos;
    public Quaternion anchorRot;
    public string     gameName;

    public override void Serialize(NetworkWriter writer)
    {
        base.Serialize(writer);

        writer.Write(gameName);
        writer.Write(anchorId);
        writer.Write(anchorPos);
        writer.Write(anchorRot);

        if (anchorData == null)
        {
            anchorData = new byte[0];
        }

        writer.WriteBytesAndSize(anchorData, anchorData.Length);
    }

    public override void Deserialize(NetworkReader reader)
    {
        base.Deserialize(reader);

        gameName  = reader.ReadString();
        anchorId  = reader.ReadString();
        anchorPos = reader.ReadVector3();
        anchorRot = reader.ReadQuaternion();

        anchorData = reader.ReadBytesAndSize();
    }
}

/// <summary>
/// Set-game-anchor request message.
/// </summary>
public class SetGameObjectRequestMsg : MessageBase
{
    public byte[]     anchorData;

    public enum Type:uint
    {
        WorldOrigin=0,
        PortalIn=1,
        PortalOut=2,
        Window=3,
    }

    public Type model;
    public string     anchorId;
    public Vector3    anchorPos;
    public Quaternion anchorRot;
    public string     gameName;

    public override void Serialize(NetworkWriter writer)
    {
        base.Serialize(writer);

        writer.Write(gameName);
        writer.Write(anchorId);
        writer.Write((uint) model);
        writer.Write(anchorPos);
        writer.Write(anchorRot);

        if (anchorData == null)
        {
            anchorData = new byte[0];
        }

        writer.WriteBytesAndSize(anchorData, anchorData.Length);
    }

    public override void Deserialize(NetworkReader reader)
    {
        base.Deserialize(reader);

        gameName  = reader.ReadString();
        anchorId  = reader.ReadString();
        model = (Type) reader.ReadPackedUInt32();
        anchorPos = reader.ReadVector3();
        anchorRot = reader.ReadQuaternion();
        anchorData = reader.ReadBytesAndSize();
    }
}

/// <summary>
/// Set-game-anchor response message.
/// </summary>
public class SetGameAnchorResponseMsg : MessageBase
{
    public bool confirmed;

    public override void Serialize(NetworkWriter writer)
    {
        base.Serialize(writer);
        writer.Write(confirmed);
    }

    public override void Deserialize(NetworkReader reader)
    {
        base.Deserialize(reader);
        confirmed = reader.ReadBoolean();
    }
}/// <summary>
/// Set-game-anchor response message.
/// </summary>
public class SetGameObjectResponseMsg : MessageBase
{
    public bool confirmed;

    public override void Serialize(NetworkWriter writer)
    {
        base.Serialize(writer);
        writer.Write(confirmed);
    }

    public override void Deserialize(NetworkReader reader)
    {
        base.Deserialize(reader);
        confirmed = reader.ReadBoolean();
    }
}