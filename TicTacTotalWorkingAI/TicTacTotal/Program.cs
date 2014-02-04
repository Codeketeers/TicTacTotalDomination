using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicTacTotal
{
    class Program
    {
        static void Main(string[] args)
        {
            GameBoard gameBoard = new GameBoard();
            bool gameOver = false;
            while (!gameOver)
            {
                gameBoard.setDepth();

                
                if (gameBoard.depth > 1)
                {
                    Console.WriteLine("Enter the coordinates for desired move: ");
                    gameBoard.parseMove(Console.ReadLine());
                }
                else if (gameBoard.depth <= 1)
                {
                    Console.WriteLine("Enter the coordinates the marker you want moved to the empty space: ");
                    gameBoard.parseMoveDeathMatch(Console.ReadLine());
                }
                Console.WriteLine(gameBoard.displayBoard());
                gameBoard.checkGameOver();
                gameBoard.getMove(1);
                Console.WriteLine(gameBoard.displayBoard());
                gameBoard.checkGameOver();


            }
        }
    }
}
