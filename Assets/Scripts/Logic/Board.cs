using System;
using UnityEngine;

public class Board {
    public const int White = 0;
    public const int Black = 1;
    public int ColorToMove;

    public int[] Square;

    public ulong[] pieces;
    public ulong[] colors;

    public Coord EnPassant;

    public int moveCount;

    public uint CastleMask;
    const uint WhiteKingsideCastle = 0b0001;
    const uint WhiteQueensideCastle = 0b0010;
    const uint BlackKingsideCastle = 0b0100;
    const uint BlackQueensideCastle = 0b1000;

    public bool IsLegal(Move move) {
        int tm = ColorToMove;
        int opp = tm == White ? Black : White;
        int mv = PieceOn(move.from);

        if (!IsPseudoLegal(move))
            return false;

        Debug.Log("IsLegal: Passed PL Check");

        if (move.flag == Move.MoveFlag.Castling) {
            // TODO 
        }

        // This stuff has to happen before (1) so that EP doesn't reveal a check
        // and nobody sacs their king
        int proposedMover;
        RemovePiece(move.from, out proposedMover);
        AddPiece(move.to, proposedMover);

        int ep = -1;
        Coord eps = Coord.Null;
        if (move.flag == Move.MoveFlag.EnPassant) {
            eps = new Coord(move.to.file, move.from.rank);
            RemovePiece(eps, out ep);
        }
 
        bool stillInCheck = InCheck(out _);
        RemovePiece(move.to, out _);
        AddPiece(move.from, proposedMover);
        if (move.flag == Move.MoveFlag.EnPassant)
            AddPiece(eps, ep);

        if (stillInCheck) 
            return false; // TODO: Why does this fail first move?
        
        Debug.Log("IsLegal: After SIC Check");

        // (1)
        ulong checkers;
        if (!InCheck(out checkers))
            return true;

        if (Piece.Type(mv) == Piece.King)
            return IsAttackedBy(move.to, opp, out _);

        if (Bitboard.PopCnt(checkers) == 2 && Piece.Type(mv) != Piece.King)
            return false;

        return true;
    }

    public bool InCheck(out ulong checkers) {
        return IsAttackedBy(
            Bitboard.LSB(pieces[Piece.King] | colors[ColorToMove]),
            ColorToMove == White ? Black : White, out checkers
        );
    }

