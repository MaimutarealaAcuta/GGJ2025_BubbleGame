using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleMotionScript : MonoBehaviour
{
    public enum BubbleMovementType
    {
        Stationary,
        Floating,
        Moving
    }

    [SerializeField]
    internal BubbleMovementType movementType;


    private Vector3 _InitialPosition;

    private Vector3 InitialPosition
    {
        get
        {
            return _InitialPosition;
        }
        set
        {
            _InitialPosition = value;
            transform.position = value;
        }
    }

    private Vector3 NextPosition;

    #region Stationary properties
    [SerializeField]
    [Range(0.1f, 5.0f)]
    internal float PositionRangeX;

    [SerializeField]
    [Range(0.1f, 5.0f)]
    internal float PositionRangeY;

    [SerializeField]
    [Range(0.1f, 5.0f)]
    internal float PositionRangeZ;

    [SerializeField]
    [Range(0.01f, 1.0f)]
    internal float StationarySpeed = 0.1f;

    #endregion

    #region Floating properties
    [SerializeField]
    [Range(0f, 5.0f)]
    internal float MaxX;

    [SerializeField]
    [Range(0f, 5.0f)]
    internal float MaxY;

    [SerializeField]
    [Range(0f, 5.0f)]
    internal float MaxZ;

    [SerializeField]
    internal float FloatingSpeed = 0.1f;
    #endregion

    #region Moving properties

    #endregion

    void Start()
    {
        InitialPosition = transform.position;

        NextPosition = InitialPosition + new Vector3(
                Random.Range(-PositionRangeX, PositionRangeX),
                Random.Range(-PositionRangeY, PositionRangeY),
                Random.Range(-PositionRangeZ, PositionRangeZ)
            );
    }

    private void OnEnable()
    {
        InitialPosition = transform.position;
        NextPosition = InitialPosition + new Vector3(
                Random.Range(-PositionRangeX, PositionRangeX),
                Random.Range(-PositionRangeY, PositionRangeY),
                Random.Range(-PositionRangeZ, PositionRangeZ)
            );
    }

    // Update is called once per frame
    void Update()
    {
        switch(movementType)
        {
            case BubbleMovementType.Stationary:
                BrownianMotion();
                break;
            case BubbleMovementType.Floating:
                FloatingMotion();
                break;
            case BubbleMovementType.Moving:
                break;
        }      
    }

    void BrownianMotion()
    {
        if ((transform.position - NextPosition).magnitude < 0.1)
        {
            NextPosition = InitialPosition + new Vector3(
                Random.Range(-PositionRangeX, PositionRangeX),
                Random.Range(-PositionRangeY, PositionRangeY),
                Random.Range(-PositionRangeZ, PositionRangeZ)
            );
        }

        transform.position = new Vector3(
            Mathf.Lerp(transform.position.x, NextPosition.x, StationarySpeed * Time.deltaTime),
            Mathf.Lerp(transform.position.y, NextPosition.y, StationarySpeed * Time.deltaTime),
            Mathf.Lerp(transform.position.z, NextPosition.z, StationarySpeed * Time.deltaTime)
        );
    }

    void FloatingMotion()
    {
        transform.transform.position = new Vector3(
            InitialPosition.x + Mathf.Sin(Time.time) * MaxX * FloatingSpeed,
            InitialPosition.y + Mathf.Sin(Time.time) * MaxY * FloatingSpeed,
            InitialPosition.z + Mathf.Sin(Time.time) * MaxZ * FloatingSpeed
        );
    }

    public void MoveTowards(Vector3 target, float speed)
    {
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
    }

    public void MoveAway(Vector3 target, float speed)
    {
        transform.position = Vector3.MoveTowards(transform.position, target, -speed * Time.deltaTime);
    }
}
