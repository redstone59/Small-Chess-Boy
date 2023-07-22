using ChessChallenge.API;
using System;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Numerics;
public class MyBot : IChessBot
{

    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();
        Move bestMove = moves[0];
        int bestEval = PieceEvaluation(board);
        foreach (Move move in moves)
        {
            board.MakeMove(move);
            if (PieceEvaluation(board) > bestEval * (board.IsWhiteToMove ? 1 : -1))
            {
                bestMove = move;
                bestEval = PieceEvaluation(board);
                Console.WriteLine(bestEval);
            };
            board.UndoMove(move);
        }
        return bestMove;
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