    public bool IsPseudoLegal(Move move) {
        int tm = ColorToMove;
        int mv = PieceOn(move.from);
        int cap = PieceOn(move.to);

        int firstRank = tm == White ? 0 : 7;
        int secondRank = tm == White ? 1 : 6;
        int thirdRank = tm == White ? 2 : 5;
        int fourthRank = tm == White ? 3 : 4;
        int lr = 7 - firstRank;

        int rankDiff = Math.Abs(move.to.rank - move.from.rank);
        int fileDiff = Math.Abs(move.to.file - move.from.file); 

        if (Piece.Type(mv) == Piece.None || !Piece.IsColor(mv, tm, true))
            return false;

        if (Piece.Type(cap) != Piece.None && Piece.IsColor(cap, tm, true))
            return false;

        if (move.flag == Move.MoveFlag.Promotion) {
            // TODO Allow promotions to Non-Queens?
        } else if (move.flag == Move.MoveFlag.EnPassant && (!move.to.Equals(EnPassant) || !Empty(move.to))) {
            return false;
        } else if (move.flag == Move.MoveFlag.Castling) {
            if (Piece.Type(mv) != Piece.King)
                return false;

            if (rankDiff != 0 || move.to.rank != firstRank)
                return false;
            if (fileDiff != 2)
                return false;

            int dir = move.to.file > move.from.file ? 1 : -1;
            int i = move.from.GetIndex();
            Coord oneSquare = new Coord(i + dir);
            Coord twoSquare = new Coord(i + dir + dir);
            if (!Empty(oneSquare) || !Empty(twoSquare))
                return false;
            int opp = tm == White ? Black : White;
            if (IsAttackedBy(oneSquare, opp, out _) || IsAttackedBy(twoSquare, opp, out _))
                return false;

            uint[] ksc = new []{WhiteKingsideCastle, BlackKingsideCastle};
            uint[] qsc = new []{WhiteQueensideCastle, BlackQueensideCastle};
            if (dir > 0) {
                if (!HasCastlePerm(ksc[tm]))
                    return false;
            } else {
                if (!HasCastlePerm(qsc[tm]))
                    return false;
            }
        }

        switch (Piece.Type(mv)) {
            case Piece.None:
                return false;
            case Piece.King:
                if (move.flag != Move.MoveFlag.Castling) {
                    return Math.Max(fileDiff, rankDiff) == 1
                        && Math.Min(fileDiff, rankDiff) == 1;
                }
                break;
            case Piece.Pawn:
                if (move.from.file != move.to.file) {
                    if (fileDiff != 1 || rankDiff != 1)
                        return false;
                    if (Empty(move.to) && move.flag != Move.MoveFlag.EnPassant)
                        return false;
                } else {
                    if (!Empty(move.to))
                        return false;
                    if (rankDiff > 2)
                        return false;
                    if (rankDiff == 2 &&
                    (move.from.rank != secondRank || move.to.rank != fourthRank || fileDiff != 0))
                        return false;
                    Coord betw = new Coord(move.from.file, thirdRank);
                    if (rankDiff == 2 && !Empty(betw))
                        return false;
                }

                if (move.to.rank == lr) {
                    // TODO Also change this with promotion Upgrades
                    if (move.flag != Move.MoveFlag.Promotion) {
                        Debug.Log("Holy Hell Batman!");
                        return false; // This shouldn't happen??
                    }
                }
                break;
            case Piece.Knight:
                if (rankDiff + fileDiff != 3 || Math.Max(fileDiff, rankDiff) != 2)
                    return false;
                break;
            case Piece.Bishop:
                ulong b = SliderSquaresFrom(move.from, false);
                if (!Bitboard.Contains(b, move.to))
                    return false;
                break;
            case Piece.Rook:
                b = SliderSquaresFrom(move.from, true);
                if (!Bitboard.Contains(b, move.to))
                    return false;
                break;
            case Piece.Queen:
                b = SliderSquaresFrom(move.from, true)
                    | SliderSquaresFrom(move.from, false);
                if (!Bitboard.Contains(b, move.to))
                    return false;
                break;
        }

        return true;
    }

    ulong SliderSquaresFrom(Coord c, bool isRook) {
        ulong bits = 0;
        ulong occupied = colors[0] | colors[1];

        int[] shifts = isRook ? (new []{1, -1, 8, -8}) : (new []{7, 9, -7, -9});
        ulong afile = 0x0101010101010101;
        ulong hfile = afile << 7;
        ulong mask = 0;
        ulong bit;
        foreach (int s in shifts) {
            bit = Bitboard.MakeBits(c);
            switch (s) {
                case 1:
                case 9:
                case -7:
                    mask = afile;
                    break;
                case -1:
                case -9:
                case 7:
                    mask = hfile;
                    break;
                default:
                    break;
            }

            while (bit > 0) {
                if (s < 0) {
                    bit >>= -s;
                } else {
                    bit <<= s;
                }
                bit &= ~mask;
                bits |= bit;
            }
        }


        return bits;
    }

