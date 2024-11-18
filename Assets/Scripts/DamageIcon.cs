using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageIcon : MonoBehaviour
{
    [SerializeField] Sprite[] numericSprites;
    [SerializeField] float lifetime = 1.5f;

    SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void ShowDamage(int damage, Vector3 position)
    {
        transform.position = position;
        ShowDamage(damage);
    }

    public void ShowDamage(int damage)
    {
        StartCoroutine(ShowDamageRoutine(damage));
    }

    IEnumerator ShowDamageRoutine(int damage)
    {
        damage = Mathf.Clamp(Mathf.Abs(damage), 0, numericSprites.Length - 1);
        sr.sprite = numericSprites[damage];
        sr.enabled = true;
        yield return new WaitForSeconds(lifetime);
        sr.enabled = false;

        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}
