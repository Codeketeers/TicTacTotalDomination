using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using TicTacTotalDomination.Util.DataServices;
using TicTacTotalDomination.Util.Models;

namespace TicTacTotalDomination.Util.NetworkCommunication
{
    public class CentralServerCommunicationChannel : ICommunicationChannel
    {
        private bool disposed = false;

        void ICommunicationChannel.ChallengePlayer(string playerName, string oppoentName, int gameId)
        {
            using(IGameDataService dataService = new GameDataService())
            {
                //We need to create a session even if the challenge isn't accepted.
                dataService.CreateCentralServerSession(gameId);
            }

            var requestData = new ChallengeRequest();
            requestData.PlayerName = playerName;
            requestData.OpponentName = oppoentName;

            string requestJSON = CentralServerCommunicationChannel.SerializeDataToJSON<ChallengeRequest>(requestData);
            var requestConfig = new ServerRequestConfig();
            requestConfig.Url = string.Format("{0}/ServerPairing.php", ConfigurationManager.AppSettings["CentralServerUrl"]);
            requestConfig.RequestData = requestJSON;
            requestConfig.GameId = gameId;
            requestConfig.ResponseAction = new Action<string,int>(ChallengePlayerCompleted);
        }

        void ICommunicationChannel.PostMove(int gameId, string playerName, int x, int y, StatusFlag flag)
        {
            var requestData = new MoveRequest();
            requestData.PlayerName = playerName;
            requestData.GameId = -1;
            requestData.X = x;
            requestData.Y = y;

            using (IGameDataService dataService = new GameDataService())
            {
                CentralServerSession session = dataService.GetCentralServerSession(null, null, gameId);
                if (session != null)
                    requestData.GameId = session.CentralServerGameId.Value;
            }

            string requestJSON = CentralServerCommunicationChannel.SerializeDataToJSON<MoveRequest>(requestData);
            var requestConfig = new ServerRequestConfig();
            requestConfig.Url = string.Format("{0}/play.php", ConfigurationManager.AppSettings["CentralServerUrl"]);
            requestConfig.RequestData = requestJSON;
            requestConfig.GameId = gameId;
            requestConfig.ResponseAction = new Action<string,int>(PostMoveCompleted);
        }

        public static string SerializeDataToJSON<T>(T data)
            where T : class
        {
            var dataStream = new MemoryStream();
            var dataSerializer = new DataContractJsonSerializer(typeof(T));
            dataSerializer.WriteObject(dataStream, data);
            byte[] dataBytes = dataStream.ToArray();
            dataStream.Close();
            string result = Encoding.UTF8.GetString(dataBytes, 0, dataBytes.Length);

            return result;
        }

        public static T DeSerializeDataFromJSON<T>(string jsonData)
            where T : class
        {
            var dataStream = new MemoryStream(Encoding.UTF8.GetByteCount(jsonData));
            var dataSerializer = new DataContractJsonSerializer(typeof(T));
            T result = dataSerializer.ReadObject(dataStream) as T;

            return result;
        }

        private void PerformServerRequest(ServerRequestConfig config)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (sender, args) =>
                {
                    var workerConfig = (ServerRequestConfig)args.Argument;

                    //TODO: Add configuration value for url.
                    var request = (HttpWebRequest)WebRequest.Create(workerConfig.Url);
                    request.Method = WebRequestMethods.Http.Post;

                    using(Stream requestStream = request.GetRequestStream())
                    using (var requestWriter = new StreamWriter(requestStream))
                    {
                        requestWriter.Write(workerConfig);
                        requestWriter.Flush();
                        requestWriter.Close();
                    }

                    string responseData = null;
                    using (var response = (HttpWebResponse)request.GetResponse())
                    using (Stream responseStream = response.GetResponseStream())
                    using (var responseReader = new StreamReader(responseStream))
                    {
                        responseData = responseReader.ReadToEnd();
                    }

                    workerConfig.ResponseAction(responseData, workerConfig.GameId);
                };
            worker.RunWorkerAsync(config);
        }

        private class ServerRequestConfig
        {
            public string Url { get; set; }
            public string RequestData { get; set; }
            public int GameId { get; set; }
            public Action<string,int> ResponseAction { get; set; }
        }

        #region Central Server Response Handling
        static void ChallengePlayerCompleted(string data, int gameId)
        {
            var response = CentralServerCommunicationChannel.DeSerializeDataFromJSON<ChallengeResponse>(data);
            if (string.IsNullOrEmpty(response.Error))
            {
                using (IGameDataService dataService = new GameDataService())
                {
                    var game = dataService.GetGame(gameId);
                    var session = dataService.GetCentralServerSession(null, null, gameId);
                    session.CentralServerGameId = response.GameId;
                    //I'll need some of the other game logic in order to finish this call.
                }
            }
        }

        static void PostMoveCompleted(string data, int gameId)
        {
            throw new NotImplementedException("PostMoveCompleted not implemented.");
        }
        #endregion

        #region Disposable
        void IDisposable.Dispose()
        {
            GC.SuppressFinalize(this);
            this.Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                }

                this.disposed = true;
            }
        }
        #endregion
    }
}
