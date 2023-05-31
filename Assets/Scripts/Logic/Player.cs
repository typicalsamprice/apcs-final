using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player {
    BoardUI ui;
    Camera cam;
    Coord selected;
    Board board;

    public event System.Action<Move> onMoveAction;

    public enum InputState { None, Selected, Dragging };
    InputState ist;

    public Player(Board board) {
        ui = GameObject.FindObjectOfType<BoardUI>();
        cam = Camera.main;
        this.board = board;
    }

    void ChooseMove(Move move) {
        onMoveAction?.Invoke(move);
    }

    public void Update() {
        Vector2 mpos = cam.ScreenToWorldPoint(Input.mousePosition);

        if (ist == InputState.None) {
            HandleSelection(mpos);
        } else if (ist == InputState.Dragging) {
            HandleDrag(mpos);
        } else if (ist == InputState.Selected) {
            HandlePAC(mpos);
        }

        if (Input.GetMouseButtonDown(1)) {
            CancelSelection();
        }
    }

    void HandlePAC(Vector2 mp) {
        if (Input.GetMouseButton(0)) {
            HandlePlacement(mp);
        }
    }

    void HandleDrag(Vector2 mp) {
        ui.DragPiece(selected, mp);
        if (Input.GetMouseButtonUp(0)) {
            HandlePlacement(mp);
        }
    }

    void HandlePlacement(Vector2 mp) {
        Coord tg;
        if (ui.SquareUnderPointer(mp, out tg)) {
            if (tg.Equals(selected)) {
                ui.ResetPiecePos(selected);
                if (ist == InputState.Dragging) {
                    ist = InputState.Selected;
                } else {
                    ist = InputState.None;
                    ui.DeselectSquare(selected);
                }
            } else {
                if (Piece.IsColor(board.PieceOn(tg), board.ColorToMove) && board.PieceOn(tg) != 0) {
                    CancelSelection();
                    HandleSelection(mp);
                } else {
                    Move.MoveFlag flag = Move.MoveFlag.None;
                    // FIXME: Do This!
                    TryMove(new Move(selected, tg, flag));
                }
            }
        } else {
            CancelSelection();
        }
    }

    void CancelSelection() {
        if (ist != InputState.None) {
            ist = InputState.None;
            ui.DeselectSquare(selected);
            ui.ResetPiecePos(selected);
        }
    }

    void TryMove(Move move) {
        if (board.IsLegal(move)) {
            Debug.Log($"Legal move: {move.GetName()}");
            ChooseMove(move);
            ist = InputState.None;
        } else {
            Debug.Log($"Illegal move: {move.GetName()}");
            CancelSelection();
        }
    }

    void HandleSelection(Vector2 mp) {
        if (Input.GetMouseButtonDown(0)) {
            if (ui.SquareUnderPointer(mp, out selected)) {
                if (Piece.IsColor(board.PieceOn(selected), board.ColorToMove, true)) {
                    ui.SelectSquare(selected);
                    ist = InputState.Dragging;
                }
            }
        }
    }
}