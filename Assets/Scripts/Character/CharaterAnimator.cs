using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharaterAnimator : MonoBehaviour
{
    [SerializeField] List<Sprite> walkDownSprite;
    [SerializeField] List<Sprite> walkUpSprite;
    [SerializeField] List<Sprite> walkLeftSprite;
    [SerializeField] List<Sprite> walkRightSprite;
    [SerializeField] FacingDirection defaultDirection = FacingDirection.Down;

    // Paramenter
    public float MoveX { get; set; }
    public float MoveY { get; set; }
    public bool IsMoving { get; set; }

    SpriteAnimator currentAnim;

    // States
    SpriteAnimator walkDownAnim;
    SpriteAnimator walkUpAnim;
    SpriteAnimator walkLeftAnim;
    SpriteAnimator walkRightAnim;

    //Refrences
    SpriteRenderer spriteRenderer;
    bool wasPreviouslyMoving;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        walkDownAnim = new SpriteAnimator(walkDownSprite, spriteRenderer);
        walkUpAnim = new SpriteAnimator(walkUpSprite, spriteRenderer);
        walkRightAnim = new SpriteAnimator(walkRightSprite, spriteRenderer);
        walkLeftAnim = new SpriteAnimator(walkLeftSprite, spriteRenderer);

        SetFacingDirection(defaultDirection);

        currentAnim = walkDownAnim;
    }

    private void Update()
    {
        var prevAnim = currentAnim;

        if(MoveX == 1)
        {
            currentAnim = walkRightAnim;
        }
        else if(MoveX == -1)
        {
            currentAnim = walkLeftAnim;
        }
        else if(MoveY == 1)
        {
            currentAnim = walkUpAnim;
        }
        else if (MoveY == -1)
        {
            currentAnim = walkDownAnim;
        }
        else
        {
            currentAnim = null;
        }

        if (currentAnim != prevAnim || IsMoving != wasPreviouslyMoving)
        {
            currentAnim.Start();
        }

        if(IsMoving)
        {
            currentAnim.HandleUpdate();
        }
        else
        {
            spriteRenderer.sprite = currentAnim.Frames[0];
        }

        wasPreviouslyMoving = IsMoving;
    }

    public void SetFacingDirection(FacingDirection dir)
    {
        if(dir == FacingDirection.Right)
        {
            MoveX = 1;
        }
        else if (dir == FacingDirection.Left)
        {
            MoveX = -1;
        }
        else if(dir == FacingDirection.Down )
        {
            MoveY = -1;
        }
        else if(dir == FacingDirection.Up)
        {
            MoveY = 1;
        }

    }

    public FacingDirection DefaultDirection
    {
        get => defaultDirection;
    }
}

public enum FacingDirection { Up, Down, Left, Right }