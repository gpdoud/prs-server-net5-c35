using System;
using System.Collections.Generic;
using PrsLibrary.Models;

namespace prs_server_net5_c35.ViewModels {

    public class Po {

        public Vendor Vendor { get; set; }
        public IEnumerable<Poline> Polines { get; set; }
        public decimal Total { get; set; }

    }
}

