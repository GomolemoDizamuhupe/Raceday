using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Part2_Raceday.Models;

namespace Part2_Raceday.Backend
{
    public class Events
    {
        //connection string to connect
        private const string connection_string = @"Data source=(localdb)\MSSQLLocalDB;database=Race_day;";

        /**************************************************/

        private const string event_columns = "EventId, OrganiserId, Name, Description, EventDate, Location, EventType, DistanceKm, BannerUrl";

        private static EventDto map_event(SqlDataReader reader)
        {
            return new EventDto
            {
                EventId = reader.GetInt32(0),
                OrganiserId = reader.GetInt32(1),
                Name = reader.GetString(2),
                Description = reader.IsDBNull(3) ? null : reader.GetString(3),
                EventDate = reader.GetDateTime(4),
                Location = reader.GetString(5),
                EventType = reader.IsDBNull(6) ? null : reader.GetString(6),
                DistanceKm = reader.IsDBNull(7) ? null : reader.GetDecimal(7),
                BannerUrl = reader.IsDBNull(8) ? null : reader.GetString(8)
            };
        }

        public List<EventDto> get_all_events()
        {
            var list = new List<EventDto>();

            using (SqlConnection connect = new SqlConnection(connection_string))
            {
                connect.Open();

                using (SqlCommand cmd = new SqlCommand("SELECT " + event_columns + " FROM Events ORDER BY EventDate;", connect))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read()) list.Add(map_event(reader));
                }
            }

