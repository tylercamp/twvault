using System;
using System.Collections.Generic;

namespace TW.Vault.Scaffold
{
    public partial class ReportBuilding
    {
        public ReportBuilding()
        {
            Report = new HashSet<Report>();
        }

        public short? Main { get; set; }
        public short? Stable { get; set; }
        public short? Garage { get; set; }
        public short? Church { get; set; }
        public short? FirstChurch { get; set; }
        public short? Smith { get; set; }
        public short? Place { get; set; }
        public short? Statue { get; set; }
        public short? Market { get; set; }
        public short? Wood { get; set; }
        public short? Stone { get; set; }
        public short? Iron { get; set; }
        public short? Farm { get; set; }
        public short? Storage { get; set; }
        public short? Hide { get; set; }
        public short? Wall { get; set; }
        public short? Watchtower { get; set; }
        public short? Barracks { get; set; }
        public short? Snob { get; set; }
        public short WorldId { get; set; }
        public long ReportBuildingId { get; set; }

        public World World { get; set; }
        public ICollection<Report> Report { get; set; }
    }
}
