using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "EntityState/AvoidEntity", fileName = "Split")]

public class AvoidEntity : EntityState 
{
	public float radiusSize = 2;
		
	public override void Initialize(Entity thisEntity, List<EntityStateChanger> stateChangers)
	{
		base.Initialize(thisEntity, stateChangers);

		CircleCollider2D cc = selfEntity.gameObject.AddComponent<CircleCollider2D>();
		
		cc.radius = radiusSize;
		cc.isTrigger = true;
			
	}
	
	public override void FixedUpdate()
	{	
		Vector3 direction = Vector3.zero;
		
		foreach (Collider2D col in selfEntity.physical.colliderInfo) {
			//Vector2 opposingDirection = selfEntity.transform.position - col.gameObject.transform.position;
			//
			//opposingDirection = ((radiusSize - opposingDirection.magnitude) * opposingDirection.normalized * Time.deltaTime);

			//selfEntity.physical.rb.AddForce(opposingDirection, ForceMode2D.Force);
			//
			if (col.CompareTag("Enemy")) {	
				float ratio = Mathf.Clamp01((col.gameObject.transform.position - selfEntity.transform.position).magnitude / radiusSize);
				direction -= ratio * (col.gameObject.transform.position - selfEntity.transform.position);
			}

		}
		
		selfEntity.physical.rb.AddForce(direction, ForceMode2D.Impulse);
		
	}
	
}
