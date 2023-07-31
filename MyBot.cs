﻿using ChessChallenge.API;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks.Sources;

public class MyBot : IChessBot
{

    Dictionary<ulong, int> position_table = new Dictionary<ulong, int>();
    int[] pieceValues = { 60, 40, 27, 27, 10 };

    public Move Think(Board board, Timer timer)
    {
        int high_score = -1000;
        int move_i = 0;
        Move[] moves = board.GetLegalMoves();
        int c = moves.Length;
        int score = 0;
        for (int i = 0; i < c; i++)
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

                }
                board.UndoMove(moves[i]);
                continue;
            }

            score = NegaMax(board, timer, 2);
            position_table.Add(board.ZobristKey, score);
            if (score > high_score)
            {
                high_score = score;
                move_i = i;


            }

            board.UndoMove(moves[i]);
        }
        return moves[move_i];
    }

    private int Eval(Board board, int depth) {
        Move[] moves = board.GetLegalMoves();
        int p1 = moves.Length;
        if (!board.TrySkipTurn())
        {
            return p1;
        }

        p1 = board.GetLegalMoves().Length + board.GetLegalMoves(true).Length
                + board.GetPieceList(PieceType.Queen, board.IsWhiteToMove).Count * pieceValues[0]
                + board.GetPieceList(PieceType.Rook, board.IsWhiteToMove).Count * pieceValues[1]
                + board.GetPieceList(PieceType.Bishop, board.IsWhiteToMove).Count * pieceValues[2]
                + board.GetPieceList(PieceType.Knight, board.IsWhiteToMove).Count * pieceValues[3]
                + board.GetPieceList(PieceType.Pawn, board.IsWhiteToMove).Count * pieceValues[4];
        board.UndoSkipTurn();

        int p2 = board.GetLegalMoves().Length + board.GetLegalMoves(true).Length
        + board.GetPieceList(PieceType.Queen, board.IsWhiteToMove).Count * pieceValues[0]
        + board.GetPieceList(PieceType.Rook, board.IsWhiteToMove).Count * pieceValues[1]
        + board.GetPieceList(PieceType.Bishop, board.IsWhiteToMove).Count * pieceValues[2]
        + board.GetPieceList(PieceType.Knight, board.IsWhiteToMove).Count * pieceValues[3]
        + board.GetPieceList(PieceType.Pawn, board.IsWhiteToMove).Count * pieceValues[4];

        return (depth%2==0) ?  (p1 - p2) : -(p1-p2);
    }

    private int NegaMax(Board board, Timer timer, int depth)
    {

        if (depth == 0)
        {
            return Eval(board, depth);
        }

        int max_score = int.MinValue;
        int score = 0;
        Move[] moves = board.GetLegalMoves();
        int c = moves.Length;
        depth--;

        for (int i = 0; i < c; i++)
        {
            board.MakeMove(moves[i]);

            score = NegaMax(board, timer, depth);
            if (score > max_score)
            {
                max_score = score;
            }

            board.UndoMove(moves[i]);
        }

        return -max_score;
    }
}
