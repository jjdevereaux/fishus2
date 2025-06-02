using SpacetimeDB.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public abstract class EntityController : MonoBehaviour
{
	const float LERP_DURATION_SEC = 0.1f;

	private static readonly int ShaderColorProperty = Shader.PropertyToID("Color");

	[DoNotSerialize] public uint EntityId;

	protected float LerpTime;
	protected Vector3 LerpStartPosition;
	protected Vector3 LerpTargetPosition;

	protected virtual void Spawn(uint entityId)
	{
		EntityId = entityId;

		var entity = GameManager.Conn.Db.Entity.EntityId.Find(entityId);
		LerpStartPosition = LerpTargetPosition = transform.position = (Vector3)entity.Position;
	}

	public void SetColor(Color color)
	{
		GetComponent<MeshRenderer>().material.SetColor(ShaderColorProperty, color);
	}

	public virtual void OnEntityUpdated(Entity newVal)
	{
		LerpTime = 0.0f;
		LerpStartPosition = transform.position;
		LerpTargetPosition = (Vector3)newVal.Position;
	}

	public virtual void OnDelete(EventContext context)
	{
		Destroy(gameObject);
	}

	public virtual void Update()
	{
		// Interpolate position and scale
		LerpTime = Mathf.Min(LerpTime + Time.deltaTime, LERP_DURATION_SEC);
		transform.position = Vector3.Lerp(LerpStartPosition, LerpTargetPosition, LerpTime / LERP_DURATION_SEC);
	}

}