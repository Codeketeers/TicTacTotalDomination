using System;
using System.Collections.Generic;

namespace TicTacTotalDomination.Util.AI
{
    public class GameBoard
    {
        private int[,] Board = new int[3, 3];
        public int Depth = Int32.MaxValue;

        public GameBoard(int[,] currentBoard)
        {
            Board = currentBoard;
        }
        public void DoMove(Move move, int player)
        {
            if (Board[move.GetX(), move.GetY()] == 0)
            {
                Board[move.GetX(), move.GetY()] = player;
            }
            else
            {
                Console.WriteLine("Invalid Move\n");
            }
        }

        public void DoMove(int x, int y, int player)
        {
            if (Board[x, y] == 0)
            {
                Board[x, y] = player;
            }
            else
            {
                Console.WriteLine("Invalid Move\n");
            }
        }

        public bool IsWon(int player)
        {
            if (Board[0, 0] == player)
            {
                if (Board[0, 0] == Board[0, 1] &&
                        Board[0, 1] == Board[0, 2])
                    return true;

                if (Board[0, 0] == Board[1, 0] &&
                        Board[1, 0] == Board[2, 0])
                    return true;
            }

            if (Board[2, 2] == player)
            {
                if (Board[2, 0] == Board[2, 1] &&
                        Board[2, 1] == Board[2, 2])
                    return true;

                if (Board[0, 2] == Board[1, 2] &&
                        Board[1, 2] == Board[2, 2])
                    return true;
            }

            if (Board[1, 1] == player)
            {
                if (Board[0, 1] == Board[1, 1] &&
                        Board[1, 1] == Board[2, 1])
                    return true;

                if (Board[1, 0] == Board[1, 1] &&
                        Board[1, 1] == Board[1, 2])
                    return true;

                if (Board[0, 0] == Board[1, 1] &&
                        Board[1, 1] == Board[2, 2])
                    return true;

                if (Board[0, 2] == Board[1, 1] &&
                        Board[1, 1] == Board[2, 0])
                    return true;
            }

            return false;
        }

        int Evaluate(int player)
        {
            int score = 0;

            if (IsWon(player))
            {
                score = (-100);
            }
            if (IsWon(-player))
            {
                score = (100);
            }

            return score;
        }

        int Negamax(int depthVar, int player, int alpha, int beta)
        {
            if (IsWon(player) || depthVar <= 0)
            {

                return Evaluate(player);
            }

            List<Move> allMoves = RemainingMoves();

            if (allMoves.Count == 0)
                return Evaluate(player);
            foreach (Move m in allMoves)
            {
                DoMove(m, -player);
                int val = -Negamax(depthVar - 1, -player, -beta, -alpha);
                UndoMove(m);
                if (val >= beta)
                    return val;

                if (val > alpha)
                    alpha = val;
            }
            return alpha;
        }

