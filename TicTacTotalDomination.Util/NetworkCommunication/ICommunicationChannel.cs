using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace TicTacTotalDomination.Util.NetworkCommunication
{
    public enum StatusFlag{None, WinningMove, DrawMove, ChallengeWin, ChallengeMove, AcceptLoss}

    [ServiceContract]
    public interface ICommunicationChannel : IDisposable
    {
        /// <summary>
        /// Posts a request for challenge to the central server. When the response is recieved the database will be updated as necessary with the response data.
        /// </summary>
        /// <param name="playerName">The (local) player name.</param>
        /// <param name="opponentName">The name of the player being challenged.</param>
        /// <param name="gameId">The TicTacTotalDomination game id of the game to be played.</param>
        void ChallengePlayer(string playerName, string opponentName, int gameId);
        /// <summary>
        /// Submits a move to the central server when playing against an oponent.
        /// </summary>
        /// <param name="gameId">The TicTacTotalDomination game id of the game to be played.</param>
        /// <param name="playerName">The name of the player who is posting the move.</param>
        /// <param name="x">The x coordinate of the move. If the move is resolving a draw conflict, this needs to be the x coordinate of the originating move.</param>
        /// <param name="y">The x coordinate of the move. If the move is resolving a draw conflict, this needs to be the x coordinate of the originating move.</param>
        /// <param name="flag">The flag (if any necessary) to indicate state of the game.</param>
        void PostMove(int gameId, string playerName, int x, int y, StatusFlag flag);
    }

    [DataContract]
    public class ChallengeRequest
    {
        [DataMember(Name = "challenge")]
        public string OpponentName { get; set; }
        [DataMember(Name = "from")]
        public string PlayerName { get; set; }
    }

    [DataContract]
    public class ChallengeResponse
    {
        [DataMember(Name = "gameID")]
        public int GameId { get; set; }
        [DataMember(Name = "message")]
        public string Messsage { get; set; }
        [DataMember(Name = "challenger")]
        public string OpponentId { get; set; }
        [DataMember(Name = "yourTurn")]
        public bool YourTurn { get; set; }
        [DataMember(Name = "x")]
        public int X { get; set; }
        [DataMember(Name = "y")]
        public int Y { get; set; }
        [DataMember(Name = "error")]
        public string Error { get; set; }
    }

    [DataContract]
    public class MoveRequest
    {
        [DataMember(Name = "x")]
        public int X { get; set; }
        [DataMember(Name = "y")]
        public int Y { get; set; }
        [DataMember(Name = "from")]
        public string PlayerName { get; set; }
        [DataMember(Name = "gameID")]
        public int GameId { get; set; }
        [DataMember(Name = "flags")]
        public string Flags { get; set; }
    }

    [DataContract]
    public class MoveResponse
    {
        [DataMember(Name = "x")]
        public int X { get; set; }
        [DataMember(Name = "y")]
        public int Y { get; set; }
        [DataMember(Name = "time")]
        public DateTime MoveDatetime { get; set; }
        [DataMember(Name = "flags")]
        public string StatusFlag { get; set; }
        [DataMember(Name = "error")]
        public string Error { get; set; }
    }
}
