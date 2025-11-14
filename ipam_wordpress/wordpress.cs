// ***********************************************************************
// Assembly         : ipam_wordpress
// Author           : mcontri
// Created          : 05-31-2019
//
// Last Modified By : mcontri
// Last Modified On : 08-21-2014
// ***********************************************************************
// <copyright file="wordpress.cs" company="IPAM">
//     Copyright ©  2014
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Data;
using System.Linq;
using MySql.Data.MySqlClient;
// START MOD: modify post name to remove special character
using System.Text.RegularExpressions;
// END MOD: modify post name to remove special character

namespace ipam_wordpress
{
    /// <summary>
    /// Class wordpress.
    /// </summary>
    public static class wordpress
    {
        #region "constants"

        /// <summary>
        /// The c post
        /// </summary>
        private const String c_post = "SELECT post_ID FROM wp_postmeta WHERE meta_key='program_code' and meta_value = @_pc";
        /// <summary>
        /// The g post
        /// </summary>
        private const String g_post = "SELECT * FROM wp_posts WHERE id = @_pi";
        /// <summary>
        /// The i post
        /// </summary>
        /// Added by Jun 12/2020
        /// The post_content, post_excerpt, to_ping, pinged, post_content_filtered
        /// in Wordpress post table need to have an value instead of null
        /// T12262020 - A - create - workshop
        /// private const String i_post = "INSERT INTO wp_posts (post_author, post_date, post_date_gmt, post_title, post_status, comment_status, ping_status, post_name, post_parent, post_type) VALUES(@post_author, @post_date, @post_date_gmt, @post_title, @post_status, @comment_status, @ping_status, @post_name, @post_parent, @post_type); SELECT last_insert_id();";
        /// This is the 12/2020 version
        /// private const String i_post = "INSERT INTO wp_posts (post_author, post_date, post_date_gmt, post_content, post_title, post_excerpt, post_status, comment_status, ping_status, post_name, to_ping, pinged, post_content_filtered, post_parent, post_type) VALUES(@post_author, @post_date, @post_date_gmt, @post_content, @post_title, @post_excerpt, @post_status, @comment_status, @ping_status, @post_name, @to_ping, @pinged, @post_content_filtered, @post_parent, @post_type); SELECT last_insert_id();";
        /// insert-or-update-workshop-freeze - 10/25/2021
        private const String i_post = "INSERT INTO wp_posts (post_author, post_date, post_date_gmt, post_content, post_title, post_excerpt, post_status, comment_status, ping_status, post_name, to_ping, pinged, post_modified, post_modified_gmt, post_content_filtered, post_parent, post_type) VALUES(@post_author, @post_date, @post_date_gmt, @post_content, @post_title, @post_excerpt, @post_status, @comment_status, @ping_status, @post_name, @to_ping, @pinged, @post_modified, @post_modified_gmt, @post_content_filtered, @post_parent, @post_type); SELECT last_insert_id();";
        /// <summary>
        /// The i meta
        /// </summary>
        private const String i_meta = "INSERT INTO wp_postmeta (post_id, meta_key, meta_value) VALUES(@post_id, @meta_key, @meta_value)";
        /// <summary>
        /// The u post
        /// </summary>
        private const String u_post = "UPDATE wp_posts SET guid = @guid, post_name = @post_name, post_title = @post_title WHERE ID=@post_ID";
        /// <summary>
        /// The u meta
        /// </summary>
        private const String u_meta = "UPDATE wp_postmeta SET meta_value = @meta_value WHERE post_id = @post_id AND meta_key = @meta_key";

