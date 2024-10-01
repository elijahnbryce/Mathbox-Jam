using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    private Transform playerTransform;
    private float currentZoom, targetZoom;

    public static CameraManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }
    void Start()
    {
        currentZoom = targetZoom = 5;
        playerTransform = PlayerMovement.Instance.transform;
        PlayerAttack.OnAttackInitiate += AttackStart;
        PlayerAttack.OnAttackHalt += AttackEnd;
    }

    void Update()
    {
        var newPos = Vector2.Lerp(transform.position, playerTransform.position, 5f * Time.deltaTime);
        //ass code fix later
        transform.position = new Vector3(newPos.x, newPos.y, -10);
        //transform.localScale = Vector2.one * currentZoom;
        //GetComponent<Camera>().orthographicSize = currentZoom = Mathf.Lerp(currentZoom, targetZoom, Time.deltaTime * 0.5f);
    }

    private void AttackStart()
    {
        targetZoom = 3f;
    }

    private void AttackEnd()
    {
        targetZoom = 5f;
    }
}
