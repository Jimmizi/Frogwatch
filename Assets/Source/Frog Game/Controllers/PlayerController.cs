using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : HumanoidController
{
    public SpriteRenderer DashChargingVisuals;
    public List<Sprite> ChargingFrames = new();

    public SpriteRenderer DashChargeVisuals;
    public Animator DashChargeAnimator;
    public ParticleSystem DashChargePtfx;
    
    public Animator DashPtfxAnimator;

    public bool DashIsUnlocked = true;


    private bool bAbortFadeIn = false;

    // Start is called before the first frame update
    protected override void Start()
    {
        HumanoidController.Player = this;

        if (!DashIsUnlocked)
        {
            Color col = DashChargeVisuals.color;
            col.a = 0.0f;
            DashChargeVisuals.color = col;

            DashChargePtfx.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        SetDashUnlocked();

        base.Start();
    }

    public void SetDashUnlocked()
    {
        StartCoroutine(PerformRecharge());
        DashIsUnlocked = true;
    }

    // Update is called once per frame
    protected override void Update()
    {
        float fHorizontal = Input.GetAxis("Horizontal");
        float fVertical = Input.GetAxis("Vertical");
        bool bInteracted = Input.GetButtonDown("Interact");
        bool bDashed = Input.GetButtonDown("Dash");

        InputDirection = new Vector2(fHorizontal, fVertical);
        InputDirection.Normalize();

        JustPressedInteract = bInteracted;
        JustPressedDash = DashIsUnlocked && bDashed;

        base.Update();
    }
    
    protected override void OnDashStart()
    {
        Debug.Log("Dash started");
        StartCoroutine(DoColorFadeOut());
        DashChargeAnimator.SetBool("DashReady", false);
        DashChargePtfx.Stop(false, ParticleSystemStopBehavior.StopEmitting);

        DashPtfxAnimator.SetTrigger("Dashed");

        StartCoroutine(PerformRecharge(true));

        if (InputDirection.x > 0.0f)
        {
            DashPtfxAnimator.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            DashPtfxAnimator.transform.localPosition = new Vector3(-0.28f, 0.0f, 0.0f);
        }
        else if (InputDirection.x < 0.0f)
        {
            DashPtfxAnimator.transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
            DashPtfxAnimator.transform.localPosition = new Vector3(0.28f, 0.0f, 0.0f);
        }
    }

    protected override void OnDashRecharged()
    {
        Debug.Log("Dash recharged");
        StartCoroutine(DoColorFadeIn());
        DashChargeAnimator.SetBool("DashReady", true);
        DashChargePtfx.Play();
    }

    IEnumerator PerformRecharge(bool bDueToHavingCharged = false)
    {
        if(bDueToHavingCharged)
        {
            dashIsAliveTimer = DashLogicalTime;
        }

        DashCooldownTimer = DashCooldown;

        while (DashCooldownTimer > 0.0f)
        {
            if (dashIsAliveTimer > 0.0f)
            {
                dashIsAliveTimer -= Time.deltaTime;
                yield return new WaitForSeconds(Time.deltaTime);
            }

            DashCooldownTimer -= Time.deltaTime;

            float fPercentageDone = DashCooldownTimer / DashCooldown;
            int iFrame = Math.Clamp(Mathf.FloorToInt(ChargingFrames.Count * (1.0f - fPercentageDone)), 0, ChargingFrames.Count - 1);
            DashChargingVisuals.sprite = ChargingFrames[iFrame];

            yield return new WaitForSeconds(Time.deltaTime);
        }

        OnDashRecharged();
        DashChargingVisuals.sprite = null;
    }

    IEnumerator DoColorFadeOut()
    {
        void MinusAlpha(float a)
        {
            Color col = DashChargeVisuals.color;
            col.a -= a;
            DashChargeVisuals.color = col;
        }

        bAbortFadeIn = true;

        while (DashChargeVisuals.color.a > 0.2f)
        {
            // Fade out over half a second
            MinusAlpha((0.8f * Time.deltaTime) * 20); // hack just speed this up a bunch (20 instead of 2)
            
            yield return new WaitForSeconds(Time.deltaTime);
        }

        Color col = DashChargeVisuals.color;
        col.a = 0.2f;
        DashChargeVisuals.color = col;

        bAbortFadeIn = false;

        yield return null;
    }

    IEnumerator DoColorFadeIn()
    {
        void AddAlpha(float a)
        {
            Color col = DashChargeVisuals.color;
            col.a += a;
            DashChargeVisuals.color = col;
        }

        while (DashChargeVisuals.color.a < 1.0f)
        {
            if (bAbortFadeIn)
            {
                break;
            }

            // Fade in over half a second
            AddAlpha((0.8f * Time.deltaTime) * 20); // hack just speed this up a bunch (20 instead of 2)
            yield return new WaitForSeconds(Time.deltaTime);
        }

        if (!bAbortFadeIn)
        {
            Color col = DashChargeVisuals.color;
            col.a = 1.0f;
            DashChargeVisuals.color = col;
        }

        yield return null;
    }
}
