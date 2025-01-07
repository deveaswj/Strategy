using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealIcon : MonoBehaviour
{
    [SerializeField] Sprite[] numericSprites;
    [SerializeField] float lifetime = 1.5f;

    SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void ShowHealing(int healing, Vector3 position)
    {
        transform.position = position;
        ShowHealing(healing);
    }

    public void ShowHealing(int healing)
    {
        StartCoroutine(ShowHealingRoutine(healing));
    }

    IEnumerator ShowHealingRoutine(int healing)
    {
        healing = Mathf.Clamp(Mathf.Abs(healing), 0, numericSprites.Length - 1);
        sr.sprite = numericSprites[healing];
        sr.enabled = true;
        yield return new WaitForSeconds(lifetime);
        sr.enabled = false;

        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}
