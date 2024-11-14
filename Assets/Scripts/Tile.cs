using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    [SerializeField] float hoverAmount = 0.1f;
    [SerializeField] LayerMask obstacleLayer;
    [SerializeField] Color hoverColor;
    [SerializeField] Color selectedColor;
    private Color defaultColor;

    private Vector3 defaultScale;

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        defaultScale = transform.localScale;
        defaultColor = spriteRenderer.color;
    }

    void OnMouseEnter()
    {
        transform.localScale += Vector3.one * hoverAmount;
        spriteRenderer.color = hoverColor;
    }

    void OnMouseExit()
    {
        transform.localScale = defaultScale;
        spriteRenderer.color = defaultColor;
    }

    public bool IsWalkable()
    {
        return !Physics2D.OverlapCircle(transform.position, 0.2f, obstacleLayer);
    }
}
