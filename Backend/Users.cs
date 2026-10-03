using Microsoft.Data.SqlClient;
using Part2_Raceday.Backend;


namespace Part2_Raceday.Backend
{
    public class Users
    {
        Auth auth = new Auth();


        public string get_own_profile(int userId)
        {
            string message = string.Empty;

            string connection_string = @"Data source=(localdb)\MSSQLLocalDB;database=Race_day;";

            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();

                    string query = @"SELECT Name, Surname, Email, Role FROM Users WHERE UserId = @UserId;";

                    using (SqlCommand run_query = new SqlCommand(query, connect))
                    {
                        run_query.Parameters.AddWithValue("@UserId", userId);

                        using (SqlDataReader reader = run_query.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                message = "Name: " + reader.GetString(0) +
                                           ", Surname: " + reader.GetString(1) +
                                           ", Email: " + reader.GetString(2) +
                                           ", Role: " + reader.GetString(3);
                            }
                            else
                            {
                                message = "User not found.";
                            }
                        }
                    }
                }
            }
            catch (Exception error)
            {
                message = error.Message;
            }

            return message;
        }


    }
}