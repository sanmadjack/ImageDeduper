using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using MySql.Data.MySqlClient;
using System.Threading.Tasks;

namespace ImageDeduplicator.ImageSources {
    public class DatabaseImageSource : AImageSource {
        String name, provider, connectionString, query;

        public DatabaseImageSource(String name, String provider, String connectionString, String query) : base(name) {
            this.name = name;
            this.provider = provider;
            this.connectionString = connectionString;
            this.query = query;
        }


        public override List<ComparableImage> getImages() {
            List<ComparableImage> output = new List<ComparableImage>();
            DbConnection con = null;
            DbDataAdapter da = null;
            try {


                switch (this.provider) {
                    case "mysql":
                        con = new MySqlConnection(this.connectionString);
                        da = new MySqlDataAdapter(query, (MySqlConnection)con);
                        break;
                    default:
                        throw new NotSupportedException("Database provider " + this.provider + " not supported");
                }

                using (DataTable dt = new DataTable()) {
                    da.Fill(dt);
                    foreach (DataRow dr in dt.Rows) {
                        if (dt.Columns.Count > 1 &&!String.IsNullOrWhiteSpace(dr[1].ToString()))
                            output.Add(new ComparableImage(this, dr[0].ToString(), dr[1].ToString()));
                        else
                            output.Add(new ComparableImage(this, dr[0].ToString()));

                    }

                }

                return output;
            } finally {
                if (da != null)
                    da.Dispose();
                if (con != null)
                    con.Dispose();
            }
        }

        protected override List<String> getImagesInternal() {
            return null;
        }

    }
}
