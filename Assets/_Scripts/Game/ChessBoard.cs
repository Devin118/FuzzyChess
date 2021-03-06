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
    public const int NUMBER_OF_ACTIONS = 6;
    private Piece[,] grid;
    public Piece selectedPiece;
    private GameController controller;
    private CreateHighlighters highlighter;
    private readonly List<String> pieceMoves = new List<String>();
    //same as the above list, but serves a different purpose
    private List<Vector2Int> pieceMoves2 = new List<Vector2Int>();
    private bool pieceTaken = false;
    //stores the piece which was delegated during this turn. var cleared when turn is over.
    public Piece delegatedPiece = null;
    private bool canCapture = false, willCapture = false;
    private bool knightHasMoved = false, knightAttemptedKill = false, commanderAttemptedKill = false;
    private bool leftBishopMovedOne = false, kingMovedOne = false, rightBishopMovedOne = false;
    public bool pieceDelegatedThisTurn = false;
    public bool LeftBishopMovedOne { get { return leftBishopMovedOne; } set { leftBishopMovedOne = value; } }
    public bool KingMovedOne { get { return kingMovedOne; } set { kingMovedOne = value; } }
    public bool RightBishopMovedOne { get { return rightBishopMovedOne; } set { rightBishopMovedOne = value; } }
    //this vector2int is used to store a delegated move in the piecemoves2 list, which will store delegations and allow for them to be undone in order.
    public readonly Vector2Int delegatedMove = new Vector2Int(999, 999);
    private String saveKnightPos = "";

    private const string DICE = "ResultDie";
    public bool selectedPieceMoved { get; set; }
    public bool acceptingInputs { get; set; }

    private void Awake()
    {
        highlighter = GetComponent<CreateHighlighters>();
        CreateBoardGrid();
        acceptingInputs = true;
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

    public Vector2Int GetCoordsFromPosition(Vector3 inputPosition)
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

        if (isSelectable(piece) && piece)
            controller.TryToChangeActiveCorp(piece.corpType);

        if (selectedPiece)
        {
            if (piece != null && selectedPiece == piece && knightHasMoved == false)
                DeselectPiece();

            else if (piece != null && selectedPiece != piece && controller.IsCorpTurnActive(piece.corpType) && knightHasMoved == false && isSelectable(piece))

                SelectPiece(piece, piece.corpType);


            else if (selectedPiece.CanMoveTo(coords) && acceptingInputs)
            {
                OnSelectedPieceMoved(coords, selectedPiece);
            }
        }
        else
        {
            if (piece != null && controller.IsCorpTurnActive(piece.corpType) && isSelectable(piece))

                SelectPiece(piece, piece.corpType);
        }
    }

    private void SelectPiece(Piece piece, CorpType corpType)

    {
        selectedPiece = piece;
        Player player = controller.activePlayer;
        
        //changes color of selectedpiece's corp while removing the color of non corp pieces
        if (selectedPiece.corpType == CorpType.Right)
        {
            foreach (Piece corpPiece in player.RightCorpPieces)
            {
                corpPiece.SetColor();
            }
            foreach (Piece corpPiece in player.LeftCorpPieces)
            {
                corpPiece.RevertColor();
            }
            foreach (Piece corpPiece in player.KingCorpPieces)
            {
                corpPiece.RevertColor();
            }
        }
        else if (selectedPiece.corpType == CorpType.King)
        {
            foreach (Piece corpPiece in player.KingCorpPieces)
            {
                corpPiece.SetColor();
            }
            foreach (Piece corpPiece in player.LeftCorpPieces)
            {
                corpPiece.RevertColor();
            }
            foreach (Piece corpPiece in player.RightCorpPieces)
            {
                corpPiece.RevertColor();
            }
        }
        else if (selectedPiece.corpType == CorpType.Left)
        {
            foreach (Piece corpPiece in player.LeftCorpPieces)
            {
                corpPiece.SetColor();
            }
            foreach (Piece corpPiece in player.KingCorpPieces)
            {
                corpPiece.RevertColor();
            }
            foreach (Piece corpPiece in player.RightCorpPieces)
            {
                corpPiece.RevertColor();
            }
        }

        List<Vector2Int> selection = selectedPiece.AvailableMoves;
        //FIX THIS to fix undoing a piece to turn 0 making it so this isn't updated, effectively blocking moves in the spot the piece was occupying.
        if (selectedPiece.CorpMoveNumber() >= 1 && !selectedPiece.CommanderMovedOne()) 
        {
            selection.Clear();
            selection.AddRange(selectedPiece.GetAdjacentSquares());
        } 
        //else if (selectedPiece.CorpMoveNumber() >= 0 && !selectedPiece.CommanderMovedOne()) selection.AddRange(selectedPiece.GetAdjacentSquares(selectedPiece.occupiedSquare));
        ShowSelectionSquares(selection);
    }


    public bool isSelectable(Piece piece)

    {
        if (piece && controller.IsTeamTurnActive(piece.team) &&
            (piece.CorpMoveNumber() < 1 || piece.pieceType == PieceType.Bishop || 
            piece.pieceType == PieceType.King || piece.CommanderMovedOne()) && piece.CorpMoveNumber() < 2)
            return true;
        else return false;

    }

    public bool CanDelegate(CorpType corpType) 
    {
        Player player = controller.activePlayer;
        if (selectedPiece.corpType == CorpType.King && selectedPiece.pieceType != PieceType.King
            && knightHasMoved == false && pieceDelegatedThisTurn == false)
        {
            if (corpType == CorpType.Left && !player.LeftBishopIsDead && player.LeftCorpPieces.Count <= 6)
                return true;
            else if (corpType == CorpType.Right && !player.RightBishopIsDead && player.RightCorpPieces.Count <= 6)
                return true;
        }
        return false;
    }

    public void Delegate(CorpType corpType) 
    {
        Player player = controller.activePlayer;
        foreach (Piece corpPiece in player.KingCorpPieces) 
        {
            if (selectedPiece.Equals(corpPiece))
            {
                selectedPiece.prevCorpType = selectedPiece.corpType;
                player.RemovePiece(selectedPiece);

                if (corpType == CorpType.Left)
                    selectedPiece.corpType = CorpType.Left;
                else selectedPiece.corpType = CorpType.Right;
                selectedPiece.isDelegated = true;

                player.AddPiece(selectedPiece);
                delegatedPiece = selectedPiece;

                //add an entry into the moves list to allow for accurate reversion
                pieceMoves2.Add(delegatedMove);
                pieceMoves2.Add(delegatedMove);
                break;
            }
        }
        pieceDelegatedThisTurn = true;
        DeselectPiece();
    }

    public bool CanRecall() 
    {
        if (selectedPiece.corpType != CorpType.King && selectedPiece.pieceType != PieceType.Bishop
            && knightHasMoved == false && pieceDelegatedThisTurn == false && selectedPiece.isDelegated)
            return true;
        else return false;
    }

    public void Recall()
    {
        Player player = controller.activePlayer;
        if (selectedPiece.corpType == CorpType.Left)
        {
            foreach (Piece corpPiece in player.LeftCorpPieces)
            {
                if (selectedPiece.Equals(corpPiece))
                {
                    selectedPiece.prevCorpType = selectedPiece.corpType;
                    player.RemovePiece(selectedPiece);

                    selectedPiece.corpType = CorpType.King;
                    selectedPiece.isDelegated = false;

                    player.AddPiece(selectedPiece);
                    delegatedPiece = selectedPiece;

                    //add an entry into the moves list to allow for accurate reversion
                    pieceMoves2.Add(delegatedMove);
                    pieceMoves2.Add(delegatedMove);
                    break;
                }
            }
        }
        else if (selectedPiece.corpType == CorpType.Right) 
        {
            foreach (Piece corpPiece in player.RightCorpPieces) 
            {
                if (corpPiece == selectedPiece) 
                {
                    selectedPiece.prevCorpType = selectedPiece.corpType;
                    player.RemovePiece(selectedPiece);
                    selectedPiece.corpType = CorpType.King;
                    selectedPiece.isDelegated = false;
                    player.AddPiece(selectedPiece);
                    delegatedPiece = selectedPiece;

                    //add an entry into the moves list to allow for accurate reversion
                    pieceMoves2.Add(delegatedMove);
                    pieceMoves2.Add(delegatedMove);
                    break;
                }
            }
        }
        pieceDelegatedThisTurn = true;
        DeselectPiece();
    }

    //revert a delegation action of any kind on a single piece (works for both delegate and recall)
    public void RevertDelegation(Piece piece)
    {
        selectedPiece = piece;
        if(piece.prevCorpType == CorpType.Right || piece.prevCorpType == CorpType.Left)
        {
            Delegate(selectedPiece.prevCorpType);
        }
        else if (piece.prevCorpType == CorpType.King)
        {
            Recall();
        }
        pieceMoves2.RemoveAt(pieceMoves2.Count - 1);
        pieceMoves2.RemoveAt(pieceMoves2.Count - 1);
        DeselectPiece();
    }

    private void ShowSelectionSquares(List<Vector2Int> selection)
    {
        Dictionary<Vector3, bool> squaresData = new Dictionary<Vector3, bool>();
        for (int i = 0; i < selection.Count; i++)
        {
            Vector3 position = GetPositionFromCoords(selection[i]);
            bool isSquareFree = GetPieceOnSquare(selection[i]) == null;

            if (position == GetPositionFromCoords(selectedPiece.occupiedSquare))
                isSquareFree = true;

            squaresData.Add(position, isSquareFree);
        }
        highlighter.ShowAvailableMoves(squaresData);
    }

    public void DeselectPiece()
    {
        Player player = controller.activePlayer;

        //removes color once a piece is deselected outside of changing to another piece
        if (selectedPiece)
        {
            if (selectedPiece.corpType == CorpType.Right)
            {
                foreach (Piece corpPiece in player.RightCorpPieces)
                {
                    corpPiece.RevertColor();
                }
            }
            else if (selectedPiece.corpType == CorpType.King)
            {
                foreach (Piece corpPiece in player.KingCorpPieces)
                {
                    corpPiece.RevertColor();
                }
            }
            else if (selectedPiece.corpType == CorpType.Left)
            {
                foreach (Piece corpPiece in player.LeftCorpPieces)
                {
                    corpPiece.RevertColor();
                }
            }
        }

        selectedPiece = null;
        highlighter.ClearMoves();
    }

    private void OnSelectedPieceMoved(Vector2Int coords, Piece piece)
    {
        Tooltip.HideTooltip_Static();
        Piece pieceOnSquare = GetPieceOnSquare(coords);
        TryToTakeOppositePiece(coords);

        //TryToTakeOppositePiece(coords);
        CheckIfCommanderMovedOne(coords);
        //String saveKnightPos = "";
        String test = "";

        if (selectedPiece.pieceType == PieceType.Knight && (!knightHasMoved || !knightAttemptedKill))
        {
            saveKnightPos = piece.occupiedSquare.ToString();
        }


        //Adds move to the move list
        if (selectedPiece.pieceType != PieceType.Knight || knightHasMoved || knightAttemptedKill || !selectedPiece.HasAdjacentEnemySquares(coords))
        {
            //if there's something on the target square, send its name as well as the other info
            if(pieceOnSquare != null)
            test = selectedPiece.GetType().ToString() + "|" + coords.ToString() + "|" + piece.occupiedSquare + "|" + pieceOnSquare.GetType().ToString();
            else
            //otherwise, send "empty" to signify there's nothing there
            test = selectedPiece.GetType().ToString() + "|" + coords.ToString() + "|" + piece.occupiedSquare + "|" + "empty";

            if (knightHasMoved || knightAttemptedKill)
            {
                if (pieceOnSquare != null)
                    test = selectedPiece.GetType().ToString() + "|" + coords.ToString() + "|" + saveKnightPos + "|" + pieceOnSquare.GetType().ToString();
                else
                    test = selectedPiece.GetType().ToString() + "|" + coords.ToString() + "|" + saveKnightPos + "|" + "empty";
                saveKnightPos = "";
            }
            pieceMoves.Add(test);
            //stores name of piece, the new coordinates, the old coordinates, in format: name|newcoords|oldcoords
            //String fullString = selectedPiece.GetType().ToString() + "|" + coords.ToString() + "|" + piece.occupiedSquare.ToString();

            //stores vector2int of new coords and original coords, in that order
            //also checks to make sure the piece being added to the "undoable" list hasn't attacked. if it has, then it can't undo its move.
            if(!pieceTaken)
            {
                pieceMoves2.Add(coords);
                pieceMoves2.Add(piece.occupiedSquare);
            }

            pieceTaken = false;
        }

        if (!canCapture || willCapture)
        {
            UpdateBoardOnPieceMove(coords, piece.occupiedSquare, piece, null);
            selectedPiece.MovePiece(coords);
        } 

        DeselectPiece();

        //If knight can move again, call the function to show the new available moves
        if (piece.pieceType == PieceType.Knight && !knightHasMoved && !knightAttemptedKill && piece.HasAdjacentEnemySquares(coords))
        {
            ShowKnightSelectionAfterMoving(piece);
        }
        else 
        {
            EndTurn();
            knightHasMoved = false;
            knightAttemptedKill = false;
        }
    }

    //see if commander used its authority in its move or not
    private void CheckIfCommanderMovedOne(Vector2Int coords) 
    {
        if (selectedPiece.pieceType == PieceType.King && MovedOneSquare(coords) && selectedPiece.CorpMoveNumber() == 0 && commanderAttemptedKill == false)
            kingMovedOne = true;
        else if (selectedPiece.pieceType == PieceType.Bishop && selectedPiece.corpType == CorpType.Left && MovedOneSquare(coords) &&
            selectedPiece.CorpMoveNumber() == 0 && commanderAttemptedKill == false)
            leftBishopMovedOne = true;
        else if (selectedPiece.pieceType == PieceType.Bishop && selectedPiece.corpType == CorpType.Right && MovedOneSquare(coords) &&
            selectedPiece.CorpMoveNumber() == 0 && commanderAttemptedKill == false)
            rightBishopMovedOne = true;

        commanderAttemptedKill = false;
    }

    //simply changes the commandermovedone bool to false
    public void UndoCommanderMovedOne(Piece piece)
    {
        if (piece.corpType == CorpType.King)
        {
            kingMovedOne = false;
        }
        else if (piece.corpType == CorpType.Left)
        {
            leftBishopMovedOne = false;
        }
        else if (piece.corpType == CorpType.Right)
        {
            rightBishopMovedOne = false;
        }
    }

    private bool MovedOneSquare(Vector2Int coords) 
    {
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(-1, 1),
            new Vector2Int(0, 1),
            new Vector2Int(1, 1),
            new Vector2Int(-1, 0),
            new Vector2Int(1, 0),
            new Vector2Int(-1, -1),
            new Vector2Int(0, -1),
            new Vector2Int(1, -1),
        };
        foreach (var direction in directions)
        {
            Vector2Int nextDirection = coords + direction;
            if (nextDirection == selectedPiece.occupiedSquare)
                return true;
        }
        return false;
    }

    public void ResetCommanderData() 
    {
        LeftBishopMovedOne = false;
        KingMovedOne = false;
        RightBishopMovedOne = false;
        pieceDelegatedThisTurn = false;
    }

    private void ShowKnightSelectionAfterMoving(Piece piece)
    {
        knightHasMoved = true;
        piece.AvailableMoves.Clear();
        List<Vector2Int> newKnightMoves = piece.GetAdjacentEnemySquares(piece.occupiedSquare);
        piece.AvailableMoves.AddRange(newKnightMoves);
        piece.AvailableMoves.Add(piece.occupiedSquare);
        SelectPiece(piece, piece.corpType);
    }

    private void TryToTakeOppositePiece(Vector2Int coords)
    {
        Piece piece = GetPieceOnSquare(coords);
        canCapture = false;
        if (piece != null && !selectedPiece.IsFromSameTeam(piece))
        {
            canCapture = true;
            //GameManager.Instance.UpdateRollState(RollState.TrueRoll);

            bool b1 = selectedPiece.GetComponent<King>();
            bool b2 = selectedPiece.GetComponent<Queen>();
            bool b3 = selectedPiece.GetComponent<Knight>();
            bool b4 = selectedPiece.GetComponent<Rook>();
            bool b5 = selectedPiece.GetComponent<Bishop>();
            bool b6 = selectedPiece.GetComponent<Pawn>();

            bool b7 = piece.GetComponent<King>();
            bool b8 = piece.GetComponent <Queen>();
            bool b9 = piece.GetComponent<Knight>();
            bool b10 = piece.GetComponent<Rook>();
            bool b11 = piece.GetComponent<Bishop>();
            bool b12 = piece.GetComponent<Pawn>();

            int result = UnityEngine.Random.Range(1, 7);
            GameUI TheGameUI = GameObject.Find("UI").GetComponent<GameUI>();
            TheGameUI.SendMessage(DICE, result);

            //adding a dice roll sound isn't going to work unless we slow down the game for a few seconds
            //since it will play at the same time as the capture/movement sounds anyway
            
            //SFXController.PlaySoundDiceRoll();

            //If knight does surprise attack add a +1 to the die roll
            if (knightHasMoved) 
            {
                result++;
                GameObject KnightScreen = GameObject.Find("KnightScreen");
                GameObject knightbonus = KnightScreen.transform.GetChild(0).gameObject;
                knightbonus.SetActive(true);
            }

            Debug.Log("Result: " + result);
            bool take = false;

            // Okay, hear me out. I know this is unreadable, but it works, and it was the quickest way I could bodge together a solution in time. Thank you for understanding. If we want to fix this in the next sprint, I can cover it - NW

            if (result >= 6)
            {
                if (b6)
                {
                    if (b7 || b8 || b9 || b10) { take = true; }
                }
            }
            if (result >= 5)
            {
                if (b10)
                {
                    if (b1 || b2 || b3 || b4 || b5) { take = true; }
                }
                if (b3)
                {
                    if (b7 || b8 || b9 || b11) { take = true; }
                }
                if (b5)
                {
                    if (b7 || b8 || b9) { take = true; }
                }
                if (b4)
                {
                    if (b11 || b12) { take = true; }
                }
                if (b6 && b11) { take = true; }
            }
            if (result >= 4)
            {
                if (b1 || b2)
                {
                    if (b7 || b8 || b9 || b11) { take = true; }
                }
                if (b5 && b11) { take = true; }
                if (b4)
                {
                    if (b7 || b8 || b9) { take = true; }
                }
                if (b6 && b12) { take = true; }
            }
            if (result >= 3)
            {
                if (b5 && b12) { take = true; }
            }
            if (result >= 2)
            {
                if (b12)
                {
                    if (b2 || b3) { take = true; }
                }
            }
            if (result >= 1)
            {
                if (b1 && b12) { take = true; }
            }

            if (selectedPiece.pieceType == PieceType.Knight) 
                knightAttemptedKill = true;

            if (selectedPiece.pieceType == PieceType.Bishop || selectedPiece.pieceType == PieceType.King)
                commanderAttemptedKill = true;

            //this is a check for the undolist to indicate an attack was made on this move
            pieceTaken = true;

            if (take) { willCapture = true; TakePiece(piece); }
            //edited by TW
            else {
                GameUI TheGameU = GameObject.Find("UI").GetComponent<GameUI>();
                TheGameU.PieceTakeFailed();
                willCapture = false; }
        }
    }


    private void TakePiece(Piece piece)
    {
        if (piece)
        {
            GameUI TheGameUI = GameObject.Find("UI").GetComponent<GameUI>();
            TheGameUI.PieceWasTaken();
            grid[piece.occupiedSquare.x, piece.occupiedSquare.y] = null;
            controller.OnPieceRemoved(piece);
            SFXController.PlaySoundCapture();
        }
    }

    private void EndTurn()
    {
        GameUI TheGameUI = GameObject.Find("UI").GetComponent<GameUI>();
        TheGameUI.updateMoveList();
        int iteratorNum = TheGameUI.GetIteratorCount();
        if (iteratorNum % NUMBER_OF_ACTIONS == 0)
        {
            ResetCommanderData();
            //prevents the "undo" button from being used after a player's turn is up.
            pieceMoves2.Clear();
            delegatedPiece = null;
        }

        controller.EndTurn();
    }

    //this simply exists to allow external methods to clear the list of piece moves for the undo function. Needed for endturn() in gamecontroller
    public void ClearUndoPieceMoves()
    {
        pieceMoves2.Clear();
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

    public int GetNumberOfUndoPieceMoves()
    {
        return pieceMoves2.Count;
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

    //TW - sends a vector2int list which contains new coords and old coords, in that order.
    public List<Vector2Int> GetUndoPieceMoves()
    {

        //List<String> newPieceMoves = new List<String>(pieceMoves2);
        List<Vector2Int> newPieceMoves = new List<Vector2Int>();
        newPieceMoves.Add(pieceMoves2[pieceMoves2.Count - 2]);
        newPieceMoves.Add(pieceMoves2[pieceMoves2.Count - 1]);
        pieceMoves2.RemoveAt(pieceMoves2.Count - 1);
        pieceMoves2.RemoveAt(pieceMoves2.Count - 1);
        Debug.Log("Printing all moves in move list:");
        foreach(Vector2Int move in pieceMoves2)
        {
            Debug.Log(move.ToString());
        }
        return newPieceMoves;
    }

    //TW - public method to return array which only contains new piece movements, 
    //discarding the current piecemoves array. Should save on computing power.
    public List<String> GetNewPieceMoves()
    {
        List<String> newPieceMoves = new List<String>(pieceMoves);
        pieceMoves.Clear();
        return newPieceMoves;
    }

    public int GetRollNeeded(Piece attackingPiece, Piece defendingPiece)
    {
        if (attackingPiece.pieceType == PieceType.King)
        {
            if (defendingPiece.pieceType == PieceType.Pawn)
                return 1;
            else if (defendingPiece.pieceType == PieceType.Rook)
                return 5;
            else return 4;
        }
        else if (attackingPiece.pieceType == PieceType.Queen)
        {
            if (defendingPiece.pieceType == PieceType.Pawn)
                return 2;
            else if (defendingPiece.pieceType == PieceType.Rook)
                return 5;
            else return 4;
        }
        else if (attackingPiece.pieceType == PieceType.Knight && !knightHasMoved)
        {
            if (defendingPiece.pieceType == PieceType.Pawn)
                return 2;
            else return 5;
        }
        else if (attackingPiece.pieceType == PieceType.Knight && knightHasMoved)
        {
            if (defendingPiece.pieceType == PieceType.Pawn)
                return 1;
            else return 4;
        }
        else if (attackingPiece.pieceType == PieceType.Bishop)
        {
            if (defendingPiece.pieceType == PieceType.Pawn)
                return 3;
            else if (defendingPiece.pieceType == PieceType.Bishop)
                return 4;
            else return 5;
        }
        else if (attackingPiece.pieceType == PieceType.Rook)
        {
            if (defendingPiece.pieceType == PieceType.Pawn)
                return 5;
            else if (defendingPiece.pieceType == PieceType.Rook)
                return 5;
            else if (defendingPiece.pieceType == PieceType.Bishop)
                return 5;
            else return 4;
        }
        else if (attackingPiece.pieceType == PieceType.Pawn)
        {
            if (defendingPiece.pieceType == PieceType.Pawn)
                return 4;
            else if (defendingPiece.pieceType == PieceType.Bishop)
                return 5;
            else return 6;
        }
        else return 0;
    }
}
