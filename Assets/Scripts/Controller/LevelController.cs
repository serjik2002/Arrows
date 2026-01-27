using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelController : MonoBehaviour
{
    [Header("Посилання")]
    [SerializeField] private LevelView _levelView;
    [SerializeField] private GridView _gridView;
    [SerializeField] private InputHandler _inputHandler;

    [Header("Камера")]
    [SerializeField] private CameraController _cameraController;

    private LevelModel _currentLevel;
    private GridCoordinate _blockedCell;

    public LevelModel CurrentLevel => _currentLevel;
    public event Action OnLevelCompleted;

    private void OnEnable()
    {
        if (_inputHandler != null)
        {
            _inputHandler.OnGridClick += ProcessCellClick;
        }
    }

    private void OnDisable()
    {
        if (_inputHandler != null)
        {
            _inputHandler.OnGridClick -= ProcessCellClick;
        }
    }

    #region Public Methods

    public void Initialize(LevelModel levelModel)
    {
        _currentLevel = levelModel;
        _gridView.Init(levelModel.Height, levelModel.Width);
        if (levelModel != null)
        {
            _levelView.RenderLevel(levelModel);
        }
        _cameraController.FitToGrid(_currentLevel.Height, _currentLevel.Width, _gridView.CellSize, _gridView.StartPosition);
    }

    #endregion

    private void ProcessCellClick(GridCoordinate coord)
    {
        if (_currentLevel == null)
        {
            Debug.LogWarning("[LevelController] Cannot process click - level not initialized");
            return;
        }

        int arrowId = _currentLevel.GetArrowIdAt(coord);

        if (arrowId <= 0)
        {
            // Empty cell clicked
            return;
        }

        if (_currentLevel.CanArrowFlyAway(arrowId, out _blockedCell))
        {
            HandleArrowRemoval(arrowId);
        }
        else
        {
            HandleBlockedArrow(arrowId);
        }
    }

    private void HandleArrowRemoval(int arrowId)
    {
        Debug.Log($"[LevelController] Arrow {arrowId} can fly away!");

        // Remove from model
        _currentLevel.RemoveArrow(arrowId);

        // Remove visual representation
        _levelView.RemoveVisualArrow(arrowId);

        // Check for level completion
        CheckLevelCompletion();
    }

    private void HandleBlockedArrow(int arrowId)
    {
        Debug.Log($"[LevelController] Arrow {arrowId} is blocked!");

        // TODO: Add shake animation or visual feedback
        _levelView.AnimateBlockedArrow(arrowId, _blockedCell.ToVector2Int());
    }

    #region Level Completion

    private void CheckLevelCompletion()
    {
        if (_currentLevel.Arrows.Count == 0)
        {
            CompleteLevel();
        }
    }

    private void CompleteLevel()
    {
        Debug.Log("[LevelController] LEVEL COMPLETE!");

        // Disable input to prevent clicks during completion
        SetInputActive(false);

        // Notify listeners
        OnLevelCompleted?.Invoke();
    }

    #endregion

    public void SetInputActive(bool active)
    {
        if (_inputHandler != null)
        {
            _inputHandler.SetInputActive(active);
        }
    }

}
