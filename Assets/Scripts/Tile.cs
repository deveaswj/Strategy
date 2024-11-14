using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    [SerializeField] float hoverAmount = 0.1f;
    [SerializeField] LayerMask obstacleLayer;
    [SerializeField] Color hoverColor;
    [SerializeField] Color highlightColor;
    private Color defaultColor;

    private Vector3 defaultScale;

    GameMaster gm;
    bool isWalkable = false;

    // Start is called before the first frame update
    void Start()
    {
        gm = FindObjectOfType<GameMaster>();
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
        spriteRenderer.color = isWalkable ? highlightColor : defaultColor;
    }

    public bool IsClear()
    {
        return !Physics2D.OverlapCircle(transform.position, 0.2f, obstacleLayer);
    }

    public void Highlight()
    {
        Debug.Log("Highlight tile: " + gameObject.name);
        spriteRenderer.color = highlightColor;
        isWalkable = true;
    }

    public void Reset()
    {
        Debug.Log("Reset tile: " + gameObject.name);
        spriteRenderer.color = defaultColor;
        isWalkable = false;
    }
}
