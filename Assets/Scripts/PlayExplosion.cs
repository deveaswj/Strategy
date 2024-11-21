using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayExplosion : MonoBehaviour
{
    public event System.Action<PlayExplosion> OnExplosionComplete;

    Animator animator;
    SpriteRenderer spriteRenderer;
    float animationLength;

    void Awake()
    {
        Debug.Log("Awake " + gameObject.name);
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void OnEnable()
    {
        Debug.Log("OnEnable " + gameObject.name);
        animationLength = animator.GetCurrentAnimatorStateInfo(0).length;
        ResetExplosion();
    }

    public void ResetExplosion()
    {
        spriteRenderer.enabled = false;
        transform.position = Vector3.zero;
        animator.ResetTrigger("Explode");
    }

    public void Explode()
    {
        StartCoroutine(ShowExplosion());
    }

    public void Explode(Vector3 position)
    {
        transform.position = position;
        Explode();
    }

    IEnumerator ShowExplosion()
    {
        Debug.Log("Exploding " + gameObject.name + " at " + transform.position + "for " + animationLength + " seconds");
        spriteRenderer.enabled = true;
        animator.SetTrigger("Explode");
        yield return new WaitForSeconds(animationLength);
        spriteRenderer.enabled = false;

        OnExplosionComplete?.Invoke(this);
    }
}
