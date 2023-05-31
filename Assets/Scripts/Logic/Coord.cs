using System;
using System.Numerics;
using UnityEngine;

public struct Coord : IComparable<Coord>
{
    public readonly int file;
    public readonly int rank; 

    public static Coord Null = new Coord(-1, 0);

    public Coord(int f, int r) {
        file = f;
        rank = r;
    }

    public Coord(int s) {
        file = s & 7;
        rank = (s >> 3) & 7;
    }

    public bool IsValid() {
        return !Coord.Null.Equals(this);
    }
    public bool IsNull() {
        return !IsValid();
    }

    public bool IsLight() {
        return (file + rank) % 2 != 0;
    }

    public int CompareTo(Coord other) {
        return (other.rank == rank && other.file == file) ? 0 : 1;
    }

    public string GetName() {
        char fc = (char)((int)'A' + file);
        char rc = (char)((int)'1' + rank);
        return $"{fc}{rc}";
    }

    public UnityEngine.Vector3 GetPos(float depth = 0) {
        return new UnityEngine.Vector3(file - 3.5f, rank - 3.5f, depth);
    }

    public int GetIndex() {
        return 8 * rank + file;
    }
}