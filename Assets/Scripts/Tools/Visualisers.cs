using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using VInspector;

public class Visualisers : MonoBehaviour
{
    [Variants("Health")]
    public string Type;
    public TextMeshProUGUI textMeshPro;

    private PlayerStats playerStats;


    void Update()
    {
        playerStats = FindAnyObjectByType<PlayerStats>();
        UpdateText();
    }

    public void UpdateText()
    {
        if(Type == "Health") textMeshPro.text = Type + ": " + playerStats.Health + "";
    }
}
