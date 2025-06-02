using SpacetimeDB.Types;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabManager : MonoBehaviour
{
	private static PrefabManager Instance;

	public FishermanController FishermanPrefab;
	public PlayerController PlayerPrefab;
    public TileController TilePrefab;

	private void Awake()
	{
		Instance = this;
	}

	public static FishermanController SpawnFisherman(Fisherman fisherman, PlayerController owner)
	{
		var entityController = Instantiate(Instance.FishermanPrefab);
		entityController.name = $"Fisherman - {fisherman.EntityId}";
		entityController.Spawn(fisherman, owner);
		owner.OnFishermanSpawned(entityController);
		return entityController;
	}

	public static PlayerController SpawnPlayer(Player player)
	{
		var playerController = Instantiate(Instance.PlayerPrefab);
		playerController.name = $"PlayerController - {player.Name}";
		playerController.Initialize(player);
		return playerController;
	}

    	public static TileController SpawnTile(Tile tile)
	{
		var entityController = Instantiate(Instance.TilePrefab);
		entityController.name = $"Tile - {tile.EntityId}";
		entityController.Spawn(tile);
		return entityController;
	}
}