        public Move GetEmptySpace()
        {
            Move move = new Move();
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (Board[i, j] == 0)
                    {
                        move.Set(i, j);
                    }
                }
            }
            return move;
        }

        int NegamaxDeathMatch(int depthVar, int player, int alpha, int beta)
        {
            if (IsWon(player) || depthVar == 0)
            {

                return Evaluate(player);
            }

            List<Move> moveList = new List<Move>();
            moveList = RemainingDeathMatchMoves(-player);
            Move emptySpace = GetEmptySpace();

            if (moveList.Count == 0)
                return Evaluate(player);
            foreach (Move m in moveList)
            {
                UndoMove(m);
                DoMove(emptySpace, -player);
                int val = -NegamaxDeathMatch(depthVar - 1, -player, -beta, -alpha);
                UndoMove(emptySpace);
                DoMove(m, -player);
                if (val >= beta)
                    return val;

                if (val > alpha)
                    alpha = val;
            }
            return alpha;
        }

        public Move FindOpeningCounter(int player)
        {
            int enemyX = -1;
            int enemyY = -1;
            Move bestMove = new Move();

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (Board[i, j] == -player)
                    {
                        enemyX = i;
                        enemyY = j;
                    }
                }
            }

            if ((enemyX + enemyY) % 2 == 0)
            {
                if (enemyX == 0)
                {
                    if (enemyY == 0)
                    {
                        bestMove.SetX(2);
                        bestMove.SetY(0);
                    }
                    else
                    {
                        bestMove.SetX(0);
                        bestMove.SetY(0);
                    }
                }
                else if (enemyX == 2)
                {
                    if (enemyY == 0)
                    {
                        bestMove.SetX(0);
                        bestMove.SetY(0);
                    }
                    else
                    {
                        bestMove.SetX(0);
                        bestMove.SetY(2);
                    }
                }
            }
            else
            {
                if (enemyY == 1)
                {
                    if (enemyX == 0)
                    {
                        bestMove.SetX(0);
                        bestMove.SetY(0);
                    }
                    else
                    {
                        bestMove.SetX(2);
                        bestMove.SetY(0);
                    }
                }
                else if (enemyX == 1)
                {
                    if (enemyY == 0)
                    {
                        bestMove.SetX(0);
                        bestMove.SetY(0);
                    }
                    else
                    {
                        bestMove.SetX(0);
                        bestMove.SetY(2);
                    }
                }
            }
            return bestMove;
        }

        public Move GetMove(int player)
        {
            SetDepth();
            if (Depth == 7 && Board[1, 1] == player)
            {
                return FindOpeningCounter(player);
            }
            else if (Depth == 1)
            {
                List<Move> moveList = new List<Move>();
                moveList = RemainingDeathMatchMoves(player);
                Move bestMove = null;
                Move emptySpace = GetEmptySpace();
                int bestScore = int.MinValue;

                foreach (Move m in moveList)
                {
                    UndoMove(m);
                    DoMove(emptySpace, player);
                    int score = -NegamaxDeathMatch(Depth, player, int.MinValue + 1, int.MaxValue);
                    UndoMove(emptySpace);
                    DoMove(m, player);
                    if (m.GetX() == 1 && m.GetY() == 1)
                    {
                        score -= 50;
                    }
                    Console.WriteLine(m.GetY() + " " + m.GetX() + " = " + score);

                    if (score > bestScore)
                    {
                        bestMove = m;
                        bestScore = score;
                    }
                }
                if (bestMove != null)
                {
                    Move move = new Move() { OriginX = bestMove.GetX(), OriginY = bestMove.GetY(), X = emptySpace.GetX(), Y = emptySpace.GetY() };
                    return move;
                }
            }
            else
            {
                List<Move> allMoves = RemainingMoves();
                Move bestMove = null;
                int bestScore = int.MinValue;

                foreach (Move m in allMoves)
                {
                    DoMove(m, player);
                    int score = -Negamax(Depth, player, int.MinValue + 1, int.MaxValue);
                    UndoMove(m);
                    if (m.GetX() == 1 && m.GetY() == 1)
                    {
                        score += 25;
                    }
                    if ((m.GetX() + m.GetY()) % 2 == 0)
                    {
                        score += 25;
                    }
                    Console.WriteLine(m.GetY() + " " + m.GetX() + " = " + score);

                    if (score > bestScore)
                    {
                        bestMove = m;
                        bestScore = score;
                    }
                }
                if (bestMove != null)
                {
                    return bestMove;
                }
            }
            return null;
        }

        public void UndoMove(Move move)
        {
            Board[move.GetX(), move.GetY()] = 0;
        }

        public void SetDepth()
        {
            Depth = 0;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (Board[i, j] == 0)
                    {
                        Depth++;
                    }
                }
            }
        }

        public List<Move> RemainingMoves()
        {
            List<Move> moves = new List<Move>();
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (Board[i, j] == 0)
                    {
                        Move move = new Move();
                        move.SetX(i);
                        move.SetY(j);
                        moves.Add(move);
                    }
                }
            }
            return moves;
        }

        public List<Move> RemainingDeathMatchMoves(int player)
        {
            List<Move> moveList = new List<Move>();
            List<Move> moves = new List<Move>();
            int emptyX = -1;
            int emptyY = -1;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (Board[i, j] == 0)
                    {
                        emptyX = i;
                        emptyY = j;
                    }
                }
            }
            int emptyXCheck = emptyX;
            int emptyYCheck = emptyY;
            if (emptyX - 1 >= 0)
            {
                emptyXCheck = emptyX - 1;
                moves.Add(new Move { X = emptyXCheck, Y = emptyYCheck });
                if (emptyY - 1 >= 0 && ((emptyX + emptyY) % 2) == 0)
                {
                    emptyYCheck = emptyY - 1;
                    moves.Add(new Move { X = emptyXCheck, Y = emptyYCheck });
                }
                if (emptyY + 1 <= 2 && ((emptyX + emptyY) % 2) == 0)
                {
                    emptyYCheck = emptyY + 1;
                    moves.Add(new Move { X = emptyXCheck, Y = emptyYCheck });
                }

            }
            if (emptyX + 1 <= 2)
            {
                emptyXCheck = emptyX + 1;
                moves.Add(new Move { X = emptyXCheck, Y = emptyYCheck });
                if (emptyY - 1 >= 0 && ((emptyX + emptyY) % 2) == 0)
                {
                    emptyYCheck = emptyY - 1;
                    moves.Add(new Move { X = emptyXCheck, Y = emptyYCheck });
                }
                if (emptyY + 1 <= 2 && ((emptyX + emptyY) % 2) == 0)
                {
                    emptyYCheck = emptyY + 1;
                    moves.Add(new Move { X = emptyXCheck, Y = emptyYCheck });
                }

            }
            if (emptyY - 1 >= 0)
            {
                emptyYCheck = emptyY - 1;
                moves.Add(new Move { X = emptyX, Y = emptyYCheck });
            }
            if (emptyY + 1 <= 2)
            {
                emptyYCheck = emptyY + 1;
                moves.Add(new Move { X = emptyX, Y = emptyYCheck });
            }
            foreach (Move m in moves)
            {
                if (Board[m.GetX(), m.GetY()] == player)
                {
                    moveList.Add(m);
                }
            }
            return moveList;
        }

    }
}