        /// <summary>
        /// The unique identifier
        /// </summary>
        private const String guid = "http://coruscant.ipam.ucla.edu/?post_type=programs&#038;p=";
        /// <summary>
        /// The key pc
        /// </summary>
        private const String key_pc = "program_code";
        /// <summary>
        /// The key ac
        /// </summary>
        private const String key_ac = "parent_code";
        /// <summary>
        /// The key sd
        /// </summary>
        private const String key_sd = "date_start";
        /// <summary>
        /// The key ed
        /// </summary>
        private const String key_ed = "date_end";
        /// <summary>
        /// The key hv
        /// </summary>
        private const String key_hv = "has_video";
        /// <summary>
        /// The key ad
        /// </summary>
        private const String key_ad = "application_deadline";
        /// <summary>
        /// The key FPC
        /// </summary>
        private const String key_fpc = "_" + key_pc; //custom field control; Program Code
        /// <summary>
        /// The key fac
        /// </summary>
        private const String key_fac = "_" + key_ac; //custom field control; Parent Code
        /// <summary>
        /// The key FSD
        /// </summary>
        private const String key_fsd = "_" + key_sd; //custom field control; Start Date
        /// <summary>
        /// The key fed
        /// </summary>
        private const String key_fed = "_" + key_ed; //custom field control; End Date
        /// <summary>
        /// The key FHV
        /// </summary>
        private const String key_fhv = "_" + key_hv; //custom field control; Has Video
        /// <summary>
        /// The key fad
        /// </summary>
        private const String key_fad = "_" + key_ad; //custom field control: Application Deadline
        /// <summary>
        /// The value FPC
        /// </summary>
        private const String value_fpc = "field_52a4495c419a1"; //custom field control ID from MySql database for Program Code
        /// <summary>
        /// The value fac
        /// </summary>
        private const String value_fac = "field_532ab10d42f3f"; //custom field control ID from MySql database for Parent Code
        /// <summary>
        /// The value FSD
        /// </summary>
        private const String value_fsd = "field_52a44972d2ba4"; //custom field control ID from MySql database for Start Date
        /// <summary>
        /// The value fed
        /// </summary>
        private const String value_fed = "field_52a449d0d2ba5"; //custom field control ID from MySql database for End Date
        /// <summary>
        /// The value FHV
        /// </summary>
        private const String value_fhv = "field_5347d751448ef"; //custom field control ID from MySql database for Has Video
        /// <summary>
        /// The value fad
        /// </summary>
        private const String value_fad = "field_5363779a50580"; //custom field control ID from MySql database for Application Deadline
        /// <summary>
        /// The sub status
        /// </summary>
        private const String sub_status = "closed";
        /// <summary>
        /// The post status
        /// </summary>
        private const String post_status = "draft";
        /// <summary>
        /// The post type
        /// </summary>
        private const String post_type = "programs";
        /// <summary>
        /// The post author
        /// </summary>
        private const int post_author = 4;
        /// <summary>
        /// The post content
        /// </summary>
        /// T12262020-A-create-workshop
        /// By Jun 12/2020
        private const String post_content = "";
        /// <summary>
        /// The post excerpt
        /// </summary>
        /// T12262020-A-create-workshop
        /// By Jun 12/2020
        private const String post_excerpt = "";
        /// <summary>
        /// The to_ping
        /// </summary>
        /// T12262020-A-create-workshop
        /// By Jun 12/2020
        private const String to_ping = "";
        /// <summary>
        /// The pinged
        /// </summary>
        /// T12262020-A-create-workshop
        /// By Jun 12/2020
        private const String pinged = "";
        /// <summary>
        /// The post content filtered
        /// </summary>
        /// T12262020-A-create-workshop
        /// By Jun 12/2020
        private const String post_content_filtered = "";

        /// <summary>
        /// The post lpparent
        /// </summary>
        private const int post_lpparent = 41;
        /// <summary>
        /// The post wsparent
        /// </summary>
        private const int post_wsparent = 48;
        /// <summary>
        /// The post ssparent
        /// </summary>
        private const int post_ssparent = 49;
        /// <summary>
        /// The post srparent
        /// </summary>
        private const int post_srparent = 50;
        /// <summary>
        /// The post separent
        /// </summary>
        private const int post_separent = 51;
        /// <summary>
        /// The post plparent
        /// </summary>
        private const int post_plparent = 52;
        /// <summary>
        /// The SQL lpparent
        /// </summary>
        private const int sql_lpparent = 1;
        /// <summary>
        /// The SQL wsparent
        /// </summary>
        private const int sql_wsparent = 2;
        /// <summary>
        /// The SQL ssparent
        /// </summary>
        private const int sql_ssparent = 3;
        /// <summary>
        /// The SQL oldseparent1
        /// </summary>
        private const int sql_oldseparent1 = 9;
        /// <summary>
        /// The SQL oldseparent2
        /// </summary>
        private const int sql_oldseparent2 = 10;
        /// <summary>
        /// The SQL srparent
        /// </summary>
        private const int sql_srparent = 11;
        /// <summary>
        /// The SQL separent
        /// </summary>
        private const int sql_separent = 13;
        /// <summary>
        /// The SQL plparent
        /// </summary>
        private const int sql_plparent = 12;

        #endregion

