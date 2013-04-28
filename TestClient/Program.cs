using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TicTacTotalDomination.Util.Serialization;
using NetCom = TicTacTotalDomination.Util.NetworkCommunication;

namespace TestClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Select a test.");
            Console.WriteLine("1: Test String Splitter");
            Console.WriteLine("2: Test Request Serialization");
            int selection = int.Parse(Console.ReadLine());

            switch(selection)
            {
                case 1:
                    TestStringSplitter();
                    break;
                case 2:
                    TestRequestSerialization();
                    break;
            }
        }

        static void TestRequestSerialization()
        {
            NetCom.ChallengeRequest request = new NetCom.ChallengeRequest();
            request.OpponentName = "TestMe";
            request.PlayerName = "TestYou";

            string jsonRequest = JsonSerializer.SerializeToJSON(request);
            Console.WriteLine("Serialized Request: {0}", jsonRequest);
            NetCom.ChallengeRequest deserializedRequest = JsonSerializer.DeseriaizeFromJSON<NetCom.ChallengeRequest>(jsonRequest);
            Console.WriteLine("PlayerName: {0}", deserializedRequest.PlayerName);
            Console.WriteLine("OpponentName: {0}", deserializedRequest.OpponentName);
            Console.ReadKey();
        }

        static void TestStringSplitter()
        {
            string again = "y";
            string test = "I am so ready to graduate. You have no idea how much going to school sucks. I'm ready to make some real money.";

            while (again == "y")
            {
                int splitSize = 0;
                Console.Write("How long should the substrings be: ");
                splitSize = int.Parse(Console.ReadLine());

                IEnumerable<string> splitPut = StringSplitter.SplitString(test, splitSize);

                string endResult = string.Join("", splitPut);

                Console.WriteLine(endResult);
                Console.WriteLine("Success: {0}", test == endResult);
                Console.Write("Continue: ");
                again = Console.ReadLine();
            }
        }
    }
}
