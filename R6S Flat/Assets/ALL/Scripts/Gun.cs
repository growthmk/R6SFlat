﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GunStats
{
    [Header("Gun ID---------")]
    public string ID;
    [Tooltip("SMG, Rifle, Pistol, MG, Shotgun, Electric, None")]
    public string ammoType;
    [Header("Stats----------")]

    public GameObject bulletLR;
    public GameObject muzzleFlash;
    public Color bulletColor;
    [HideInInspector]
    public Transform barrelEnd;
    public float damage;
    public float accuracyOffset;
    public float fireRate;
    [Tooltip("Must be long enought to hit at least something")]
    public float distance = 99;
    public LayerMask hitLayers;
    public LayerMask damageLayers;
    //[HideInInspector]
    public float fireRate_timer;
    public int magSize;
    public int magSize_counter;
    [Tooltip("(SECONDS)")]
    public float reloadSpeed;
    //[HideInInspector]
    public float reloadSpeed_timer;
    public bool fullAuto = true;
    [Header("fullAuto == false")]
    public float cameraShakeDuration;
}


public class Gun : MonoBehaviour
{
    public GunStats STATS;
    [HideInInspector]
    public bool reloading;
    // Start is called before the first frame update
    void Start()
    {
        STATS.magSize_counter = STATS.magSize;
        STATS.reloadSpeed_timer = STATS.reloadSpeed;
        STATS.barrelEnd = transform.parent.GetComponent<PlayerStats>().barrelEnd;
    }

    private void Update()
    {
        if (STATS.magSize_counter <= 0 || Input.GetKeyDown("r"))
            reloading = true;

        if (reloading)
        {
            //Debug.Log("Reloading");
            STATS.reloadSpeed_timer -= Time.deltaTime;
            if (STATS.reloadSpeed_timer <= 0)
            {
                STATS.magSize_counter = STATS.magSize;
                STATS.reloadSpeed_timer = STATS.reloadSpeed;
                reloading = false;
            }
        }

        

        if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
            UTIL.GetPlayer().GetComponent<Animator>().SetBool("Aim", true);
        else
            UTIL.GetPlayer().GetComponent<Animator>().SetBool("Aim", false);


        if (STATS.fullAuto)
        {
            if (Input.GetMouseButton(0) && STATS.fireRate_timer <= 0 && !reloading)
            {
                Shoot();
                ShakeCamera(true);
            }
            else
                ShakeCamera(false);
        }
        else
        {
            if (Input.GetMouseButtonDown(0) && STATS.fireRate_timer <= 0 && !reloading)
            {
                Shoot();
                ShakeCamera(STATS.cameraShakeDuration);
            }
        }
        STATS.fireRate_timer -= Time.deltaTime;

    }

    void Shoot()
    {
        float temp_ao = STATS.accuracyOffset * UTIL.FastDist(transform.position, UTIL.MousePos(), 0.1f);

        Vector3 targetPos = UTIL.MousePos() + new Vector3(Random.Range(-temp_ao, temp_ao), Random.Range(-temp_ao, temp_ao));
        Vector3 fireDir = targetPos - transform.position;
        RaycastHit2D shot = Physics2D.Raycast(STATS.barrelEnd.position, fireDir, STATS.distance, STATS.hitLayers);
        if (shot.collider != null)
        {
            if (STATS.damageLayers == (STATS.damageLayers | (1 << shot.collider.gameObject.layer)))
            {
                //Debug.Log("hit");
                if (shot.collider.gameObject.GetComponent<PlayerStats>() != null)
                {
                    shot.collider.gameObject.GetComponent<PlayerStats>().TakeDamage(STATS.damage);
                }
            }
            //Debug.Log("hit");
            LineRenderer lr = Instantiate(STATS.bulletLR, transform.position, Quaternion.identity).GetComponent<LineRenderer>();
            lr.SetPosition(0, transform.position);
            lr.SetPosition(1, shot.point);

            Instantiate(STATS.muzzleFlash, STATS.barrelEnd.position, UTIL.GetPlayer().transform.rotation);

            STATS.fireRate_timer = STATS.fireRate;
            STATS.magSize_counter -= 1;
        }
    }

    void ShakeCamera(bool shake)
    {
        Debug.Log(1);

        if (shake)
        {
            UTIL.GetPlayer().GetComponent<CameraShake>().Shake(true);
            UTIL.GetPlayer().GetComponent<CameraShake>().Shake(-1);
        }
        else
        {
            UTIL.GetPlayer().GetComponent<CameraShake>().Shake(false);
            UTIL.GetPlayer().GetComponent<CameraShake>().Shake(0f);
        }
    }
    void ShakeCamera(float duration)
    {
        UTIL.GetPlayer().GetComponent<CameraShake>().Shake(duration);
    }
}
