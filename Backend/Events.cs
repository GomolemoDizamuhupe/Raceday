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
