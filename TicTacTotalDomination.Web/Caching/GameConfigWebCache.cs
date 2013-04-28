using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using TicTacTotalDomination.Util.Caching;
using TicTacTotalDomination.Util.DataServices;
using TicTacTotalDomination.Util.Games;
using TicTacTotalDomination.Util.Models;
using TicTacTotalDomination.Util.Serialization;

namespace TicTacTotalDomination.Web.Caching
{
    public class GameConfigWebCache
        : IGameConfigCacheProvider
    {
        Cache HttpCache { get { return HttpRuntime.Cache; } }

        void IGameConfigCacheProvider.CacheConfig(int matchId, Util.Games.GameConfiguration config)
        {
            this.HttpCache.Insert(matchId.ToString(), config, null, Cache.NoAbsoluteExpiration, new TimeSpan(0, 30, 0));
        }

        GameConfiguration IGameConfigCacheProvider.GetConfig(int matchId)
        {
            if (this.HttpCache[matchId.ToString()] == null)
            {
                using (IGameDataService gameDataService = new GameDataService())
                {
                    IEnumerable<ConfigSection> sections = gameDataService.GetConfigSections(matchId);
                    var jsonConfig = string.Join("", sections.OrderBy(section => section.SectionId));
                    GameConfiguration config = JsonSerializer.DeseriaizeFromJSON<GameConfiguration>(jsonConfig);

                    (this as IGameConfigCacheProvider).CacheConfig(matchId, config);
                }
            }

            return this.HttpCache[matchId.ToString()] as GameConfiguration;
        }
    }
}