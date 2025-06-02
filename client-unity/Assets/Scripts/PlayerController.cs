using System.Collections.Generic;
using System.Linq;
using SpacetimeDB;
using SpacetimeDB.Types;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	const int SEND_UPDATES_PER_SEC = 20;
	const float SEND_UPDATES_FREQUENCY = 1f / SEND_UPDATES_PER_SEC;

    public static PlayerController Local { get; private set; }

	private uint PlayerId;
    private float LastMovementSendTimestamp;
	private List<FishermanController> OwnedFishermans = new List<FishermanController>();
	public string Username => GameManager.Conn.Db.Player.PlayerId.Find(PlayerId).Name;
	public int NumberOfOwnedFishermans => OwnedFishermans.Count;
	public bool IsLocalPlayer => this == Local;

	public void Initialize(Player player)
    {
        PlayerId = player.PlayerId;
        if (player.Identity == GameManager.LocalIdentity)
        {
            Local = this;
        }
	}

    private void OnDestroy()
    {
        // If we have any fishermans, destroy them
        foreach (var fisherman in OwnedFishermans)
        {
            if (fisherman != null)
            {
                Destroy(fisherman.gameObject);
            }
        }
        OwnedFishermans.Clear();
    }

    public void OnFishermanSpawned(FishermanController fisherman)
    {
        OwnedFishermans.Add(fisherman);
    }

    public void OnFishermanDeleted(FishermanController deletedFisherman)
	{
		if (OwnedFishermans.Remove(deletedFisherman) && IsLocalPlayer && OwnedFishermans.Count == 0)
		{
			// DeathScreen.Instance.SetVisible(true);
		}
	}

   public Vector3? FishermanLocation()
    {
        if (OwnedFishermans.Count == 0)
        {
            return Vector3.zero;
        }
        
        Vector3 totalPos = Vector3.zero;
        foreach (var fisherman in OwnedFishermans)
        {
            var entity = GameManager.Conn.Db.Entity.EntityId.Find(fisherman.EntityId);
            var position = fisherman.transform.position;
            totalPos += (Vector3)position;
        }

        return totalPos;
	}



private Vector3 direction = Vector3.zero;

    public void Update()
{
        direction.y = 0;
        direction.x = 0;
        direction.z = 0;
    if (!IsLocalPlayer || NumberOfOwnedFishermans == 0)
    {
        return;
    }
    if (Input.GetKey(KeyCode.A))
    {
        direction.x -= 1;
    }
    if (Input.GetKey(KeyCode.D))
    {
        direction.x += 1;
    }
        if (Input.GetKey(KeyCode.S))
    {
        direction.z -= 1;
    }
    if (Input.GetKey(KeyCode.W))
    {
        direction.z += 1;
    }
    // Throttled input requests
    if (Time.time - LastMovementSendTimestamp >= SEND_UPDATES_FREQUENCY)
    {
        LastMovementSendTimestamp = Time.time;
        GameManager.Conn.Reducers.UpdatePlayerInput(direction);
    }
}

	//Automated testing members
    /*
	private bool testInputEnabled;
	private Vector2 testInput;

	public void SetTestInput(Vector2 input) => testInput = input;
	public void EnableTestInput() => testInputEnabled = true;
    */
}