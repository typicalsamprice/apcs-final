using UnityEngine;

[CreateAssetMenu(menuName = "Pieces")]
public class PieceTheme : ScriptableObject {
    public PSprites whiteSprites;
    public PSprites blackSprites;

    public Sprite Lookup(int pc) {
        bool isWhite = Piece.IsColor(pc, Piece.White);
        PSprites spl = isWhite ? whiteSprites : blackSprites;
        switch (Piece.Type(pc)) {
            case Piece.Pawn:
                return spl.pawn;
            case Piece.Knight:
                return spl.knight;
            case Piece.Bishop:
                return spl.bishop;
            case Piece.Rook:
                return spl.rook;
            case Piece.Queen:
                return spl.queen;
            case Piece.King:
                return spl.king;
            default:
                if (pc != 0)
                    Debug.Log(pc);
                return null;
        }
    }
}

[System.Serializable]
public class PSprites {
    public Sprite pawn, knight, bishop, rook, queen, king;

    public Sprite this [int i] {
        get {
            return new Sprite[] { pawn, knight, bishop, rook, queen, king }[i];
        }
    }
}