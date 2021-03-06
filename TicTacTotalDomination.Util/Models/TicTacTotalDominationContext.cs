using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using TicTacTotalDomination.Util.Models.Mapping;

namespace TicTacTotalDomination.Util.Models
{
    public partial class TicTacTotalDominationContext : DbContext
    {
        static TicTacTotalDominationContext()
        {
            Database.SetInitializer<TicTacTotalDominationContext>(null);
        }

        public TicTacTotalDominationContext()
            : base("Name=TicTacTotalDominationContext")
        {
        }

        public DbSet<AIGame> AIGames { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<AuditLogSection> AuditLogSections { get; set; }
        public DbSet<CentralServerSession> CentralServerSessions { get; set; }
        public DbSet<ConfigSection> ConfigSections { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<GameMove> GameMoves { get; set; }
        public DbSet<Match> Matches { get; set; }
        public DbSet<Player> Players { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new AIGameMap());
            modelBuilder.Configurations.Add(new AuditLogMap());
            modelBuilder.Configurations.Add(new AuditLogSectionMap());
            modelBuilder.Configurations.Add(new CentralServerSessionMap());
            modelBuilder.Configurations.Add(new ConfigSectionMap());
            modelBuilder.Configurations.Add(new GameMap());
            modelBuilder.Configurations.Add(new GameMoveMap());
            modelBuilder.Configurations.Add(new MatchMap());
            modelBuilder.Configurations.Add(new PlayerMap());
        }
    }
}
