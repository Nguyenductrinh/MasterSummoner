using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

public class Character : MonoBehaviour
{
    public float moveSpeed;

    public bool IsMoving {  get; private set; }

    public float OffsetY { get; private set; } = 0.3f;

    CharaterAnimator animator;

    private void Awake()
    {
        animator = GetComponent<CharaterAnimator>();
        SetPositionAndSnapToTile(transform.position);
    }

    public void SetPositionAndSnapToTile(Vector2 pos)
    {
        pos.x = Mathf.Floor(pos.x) + 0.5f;
        pos.y = Mathf.Floor(pos.y) + 0.5f + OffsetY;

        transform.position = pos;
    }

    public IEnumerator Move(Vector2 moveVec, Action OnMoveOver)
    {
        animator.MoveX = Mathf.Clamp(moveVec.x, -1f, 1f);
        animator.MoveY = Mathf.Clamp(moveVec.y, -1f, 1f);

        var targetPos = transform.position;
        targetPos.x += moveVec.x;
        targetPos.y += moveVec.y;

        if (! IsPathClear(targetPos))
        {
            yield break;
        }

        IsMoving = true;

        // Di chuyển đến khi đạt tới vị trí mục tiêu
        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPos; // Đảm bảo vị trí chính xác sau khi di chuyển

        IsMoving = false;

        OnMoveOver?.Invoke();
    }

    public void HandleUpdate()
    {
        animator.IsMoving = IsMoving;
    }

    private bool IsPathClear(Vector3 targetPos)
    {
        var diff = targetPos - transform.position;  
        var dir = diff.normalized;
        var collisionLayer = GameLayers.i.SolidLayer | GameLayers.i.InteractebLayer | GameLayers.i.PlayerLayer | GameLayers.i.WaterLayer;

        if (Physics2D.BoxCast(transform.position + dir, new Vector2(0.2f, 0.2f), 0f, dir, diff.magnitude - 1, collisionLayer) == true)
        {
            return false;
        }
        return true;
    }

    private bool IsWalkable(Vector3 targetPos)
    {
        // Kiểm tra xem vị trí có collider của layer `solidObjectsLayer` không
        if (Physics2D.OverlapCircle(targetPos, 0.2f,GameLayers.i.SolidLayer | GameLayers.i.InteractebLayer) != null)
        {
            return false;
        }
        return true;
    }

    public void LookTowards(Vector3 tagetPos)
    {
        var xdiff = Mathf.Floor(tagetPos.x) - Mathf.Floor(transform.position.x);
        var ydiff = Mathf.Floor(tagetPos.y) - Mathf.Floor(transform.position.y);

        if(xdiff == 0 || ydiff == 0)
        {
            animator.MoveX = Mathf.Clamp(xdiff, -1f, 1f);
            animator.MoveY = Mathf.Clamp(ydiff, -1f, 1f);
        }
        else
        {
            Debug.Log("Erro in look towards! You can't ask the character to look diagonally");
        }
    }

    public CharaterAnimator Animator { 
        get => animator; 
    } 
}
