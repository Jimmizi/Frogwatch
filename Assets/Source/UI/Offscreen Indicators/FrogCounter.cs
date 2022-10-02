using TMPro;
using UnityEngine;

public class FrogCounter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI frogCount;
    void Update()
    {
        
        int aCounter = FrogController.GetNumberOfFrogs();
        string Count = ($"{aCounter}");
        frogCount.text = Count;
    }
   
}