        #region "structs"
        /// <summary>
        /// Struct post
        /// </summary>
        public struct post
        {
            /// <summary>
            /// The comment status
            /// </summary>
            public String comment_status;
            /// <summary>
            /// The unique identifier
            /// </summary>
            public String guid;
            /// <summary>
            /// The ping status
            /// </summary>
            public String ping_status;
            /// <summary>
            /// The post slug
            /// </summary>
            public String post_slug;
            /// <summary>
            /// The post status
            /// </summary>
            public String post_status;
            /// <summary>
            /// The post title
            /// </summary>
            public String post_title;
            /// <summary>
            /// The post type
            /// </summary>
            public String post_type;
            /// <summary>
            /// The post author
            /// </summary>
            public int post_author;
            /// <summary>
            /// T12262020-A-create-workshop
            /// By Jun 12/2020
            public String post_content;
            /// </summary>
            /// <summary>
            /// T12262020-A-create-workshop
            /// By Jun 12/2020
            public String post_excerpt;
            /// <summary>
            /// T12262020-A-create-workshop
            /// By Jun 12/2020
            public String to_ping;
            /// <summary>
            /// T12262020-A-create-workshop
            /// By Jun 12/2020
            public String pinged;
            /// <summary>
            /// T12262020-A-create-workshop
            /// By Jun 12/2020
            public String post_content_filtered;
            /// <summary>
            /// The post identifier
            /// </summary>
            public int? post_id;
            /// <summary>
            /// The post parent
            /// </summary>
            public int post_parent;
            /// <summary>
            /// The meta fields
            /// </summary>
            public meta[] meta_fields;
        }

        /// <summary>
        /// Struct meta
        /// </summary>
        public struct meta
        {
            /// <summary>
            /// The key
            /// </summary>
            public String key;
            /// <summary>
            /// The value
            /// </summary>
            public String value;
        }
        #endregion

        #region "functions"

        /// <summary>
        /// Checks the post.
        /// </summary>
        /// <param name="_p">Instatiated post</param>
        public static void checkPost(ref post _p)
        {
            if (connection.mysql_conn.State == ConnectionState.Closed)
                connection.mysql_conn.Open();
            MySqlCommand _cmd = new MySqlCommand();
            String _sql = c_post;

            _cmd.Connection = connection.mysql_conn;
            _cmd.CommandType = CommandType.Text;
         //   _cmd.Prepare(); -- move down

            _cmd.Parameters.AddWithValue("@_pc", _p.meta_fields.First(p => p.key == "program_code").value);
            _cmd.CommandText = _sql;

            _cmd.Prepare();
            _p.post_id = Convert.ToInt32(_cmd.ExecuteScalar());

            connection.mysql_conn.Close();

        }

        /// <summary>
        /// Gets the post identifier.
        /// </summary>
        /// <param name="_pc">The pc.</param>
        /// <returns>Int32.</returns>
        public static Int32 getPostID(string _pc)
        {
            if (connection.mysql_conn.State == ConnectionState.Closed)
                connection.mysql_conn.Open();
            MySqlCommand _cmd = new MySqlCommand();
            Int32 _pId = 0;
            String _sql = c_post;

            _cmd.Connection = connection.mysql_conn;
            _cmd.CommandType = CommandType.Text;
           // _cmd.Prepare(); -- move down

            _cmd.Parameters.AddWithValue("@_pc", _pc);
            _cmd.CommandText = _sql;

            _cmd.Prepare();
            _pId = Convert.ToInt32(_cmd.ExecuteScalar());

            connection.mysql_conn.Close();
            return _pId;
        }

        /// <summary>
        /// Gets the post data.
        /// </summary>
        /// <param name="_p">The p.</param>
        /// <returns>DataTable.</returns>
        public static DataTable getPostData(post _p)
        {
            if (connection.mysql_conn.State == ConnectionState.Closed)
                connection.mysql_conn.Open();
            DataTable _dt = new DataTable();
            MySqlCommand _cmd = new MySqlCommand();
            MySqlDataAdapter _da = new MySqlDataAdapter();
            String _sql = g_post;

            _cmd.Connection = connection.mysql_conn;
            _cmd.CommandType = CommandType.Text;
          //  _cmd.Prepare(); --move down

            _cmd.Parameters.AddWithValue("@_pi", _p.post_id);
            _cmd.CommandText = _sql;

            _cmd.Prepare();
            _cmd.ExecuteNonQuery();

            _da = new MySqlDataAdapter(_cmd);
            _da.Fill(_dt);

            connection.mysql_conn.Close();

            return _dt;
        }

