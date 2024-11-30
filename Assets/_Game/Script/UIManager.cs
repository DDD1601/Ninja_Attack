using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    //public static UIManager Instance 
    //{
    //    get
    //    {
    //        if (instance == null)
    //        {
    //            instance = FindAnyObjectByType<UIManager>();
    //        }

    //        return instance;
    //    }
    //}
    
    private void Awake()
    {
        instance = this;
    }
    [SerializeField] TextMeshProUGUI coinText;

    public void SetCoin(int coin)
    {
        coinText.text = coin.ToString();
    }
}
