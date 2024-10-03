using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
	[SerializeField]
	private GameObject scissorEnemy;
	[SerializeField]
	private GameObject splittingEnemy;

	[SerializeField]
	private float scissorInterval = 5f;
	[SerializeField]
	private float splittingInterval = 8f;
    // Start is called before the first frame update
    void Start()
    {
       StartCoroutine(spawnEnemy(scissorInterval, scissorEnemy));
       StartCoroutine(spawnEnemy(splittingInterval, splittingEnemy)); 
    }
	
    private IEnumerator spawnEnemy(float interval, GameObject enemy)
    {
	yield return new WaitForSeconds(interval);
	GameObject newEnemy = Instantiate(enemy, new Vector3(Random.Range(-10f, 10f), Random.Range(-10f, 10f), 0), Quaternion.identity);
	StartCoroutine(spawnEnemy(interval, enemy));
    }
}
