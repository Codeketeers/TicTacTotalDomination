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
    public interface ICommunicationChannel
    {
        void ChallengePlayer(string playerId, string opponentId);
        void PostMove(int gameId, string playerId, int x, int y, StatusFlag flag);
    }

    [DataContract]
    public class ChallengeResult
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
    public class MoveResult
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
