using UnityEngine;
public class Bitboard {
    public static bool Contains(ulong bits, Coord c) {
        return Bitboard.Contains(bits, c.GetIndex());
    }
    public static bool Contains(ulong bits, int s) {
        return (((ulong)1 << s) & bits) > 0;
    }

    public static ulong MakeBits(Coord c) {
        return ((ulong)1 << c.GetIndex());
    }
    public static ulong MakeBits(int s) {
        return ((ulong)1 << s);
    }

    public static Coord LSB(ulong bits) {
        if (bits == 0)
            return Coord.Null;
        ulong b = bits;
        int i = 0;
        while ((b & 1) == 0) {
            b >>= 1;
            ++i;
        }

        return new Coord(i);
    }

    public static int PopCnt(ulong bits) {
        ulong b = bits;
        int i = 0;
        while (b > 0) {
            b &= b - 1;
            i++;
        }
        return i;
    }
}