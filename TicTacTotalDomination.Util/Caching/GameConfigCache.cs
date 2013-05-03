using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TicTacTotalDomination.Util.Games;

namespace TicTacTotalDomination.Util.Caching
{
    public class GameConfigCache
    {
        private static Lazy<GameConfigCache> _Instance = new Lazy<GameConfigCache>(() => new GameConfigCache());
        public static GameConfigCache Instance { get { return _Instance.Value; } }
        private GameConfigCache() { }

        private IGameConfigCacheProvider Cache;

        public void ApplyCacheMechanism(IGameConfigCacheProvider cacheMechanism)
        {
            this.Cache = cacheMechanism;
        }

        public void CacheConfig(int matchId, GameConfiguration config)
        {
            this.Cache.CacheConfig(matchId, config);
        }

        public GameConfiguration GetConfig(int matchId)
        {
            return this.Cache.GetConfig(matchId);
        }
    }

    public interface IGameConfigCacheProvider
    {
        void CacheConfig(int matchId, GameConfiguration config);
        GameConfiguration GetConfig(int matchId);
    }
}