        /// <summary>
        /// Sets the post.
        /// </summary>
        /// <param name="_pc">program code</param>
        /// <param name="_pn">program name</param>
        /// <param name="_ac">The ac.</param>
        /// <param name="_sd">start date</param>
        /// <param name="_ed">end date</param>
        /// <param name="_hv">The hv.</param>
        /// <param name="_ad">The ad.</param>
        /// <param name="_pt">program type</param>
        /// <returns>post.</returns>
        public static post setPost(String _pc, String _pn, String _ac, String _sd, String _ed, String _hv, String _ad, Int32 _pt)
        {
            post _new = default(post);
            setPostDefaults(ref _new);
            setPostData(ref _new, _pn, _pt, null);
            setPostMeta(ref _new, _pc, _ac, _sd, _ed, _hv, _ad);
            return _new;
        }

        /// <summary>
        /// Sets the post for video.
        /// </summary>
        /// <param name="_p">The p.</param>
        /// <param name="_hv">The hv.</param>
        /// <returns>post.</returns>
        public static post setPostForVideo(post _p, String _hv)
        {
            setPostMetaVideo(ref _p, _hv);
            return _p;
        }

        /// <summary>
        /// Sets the post defaults.
        /// </summary>
        /// <param name="_p">Instatiated Post</param>
        private static void setPostDefaults(ref post _p)
        {
            _p.post_author = post_author;
            _p.post_status = post_status;
            _p.post_type = post_type;
            _p.comment_status = sub_status;
            _p.ping_status = sub_status;
            // Added by Jun 12/2020
            // The post_content in Wordpress post table need to have an value instead of null
            // T12262020 - A - create - workshop
            _p.post_content = post_content;
            _p.post_excerpt = post_excerpt;
            _p.to_ping = to_ping;
            _p.pinged = pinged;
            _p.post_content_filtered = post_content_filtered;
            
        }

        /// <summary>
        /// Sets the post meta video.
        /// </summary>
        /// <param name="_p">The p.</param>
        /// <param name="_hvv">The HVV.</param>
        private static void setPostMetaVideo(ref post _p, String _hvv)
        {
            setPostMeta(ref _p, null, null, null, null, _hvv, null);
        }

        /// <summary>
        /// Sets the post meta.
        /// </summary>
        /// <param name="_p">Instatiated post</param>
        /// <param name="_pcv">program code value</param>
        /// <param name="_acv">The acv.</param>
        /// <param name="_sdv">start date value</param>
        /// <param name="_edv">end date value</param>
        /// <param name="_hvv">The HVV.</param>
        /// <param name="_adv">The adv.</param>
        private static void setPostMeta(ref post _p, String _pcv, String _acv, String _sdv, String _edv, String _hvv, string _adv)
        {
            meta _pc = default(meta);
            meta _ac = default(meta);
            meta _sd = default(meta);
            meta _ed = default(meta);
            meta _hv = default(meta);
            meta _ad = default(meta);
            meta _f_pc = default(meta);
            meta _f_ac = default(meta);
            meta _f_sd = default(meta);
            meta _f_ed = default(meta);
            meta _f_hv = default(meta);
            meta _f_ad = default(meta);

            if (!String.IsNullOrEmpty(_pcv))
            {
                _pc.key = key_pc;
                _pc.value = _pcv;
                _f_pc.key = key_fpc;
                _f_pc.value = value_fpc;
            }
            if (!String.IsNullOrEmpty(_acv))
            {
                _ac.key = key_ac;
                _ac.value = _acv;
                _f_ac.key = key_fac;
                _f_ac.value = value_fac;
            }
            else
            {
                _ac.key = key_ac;
                _ac.value = "";
                _f_ac.key = key_fac;
                _f_ac.value = value_fac;
            }
            if (!String.IsNullOrEmpty(_sdv))
            {
                _sd.key = key_sd;
                _sd.value = _sdv;
                _f_sd.key = key_fsd;
                _f_sd.value = value_fsd;
            }
            if (!String.IsNullOrEmpty(_edv))
            {
                _ed.key = key_ed;
                _ed.value = _edv;
                _f_ed.key = key_fed;
                _f_ed.value = value_fed;
            }
            if (!String.IsNullOrEmpty(_hvv))
            {
                _hv.key = key_hv;
                _hv.value = _hvv;
                _f_hv.key = key_fhv;
                _f_hv.value = value_fhv;
            }
            if (!String.IsNullOrEmpty(_adv))
            {
                _ad.key = key_ad;
                _ad.value = _adv;
                _f_ad.key = key_fad;
                _f_ad.value = value_fad;
            }

            _p.meta_fields = new meta[]
            {
                _pc,
                _ac,
                _sd,
                _ed,
                _hv,
                _ad,
                _f_pc,
                _f_ac,
                _f_sd,
                _f_ed,
                _f_hv,
                _f_ad
            };
        }