    public bool IsAttackedBy(Coord square, int color, out ulong attackers) {
        // My Bread And Freaking Butter. Simply Beautiful I could do this in my sleep.
        // I probably should.. I started today and it's been 8 hours almost nonstop.
        // Logtime; 5/29 11:59 PM
        ulong pawns = (pieces[Piece.Pawn] & colors[color]);
        ulong knights = (pieces[Piece.Knight] & colors[color]);
        ulong bishops = (pieces[Piece.Bishop] & colors[color]);
        ulong rooks = (pieces[Piece.Rook] & colors[color]);
        ulong queens = (pieces[Piece.Queen] & colors[color]);
        ulong king = (pieces[Piece.King] & colors[color]);

        ulong att = 0;
        if (color == White) {
            att |= (pawns << 9) &~ ((ulong)0x0101010101010101 << 7);
            att |= (pawns << 7) &~ (ulong)0x0101010101010101;
        } else {
            att |= (pawns >> 7) &~ ((ulong)0x0101010101010101 << 7);
            att |= (pawns >> 9) &~ (ulong)0x0101010101010101;
        }

        if (Bitboard.Contains(att, square)) {
            if (color == White) {
                attackers = (att >> 7) &~ ((ulong)0x0101010101010101 << 7);
                attackers |= (att >> 9) &~ (ulong)0x0101010101010101;
                attackers &= pawns;
            } else {
                attackers = (att << 9) &~ ((ulong)0x0101010101010101 << 7);
                attackers |= (att << 7) &~ (ulong)0x0101010101010101;
                attackers &= pawns;
            }
            return true;
        }

        att = 0;
        att |= (knights << 15) &~ ((ulong)0x0101010101010101 << 7);
        att |= (knights << 17) &~ (ulong)0x0101010101010101;

        att |= (knights << 10) &~ ((ulong)0x0101010101010101 | ((ulong)0x0101010101010101 << 1));
        att |= (knights >> 6) &~ ((ulong)0x0101010101010101 | ((ulong)0x0101010101010101 << 1));

        att |= (knights >> 17) &~ ((ulong)0x0101010101010101 << 7);
        att |= (knights >> 15) &~ (ulong)0x0101010101010101;

        att |= (knights >> 10) &~ ((ulong)0x8080808080808080 | ((ulong)0x8080808080808080 >> 1));
        att |= (knights << 6) &~ ((ulong)0x8080808080808080 | ((ulong)0x8080808080808080 >> 1));
        if (Bitboard.Contains(att, square)) {
            // We are NOT doing this.
            // AKA Assume that if we ARE in check but the board is empty
            // It's a Knight Check and we can simply walk out
            attackers = 0;
            return true;
        }

        att = 0;
        att |= (king << 9) &~ ((ulong)0x0101010101010101 << 7);
        att |= (king << 7) &~ (ulong)0x0101010101010101;
        att |= ((king >> 7) | (king << 1)) &~ ((ulong)0x0101010101010101 << 7);
        att |= ((king >> 9) | (king >> 1)) &~ (ulong)0x0101010101010101;
        att |= king >> 8;
        att |= king << 8;
        if (Bitboard.Contains(att, square)) {
            attackers = king;
            return true;
        }

        att = SliderSquaresFrom(square, false);
        if ((bishops & att) > 0) {
            attackers = bishops & att;
            return true;
        }
        ulong rk = SliderSquaresFrom(square, true);
        if ((rooks & rk) > 0) {
            attackers = rooks & rk;
            return true;
        }

        attackers = (rk | att) & queens;
        return attackers > 0;
    }

