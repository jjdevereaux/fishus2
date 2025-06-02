using System;
using System.Collections;
using System.Collections.Generic;
using SpacetimeDB;
using SpacetimeDB.Types;
using UnityEngine;


public class GameManager : MonoBehaviour
{
    const string SERVER_URL = "http://127.0.0.1:3000";
    const string MODULE_NAME = "fishus";

    public static event Action OnConnected;
    public static event Action OnSubscriptionApplied;

//    public float borderThickness = 2;
//    public Material borderMaterial;

	public static GameManager Instance { get; private set; }
    public static Identity LocalIdentity { get; private set; }
    public static DbConnection Conn { get; private set; }
    public static Dictionary<uint, EntityController> Entities = new Dictionary<uint, EntityController>();
	public static Dictionary<uint, PlayerController> Players = new Dictionary<uint, PlayerController>();

    private void Start()
    {
        Instance = this;
        Application.targetFrameRate = 60;

        // In order to build a connection to SpacetimeDB we need to register
        // our callbacks and specify a SpacetimeDB server URI and module name.
        var builder = DbConnection.Builder()
            .OnConnect(HandleConnect)
            .OnConnectError(HandleConnectError)
            .OnDisconnect(HandleDisconnect)
            .WithUri(SERVER_URL)
            .WithModuleName(MODULE_NAME);

        // If the user has a SpacetimeDB auth token stored in the Unity PlayerPrefs,
        // we can use it to authenticate the connection.
        if (AuthToken.Token != "")
        {
            builder = builder.WithToken(AuthToken.Token);
        }

        // Building the connection will establish a connection to the SpacetimeDB
        // server.
        Conn = builder.Build();
    }
        // Create area of size defined in config
    private void SetupArea(float worldSize)
    {
        CameraController.WorldSize = worldSize;
    }





       // Called when we connect to SpacetimeDB and receive our client identity
    void HandleConnect(DbConnection conn, Identity identity, string token)
    {
        Debug.Log("Connected.");
        AuthToken.SaveToken(token);
        LocalIdentity = identity;

        conn.Db.Fisherman.OnInsert += FishermanOnInsert;
        conn.Db.Entity.OnUpdate += EntityOnUpdate;
        conn.Db.Entity.OnDelete += EntityOnDelete;
        conn.Db.Player.OnInsert += PlayerOnInsert;
        conn.Db.Player.OnDelete += PlayerOnDelete;
        conn.Db.Tile.OnInsert += TileOnInsert;
       // conn.Db.Item.OnInsert += ItemOnInsert;
       // conn.Db.Item.OnDelete += ItemOnDelete;
        OnConnected?.Invoke();

        // Request all tables
        Conn.SubscriptionBuilder()
            .OnApplied(HandleSubscriptionApplied)
            .SubscribeToAllTables();
    }

    // private static void ItemOnInsert(EventContext context, Item insertedValue){
    //    // add functionality for item placement in inventory
    // }

    // private static void ItemOnDelete(EventContext context, Item deletedValue){
    //     // add functionality for item removal from inventory
    // }


    private static void FishermanOnInsert(EventContext context, Fisherman insertedValue)
    {
        var player = GetOrCreatePlayer(insertedValue.PlayerId);
        var entityController = PrefabManager.SpawnFisherman(insertedValue, player);
        Entities.Add(insertedValue.EntityId, entityController);
    }

    private static void EntityOnUpdate(EventContext context, Entity oldEntity, Entity newEntity)
    {
        if (!Entities.TryGetValue(newEntity.EntityId, out var entityController))
        {
            return;
        }
        entityController.OnEntityUpdated(newEntity);
    }

    private static void EntityOnDelete(EventContext context, Entity oldEntity)
    {
        if (Entities.Remove(oldEntity.EntityId, out var entityController))
        {
            entityController.OnDelete(context);
        }
    }

    private static void PlayerOnInsert(EventContext context, Player insertedPlayer)
    {
        GetOrCreatePlayer(insertedPlayer.PlayerId);
    }

    private static void PlayerOnDelete(EventContext context, Player deletedvalue)
    {
        if (Players.Remove(deletedvalue.PlayerId, out var playerController))
        {
            GameObject.Destroy(playerController.gameObject);
        }
    }

    private static void TileOnInsert(EventContext context, Tile insertedValue)
    {
        var entityController = PrefabManager.SpawnTile(insertedValue);
        Entities.Add(insertedValue.EntityId, entityController);
    }


    private static PlayerController GetOrCreatePlayer(uint playerId)
    {
        if (!Players.TryGetValue(playerId, out var playerController))
        {
            var player = Conn.Db.Player.PlayerId.Find(playerId);
            playerController = PrefabManager.SpawnPlayer(player);
            Players.Add(playerId, playerController);
        }

        return playerController;
    }

    void HandleConnectError(Exception ex)
    {
        Debug.LogError($"Connection error: {ex}");
    }

    void HandleDisconnect(DbConnection _conn, Exception ex)
    {
        Debug.Log("Disconnected.");
        if (ex != null)
        {
            Debug.LogException(ex);
        }
    }

    private void HandleSubscriptionApplied(SubscriptionEventContext ctx)
    {
        Debug.Log("Subscription applied!");
        OnSubscriptionApplied?.Invoke();

        var worldSize = Conn.Db.Config.Id.Find(0).WorldSize * Conn.Db.Config.Id.Find(0).TileSize;
        var tileSize = Conn.Db.Config.Id.Find(0).TileSize;
        ctx.Reducers.EnterGame("Fishus");
    }

    public static bool IsConnected()
    {
        return Conn != null && Conn.IsActive;
    }

    public void Disconnect()
    {
        Conn.Disconnect();
        Conn = null;
    }
}


namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}