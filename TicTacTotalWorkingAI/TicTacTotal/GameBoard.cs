using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicTacTotal
{
    class GameBoard
    {
        private int[,] board = new int[3, 3];
        public int depth = Int32.MaxValue;

        public GameBoard()
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    board[i, j] = 0;
                }
            }
        }
        public void doMove(Move move, int player)
        {
            if (board[move.getX(), move.getY()] == 0)
            {
                board[move.getX(), move.getY()] = player;
            }
            else
            {
                Console.WriteLine("Invalid Move\n");
            }
        }

        public void doMove(int x, int y, int player)
        {
            if (board[x, y] == 0)
            {
                board[x, y] = player;
            }
            else
            {
                Console.WriteLine("Invalid Move\n");
            }
        }

        public void parseMove(String input)
        {
            string[] parsedMove = input.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            int y = Int32.Parse(parsedMove[0]);
            int x = Int32.Parse(parsedMove[1]);
            Move move = new Move();
            move.setX(x);
            move.setY(y);
            doMove(move, -1);

        }

        public void parseMoveDeathMatch(String input)
        {
            Move emptySpace = getEmptySpace();
            string[] parsedMove = input.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            int y = Int32.Parse(parsedMove[0]);
            int x = Int32.Parse(parsedMove[1]);
            Move move = new Move();
            move.set(x, y);
            undoMove(move);
            doMove(emptySpace, -1);

        }

        public bool checkGameOver()
        {
            if (isWon(1))
            {
                Console.WriteLine("Player 1 wins!");
                return true;
            }
            else if (isWon(-1))
            {
                Console.WriteLine("Player -1 wins!");
                return true;
            }
            return false;
        }

        public bool isWon(int player)
        {
            if (board[0, 0] == player)
            {
                if (board[0, 0] == board[0, 1] &&
                        board[0, 1] == board[0, 2])
                    return true;

                if (board[0, 0] == board[1, 0] &&
                        board[1, 0] == board[2, 0])
                    return true;
            }

            if (board[2, 2] == player)
            {
                if (board[2, 0] == board[2, 1] &&
                        board[2, 1] == board[2, 2])
                    return true;

                if (board[0, 2] == board[1, 2] &&
                        board[1, 2] == board[2, 2])
                    return true;
            }

            if (board[1, 1] == player)
            {
                if (board[0, 1] == board[1, 1] &&
                        board[1, 1] == board[2, 1])
                    return true;

                if (board[1, 0] == board[1, 1] &&
                        board[1, 1] == board[1, 2])
                    return true;

                if (board[0, 0] == board[1, 1] &&
                        board[1, 1] == board[2, 2])
                    return true;

                if (board[0, 2] == board[1, 1] &&
                        board[1, 1] == board[2, 0])
                    return true;
            }

            return false;
        }

        int evaluate(int player)
        {
            int score = 0;

            if (isWon(player))
            {
                score = (-100);
            }
            if (isWon(-player))
            {
                score = (100);
            }

            return score;
        }

        int negamax(int depthVar, int player, int alpha, int beta)
        {
            if (isWon(player) || depthVar <= 0)
            {

                return evaluate(player);
            }

            List<Move> allMoves = remainingMoves();

            if (allMoves.Count == 0)
                return evaluate(player);
            foreach (Move m in allMoves)
            {
                doMove(m, -player);
                int val = -negamax(depthVar - 1, -player, -beta, -alpha);
                undoMove(m);
                if (val >= beta)
                    return val;

                if (val > alpha)
                    alpha = val;
            }
            return alpha;
        }

        public Move getEmptySpace()
        {
            Move move = new Move();
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (board[i, j] == 0)
                    {
                        move.set(i, j);
                    }
                }
            }
            return move;
        }

        int negamaxDeathMatch(int depthVar, int player, int alpha, int beta)
        {
            if (isWon(player) || depthVar == 0)
            {

                return evaluate(player);
            }

            DeathMatchHelper deathMatchHelper = remainingDeathMatchMoves(-player);
            Move emptySpace = getEmptySpace();

            if (deathMatchHelper.moveList.Count == 0)
                return evaluate(player);
            foreach (Move m in deathMatchHelper.moveList)
            {
                undoMove(m);
                doMove(emptySpace, -player);
                int val = -negamaxDeathMatch(depthVar - 1, -player, -beta, -alpha);
                undoMove(emptySpace);
                doMove(m, -player);
                if (val >= beta)
                    return val;

                if (val > alpha)
                    alpha = val;
            }
            return alpha;
        }

        public Move findOpeningCounter(int player)
        {
            int enemyX = -1;
            int enemyY = -1;
            Move bestMove = new Move();

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (board[i, j] == -player)
                    {
                        enemyX = i;
                        enemyY = j;
                    }
                }
            }

            if ((enemyX + enemyY)%2 == 0)
            {
                if (enemyX == 0)
                {
                    if (enemyY == 0)
                    {
                        bestMove.setX(2);
                        bestMove.setY(0);
                    }
                    else
                    {
                        bestMove.setX(0);
                        bestMove.setY(0);
                    }
                }
                else if (enemyX == 2)
                {
                    if (enemyY == 0)
                    {
                        bestMove.setX(0);
                        bestMove.setY(0);
                    }
                    else
                    {
                        bestMove.setX(0);
                        bestMove.setY(2);
                    }
                }
            }
            else
            {
                if (enemyY == 1)
                {
                    if (enemyX == 0)
                    {
                        bestMove.setX(0);
                        bestMove.setY(0);
                    }
                    else
                    {
                        bestMove.setX(2);
                        bestMove.setY(0);
                    }
                }
                else if (enemyX == 1)
                {
                    if (enemyY == 0)
                    {
                        bestMove.setX(0);
                        bestMove.setY(0);
                    }
                    else
                    {
                        bestMove.setX(0);
                        bestMove.setY(2);
                    }
                }
            }
            return bestMove;
        }

        public void getMove(int player)
        {
            setDepth();
            if (depth == 8 && board[1, 1] == player)
            {
                doMove(findOpeningCounter(player), player);
            }
            else if (depth == 1)
            {
                DeathMatchHelper deathMatchHelper = remainingDeathMatchMoves(player);
                Move bestMove = null;
                Move emptySpace = getEmptySpace();
                int bestScore = int.MinValue;

                foreach (Move m in deathMatchHelper.moveList)
                {
                        undoMove(m);
                        doMove(emptySpace, player);
                        int score = -negamaxDeathMatch(depth, player, int.MinValue + 1, int.MaxValue);
                        undoMove(emptySpace);
                        doMove(m, player);
                        if (m.getX() == 1 && m.getY() == 1)
                        {
                            score -= 50;
                        }
                    Console.WriteLine(m.getY() + " " + m.getX() + " = " + score);

                    if (score > bestScore)
                    {
                        bestMove = m;
                        bestScore = score;
                    }
                }
                if (bestMove != null)
                {
                    undoMove(bestMove);
                    doMove(emptySpace, player);
                }
            }
            else
            {
                List<Move> allMoves = remainingMoves();
                Move bestMove = null;
                int bestScore = int.MinValue;

                foreach (Move m in allMoves)
                {
                    doMove(m, player);
                    int score = -negamax(depth, player, int.MinValue + 1, int.MaxValue);
                    undoMove(m);
                    if (m.getX() == 1 && m.getY() == 1)
                    {
                        score += 25;
                    }
                    if ((m.getX() + m.getY())%2 == 0)
                    {
                        score += 25;
                    }
                    Console.WriteLine(m.getY() + " " + m.getX() + " = " + score);

                    if (score > bestScore)
                    {
                        bestMove = m;
                        bestScore = score;
                    }
                }
                if (bestMove != null)
                {
                    doMove(bestMove, player);
                }
            }
        }

        public void undoMove(Move move)
        {
            board[move.getX(), move.getY()] = 0;
        }

        public void setDepth()
        {
            depth = 0;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (board[i, j] == 0)
                    {
                        depth++;
                    }
                }
            }
        }

        public List<Move> remainingMoves()
        {
            List<Move> moves = new List<Move>();
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (board[i, j] == 0)
                    {
                        Move move = new Move();
                        move.setX(i);
                        move.setY(j);
                        moves.Add(move);
                    }
                }
            }
            return moves;
        }

        public DeathMatchHelper remainingDeathMatchMoves(int player)
        {
            DeathMatchHelper deathMatchHelper = new DeathMatchHelper();
            List<Move> moves = new List<Move>();
            int emptyX = -1;
            int emptyY = -1;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (board[i, j] == 0)
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
                moves.Add(new Move{x = emptyXCheck, y=emptyYCheck});
                if (emptyY - 1 >= 0)
                {
                    emptyYCheck = emptyY - 1;
                    moves.Add(new Move { x = emptyXCheck, y = emptyYCheck });
                }
                if (emptyY + 1 <= 2)
                {
                    emptyYCheck = emptyY + 1;
                    moves.Add(new Move { x = emptyXCheck, y = emptyYCheck });
                }

            }
            if (emptyX + 1 <= 2)
            {
                emptyXCheck = emptyX + 1;
                moves.Add(new Move { x = emptyXCheck, y = emptyYCheck });
                if (emptyY - 1 >= 0)
                {
                    emptyYCheck = emptyY - 1;
                    moves.Add(new Move { x = emptyXCheck, y = emptyYCheck });
                }
                if (emptyY + 1 <= 2)
                {
                    emptyYCheck = emptyY + 1;
                    moves.Add(new Move { x = emptyXCheck, y = emptyYCheck });
                }

            }
            if (emptyY - 1 >= 0)
            {
                emptyYCheck = emptyY - 1;
                moves.Add(new Move { x = emptyX, y = emptyYCheck });
            }
            if (emptyY + 1 <= 2)
            {
                emptyYCheck = emptyY + 1;
                moves.Add(new Move { x = emptyX, y = emptyYCheck });
            }
            foreach (Move m in moves)
            {
                if (board[m.getX(), m.getY()] == player)
                {
                    deathMatchHelper.moveList.Add(m);
                }
            }
            return deathMatchHelper;
        }

        public String displayBoard()
        {
            String boardDisplay = board[0, 0] + "\t" + board[0, 1] + "\t" + board[0, 2] + "\n" + board[1, 0] + "\t" +
                                  board[1, 1] + "\t" + board[1, 2] + "\n" + board[2, 0] + "\t" + board[2, 1] + "\t" +
                                  board[2, 2] + "\n";

            return boardDisplay;
        }

    }
}