        /// <summary>
        /// Sets the post data.
        /// </summary>
        /// <param name="_p">Instatiated post</param>
        /// <param name="_pn">program name</param>
        /// <param name="_pt">program type</param>
        /// <param name="_id">post ID</param>
        private static void setPostData(ref post _p, String _pn, Int32 _pt, Int32? _id)
        {
            _p.post_id = _id;
            // START MOD: modify-post-name
            // _p.post_slug = _pn.ToLower().Replace(",", "").Replace(":", "").Replace(";", "").Replace(".", "").Replace(' ', '-');
            // _p.post_slug = _pn.ToLower().Replace(",", "").Replace(":", "").Replace(";", "").Replace(".", "").Replace(' ', '-').Replace("(", "").Replace(")", "").Replace("?", "");

            _p.post_slug = Regex.Replace(_pn.ToLower(), @"[^0-9a-zA-Z]+", " ").Replace(' ', '-');

            // END MOD: modify-post-name
            _p.post_title = _pn;
            // Added by Jun 12/2020
            // The post_content in Wordpress post table need to have an value instead of null
            // T12262020 - A - create - workshop
            _p.post_content = "";
            _p.post_excerpt = "";
            _p.to_ping = "";
            _p.pinged = "";
            _p.post_content_filtered = "";
            switch (_pt)
            {
                case sql_lpparent:
                    _p.post_parent = post_lpparent;
                    break;
                case sql_plparent:
                    _p.post_parent = post_plparent;
                    break;
                case sql_separent:
                case sql_oldseparent1:
                case sql_oldseparent2:
                    _p.post_parent = post_separent;
                    break;
                case sql_srparent:
                    _p.post_parent = post_srparent;
                    break;
                case sql_ssparent:
                    _p.post_parent = post_ssparent;
                    break;
                case sql_wsparent:
                    _p.post_parent = post_wsparent;
                    break;
            }
        }

        /// <summary>
        /// Inserts or updates wordpress post
        /// </summary>
        /// <param name="_p">Instatiated post</param>
        /// <param name="_st">Save type -&gt; [1:INSERT] OR [2:UPDATE]</param>
        public static void savePost(ref wordpress.post _p, Int32 _st)
        {
            checkPost(ref _p);

            switch (_st)
            {
                case 1:
                    insert.Post(ref _p);
                    insert.Meta(_p);
                    break;
                case 2:
                    update.Post(ref _p);
                    update.Meta(_p);
                    break;
            }
        }

