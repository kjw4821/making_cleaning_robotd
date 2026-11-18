using System.Collections;
using UnityEngine;

public enum RobotDirection
{
    Right,
    Down,
    Left,
    Up
}

[DisallowMultipleComponent]
public class RobotController : MonoBehaviour
{
    [SerializeField] private MapManager mapManager;
    [SerializeField] private Transform robotStart;
    [SerializeField] private Transform visual;
    [SerializeField, Min(0.01f)] private float moveDuration = 0.25f;
    [SerializeField, Min(0.01f)] private float ruleInterval = 0.25f;

    private Coroutine automaticRules;

    public Vector3Int CurrentCell { get; private set; }
    public RobotDirection Direction { get; private set; } = RobotDirection.Right;
    public bool IsBusy { get; private set; }
    public bool IsInitialized { get; private set; }
    public Vector3Int ForwardCell => CurrentCell + GetDirectionOffset(Direction);

    private void Start()
    {
        ResetToStart();
    }

#if UNITY_EDITOR
    private void Update()
    {
        var keyboard = UnityEngine.InputSystem.Keyboard.current;
        if (keyboard == null)
            return;

        if (keyboard.pKey.wasPressedThisFrame)
        {
            if (automaticRules == null)
                StartAutomaticRules();
            else
                PauseAutomaticRules();
            return;
        }

        if (automaticRules != null)
            return;

        if (keyboard.nKey.wasPressedThisFrame)
            TryRunRuleStep();
        else if (keyboard.spaceKey.wasPressedThisFrame)
            TryMoveForward();
        else if (keyboard.rKey.wasPressedThisFrame)
            TryTurnRight();
        else if (keyboard.cKey.wasPressedThisFrame)
            TryClean();
    }
#endif

    public bool ResetToStart()
    {
        if (mapManager == null || !mapManager.IsConfigured || robotStart == null || visual == null)
        {
            Debug.LogError("RobotController requires a configured MapManager, RobotStart and Visual.", this);
            return false;
        }

        Vector3Int startCell = mapManager.WorldToCell(robotStart.position);
        if (!mapManager.CanMoveTo(startCell))
        {
            Debug.LogError($"RobotStart cell {startCell} must have a floor and no wall.", this);
            return false;
        }

        PauseAutomaticRules();
        StopAllCoroutines();
        CurrentCell = startCell;
        transform.position = mapManager.GetCellCenterWorld(startCell);
        visual.localPosition = Vector3.zero;
        IsBusy = false;
        IsInitialized = true;
        SetDirection(RobotDirection.Right);
        return true;
    }

    [ContextMenu("Move Forward (Play Mode)")]
    private void MoveForwardFromInspector()
    {
        TryMoveForward();
    }

    public bool TryMoveForward()
    {
        if (!Application.isPlaying || !isActiveAndEnabled || !IsInitialized || IsBusy)
            return false;

        Vector3Int targetCell = ForwardCell;
        if (!mapManager.CanMoveTo(targetCell))
            return false;

        IsBusy = true;
        StartCoroutine(MoveToCell(targetCell, mapManager.GetCellCenterWorld(targetCell)));
        return true;
    }

    private IEnumerator MoveToCell(Vector3Int targetCell, Vector3 targetPosition)
    {
        Vector3 startPosition = transform.position;
        float duration = Mathf.Max(0.01f, moveDuration);
        float elapsed = 0f;
        while (elapsed < duration)
        {
            yield return null;
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsed / duration);
        }

        CurrentCell = targetCell;
        IsBusy = false;
    }

    private void OnDisable()
    {
        PauseAutomaticRules();
        if (!IsBusy)
            return;

        StopAllCoroutines();
        transform.position = mapManager.GetCellCenterWorld(CurrentCell);
        IsBusy = false;
    }

    public bool TryRunRuleStep()
    {
        return automaticRules == null && RunRuleStep();
    }

    public void StartAutomaticRules()
    {
        if (!Application.isPlaying || !isActiveAndEnabled || !IsInitialized || automaticRules != null)
            return;

        automaticRules = StartCoroutine(RunAutomaticRules());
    }

    public void PauseAutomaticRules()
    {
        if (automaticRules == null)
            return;

        StopCoroutine(automaticRules);
        automaticRules = null;
    }

    private IEnumerator RunAutomaticRules()
    {
        while (true)
        {
            while (IsBusy)
                yield return null;

            RunRuleStep();

            while (IsBusy)
                yield return null;

            yield return new WaitForSeconds(Mathf.Max(0.01f, ruleInterval));
        }
    }

    private bool RunRuleStep()
    {
        if (!Application.isPlaying || !isActiveAndEnabled || !IsInitialized || IsBusy)
            return false;

        if (mapManager.HasDust(CurrentCell))
            return TryClean();

        if (!mapManager.CanMoveTo(ForwardCell))
            return TryTurnRight();

        return TryMoveForward();
    }

    public bool TryClean()
    {
        if (!Application.isPlaying || !isActiveAndEnabled || !IsInitialized || IsBusy)
            return false;

        return mapManager.TryRemoveDust(CurrentCell);
    }

    public bool TryTurnRight()
    {
        if (!Application.isPlaying || !isActiveAndEnabled || !IsInitialized || IsBusy)
            return false;

        RobotDirection nextDirection = Direction == RobotDirection.Up
            ? RobotDirection.Right
            : (RobotDirection)((int)Direction + 1);
        SetDirection(nextDirection);
        return true;
    }

    public void SetDirection(RobotDirection direction)
    {
        // Reject invalid enum values before changing either state or appearance.
        GetDirectionOffset(direction);
        if (!IsInitialized || IsBusy)
            return;

        Direction = direction;
        visual.localRotation = Quaternion.Euler(0f, 0f, -90f * (int)direction);
    }

    public static Vector3Int GetDirectionOffset(RobotDirection direction)
    {
        switch (direction)
        {
            case RobotDirection.Right: return Vector3Int.right;
            case RobotDirection.Down: return Vector3Int.down;
            case RobotDirection.Left: return Vector3Int.left;
            case RobotDirection.Up: return Vector3Int.up;
            default: throw new System.ArgumentOutOfRangeException(nameof(direction), direction, null);
        }
    }
}
