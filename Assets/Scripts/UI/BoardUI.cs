using UnityEngine;
using System.Collections.Generic;

public class BoardUI : MonoBehaviour
{
    public Color LightColor;
    public Color DarkColor;
    public Color LightSelectedColor;
    public Color DarkSelectedColor;

    public Move lastMove;

    public PieceTheme pieceTheme;

    const float pcDepth = -0.1f;
    const float pcDragDepth = -0.2f;

    MeshRenderer[,] sqRenders;
    SpriteRenderer[,] pcRenders;

    void Awake() {
        CreateUI();
    }

    void CreateUI() {
        Shader shader = Shader.Find("Unlit/Color");
        sqRenders = new MeshRenderer[8, 8];
        pcRenders = new SpriteRenderer[8, 8];

        for (int rank = 0; rank < 8; rank++) {
            for (int file = 0; file < 8; file++) {
                Coord c = new Coord(file, rank);
                Transform sT = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
                sT.parent = transform;
                sT.name = c.GetName();
                sT.position = c.GetPos();
                Material mat = new Material(shader);

                sqRenders[file, rank] = sT.gameObject.GetComponent<MeshRenderer>();
                sqRenders[file, rank].material = mat;

                SpriteRenderer pr = new GameObject("Piece").AddComponent<SpriteRenderer>();
                pr.transform.parent = sT;
                pr.transform.position = c.GetPos(pcDepth);
                pr.transform.localScale = Vector3.one * 100 / (2000 / 6f);
                pcRenders[file, rank] = pr;
            }
        }
    }

    public void OnMoveMade(Move move, Board board) {
        lastMove = move;
        RenderBoard(board);
        RecolorSquares();
    }

    public void HighlightLastMove() {
        ColorSquare(lastMove.from, LightSelectedColor, DarkSelectedColor);
        ColorSquare(lastMove.to, LightSelectedColor, DarkSelectedColor);
    }

    public void RecolorSquares() {
        for (int rank = 0; rank < 8; rank++) {
            for (int file = 0; file < 8; file++) {
                Coord c = new Coord(file, rank);
                ColorSquare(c, LightColor, DarkColor);
            }
        }
    }

    void ColorSquare(Coord c, Color ifLight, Color ifDark) {
        MeshRenderer sr = sqRenders[c.file, c.rank];
        if (c.IsLight()) {
            sr.material.color = ifLight;
        } else {
            sr.material.color = ifDark;
        }
    }

    public void RenderBoard(Board board) {
        for (int rank = 0; rank < 8; rank++) {
            for (int file = 0; file < 8; file++) {
                Coord c = new Coord(file, rank);
                int pc = board.Square[c.GetIndex()];
                pcRenders[file, rank].sprite = pieceTheme.Lookup(pc);
                pcRenders[file, rank].transform.position = c.GetPos(pcDepth);
            }
        }
    }

    public void ResetPiecePos(Coord c) {
        pcRenders[c.file, c.rank].transform.position = c.GetPos(pcDepth);
    }

    public void DragPiece(Coord c, Vector2 mp) {
        pcRenders[c.file, c.rank].transform.position = new Vector3(mp.x, mp.y, pcDragDepth);
        ColorSquare(c, LightSelectedColor, DarkSelectedColor);
    }

    public void SelectSquare(Coord c) {
        ColorSquare(c, LightSelectedColor, DarkSelectedColor); // TODO Pull into theme?
    }

    public void DeselectSquare(Coord c) {
        RecolorSquares();
    }

    public bool SquareUnderPointer(Vector2 mp, out Coord target) {
        int file = (int)(mp.x + 4);
        int rank = (int)(mp.y + 4);

        target = new Coord(file, rank);
        return file >= 0 && file < 8 && rank >= 0 && rank < 8;
    }
}