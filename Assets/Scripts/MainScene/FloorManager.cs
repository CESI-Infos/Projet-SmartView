using UnityEngine;

public class FloorManager : MonoBehaviour
{
    public static FloorManager Instance { get; private set; }
    public GameObject[] floors;
    public GameObject ui;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Another instance of GameManager already exist");
            Destroy(gameObject);
            return;
        }
    } 

    public void showFloor(int floorIndex)
    {
        if (floorIndex == 0)
        {
            ui.SetActive(false);
        }
        else
        {
            ui.SetActive(true);
        }
        for (int i = 0; i < floors.Length; i++)
        {
            floors[i].SetActive(i == floorIndex);
        }
    }
}
