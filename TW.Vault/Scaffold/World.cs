using System;
using System.Collections.Generic;

namespace TW.Vault.Scaffold
{
    public partial class World
    {
        public World()
        {
            Ally = new HashSet<Ally>();
            Command = new HashSet<Command>();
            CommandArmy = new HashSet<CommandArmy>();
            Conquer = new HashSet<Conquer>();
            CurrentBuilding = new HashSet<CurrentBuilding>();
            CurrentPlayer = new HashSet<CurrentPlayer>();
            CurrentVillage = new HashSet<CurrentVillage>();
            Player = new HashSet<Player>();
            Report = new HashSet<Report>();
            ReportArmy = new HashSet<ReportArmy>();
            ReportBuilding = new HashSet<ReportBuilding>();
            Transaction = new HashSet<Transaction>();
            User = new HashSet<User>();
            UserLog = new HashSet<UserLog>();
            Village = new HashSet<Village>();
            WorldSettings = new HashSet<WorldSettings>();
        }

        public short Id { get; set; }
        public string Name { get; set; }
        public string Hostname { get; set; }

        public ICollection<Ally> Ally { get; set; }
        public ICollection<Command> Command { get; set; }
        public ICollection<CommandArmy> CommandArmy { get; set; }
        public ICollection<Conquer> Conquer { get; set; }
        public ICollection<CurrentBuilding> CurrentBuilding { get; set; }
        public ICollection<CurrentPlayer> CurrentPlayer { get; set; }
        public ICollection<CurrentVillage> CurrentVillage { get; set; }
        public ICollection<Player> Player { get; set; }
        public ICollection<Report> Report { get; set; }
        public ICollection<ReportArmy> ReportArmy { get; set; }
        public ICollection<ReportBuilding> ReportBuilding { get; set; }
        public ICollection<Transaction> Transaction { get; set; }
        public ICollection<User> User { get; set; }
        public ICollection<UserLog> UserLog { get; set; }
        public ICollection<Village> Village { get; set; }
        public ICollection<WorldSettings> WorldSettings { get; set; }
    }
}
