using ChessChallenge.API;
using System;
using System.Collections.Generic;
//using System.Linq;
//using System.Numerics;
public class MyBot : IChessBot
{
    // Bot settings   
    const int SearchDepth = 3;
    const int CheckmateFlag = 0x8000;
    public double GameProgression = 0.0;
    bool isWhite;

    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();
        List<Move> bestMoves = new() { moves[0] };
        double bestEval = CompleteEvaluation(board);
        bool isBetterMove;
        isWhite = board.IsWhiteToMove;
        GameProgression = (double) board.PlyCount / 50.0d;
        Console.WriteLine(GameProgression);

        /* Search todos:
         * -SEARCH DEPTH
         * -thats really it eval is also needed
         * -transposition tables maybe
         */
        foreach (Move move in moves)
        {
            board.MakeMove(move);

            if (board.IsInCheckmate() || board.GetLegalMoves().Length == 1)
            {
                return move;
            }

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

    public double CompleteEvaluation(Board board)
    {
        /* Complete Evaluation todo:
         * !- CHECKMATE DETECTION yes yes yes yes
         * !- and also check priority i guess
         * - Piece Evaluation but guiding the pieces towards certain squares
         * - King Safety Evaluation
         */
        double eval = 0;
        
        eval += PieceEvaluation(board);
        eval += board.IsInCheck() ? 1000 : 0;

        return eval;
    }
    public double PieceEvaluation(Board board)
    {
        double pieceEval = 0;
        double deltaEval;

        //thanks to @viren on the stockfish discord for the values (https://media.discordapp.net/attachments/329957785900941323/1131957627832782950/image.png)
        double[] pieceValues = { 0, 100, 233, 279, 428, 873, 0 };

        foreach (PieceList pieces in board.GetAllPieceLists())
        {
            deltaEval = 0;
            if (pieces.TypeOfPieceInList == PieceType.King) { continue; };

            foreach (Piece piece in pieces)
            {
                deltaEval = pieceValues[(int)piece.PieceType] * CorrectionTerm(piece);
                pieceEval += deltaEval * (pieces.IsWhitePieceList ? 1 : -1);
            }
        }

        return pieceEval;
    }

    public double CorrectionTerm(Piece piece)
    {
        double correction = 1;

        //Don't think the bishop OR queen requires correction, tbh.
        switch (piece.PieceType) 
        {
            case PieceType.Pawn:
                /* Open-Mid: Second rank, unless center, then fourth rank
                 * Endgame: Eighth rank
                 */
                correction = LinearInterpolate(1, Math.Abs(Rank(7) - piece.Square.Rank), GameProgression);
                break;
            case PieceType.Knight:
                /* Open-Mid: Center of board.
                 * Endgame: No correction term.
                 */
                break;
            case PieceType.Rook:
                /* Open-Mid: Castle squares
                 * Endgame: None
                 */
                break;
        }

        return correction;
    }

    public static int DistanceOfSquares(Square start, Square end)
    {
        int yDistance = Math.Abs(start.Rank - end.Rank);
        int xDistance = Math.Abs(start.File - end.File);
        int diagonalReduction = Math.Min(yDistance, xDistance);
        return yDistance + xDistance - diagonalReduction;
    }

    public static double LinearInterpolate(float start, float end, double percent)
    {
        return (end-start)*percent+start;
    }

    public int Rank(int rank)
    {
        return isWhite ? rank : 7 - rank;
    }
}