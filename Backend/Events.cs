using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace Part2_Raceday.Backend
{
    public class Events
    {

        public string uploading_event(int organiser_id, string name, string description, string event_date, string location)
        {
            //Converting the string "event_date" to DateTime
            Convert.ToDateTime(event_date);

            //temp message
            string message = string.Empty;

            string role = role_valiation(organiser_id);
            if (role == "Organiser")
            {
                //connection string to connect
                string connection_string = @"Data source=(localdb)\MSSQLLocalDB;database=Race_day;";
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

                        query = @"INSERT INTO Events
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
            if (role == "Organiser")
            {
                //connection string to connect
                string connection_string = @"Data source=(localdb)\MSSQLLocalDB;database=Race_day;";

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
            if (role == "Organiser")
            {
                //connection string to connect
                string connection_string = @"Data source=(localdb)\MSSQLLocalDB;database=Race_day;";

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



        public string viewing_event_using_eventid(int event_id)
        {
            //temp message
            string message = string.Empty;

            //connection string to connect
            string connection_string = @"Data source=(localdb)\MSSQLLocalDB;database=Race_day;";


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
            //connection string to connect
            string connection_string = @"Data source=(localdb)\MSSQLLocalDB;database=Race_day;";
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
            return role;
        }


    }
}
