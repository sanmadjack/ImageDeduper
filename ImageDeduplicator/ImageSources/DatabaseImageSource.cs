using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using MySql.Data.MySqlClient;
using System.Threading.Tasks;
using Npgsql;

namespace ImageDeduplicator.ImageSources {
    public class DatabaseImageSource : AImageSource {
        public const string PROVIDER_MYSQL = "mysql";
        public const string PROVIDER_POSTGRES = "postgres";


        protected String name, provider, connectionString, selectQuery, deleteQuery, mergeQuery;

        public DatabaseImageSource(String name, String provider, String connectionString, String query, String deleteQuery) : base(name) {
            this.name = name;
            this.provider = provider;
            this.connectionString = connectionString;
            this.selectQuery = query;
            this.deleteQuery = deleteQuery;
        }

        protected String getParameterMarker() {
            switch (this.provider) {
                case PROVIDER_MYSQL:
                case PROVIDER_POSTGRES:
                    return "@";
                default:
                    throw new NotSupportedException("Database provider " + this.provider + " not supported");
            }
        }

        protected DbConnection getConnection() {
            switch (this.provider) {
                case PROVIDER_MYSQL:
                    return new MySqlConnection(this.connectionString);
                case PROVIDER_POSTGRES:
                    return new NpgsqlConnection(this.connectionString);
                default:
                    throw new NotSupportedException("Database provider " + this.provider + " not supported");
            }
        }

        protected List<DbCommand> getCommands(DbConnection con, String command) {
            List<DbCommand> output = new List<DbCommand>();
            foreach (String query in command.Split(';')) {
                if (string.IsNullOrWhiteSpace(query))
                    continue;
                DbCommand cmd;
                switch (this.provider) {
                    case PROVIDER_MYSQL:
                        cmd = new MySqlCommand(command, (MySqlConnection)con);
                        cmd.CommandTimeout = 120;
                        output.Add(cmd);
                        break;
                    case PROVIDER_POSTGRES:
                        cmd = new NpgsqlCommand(command, (NpgsqlConnection)con);
                        cmd.CommandTimeout = 120;
                        output.Add(cmd);
                        break;
                    default:
                        throw new NotSupportedException("Database provider " + this.provider + " not supported");
                }
            }
            return output;
        }

        protected virtual DbCommand getSelectCommand(DbConnection con) {
            return getCommands(con, selectQuery)[0];
        }
        protected virtual List<DbCommand> getDeleteCommands(DbConnection con, ComparableImage image) {
            List<DbCommand> output = getCommands(con, deleteQuery);
            foreach(DbCommand cmd in output) {
                cmd.Parameters.Add(getParameter("ID", image.InternalIdentifier));
            }
            return output;
        }



        protected virtual List<DbCommand> getMergeCommands(DbConnection con, ComparableImage target, ComparableImage source) {
            List<DbCommand> cmds = getCommands(con, mergeQuery);
            List<DbCommand> output = new List<DbCommand>();
            foreach (DbCommand cmd in output) {
                cmd.Parameters.Add(getParameter("TargetID", target.InternalIdentifier));
                cmd.Parameters.Add(getParameter("SourceID", source.InternalIdentifier));
            }
            return output;
        }

        protected DbDataAdapter getDataAdapter(DbCommand cmd) {
            switch (this.provider) {
                case PROVIDER_MYSQL:
                    return new MySqlDataAdapter((MySqlCommand)cmd);
                case PROVIDER_POSTGRES:
                    return new NpgsqlDataAdapter((NpgsqlCommand)cmd);
                default:
                    throw new NotSupportedException("Database provider " + this.provider + " not supported");
            }
        }

        protected Object getParameter(String name, Object value) {
            switch (this.provider) {
                case PROVIDER_MYSQL:
                    return new MySqlParameter(name, value);
                case PROVIDER_POSTGRES:
                    return new NpgsqlParameter(name, value);
                default:
                    throw new NotSupportedException("Database provider " + this.provider + " not supported");
            }
        }
        public override List<ComparableImage> getImages() {
            List<ComparableImage> output = new List<ComparableImage>();
            using (DbConnection con = getConnection()) {
                using (DbCommand cmd = getSelectCommand(con)) { 
                using (DbDataAdapter da = getDataAdapter(cmd)) {
                        using (DataTable dt = new DataTable()) {
                            da.Fill(dt);
                            foreach (DataRow dr in dt.Rows) {
                                if (dt.Columns.Count > 1 && !String.IsNullOrWhiteSpace(dr[1].ToString())) {
                                    output.Add(createComparableImage(dr));
                                } else
                                    throw new Exception("Two columns are required, one for the image path and one for the ID");

                            }
                        }
                    }
                    return output;
                }
            }
        }

        protected virtual ComparableImage createComparableImage(DataRow dr) {
            ComparableImage img = new ComparableImage(this, dr[1].ToString(), dr[2].ToString(), dr[0].ToString());
            img.InternalIdentifier = dr[0];
            return img;
        }

        public override void deleteImage(ComparableImage image) {
            if (image.InternalIdentifier == null)
                throw new Exception("No internal identifier found");
            if (String.IsNullOrEmpty(this.deleteQuery))
                throw new Exception("No delete query specified");

            using (DbConnection con = getConnection()) {
                con.Open();
                using (DbTransaction t = con.BeginTransaction()) {
                    deleteImageFromDatabase(con, t, image);
                    t.Commit();
                }
            }
            // This will delete the actual file from the filesystem
            base.deleteImage(image);
        }

        protected virtual void deleteImageFromDatabase(DbConnection con, DbTransaction t, ComparableImage image) {
                foreach (String query in this.deleteQuery.Split(';')) {
                    if (string.IsNullOrWhiteSpace(query))
                        continue;
                    using (DbCommand cmd = con.CreateCommand()) {
                        cmd.CommandText = query;
                        cmd.Transaction = t;
                        cmd.Parameters.Add(getParameter("ID", image.InternalIdentifier));
                        cmd.ExecuteNonQuery();
                    }
                }
            // This will delete the actual file from the filesystem
        }

        protected override List<String> getImagesInternal() {
            return null;
        }

    }
}
