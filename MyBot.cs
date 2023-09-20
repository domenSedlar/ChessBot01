using ChessChallenge.API;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
//using System.Linq;

public class MyBot : IChessBot
{
    const int multiplier = 30;
    int[] pieceValues = { 9* multiplier, 5 * multiplier, 3 * multiplier, 3 * multiplier, 1 * multiplier };

    int time_limit = -1;
    int time_buffer = -1;
    int expected_moves = 40;
    int buffer_factor = 20;

    public Move Think(Board board, Timer timer)
    {
        if(time_limit == -1)
        {
            time_limit = timer.MillisecondsRemaining;
            time_buffer = (int) (time_limit / buffer_factor);
        }

        int high_score = int.MinValue;
        Move[] moves = board.GetLegalMoves();
        int score = 0;
        Move bm = moves[0];
        var ordered_moves = new SortedDictionary<int, List<Move>>(); 

        foreach (var move in moves)
        {
            board.MakeMove(move);
            if (board.IsInCheckmate())
            {
                return move;
            }
            board.UndoMove(move);

            score = -AlphaBeta(board, int.MinValue, int.MaxValue, 0, move);

            if (!ordered_moves.ContainsKey(score))
            {
                List<Move> new_moves = new List<Move>();
                new_moves.Add(move);
                ordered_moves.Add(score, new_moves);
            }
            else
            {

                ordered_moves[score].Add(move);
            }
            board.UndoMove(move);

            //score = NegaMax(board, timer, 2, move);
            if (score > high_score)
            {
                high_score = score;
                bm = move;
            }
        }

        Move nbm = bm;
        int new_high_score = int.MinValue;
        int i = 1;
        if (timer.MillisecondsRemaining < time_buffer)
        {
            time_limit = timer.MillisecondsRemaining;
            expected_moves = 30;
            time_buffer = 0;
        }
        int time_for_move = (time_limit-time_buffer) / expected_moves;

        while (timer.MillisecondsElapsedThisTurn < time_for_move)
        {

            var new_ordered_moves = new SortedDictionary<int, List<Move>>();

            foreach (var m in ordered_moves.Values)
            {
                foreach (var move in m)
                {
                    if (timer.MillisecondsElapsedThisTurn > time_for_move || timer.MillisecondsRemaining < 600)
                    {
                        return bm;
                    }
                    board.MakeMove(move);
                    if (board.IsInCheckmate())
                    {
                        return move;
                    }
                    board.UndoMove(move);

                    score = -AlphaBeta(board, int.MinValue, int.MaxValue, i, move);
                    board.UndoMove(move);

                    if (!new_ordered_moves.ContainsKey(score))
                    {
                        List<Move> new_moves = new List<Move>();
                        new_moves.Add(move);
                        new_ordered_moves.Add(score, new_moves);
                    }
                    else
                    {

                        new_ordered_moves[score].Add(move);
                    }

                    //score = NegaMax(board, timer, 2, move);
                    if (score > new_high_score)
                    {
                        new_high_score = score;
                        nbm = move;
                    }
                    if (timer.MillisecondsElapsedThisTurn > time_for_move || timer.MillisecondsRemaining < 600)
                    {
                        return bm;
                    }
                }
            }
            ordered_moves = new_ordered_moves;
            bm = nbm;
            i++;
        }


        return bm;
    }

    private int Eval(Board board, Move move)
    {
        int p1 = GetColorValue(board);
        board.MakeMove(move);
        Move[] moves = board.GetLegalMoves();

        int p2 = GetColorValue(board);

        if (board.TrySkipTurn())
        {
            p1 = GetColorValue(board);
            board.UndoSkipTurn();
        }
        else if (board.IsInCheckmate())
        {
            return -int.MaxValue;
        }

        return p1 - p2;
    }

    /*
     alpha beta(depthleft=?, alpha=MinValue, beta=MaxValue)
            if depthleft == 0:
                return -Eval()
            
            depthleft--
            
            score

            foreach move:
                score = -alphabeta(depthleft, -beta, -alpha)
                alpha = max(score, alpha)

                if score >= beta:
                    break
            return alpha
     */

    private int AlphaBeta(Board board, int alpha, int beta, int depthleft, Move m)
    {
        if (depthleft == 0)
        {
            return -Eval(board, m);
        }
        depthleft--;
        int score;
        board.MakeMove(m);

        foreach(Move mv in board.GetLegalMoves())
        {
            score = -AlphaBeta(board, int.MinValue, -alpha, depthleft, mv);
            board.UndoMove(mv);
            if (score > alpha)
            {
                alpha = score;
            }
            if (score >= beta){
                return alpha;
            }
        }

        return alpha;
    }

    private int GetColorValue(Board board)
    {
        return board.GetLegalMoves().Length + board.GetLegalMoves(true).Length
        + board.GetPieceList(PieceType.Queen, board.IsWhiteToMove).Count * pieceValues[0]
        + board.GetPieceList(PieceType.Rook, board.IsWhiteToMove).Count * pieceValues[1]
        + board.GetPieceList(PieceType.Bishop, board.IsWhiteToMove).Count * pieceValues[2]
        + board.GetPieceList(PieceType.Knight, board.IsWhiteToMove).Count * pieceValues[3]
        + board.GetPieceList(PieceType.Pawn, board.IsWhiteToMove).Count * pieceValues[4];
    }

}