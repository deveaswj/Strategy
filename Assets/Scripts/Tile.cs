using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    [SerializeField] float hoverAmount = 0.1f;
    [SerializeField] LayerMask obstacleLayer;

    private Vector3 defaultScale;

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        defaultScale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnMouseEnter()
    {
        transform.localScale += Vector3.one * hoverAmount;
    }

    void OnMouseExit()
    {
        transform.localScale = defaultScale;
    }

    public bool IsWalkable()
    {
        return !Physics2D.OverlapCircle(transform.position, 0.2f, obstacleLayer);
    }
}
