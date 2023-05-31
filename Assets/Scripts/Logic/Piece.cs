public class Piece {
    public const int None = 0;
    public const int King = 1;
    public const int Pawn = 2;
    public const int Knight = 3;
    public const int Bishop = 5;
    public const int Rook = 6;
    public const int Queen = 7;
    public const int White = 8;
    public const int Black = 16;

    public const int tMask = 0b00111;
    public const int cMask = 0b11000;
    public const int bMask = 0b10000;
    public const int wMask = 0b01000;

    /*
    * Jesus Christ I Want To F***ing Cry.
    * 1:00 AM on 5/30 Now.
    * This overload is my cry for help at this moment.
    * I spent the last two hours wondering why my clicks would only work
    * on the empty squares.
    * 
    * The problem was that Piece.White and Board.White are different.
    * Because the 'Board' colors are for indexing
    * And the 'Piece' colors are for bit-twiddling and masks, etc.
    *
    * At least the fix is pretty (pretty trash, amirite? Overloading with an optional
    * is bad practice, yadda yadda) and lets me at least show off my knowledge of that.
    *
    * But holy crap. Not a fan of that bug of my own incompetence.
    * Next is just to play moves correctly (switch sides, etc.)
    * And PGN tracking afterwards. But that's easier. Mostly.
    *
    * That stupid SAN will kill me though. It's going to be an abbreviated version.
    * Not dealing with any stupid fxg1=Q+ or Qa6xb7# type moves.
    * May even just be super stupid and do straight UCI notation instead of the SAN style.
    * 
    * I need sleep. May my sacrifice not be in vain.
    */
    public static bool IsColor(int p, int color, bool boardColors = false) {
        if (boardColors)
            return (p >> 4) == color;
        return (p & cMask) == color;
    }

    public static int Color(int p) {
        return p & cMask;
    }

    public static int Type(int p) {
        return p & tMask;
    }

    public static bool IsRookOrQueen(int p) {
        return (p & Rook) == Rook;
    }
    public static bool IsBishopOrQueen(int p) {
        return (p & Bishop) == Bishop;
    }
    public static bool IsSlider(int p) {
        return (p & 4) > 0;
    }
}