    public void PlayMove(Move move) {
        EnPassant = Coord.Null;
        int moved;
        int cap;
        RemovePiece(move.from, out moved);
        RemovePiece(move.to, out cap);

        AddPiece(move.to, moved);
        if (move.flag == Move.MoveFlag.EnPassant) {
            RemovePiece(new Coord(move.to.file, move.from.rank), out _);
        }

        if (Piece.Type(moved) == Piece.King
            && Math.Abs(move.from.GetIndex() - move.to.GetIndex()) == 2
            && move.flag == Move.MoveFlag.Castling)
            {
                // Castle
                int rook;
                Coord rookSquare;
                if (move.to.file > move.from.file) {
                    rookSquare = new Coord(7, move.from.rank);
                } else {
                    rookSquare = new Coord(0, move.from.rank);
                }
                RemovePiece(rookSquare, out rook);
                Coord betweenFromAndTo = new Coord((move.to.GetIndex() + move.from.GetIndex())/2);
                AddPiece(betweenFromAndTo, rook);
                if (ColorToMove == White) {
                    if (move.to.file > move.from.file) {
                        CastleMask ^= WhiteKingsideCastle;
                    } else {
                        CastleMask ^= WhiteQueensideCastle;
                    }
                } else {
                    if (move.to.file > move.from.file) {
                        CastleMask ^= BlackKingsideCastle;
                    } else {
                        CastleMask ^= BlackQueensideCastle;
                    }
                }
            }

        if (move.flag == Move.MoveFlag.Promotion) {
            // TODO Promotion upgrade here too
            RemovePiece(move.to, out _);
            int pcolor = ColorToMove == White ? Piece.White : Piece.Black;
            AddPiece(move.to, pcolor | Piece.Queen);
        }

        if (Piece.Type(moved) == Piece.Pawn
            && Math.Abs(move.from.rank - move.to.rank) == 2) {
                EnPassant = new Coord(move.from.file, (move.from.rank + move.to.rank)/2);
            }

        ColorToMove = ColorToMove == White ? Black : White;
        if (ColorToMove == White)
            ++moveCount;
    }

    void Initialize() {
        Square = new int[64];
        ColorToMove = White;
        EnPassant = Coord.Null;

        CastleMask = 0xf;

        pieces = new ulong[8];
        colors = new ulong[2];
    }

    public void ParseFEN(string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1") {
        Initialize();
        int fenlen = fen.Length;
        int i = 0;
        int file = 0;
        int rank = 7;
        for (; i < fenlen; i++) {
            if (fen[i] == ' ')
                break;
            
            char c = fen[i];
            Coord coord = new Coord(file, rank);
            if (c == '/') {
                file = 0;
                rank--;
                continue;
            }

            if ((int)c >= (int)'1' && (int)c <= (int)'8') {
                file += (int)c - (int)'0';
                continue;
            }

            int color = Char.IsUpper(c) ? Piece.White : Piece.Black;
            char lc = Char.ToLower(c);
            int ty = -1;
            switch (lc) {
                case 'p':
                    ty = Piece.Pawn;
                    break;
                case 'n':
                    ty = Piece.Knight;
                    break;
                case 'b':
                    ty = Piece.Bishop;
                    break;
                case 'r':
                    ty = Piece.Rook;
                    break;
                case 'q':
                    ty = Piece.Queen;
                    break;
                case 'k':
                    ty = Piece.King;
                    break;
                default:
                    break;
            }

            if (ty < 0) {
                Debug.Log(lc);
                continue;
            }

            int p = ty | color;
            AddPiece(coord, p);
            file++;
        }

        ++i;
        if (i >= fenlen)
            return;

        // Not dealing with the rest of this garbage.
    }

    bool HasCastlePerm(uint perm) {
        return (CastleMask & perm) == perm;
    }

    void AddPiece(Coord c, int p) {
        pieces[Piece.Type(p)] |= Bitboard.MakeBits(c);
        colors[Piece.Color(p) == Piece.White ? 0 : 1] |= Bitboard.MakeBits(c);
        Square[c.GetIndex()] = p;
    }

    void RemovePiece(Coord c, out int p) {
        p = PieceOn(c);
        if (Piece.Type(p) != Piece.None) {
            ulong bit = Bitboard.MakeBits(c);
            colors[Piece.Color(p) == Piece.White ? 0 : 1] ^= bit;
            pieces[Piece.Type(p)] ^= bit;
            Square[c.GetIndex()] = 0;
        }
    }

    public int PieceOn(Coord c) {
        return Square[c.GetIndex()];
    }
    public bool Empty(Coord c) {
        return Piece.Type(PieceOn(c)) == Piece.None;
    }
}