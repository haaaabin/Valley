using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Player : MonoBehaviour
{
    public Animator anim;
    public Animator doorAnim;
    public Tilemap houseRoofTileMap;
    public InventoryManager inventoryManager;
    public int money = 0;

    private Rigidbody2D rb;
    private RaycastHit2D rayHit;
    private TileManager tileManager;
    private Vector2 movement;
    private Vector2 lastMoveDirection;
    private float moveSpeed = 3;
    private bool isHoeing = false;
    private bool isWatering = false;
    private bool isAxing = false;
    private bool isOpenDoor = false;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>(); ;

        inventoryManager = GetComponent<InventoryManager>();
        tileManager = GameManager.instance.tileManager;
    }

    private void Update()
    {
        if (GameManager.instance.timeManager.isDayEnding)
            return;

        GetInput();
        UpdateAnimation();
        Plow();
        Hit();
        OpenDoor();
        OpenPostBox();

        Debug.DrawRay(rb.position + Vector2.up * 0.1f, lastMoveDirection * 1f, new Color(0, 1, 0));
    }

    private void FixedUpdate()
    {
        if (!GameManager.instance.timeManager.isDayEnding && !isHoeing && !isWatering && !isAxing)
            Move();
    }

    private void Move()
    {
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }

    private void GetInput()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // 대각선 이동 시 속도 일정하게 유지
        if (movement.magnitude > 1)
        {
            movement = movement.normalized;
        }

        // 마지막 이동 방향 저장
        if (movement != Vector2.zero)
        {
            lastMoveDirection = movement;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (UIManager.instance.settingPanel.activeSelf)
                UIManager.instance.settingPanel.SetActive(false);
        }
    }

    private void UpdateAnimation()
    {
        // walk
        anim.SetFloat("Horizontal", movement.x);
        anim.SetFloat("Vertical", movement.y);
        anim.SetFloat("Speed", movement.sqrMagnitude);

        // idle
        if (movement.sqrMagnitude == 0)
        {
            anim.SetFloat("LastHorizontal", lastMoveDirection.x);
            anim.SetFloat("LastVertical", lastMoveDirection.y);
        }
    }

    private void Hit()
    {
        rayHit = Physics2D.Raycast(rb.position, lastMoveDirection, 1f, LayerMask.GetMask("Tree"));
        if (rayHit.collider != null)
        {
            Tree tree = rayHit.collider.GetComponent<Tree>();
            if (Input.GetMouseButtonDown(0))
            {
                if (inventoryManager.toolbar.selectedSlot.itemName == "Axe")
                {
                    isAxing = true;
                    anim.SetTrigger("isAxing");
                    tree.hitCount++;
                    StartCoroutine(WaitForAnimation());
                }
            }
        }
    }

    private void OpenDoor()
    {
        rayHit = Physics2D.Raycast(rb.position, lastMoveDirection, 1f, LayerMask.GetMask("Door"));
        if (rayHit.collider != null)
        {
            if (Input.GetMouseButtonDown(1))
            {
                isOpenDoor = true;
                doorAnim.SetBool("isOpen", isOpenDoor);
            }
        }
        else
        {
            isOpenDoor = false;
            doorAnim.SetBool("isOpen", isOpenDoor);
        }
    }

    private void OpenPostBox()
    {
        rayHit = Physics2D.Raycast(rb.position, lastMoveDirection, 1f, LayerMask.GetMask("PostBox"));
        if (rayHit.collider != null)
        {
            if (UIManager.instance.speechBubble.activeSelf)
            {
                if (Input.GetMouseButtonDown(1))
                {
                    UIManager.instance.ShowPostPanel();
                }
            }
        }
    }

    private void Plow()
    {
        if (isHoeing) return;
        if (tileManager == null) return;
        if (inventoryManager == null || inventoryManager.toolbar == null || inventoryManager.toolbar.selectedSlot == null) return;
        if (inventoryManager.toolbar.selectedSlot.itemName == null) return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePosition = Input.mousePosition;
            Vector3 worldMousePosition = UnityEngine.Camera.main.ScreenToWorldPoint(mousePosition);
            worldMousePosition.z = 0;

            Vector3Int targetPosition = new Vector3Int(Mathf.FloorToInt(worldMousePosition.x), Mathf.FloorToInt(worldMousePosition.y), 0);

            Vector3 playerPosition = transform.position;
            Vector3Int gridPlayerPosition = new Vector3Int(Mathf.FloorToInt(playerPosition.x), Mathf.FloorToInt(playerPosition.y), 0);

            Vector3Int[] validTiles = new Vector3Int[]
            {
                gridPlayerPosition,
                gridPlayerPosition + new Vector3Int(0,1,0),
                gridPlayerPosition + new Vector3Int(0,-1,0),
                gridPlayerPosition + new Vector3Int(-1,0,0),
                gridPlayerPosition + new Vector3Int(1,0,0)
            };

            bool isValidTile = false;

            foreach (var tile in validTiles)
            {
                if (tile == targetPosition)
                {
                    isValidTile = true;
                    break;
                }
            }

            if (isValidTile)
            {
                string tileName = tileManager.GetTileName(targetPosition);
                string tileState = tileManager.GetTileState(targetPosition);

                if (tileName != null)
                {
                    if (tileName == "Interactable" && inventoryManager.toolbar.selectedSlot.itemName == "Hoe")
                    {
                        isHoeing = true;
                        anim.SetTrigger("isHoeing");
                        tileManager.SetInteracted(targetPosition);

                        StartCoroutine(WaitForAnimation());
                    }

                    if (tileName == "Plowed")
                    {
                        if (inventoryManager.toolbar.selectedSlot.itemName == "RiceSeed" || inventoryManager.toolbar.selectedSlot.itemName == "TomatoSeed")
                        {
                            PlantData plantData = inventoryManager.toolbar.selectedSlot.plantData;
                            Debug.Log(plantData.plantName);

                            inventoryManager.toolbar.selectedSlot.RemoveItem();  // 씨앗 갯수 줄이기

                            GameManager.instance.plantGrowthManager.PlantSeed(targetPosition, plantData);  // 씨앗 심기

                            if (inventoryManager.toolbar.selectedSlot.isEmpty)
                            {
                                inventoryManager.toolbar.selectedSlot = null;  // 씨앗이 다 떨어지면 슬롯 비우기
                            }
                        }
                        else if (inventoryManager.toolbar.selectedSlot.itemName == "Watering") // && !tileManager.GetWateringTile(targetPosition))
                        {
                            isWatering = true;
                            anim.SetTrigger("isWatering");
                            tileManager.WaterTile(targetPosition);

                            StartCoroutine(WaitForAnimation());
                        }
                    }
                }

                if (tileState != null)
                {
                    if (tileState == "Grown" && inventoryManager.toolbar.selectedSlot.itemName == "Hoe")
                    {
                        tileManager.RemoveTile(targetPosition);
                        GameManager.instance.plantGrowthManager.HarvestPlant(targetPosition);
                    }
                }
            }
        }
    }

    private IEnumerator WaitForAnimation()
    {
        yield return new WaitForSeconds(0.7f);

        if (isHoeing)
            isHoeing = false;
        if (isWatering)
            isWatering = false;
        if (isAxing)
            isAxing = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("DayEndCheckPoint"))
        {
            Debug.Log("침대 닿음");
            UIManager.instance.dayEndPanel.SetActive(true);
        }
        else
        {
            UIManager.instance.dayEndPanel.SetActive(false);
        }

        if (other.CompareTag("HouseRoof"))
        {
            houseRoofTileMap.color = new Color(1f, 1f, 1f, 0f);
        }

        if (other.CompareTag("StartTuto"))
        {
            Debug.Log("startTuto");
            lastMoveDirection = new Vector2(1, 0);
            anim.SetFloat("LastHorizontal", lastMoveDirection.x);
            anim.SetFloat("LastVertical", lastMoveDirection.y);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("HouseRoof"))
        {
            houseRoofTileMap.color = new Color(1f, 1f, 1f, 1f);
        }
    }

    public void DropItem(Item item)
    {
        Debug.Log(item.name);
        Vector3 spawnLocation = transform.position;

        Vector3 spawnOffset = Random.insideUnitCircle * 1.25f;

        Item droppedItem = Instantiate(item, spawnLocation + spawnOffset, Quaternion.identity);

        droppedItem.rigid.AddForce(spawnOffset * 0.3f, ForceMode2D.Impulse);
    }

    public void DropItem(Item item, int numToDrop)
    {
        for (int i = 0; i < numToDrop; i++)
        {
            DropItem(item);
        }
    }

    public void SetPosition()
    {
        transform.position = Vector2.zero;
        lastMoveDirection = new Vector2(0, -1);

        anim.enabled = true;
        // 애니메이션의 idle 방향 업데이트
        anim.SetFloat("LastHorizontal", lastMoveDirection.x);
        anim.SetFloat("LastVertical", lastMoveDirection.y);
    }

    public bool IsAxing()
    {
        return isAxing;
    }
}
