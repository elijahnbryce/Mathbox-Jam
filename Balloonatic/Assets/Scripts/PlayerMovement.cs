using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
//using static UnityEditor.Experimental.GraphView.GraphView;

public class PlayerMovement : MonoBehaviour
{
    [Header("Variables")]
    [SerializeField] public float movementSpeed;
    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Transform secondHand;
    [SerializeField] private float followingDistance = 5f;
    private float followingSpeed = 1f;
    private Rigidbody2D rigidBody;
    [HideInInspector] public Vector2 PlayerPosition;
    private bool facingDir = true;
    private bool attacking = false;
    public bool FacingDir { get { return facingDir; } }
    [HideInInspector] public bool CanMove;
    [HideInInspector] public bool Moving;
    [HideInInspector] public Vector2 currentPos, targetPos;
    private Vector2 secondCurrentPos, secondTargetPos;
    private Vector2 cachedDirection;
    private Vector2 cachedMovementDirection;
    float counter;

    private float upgradeTimer = 8;
    private int upgradeIndex = 0;

    //private static GameManager gm;

    public static PlayerMovement Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }
    void Start()
    {
        CanMove = true;
        rigidBody = GetComponent<Rigidbody2D>();

        PlayerAttack.OnAttackInitiate += AttackStart;
        PlayerAttack.OnAttackHalt += AttackEnd;
    }

    //cleanup later
    private void AttackStart()
    {
        attacking = true;
        followingDistance = 0.5f;
        followingSpeed = 8f;
        //StartCoroutine(nameof(AttackStretch));
    }

    //cleanup later
    //private IEnumerator AttackStretch()
    //{
    //    var timer = 0f;
    //    while (timer < 2.5f)
    //    {
    //        yield return null;
    //        followingDistance += Time.deltaTime * 1.2f;
    //    }
    //}

    //cleanup later
    private void AttackEnd()
    {
        //StopCoroutine(nameof(AttackStretch));
        attacking = false;
        followingDistance = 5f;
        followingSpeed = 2.5f;
    }

    public float GetAttackPower()
    {
        return (secondHand.position - transform.position).magnitude / 5f;
    }

    void Update()
    {
        counter += Time.deltaTime;
        if (!CanMove)
        {
            transform.position = currentPos = Vector2.Lerp(currentPos, targetPos, Time.deltaTime * 5f);
        }

        //implement raycast later
        //secondTargetPos = PlayerPosition + new Vector2(facingDir ? 1 : -1, 0) * followingDistance;
        secondHand.position = secondCurrentPos = Vector3.Slerp(secondCurrentPos, secondTargetPos, Time.deltaTime * followingSpeed);
    }

    public void SnapPosition(Vector3 newPosition)
    {
        transform.position = currentPos = newPosition;
    }

    private void FixedUpdate()
    {
        PlayerPosition = transform.position;
        if (!CanMove) return;
        var movement = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (GameManager.Instance.upgradeList.ContainsKey(UpgradeType.Confusion)) { movement *= -Vector2.one; } // Confusion Ability

        Vector3 mouseScreenPosition = Input.mousePosition;
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(new Vector3(mouseScreenPosition.x, mouseScreenPosition.y, Camera.main.nearClipPlane));
        facingDir = transform.position.x > mouseWorldPosition.x;

        Moving = movement.magnitude > 0;
        if (Moving)
        {
            if (counter > 0.75f)
            {
                SoundManager.Instance.PlaySoundEffect("player_walk");
                counter = 0;
            }
        }
        if (movement.magnitude > 1) movement /= movement.magnitude;
        var gm = GameManager.Instance;
        var speedMult = Mathf.Clamp01(gm.GetHealthRatio() * 2);
        rigidBody.velocity = movement * movementSpeed * speedMult * gm.GetPowerMult(UpgradeType.Lightning, 1.5f);


        if (!attacking)
        {
            secondTargetPos = PlayerPosition + new Vector2(!facingDir ? 1 : -1, 0) * 5f;
            return;
        }

        //second hand movement
        //optimize later

        Vector3 direction = mouseWorldPosition - transform.position;
        cachedDirection = direction;

        Debug.DrawRay(transform.position, direction, Color.green);
        Debug.DrawRay(transform.position, direction * -1, Color.red);

        direction.Normalize();

        int layerMask = (1 << 6) | (1 << 12);
        layerMask = ~layerMask;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, -direction, followingDistance, layerMask);
        //Debug.Log(hit.rigidbody);
        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            secondTargetPos = hit.point;
        }
        else
        {
            secondTargetPos = transform.position + direction * 5;
        }
    }

    public Vector2 GetDirectionToMouse()
    {
        return cachedDirection;
    }

    public Vector2 GetDirectionToMouse(bool _)
    {
        if (!PlayerAttack.Instance.Attacking)
        {
            if (Moving)
            {
                cachedMovementDirection = -rigidBody.velocity;
            }
            return cachedMovementDirection;
        }
        return cachedDirection;
    }

    public Vector2 GetDirectionToPrimaryHand()
    {
        var dir = secondHand.position - transform.position;
        dir.Normalize();
        Debug.DrawRay(secondHand.position, dir, Color.magenta);
        return dir;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        switch (collision.gameObject.tag)
        {
            case "Selection":
                upgradeIndex = int.Parse(collision.gameObject.name);
                StartCoroutine(nameof(UpgradeCountdown));
                break;

            case "Enemy":
                Debug.Log("Player damaged");
                GameManager.Instance.UpdateHealth();
                // status change
                break;
            case "Coin":
                collision.GetComponent<Coin>().ClaimCoin();
                Debug.Log("Picked up coin.");
                    break;
            default:
                break;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        switch (collision.gameObject.tag)
        {
            case "Enemy":
                Debug.Log("Player damaged");
                GameManager.Instance.UpdateHealth();
                break;
        }
    }

    private IEnumerator UpgradeCountdown()
    {
        while(upgradeTimer > 0)
        {
            upgradeTimer -= Time.deltaTime;
            if (Input.GetMouseButtonDown(0))
            {
                upgradeTimer = 0;
            }
            yield return null;
        }
        UpgradeManager.Instance.ClaimUpgrade(upgradeIndex);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Selection"))
        {
            StopCoroutine(nameof(UpgradeCountdown));
            upgradeTimer = 8;
        }
    }

}
