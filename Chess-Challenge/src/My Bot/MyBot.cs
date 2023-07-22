using ChessChallenge.API;
using System;
using System.Collections.Generic;
//using System.Linq;
//using System.Numerics;
public class MyBot : IChessBot
{
    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();
        List<Move> bestMoves = new List<Move>() { moves[0] };
        int bestEval = CompleteEvaluation(board);
        bool isBetterMove;

        foreach (Move move in moves) //Move to Search Function to make this cleaner (or not if that wastes tokens)
        {
            board.MakeMove(move);

            isBetterMove = CompleteEvaluation(board) < bestEval;
            if (!board.IsWhiteToMove) { isBetterMove ^= true; };

            if (CompleteEvaluation(board) == bestEval) { bestMoves.Add(move); };

            if (isBetterMove)
            {
                bestMoves.Clear();
                bestMoves.Add(move);
                bestEval = CompleteEvaluation(board);
            };
            
            board.UndoMove(move);
        }

        Random rand = new();
        return bestMoves[rand.Next(0,bestMoves.Count - 1)];
    }

    public int CompleteEvaluation(Board board)
    {
        /* Complete Evaluation todo:
         * - CHECKMATE DETECTION yes yes yes yes
         * - and also check priority i guess
         * - Piece Evaluation but guiding the pieces towards certain squares
         * - King Safety Evaluation
         */
        int eval = 0;
        
        eval += PieceEvaluation(board);

        return eval;
    }
    public int PieceEvaluation(Board board)
    {
        int pieceEval = 0;
        int deltaEval;

        foreach (PieceList pieces in board.GetAllPieceLists())
        {
            deltaEval = 0;
            if (pieces.TypeOfPieceInList == PieceType.King) { continue; };
            /*
             * switch statement in desperate need of an efficientizing :3
             * thanks to @viren on the stockfish discord for the values (https://media.discordapp.net/attachments/329957785900941323/1131957627832782950/image.png)
             */
            switch (pieces.TypeOfPieceInList)
            {
                case PieceType.Pawn: deltaEval = 100; break;
                case PieceType.Knight: deltaEval = 233; break;
                case PieceType.Bishop: deltaEval = 279; break;
                case PieceType.Rook: deltaEval = 428; break;
                case PieceType.Queen: deltaEval = 873; break;
            }
            pieceEval += deltaEval * (pieces.IsWhitePieceList ? 1 : -1) * pieces.Count;
        }

        return pieceEval;
    }
}