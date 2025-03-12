using System;
using System.IO;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace ImageDeduplicator.ImageSources
{
    public class ShimmieImageSource : DatabaseImageSource
    {
        String tags = String.Empty, imagePath;

        const String selectQueryFields =
            " select i.id, " +
            @" CONCAT(CONCAT(CONCAT(%IMAGES_PATH%,CONCAT('\',SUBSTRING(hash,1,2))),'\'),hash) image_file, " +
            //@" CONCAT(CONCAT(CONCAT(%THUMBS_PATH%,CONCAT('\',SUBSTRING(hash,1,2))) ,'\'),hash) thumb_file, " +
            @" '' thumb_file, " +
            //" GROUP_CONCAT(distinct t.tag SEPARATOR ' ') tags " +
            " array_to_string(array_agg(t.tag), ',') tags " +
            " from images i " +
            " INNER JOIN image_tags it ON it.image_id = i.id " +
            " INNER JOIN tags t ON t.id = it.tag_id";

        const String selectQueryPrefix = selectQueryFields +
            " where i.id IN" +
            " (select distinct image_id from image_tags where tag_id In" +
            " (select distinct id from tags where tag IN (";
        const String selectQuerySuffix = "))) GROUP BY i.id, i.hash";

        const String selectByIDRangeQuery = selectQueryFields +
            " where @StartID <= i.id AND i.id <= @EndID GROUP BY i.id, i.hash";

        const String refreshQuery = selectQueryFields +
            " where i.id = @ID GROUP BY i.id, i.hash";

        const String shimmieDeleteQuery = " delete from images where id = @ID";

        long StartId = -1, EndId = -1;

        public ShimmieImageSource(String provider, String connectionString, String tags, String imagePath) :
            base(tags, provider, connectionString,
            "", shimmieDeleteQuery)
        {
            this.tags = tags;
            this.imagePath = imagePath;
        }

        public ShimmieImageSource(String provider, String connectionString, long start_id, long end_id, String imagePath)
            : base(start_id.ToString() + "-" + end_id.ToString(), provider, connectionString,
            "", shimmieDeleteQuery)
        {
            this.StartId = start_id;
            this.EndId = end_id;
            this.imagePath = imagePath;
        }

        private Dictionary<String, String> getTags()
        {
            int i = 0;
            Dictionary<String, String> output = new Dictionary<string, string>();
            foreach (String tag in tags.Split(' '))
            {
                if (String.IsNullOrWhiteSpace(tag))
                    continue;
                output.Add("Tag" + i.ToString(), tag);
                i++;
            }
            return output;
        }

        const String IMAGES_PARAM = "IMAGE_PATH", THUMBS_PARAM = "THUMBS_PATH";

        private String replacePaths(String input)
        {
            return input.Replace("%IMAGES_PATH%", getParameterMarker() + IMAGES_PARAM).Replace("%THUMBS_PATH%", getParameterMarker() + THUMBS_PARAM);
        }

        protected override DbCommand getSelectCommand(DbConnection con)
        {
            DbCommand cmd;
            if (String.IsNullOrEmpty(tags))
            {
                cmd = base.getCommands(con, selectByIDRangeQuery)[0];
                cmd.Parameters.Add(getParameter("StartID", this.StartId));
                cmd.Parameters.Add(getParameter("EndID", this.EndId));
            }
            else
            {
                Dictionary<String, String> tags = getTags();
                cmd = base.getCommands(con, replacePaths(selectQueryPrefix) + getParameterMarker() + String.Join(", " + getParameterMarker(), tags.Keys.ToArray()) + selectQuerySuffix)[0];
                foreach (String name in tags.Keys)
                {
                    cmd.Parameters.Add(getParameter(name, tags[name]));
                }
            }
            cmd.CommandText = replacePaths(cmd.CommandText);
            cmd.Parameters.Add(getParameter(IMAGES_PARAM, Path.Combine(imagePath, "images")));
            cmd.Parameters.Add(getParameter(THUMBS_PARAM, Path.Combine(imagePath, "thumbs")));

            return cmd;
        }

        protected override ComparableImage createComparableImage(DataRow dr)
        {
            ComparableImage output = base.createComparableImage(dr);
            output.OverrideDisplayName = dr["id"] + " - " + dr["tags"];
            return output;
        }

        public override ComparableImage Refresh(ComparableImage image)
        {
            using (DbConnection con = getConnection())
            {
                con.Open();
                return RefreshFromDatabase(con, null, image);
            }
        }

        protected virtual ComparableImage RefreshFromDatabase(DbConnection con, DbTransaction t, ComparableImage image)
        {
            using (DbCommand cmd = getRefreshCommand(con, image))
            {
                using (DbDataAdapter da = getDataAdapter(cmd))
                {
                    using (DataTable dt = new DataTable())
                    {
                        da.Fill(dt);
                        if (dt.Rows.Count == 0)
                            throw new Exception("Image ID not found: " + image.InternalIdentifier);
                        return createComparableImage(dt.Rows[0]);
                    }
                }
            }
        }

        protected virtual DbCommand getRefreshCommand(DbConnection con, ComparableImage image)
        {
            List<DbCommand> output = getCommands(con, replacePaths(refreshQuery));
            foreach (DbCommand cmd in output)
            {
                cmd.Parameters.Add(getParameter("ID", image.InternalIdentifier));
                cmd.Parameters.Add(getParameter(IMAGES_PARAM, Path.Combine(imagePath, "images")));
                cmd.Parameters.Add(getParameter(THUMBS_PARAM, Path.Combine(imagePath, "thumbs")));
            }
            return output[0];
        }

        public override ComparableImage mergeImages(ComparableImage source, ComparableImage target)
        {
            List<ComparableImage> output = new List<ComparableImage>();
            using (DbConnection con = getConnection())
            {
                con.Open();
                List<Object> existingTags = new List<Object>();
                using (DbCommand tagsCmd = getCommands(con, "SELECT * FROM image_tags WHERE image_id = @ID")[0])
                {
                    tagsCmd.Parameters.Add(getParameter("ID", target.InternalIdentifier));
                    using (DbDataAdapter da = getDataAdapter(tagsCmd))
                    {
                        using (DataTable dt = new DataTable())
                        {
                            da.Fill(dt);
                            foreach (DataRow tagsDr in dt.Rows)
                            {
                                existingTags.Add(tagsDr["tag_id"]);
                            }
                        }
                    }
                }



                    using (DbTransaction t = con.BeginTransaction())
                    {
                        using (DbCommand cmd = getCommands(con, "SELECT * FROM image_tags WHERE image_id = @ID")[0])
                        {
                            cmd.Parameters.Add(getParameter("ID", source.InternalIdentifier));
                            cmd.Transaction = t;
                            using (DbDataAdapter da = getDataAdapter(cmd))
                            {
                                using (DataTable dt = new DataTable())
                                {
                                    da.Fill(dt);

                                    using (DbCommand iCmd = getCommands(con, "INSERT INTO image_tags (image_id, tag_id) VALUES (@ImageID, @TagID)")[0])
                                    {
                                        iCmd.Parameters.Add(getParameter("ImageID", target.InternalIdentifier));
                                        iCmd.Parameters.Add(getParameter("TagID", 0));
                                        iCmd.Transaction = t;

                                        foreach (DataRow dr in dt.Rows)
                                        {
                                            if (existingTags.Contains(dr["tag_id"]))
                                            {
                                                continue;
                                            }
                                            try
                                            {
                                                iCmd.Parameters["TagId"].Value = dr["tag_id"];
                                                iCmd.ExecuteNonQuery();
                                            }
                                            catch (DbException ex)
                                            {
                                                if (ex.Message.ToLower().Contains("duplicate entry") ||
                                                    ex.Message.ToLower().Contains("duplicate key"))
                                                    continue;
                                                throw ex;
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        this.deleteImageFromDatabase(con, t, source);

                        t.Commit();

                        this.deleteImageFromDisk(source);

                        return RefreshFromDatabase(con, null, target);
                    }
                }
            }
        }
    }
