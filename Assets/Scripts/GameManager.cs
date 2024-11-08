using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public ItemManager itemManager;
    public TileManager tileManager;
    public Player player;
    public TimeManager timeManager;
    public PlantGrowthManager plantGrowthManager;
    public ItemBox itemBox;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }

        DontDestroyOnLoad(this.gameObject);

        itemManager = GetComponent<ItemManager>();
        tileManager = GetComponent<TileManager>();
        timeManager = GetComponent<TimeManager>();
        plantGrowthManager = GetComponent<PlantGrowthManager>();
        itemBox = FindObjectOfType<ItemBox>();

        player = FindObjectOfType<Player>();

       
    }


}
