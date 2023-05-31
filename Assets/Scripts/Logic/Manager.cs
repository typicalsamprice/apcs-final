using UnityEngine;

public class Manager : MonoBehaviour {
    public enum State { Playing, Mate, Stalemate, Draw };

    public event System.Action<Move> moveChosen;

    State st;
    BoardUI boardUI;
    public Board board;

    Player wPlayer;
    Player bPlayer;
    public Player mover;
    
    void Start() {
        board = new Board();
        boardUI = FindObjectOfType<BoardUI>();

        board.ParseFEN();
        boardUI.RenderBoard(board);
        boardUI.RecolorSquares();

        st = State.Playing;

        wPlayer = new Player(board);
        bPlayer = new Player(board);

        wPlayer.onMoveAction += OnMoveChosen;
        bPlayer.onMoveAction += OnMoveChosen;

        mover = wPlayer;
    }

    void Update() {
        if (st == State.Playing) {
            mover.Update();
        }

        // In here, export PGN if they press "E"
    }

    void OnMoveChosen(Move move) {
        board.PlayMove(move);
        moveChosen?.Invoke(move);
        boardUI.OnMoveMade(move, board);

        if (st == State.Playing) {
            mover = board.ColorToMove == Board.White ? wPlayer : bPlayer;
        }
    }
}