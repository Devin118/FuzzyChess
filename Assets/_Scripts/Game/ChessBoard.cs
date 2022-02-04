using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CreateHighlighters))]
public class ChessBoard : MonoBehaviour
{
    [SerializeField] private Transform bottomLeftMarker;
    [SerializeField] private float squareSize;
    public const int BOARD_SIZE = 8;
    private Piece[,] grid;
    private Piece selectedPiece;
    private GameController controller;
    private CreateHighlighters highlighter;
    private readonly List<String> pieceMoves = new List<String>();

    private void Awake()
    {
        highlighter = GetComponent<CreateHighlighters>();
        CreateBoardGrid();
    }

    public void SetDependencies(GameController controller)
    {
        this.controller = controller;
    }

    private void CreateBoardGrid()
    {
        grid = new Piece[BOARD_SIZE, BOARD_SIZE];
    }

    public Vector3 GetPositionFromCoords(Vector2Int coords)
    {
        return bottomLeftMarker.position + new Vector3(coords.x * squareSize, 0f, coords.y * squareSize);
    }

    private Vector2Int GetCoordsFromPosition(Vector3 inputPosition)
    {
        int x = Mathf.FloorToInt(inputPosition.x / squareSize) + BOARD_SIZE / 2;
        int y = Mathf.FloorToInt(inputPosition.z / squareSize) + BOARD_SIZE / 2;
        return new Vector2Int(x, y);
    }

    //modified by TW
    public void OnSquareSelected(Vector3 inputPosition)
    {
        Vector2Int coords = GetCoordsFromPosition(inputPosition);
        Piece piece = GetPieceOnSquare(coords);
        if (selectedPiece)
        {
            if (piece != null && selectedPiece == piece)
                DeselectPiece();
            else if (piece != null && selectedPiece != piece && controller.IsTeamTurnActive(piece.team))
                SelectPiece(piece);

            else if (selectedPiece.CanMoveTo(coords))
            {
                //Debug.Log("moved to" + coords);
                //Debug.Log("Selected piece" + selectedPiece.GetType());
                String test = selectedPiece.GetType().ToString() + "|" + coords.ToString();
                //Debug.Log("concat: " + test);
                pieceMoves.Add(test);
                OnSelectedPieceMoved(coords, selectedPiece);

                // NOTE TO MYSELF: Though the 3-turn logic isn't implemented yet, the functionality of piece colors could
                // be rewritten simply by calling the color change function every 3 turns instead of every end turn,
                // and checking the above for the "skip" button to be pressed. If pressed, change color immediately after
                // the turn, no matter what
                
                //Call on the GameUI script to get an object from it. 
                //Commented out since I realized you can just do this every time the turn ends,
                //meaning waiting to live update each time the menu is opened is kind of pointless.
                /**
                GameUI TheGameUI = GameObject.Find("UI").GetComponent<GameUI>();
                Boolean state = TheGameUI.GetMoveListState();
                if (state)
                    TheGameUI.updateMoveList();
                **/
            }
        }
        else
        {
            if (piece != null && controller.IsTeamTurnActive(piece.team))
                SelectPiece(piece);
        }
    }

    private void SelectPiece(Piece piece)
    {
        selectedPiece = piece;
        List<Vector2Int> selection = selectedPiece.AvailableMoves;
        ShowSelectionSquares(selection);
    }

    private void ShowSelectionSquares(List<Vector2Int> selection)
    {
        Dictionary<Vector3, bool> squaresData = new Dictionary<Vector3, bool>();
        for (int i = 0; i < selection.Count; i++)
        {
            Vector3 position = GetPositionFromCoords(selection[i]);
            bool isSquareFree = GetPieceOnSquare(selection[i]) == null;
            squaresData.Add(position, isSquareFree);
        }
        highlighter.ShowAvailableMoves(squaresData);
    }

    public void DeselectPiece()
    {
        selectedPiece = null;
        highlighter.ClearMoves();
    }

    private void OnSelectedPieceMoved(Vector2Int coords, Piece piece)
    {
        TryToTakeOppositePiece(coords);
        UpdateBoardOnPieceMove(coords, piece.occupiedSquare, piece, null);
        selectedPiece.MovePiece(coords);
        DeselectPiece();
        EndTurn();
    }

    private void TryToTakeOppositePiece(Vector2Int coords)
    {
        Piece piece = GetPieceOnSquare(coords);
        if (piece != null && !selectedPiece.IsFromSameTeam(piece))
            TakePiece(piece);
    }

    private void TakePiece(Piece piece)
    {
        if (piece)
        {
            grid[piece.occupiedSquare.x, piece.occupiedSquare.y] = null;
            controller.OnPieceRemoved(piece);
        }
    }

    //modified to send a signal to the swapcolor function of GameUI in order to change the color of sprites per team
    private void EndTurn()
    {
        GameUI TheGameUI = GameObject.Find("UI").GetComponent<GameUI>();
        TheGameUI.updateMoveList();
        TheGameUI.SwapPieceColor();
        controller.EndTurn();
    }

    public void UpdateBoardOnPieceMove(Vector2Int newCoords, Vector2Int oldCoords, Piece newPiece, Piece oldPiece)
    {
        grid[oldCoords.x, oldCoords.y] = oldPiece;
        grid[newCoords.x, newCoords.y] = newPiece;
    }

    public Piece GetPieceOnSquare(Vector2Int coords)
    {
        if (CheckIfCoordsAreOnBoard(coords))
            return grid[coords.x, coords.y];
        return null;
    }

    public bool CheckIfCoordsAreOnBoard(Vector2Int coords)
    {
        if (coords.x < 0 || coords.y < 0 || coords.x >= BOARD_SIZE || coords.y >= BOARD_SIZE)
            return false;
        return true;
    }

    public int GetNumberOfPieceMoves()
    {
        return pieceMoves.Count;
    }

    public bool HasPiece(Piece piece)
    {
        for (int i = 0; i < BOARD_SIZE; i++)
        {
            for (int j = 0; j < BOARD_SIZE; j++)
            {
                if (grid[i, j] == piece)
                    return true;
            }
        }
        return false;
    }

    public void SetPieceOnBoard(Vector2Int coords, Piece piece)
    {
        if (CheckIfCoordsAreOnBoard(coords))
            grid[coords.x, coords.y] = piece;
    }

    //TW - public method to return array which only contains new piece movements, 
    //discarding the current piecemoves array. Should save on computing power.
    public List<String> GetNewPieceMoves()
    {
        List<String> newPieceMoves = new List<String>(pieceMoves);
        pieceMoves.Clear();
        return newPieceMoves;
    }
}
