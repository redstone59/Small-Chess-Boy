using ChessChallenge.API;
using System;
using System.Collections.Generic;
//using System.Linq;
//using System.Numerics;
public class MyBot : IChessBot
{
    // Bot settings   
    const int SearchDepth = 4;
    public double GameProgression = 0;
    bool isWhite;

    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();
        List<Move> bestMoves = new() { moves[0] };
        List<Move> moveList = new();
        double bestEval = CompleteEvaluation(board,new Square());
        bool isBetterMove;
        isWhite = board.IsWhiteToMove;
        GameProgression = (double)board.PlyCount / 75;

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

            isBetterMove = CompleteEvaluation(board, move.TargetSquare) < bestEval;
            if (!board.IsWhiteToMove) { isBetterMove ^= true; };

            if (CompleteEvaluation(board, move.TargetSquare) == bestEval) { bestMoves.Add(move); };

            if (isBetterMove)
            {
                bestMoves.Clear();
                bestMoves.Add(move);
                bestEval = CompleteEvaluation(board, move.TargetSquare);
            };
            board.UndoMove(move);
        }

        Random rand = new();
        return bestMoves[rand.Next(0,bestMoves.Count - 1)];
    }
    public double CompleteEvaluation(Board board, Square square)
    {
        /* Complete Evaluation todo:
         * !- CHECKMATE DETECTION yes yes yes yes
         * !- and also check priority i guess
         * - Piece Evaluation but guiding the pieces towards certain squares
         * - King Safety Evaluation
         */
        double eval = 0;
        
        eval += PieceEvaluation(board)
             + (board.IsInCheck() ? 1000 * GameProgression : 0) //re-eval
             - (board.SquareIsAttackedByOpponent(square) ? 100 : 0); //re-eval, this just makes it scared to trade.

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

            foreach (Piece piece in pieces)
            {
                deltaEval = pieceValues[(int)piece.PieceType];// * CorrectionTerm(board, piece);
                pieceEval += deltaEval * (pieces.IsWhitePieceList ? 1 : -1);
            }
        }

        return pieceEval;
    }

    public double CorrectionTerm(Board board, Piece piece) //All of these fucking suck. Work on searching.
    {
        double correction = 1;

        //Don't think the bishop OR queen requires correction, tbh.
        switch (piece.PieceType) 
        {
            case PieceType.Pawn:
                /* Open-Mid: Second rank, unless center, then fourth rank
                 * Endgame: Eighth rank
                 */
                correction = LinearInterpolate(1,
                            Math.Abs(Rank(7) - piece.Square.Rank) + DistanceOfSquares(piece.Square, board.GetKingSquare(!isWhite)) * 100, 
                            GameProgression);
                break;
            case PieceType.Knight:
                /* Open-Mid: Center of board.
                 * Endgame: No correction term.
                 */
                correction = LinearInterpolate(4 - DistanceFromCenter(piece.Square), 1, GameProgression);
                break;
            case PieceType.Rook:
                /* Open-Mid: Castle squares
                 * Endgame: None
                 */
                //correction = LinearInterpolate(
                //                Math.Min(Math.Abs(3 - piece.Square.File), Math.Abs(5 - piece.Square.File)),
                //                1,
                //                GameProgression
                //           );
                break; 
            case PieceType.King:
                /* Open-Mid: Castle squares
                 * Endgame: Center of Board
                 */
                //correction = LinearInterpolate(
                //                Math.Abs(Rank(0) - piece.Square.Rank), 
                //                4 - DistanceFromCenter(piece.Square),
                //                GameProgression
                //             );
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

    public static int DistanceFromCenter(Square square)
    {
        return Math.Min(DistanceOfSquares(square, new Square("d4")), DistanceOfSquares(square, new Square("e5")));
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