        /// <summary>
        /// Class insert.
        /// </summary>
        public class insert
        {
            /// <summary>
            /// Posts the specified p.
            /// </summary>
            /// <param name="_p">Instatiated post</param>
            public static void Post(ref post _p)
            {
                if (connection.mysql_conn.State == ConnectionState.Closed)
                    connection.mysql_conn.Open();
                MySqlCommand _cmd = new MySqlCommand();
                String _sql = i_post;

                _cmd.CommandType = CommandType.Text;
                _cmd.Connection = connection.mysql_conn;
                _cmd.CommandText = _sql;

                _cmd.Parameters.AddWithValue("@post_author", _p.post_author);
                _cmd.Parameters.AddWithValue("@post_date", DateTime.Now);
                _cmd.Parameters.AddWithValue("@post_date_gmt", DateTime.Now);
                // T12262020-A-create-workshop
                // By Jun 12/2020
                _cmd.Parameters.AddWithValue("@post_content", _p.post_content);
                _cmd.Parameters.AddWithValue("@post_title", _p.post_title);
                _cmd.Parameters.AddWithValue("@to_ping", _p.to_ping);
                _cmd.Parameters.AddWithValue("@pinged", _p.pinged);
                _cmd.Parameters.AddWithValue("@post_content_filtered", _p.post_content_filtered);

                // T12262020-A-create-workshop
                // By Jun 12/2020
                _cmd.Parameters.AddWithValue("@post_excerpt", _p.post_excerpt);
                _cmd.Parameters.AddWithValue("@post_status", _p.post_status);
                _cmd.Parameters.AddWithValue("@comment_status", _p.comment_status);
                _cmd.Parameters.AddWithValue("@ping_status", _p.ping_status);
                _cmd.Parameters.AddWithValue("@post_name", _p.post_slug);
                _cmd.Parameters.AddWithValue("@post_parent", _p.post_parent);
                _cmd.Parameters.AddWithValue("@post_type", _p.post_type);

                // insert-or-update-workshop-freeze
                _cmd.Parameters.AddWithValue("@post_modified", DateTime.Now);
                _cmd.Parameters.AddWithValue("@post_modified_gmt", DateTime.Now);
           

                _p.post_id = Convert.ToInt32(_cmd.ExecuteScalar());

                _p.guid = guid + _p.post_id.ToString();
                connection.mysql_conn.Close();
                update.Post(ref _p);
            }
            /// <summary>
            /// Metas the specified p.
            /// </summary>
            /// <param name="_p">Instatiated post</param>
            public static void Meta(post _p)
            {
                String _sql = i_meta;
                foreach (meta _c in _p.meta_fields)
                {
                    if (connection.mysql_conn.State == ConnectionState.Closed)
                        connection.mysql_conn.Open();
                    MySqlCommand _cmd = new MySqlCommand();

                    _cmd.Connection = connection.mysql_conn;
                    _cmd.CommandType = CommandType.Text;
                    _cmd.Prepare();

                    _cmd.Parameters.Clear();
                    _cmd.Parameters.AddWithValue("@post_id", _p.post_id);
                    _cmd.Parameters.AddWithValue("@meta_key", _c.key);
                    _cmd.Parameters.AddWithValue("@meta_value", _c.value);

                    _cmd.CommandText = _sql;
                    _cmd.ExecuteNonQuery();

                    connection.mysql_conn.Close();
                }
            }
        }
        /// <summary>
        /// Class update.
        /// </summary>
        public class update
        {
            /// <summary>
            /// Posts the specified p.
            /// </summary>
            /// <param name="_p">Instatiated post</param>
            public static void Post(ref post _p)
            {
                if (connection.mysql_conn.State == ConnectionState.Closed)
                    connection.mysql_conn.Open();
                MySqlCommand _cmd = new MySqlCommand();
                String _sql = u_post;

                _p.guid = guid + _p.post_id.ToString();
                _cmd.Connection = connection.mysql_conn;
                _cmd.CommandType = CommandType.Text;
                _cmd.CommandText = _sql;
            //    _cmd.Prepare(); -- move the statement to after the parameter

                _cmd.Parameters.AddWithValue("@post_name", _p.post_slug);
                _cmd.Parameters.AddWithValue("@post_title", _p.post_title);
                _cmd.Parameters.AddWithValue("@guid", _p.guid);
                _cmd.Parameters.AddWithValue("@post_ID", _p.post_id);

                _cmd.Prepare();
                _cmd.ExecuteNonQuery();

                connection.mysql_conn.Close();
            }
            /// <summary>
            /// Metas the specified p.
            /// </summary>
            /// <param name="_p">Instatiated post</param>
            public static void Meta(post _p)
            {
                foreach (meta _c in _p.meta_fields)
                {
                    if (!String.IsNullOrEmpty(_c.value))
                    {
                        if (connection.mysql_conn.State == ConnectionState.Closed)
                            connection.mysql_conn.Open();
                        MySqlCommand _cmd = new MySqlCommand();
                        String _sql = u_meta;

                        _cmd.Connection = connection.mysql_conn;
                        _cmd.CommandType = CommandType.Text;
                        _cmd.CommandText = _sql;
                     //    _cmd.Prepare(); -- move down

                        _cmd.Parameters.Clear();
                        _cmd.Parameters.AddWithValue("@post_id", _p.post_id);
                        _cmd.Parameters.AddWithValue("@meta_key", _c.key);
                        _cmd.Parameters.AddWithValue("@meta_value", _c.value);

                        _cmd.CommandText = _sql;
                        _cmd.Prepare();
                        _cmd.ExecuteNonQuery();

                        connection.mysql_conn.Close();
                    }
                }
            }
        }
        #endregion
    }
}