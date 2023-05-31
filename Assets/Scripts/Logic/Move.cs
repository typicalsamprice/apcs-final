using UnityEngine;

public class Move {
    public Coord from;
    public Coord to;

    public enum MoveFlag { None, EnPassant, Castling, Promotion };
    public MoveFlag flag;

    public Move(Coord from, Coord to) {
        this.from = from;
        this.to = to;
        this.flag = MoveFlag.None;
    }
    public Move(Coord from, Coord to, MoveFlag flag) {
        this.from = from;
        this.to = to;
        this.flag = flag;
    }

    public string GetName() {
        return $"{from.GetName()}{to.GetName()}";
    }
}