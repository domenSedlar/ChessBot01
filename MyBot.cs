using ChessChallenge.API;
using System;
using System.Collections;
using System.Collections.Generic;

public class MyBot : IChessBot
{
    Dictionary<ulong, int> position_table = new Dictionary<ulong, int>();
    int[] pieceValues = { 50, 30, 20, 20, 5 };

    public Move Think(Board board, Timer timer)
    {

        int high_score = -1000;
        int move_i = 0;
        Move[] moves = board.GetLegalMoves();
        int c = moves.Length;
        int score = 0;
        for (int i = 0; i<c;i++)
        {

            board.MakeMove(moves[i]);
            if (board.IsInCheckmate())
            {
                position_table.Add(board.ZobristKey, 1000);

                return moves[i];
            }

            if (position_table.ContainsKey(board.ZobristKey))
            {
                if (position_table[board.ZobristKey] > high_score)
                {
                    high_score = score;
                    move_i = i;
                    Console.WriteLine(score);
                    Console.WriteLine(moves[i].StartSquare.Name + " - " + moves[i].TargetSquare.Name);
                }
                board.UndoMove(moves[i]);
                continue;
            }

            score = RecursiveFunction(board, timer, 0);
            position_table.Add(board.ZobristKey, score);
            if (score > high_score){
                high_score = score;
                move_i = i;
                Console.WriteLine(score);
                Console.WriteLine(moves[i].StartSquare.Name + " - " + moves[i].TargetSquare.Name);

            }

            board.UndoMove(moves[i]);
        }
        return moves[move_i];
    }

    private int RecursiveFunction(Board board, Timer timer, int depth) {
        int high_score = -1000;
        int low_score = 1000;
        int score = 0;
        Move[] moves = board.GetLegalMoves();
        int c = moves.Length;
        int lm = 2;

        if (depth == lm && lm%2 == 0)
        {
            if (!board.TrySkipTurn())
            {
                return c;
            }
            c = board.GetLegalMoves().Length + board.GetLegalMoves(true).Length 
                + board.GetPieceList(PieceType.Queen, board.IsWhiteToMove).Count * pieceValues[0] 
                + board.GetPieceList(PieceType.Rook, board.IsWhiteToMove).Count * pieceValues[1]
                + board.GetPieceList(PieceType.Bishop, board.IsWhiteToMove).Count * pieceValues[2] 
                + board.GetPieceList(PieceType.Knight, board.IsWhiteToMove).Count * pieceValues[3] 
                + board.GetPieceList(PieceType.Pawn, board.IsWhiteToMove).Count * pieceValues[4];
            board.UndoSkipTurn();
            return c -board.GetLegalMoves().Length-board.GetLegalMoves(true).Length
                 - board.GetPieceList(PieceType.Queen, board.IsWhiteToMove).Count * pieceValues[0]
                - board.GetPieceList(PieceType.Rook, board.IsWhiteToMove).Count * pieceValues[1]
                - board.GetPieceList(PieceType.Bishop, board.IsWhiteToMove).Count * pieceValues[2]
                - board.GetPieceList(PieceType.Knight, board.IsWhiteToMove).Count * pieceValues[3]
                - board.GetPieceList(PieceType.Pawn, board.IsWhiteToMove).Count * pieceValues[4];
        }

        if (depth == 3 && lm%2 == 1)
        {
            if (!board.TrySkipTurn())
            {
                return board.GetLegalMoves().Length;
            }
            c = board.GetLegalMoves().Length;
            board.UndoSkipTurn();
            return board.GetLegalMoves().Length - c;
        }
        depth++;

        for (int i = 0; i < c; i++)
        {
            board.MakeMove(moves[i]);

            score = RecursiveFunction(board, timer, depth);
            if (score > high_score)
            {
                high_score = score;
            }

            if (score < low_score)
            {
                low_score = score;
            }
         
            board.UndoMove(moves[i]);
        }

        return (depth%2== 0) ? high_score:low_score;
    }
}