            return list;
        }

        public EventDto? get_event_by_id(int event_id)
        {
            using (SqlConnection connect = new SqlConnection(connection_string))
            {
                connect.Open();

                using (SqlCommand cmd = new SqlCommand("SELECT " + event_columns + " FROM Events WHERE EventId = @EventId;", connect))
                {
                    cmd.Parameters.AddWithValue("@EventId", event_id);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        return reader.Read() ? map_event(reader) : null;
                    }
                }
            }
        }

        // Returns the new EventId, or -1 if the user is not an organiser
        public int create_event(EventRequest req)
        {
            if (role_valiation(req.OrganiserId) != "organiser") return -1;

            using (SqlConnection connect = new SqlConnection(connection_string))
            {
                connect.Open();

                string query = @"INSERT INTO Events (OrganiserId, Name, Description, EventDate, Location, EventType, DistanceKm)
                         OUTPUT INSERTED.EventId
                         VALUES (@OrganiserId, @Name, @Description, @EventDate, @Location, @EventType, @DistanceKm);";

                using (SqlCommand cmd = new SqlCommand(query, connect))
                {
                    cmd.Parameters.AddWithValue("@OrganiserId", req.OrganiserId);
                    cmd.Parameters.AddWithValue("@Name", req.Name);
                    cmd.Parameters.AddWithValue("@Description", (object?)req.Description ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@EventDate", req.EventDate);
                    cmd.Parameters.AddWithValue("@Location", req.Location);
                    cmd.Parameters.AddWithValue("@EventType", (object?)req.EventType ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DistanceKm", (object?)req.DistanceKm ?? DBNull.Value);

                    return (int)cmd.ExecuteScalar();
                }
            }
        }

        // True if a row was updated (event exists and belongs to this organiser)
        public bool update_event(int event_id, EventRequest req)
        {
            using (SqlConnection connect = new SqlConnection(connection_string))
            {
                connect.Open();

                string query = @"UPDATE Events
                         SET Name = @Name, Description = @Description, EventDate = @EventDate,
                             Location = @Location, EventType = @EventType, DistanceKm = @DistanceKm
                         WHERE EventId = @EventId AND OrganiserId = @OrganiserId;";

                using (SqlCommand cmd = new SqlCommand(query, connect))
                {
                    cmd.Parameters.AddWithValue("@Name", req.Name);
                    cmd.Parameters.AddWithValue("@Description", (object?)req.Description ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@EventDate", req.EventDate);
                    cmd.Parameters.AddWithValue("@Location", req.Location);
                    cmd.Parameters.AddWithValue("@EventType", (object?)req.EventType ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DistanceKm", (object?)req.DistanceKm ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@EventId", event_id);
                    cmd.Parameters.AddWithValue("@OrganiserId", req.OrganiserId);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        // 1 = deleted, 0 = not found, -1 = event still has categories
        public int delete_event_by_id(int event_id, int organiser_id)
        {
            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();

                    using (SqlCommand cmd = new SqlCommand("DELETE FROM Events WHERE EventId = @EventId AND OrganiserId = @OrganiserId;", connect))
                    {
                        cmd.Parameters.AddWithValue("@EventId", event_id);
                        cmd.Parameters.AddWithValue("@OrganiserId", organiser_id);
                        return cmd.ExecuteNonQuery() > 0 ? 1 : 0;
                    }
                }
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                return -1;
            }
        }

        /**************************************************

        public List<EventDto> get_all_events()
        {
            var list = new List<EventDto>();

            using (SqlConnection connect = new SqlConnection(connection_string))
            {
                connect.Open();

                string query = "SELECT EventId, OrganiserId, Name, Description, EventDate, Location FROM Events ORDER BY EventDate;";

                using (SqlCommand cmd = new SqlCommand(query, connect))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new EventDto
                        {
                            EventId = reader.GetInt32(0),
                            OrganiserId = reader.GetInt32(1),
                            Name = reader.GetString(2),
                            Description = reader.IsDBNull(3) ? null : reader.GetString(3),
                            EventDate = reader.GetDateTime(4),
                            Location = reader.GetString(5)
                        });
                    }
                }
            }

            return list;
        }

        public EventDto? get_event_by_id(int event_id)
        {
            using (SqlConnection connect = new SqlConnection(connection_string))
            {
                connect.Open();

                string query = "SELECT EventId, OrganiserId, Name, Description, EventDate, Location FROM Events WHERE EventId = @EventId;";

                using (SqlCommand cmd = new SqlCommand(query, connect))
                {
                    cmd.Parameters.AddWithValue("@EventId", event_id);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read()) return null;

                        return new EventDto
                        {
                            EventId = reader.GetInt32(0),
                            OrganiserId = reader.GetInt32(1),
                            Name = reader.GetString(2),
                            Description = reader.IsDBNull(3) ? null : reader.GetString(3),
                            EventDate = reader.GetDateTime(4),
                            Location = reader.GetString(5)
                        };
                    }
                }
            }
        }
        *************************************************/

        public string uploading_event(int organiser_id, string name, string description, string event_date, string location)
        {
            //Converting the string "event_date" to DateTime
            Convert.ToDateTime(event_date);

            //temp message
            string message = string.Empty;

            string role = role_valiation(organiser_id);
            if (role == "organiser")
            {
                //try and catch
                try
                {
                    //then use the using method to prevent
                    //memory leaks

                    //sql connection is used for connecting to the database

                    using (SqlConnection connect = new SqlConnection(connection_string))
                    {
                        connect.Open();

                        //do a query to insert the user info
                        string query = string.Empty;
                        query = @"INSERT INTO Events (OrganiserId, Name, Description, EventDate, Location)
        VALUES('" + organiser_id + "', '" + name + "','" + description + "', '" + event_date + "', '" + location + "');";
                        //crearing sn instance to run the query
                        //using SqlCommand
                        SqlCommand run_query = new SqlCommand(query, connect);

                        //run non-query
                        run_query.ExecuteNonQuery();

                        //message
                        message = "Event Created Successfully.";

                        //then closing the connection
                        connect.Close();
                    }
                }
                catch (Exception error)
                {
                    //error messages
                    message = error.Message;
                }
            }
            else
            {
                message = "Event can not be added because of your role. You are not a organiser.";
            }

            //return the message
            return message;
        }

        
        public string editing_event(int event_id, int organiser_id, string name, string description, string event_date, string location)
        {
            //Converting the string "event_date" to DateTime
            Convert.ToDateTime(event_date);

            //temp message
            string message = string.Empty;

            string role = role_valiation(organiser_id);
            if (role == "organiser")
            {

                //try and catch
                try
                {
                    //then use the using method to prevent
                    //memory leaks

                    //sql connection is used for connecting to the database

                    using (SqlConnection connect = new SqlConnection(connection_string))
                    {
                        connect.Open();

                        //do a query to insert the user info
                        string query = string.Empty;

                        query = @"UPDATE Events SET Name = '" + name + "'," +
                                                    "Description = '" + description + "'," +
                                                    "EventDate = '" + event_date + "'," +
                                                    "Location = '" + location + "'" +
                                                    "WHERE OrganiserId = '" + organiser_id + "'" +
                                                    "AND EventId = '" + event_id + "';";

                        //crearing sn instance to run the query
                        //using SqlCommand
                        SqlCommand run_query = new SqlCommand(query, connect);

                        //run non-query
                        run_query.ExecuteNonQuery();

                        //message
                        message = "Event Updated Successfully.";

                        //then closing the connection
                        connect.Close();
                    }
                }
                catch (Exception error)
                {
                    //error messages
                    message = error.Message;
                }
            }
            else
            {
                message = "Event can not be edited because of your role. You are not a organiser.";
            }
            //return the message

            return message;
        }



        public string deleting_event(int event_id, int organiser_id)
        {
            //temp message
            string message = string.Empty;
            string role = role_valiation(organiser_id);
            if (role == "organiser")
            {

                //try and catch
                try
                {
                    //then use the using method to prevent
                    //memory leaks
                    //sql connection is used for connecting to the database
                    using (SqlConnection connect = new SqlConnection(connection_string))
                    {
                        connect.Open();

                        //do a query to insert the user info
                        string query = string.Empty;

                        query = @"DELETE FROM Events 
                                WHERE EventId = '" + event_id + "' AND OrganiserId = '" + organiser_id + "';";

                        //crearing sn instance to run the query
                        //using SqlCommand
                        SqlCommand run_query = new SqlCommand(query, connect);

                        //run non-query
                        run_query.ExecuteNonQuery();

                        //message
                        message = "Event Deleted Successfully.";

                        //then closing the connection
                        connect.Close();
                    }
                }
                catch (Exception error)
                {
                    //error messages
                    message = error.Message;
                }
            }
            else
            {
                message = "Event can not be edited because of your role. You are not a organiser.";
            }
            //return the message

            return message;
        }


        public List <string> viewing_event()
        {
            //temp message
            List <string> message = new List<string>();

            //try and catch
            try
            {
                //then use the using method to prevent
                //memory leaks
                //sql connection is used for connecting to the database
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();

                    //do a query to insert the user info
                    string query = string.Empty;

                    query = @"SELECT * FROM Events;";

                    //crearing sn instance to run the query
                    //using SqlCommand
                    using (SqlCommand run_query = new SqlCommand(query, connect))
                    {

                        using (SqlDataReader reader = run_query.ExecuteReader())
                        {

                            if (reader.Read())
                            {
                                do
                                {
                                    message.Add("Name: " + reader["Name"] + ", " +
                                              "Description: " + reader["Description"] + ", " +
                                              "EventDate: " + Convert.ToDateTime(reader["EventDate"]).ToString("yyyy-MM-dd") + ", " +
                                              "Location: " + reader["Location"]);
                                }while (reader.Read());
                            }
                            else
                            {
                                //message
                                message.Add("There is Events not found.");
                            }
                        }
                    }

                    //then closing the connection
                    connect.Close();
                }
            }
            catch (Exception error)
            {
                //error messages
                //message = error.Message;

                message.Add(error.Message);
            }
            //return the message

            return message;
        }


        public string viewing_event_using_eventid(int event_id)
        {
            //temp message
            string message = string.Empty;

            //try and catch
            try
            {
                //then use the using method to prevent
                //memory leaks
                //sql connection is used for connecting to the database
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();

                    //do a query to insert the user info
                    string query = string.Empty;

                    query = @"SELECT * FROM Events WHERE EventId = @EventId;";

                    //crearing sn instance to run the query
                    //using SqlCommand
                    using (SqlCommand run_query = new SqlCommand(query, connect))
                    {
                        run_query.Parameters.AddWithValue("@EventId", event_id);

                        using (SqlDataReader reader = run_query.ExecuteReader()) 
                        {

                            if (reader.Read())
                            {

                                message = "Name: " + reader["Name"] + ", \n" +
                                          "Description: " + reader["Description"] + ", \n" +
                                          "EventDate: " + Convert.ToDateTime(reader["EventDate"]).ToString("yyyy-MM-dd") + ", \n" +
                                          "Location: " + reader["Location"];
                            }
                            else 
                            {
                                //message
                                message = "Event not found.";
                            }
                        }
                    }

                    //then closing the connection
                    connect.Close();
                }
            }
            catch (Exception error)
            {
                //error messages
                message = error.Message;
            }
            //return the message

            return message;
        }



        public string role_valiation(int id)
        {
            string role = string.Empty;

            try
            {
                //sql connection is used for connecting to the database
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();

                    //do a query to insert the user info
                    string query = string.Empty;

                    query = @"SELECT Role FROM Users WHERE UserId = @UserId;";

                    //crearing sn instance to run the query
                    //using SqlCommand
                    SqlCommand run_query = new SqlCommand(query, connect);
                    run_query.Parameters.AddWithValue("@UserId", id);

                    //ExecuteScalar returns a single value(the count)
                    object results = run_query.ExecuteScalar();

                    if (results != null)
                    {
                        role = results.ToString();
                    }

                    //then closing the connection
                    connect.Close();
                }
            }
            catch (Exception error)
            {
                role = error.Message;
            }

            //returing the role
            return role.ToLower();
        }


    }
}
