using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Coldown : MonoBehaviour
{
    [SerializeField] private Image imageCooldown;
    [SerializeField] private Text textCooldown;

    private bool isCooldown = false;
    private float cooldownTime = 10.0f;
    private float cooldownTimer = 0.0f;


    // Start is called before the first frame update
    void Start()
    {
        imageCooldown.fillAmount = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (isCooldown)
            ApplyCooldown();
    }

    void ApplyCooldown()
    {
        //Substract time since last called
        cooldownTimer -= Time.deltaTime;

        if (cooldownTimer < 0.0f)
        {
            isCooldown = false;
            imageCooldown.fillAmount = 0.0f;
        }
        else
        {
            textCooldown.text = Mathf.RoundToInt(cooldownTimer).ToString();
            imageCooldown.fillAmount = cooldownTimer / cooldownTime;
        }
    }

    public bool StartTimer()
    {
        if (isCooldown)
            return false;
        else
        {
            isCooldown = true;
            cooldownTimer = cooldownTime;
            return true;
        }
    }

    public void ResetCooldown()
    {
        isCooldown = false;
        cooldownTimer = 0f;
    }
}
