using Microsoft.Data.SqlClient;
using Part2_Raceday.Backend;


namespace Part2_Raceday.Backend
{
    public class Users
    {

        private const string connection_string = @"Data source=(localdb)\MSSQLLocalDB;database=Race_day;";

        public string get_own_profile(int userId)
        {
            string message = string.Empty;

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

        public string update_own_profile(int userId, string name, string surname, string email)
        {
            string message = string.Empty;

            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();

                    bool email_correct = false;

                    if (email.ToLower().Trim().EndsWith("@gmail.com"))
                    {
                        email_correct = true;
                    }

                    if (email_correct)
                    {

                        string query = @"UPDATE Users
                                     SET Name = @name, Surname = @surname, Email = @email
                                     WHERE UserId = @UserId;";

                        using (SqlCommand run_query = new SqlCommand(query, connect))
                        {
                            run_query.Parameters.AddWithValue("@name", name);
                            run_query.Parameters.AddWithValue("@surname", surname);
                            run_query.Parameters.AddWithValue("@email", email.Trim());
                            run_query.Parameters.AddWithValue("@UserId", userId);

                            int rows_affected = run_query.ExecuteNonQuery();

                            if (rows_affected > 0)
                            {
                                message = "Profile updated successfully.";
                            }
                            else
                            {
                                message = "User not found.";
                            }
                        }
                    }
                    else
                    {
                        message = "Email is invalid.";
                    }

                    connect.Close();

                }
            }
            catch (Exception error)
            {
                message = error.Message;
            }

            return message;
        }

        public string get_user_by_id(int userId)
        {
            string message = string.Empty;

            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();

                    string query = @"SELECT Name, Surname, Role FROM Users WHERE UserId = @UserId;";

                    using (SqlCommand run_query = new SqlCommand(query, connect))
                    {
                        run_query.Parameters.AddWithValue("@UserId", userId);

                        using (SqlDataReader reader = run_query.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                message = "Name: " + reader.GetString(0) +
                                           ", Surname: " + reader.GetString(1) +
                                           ", Role: " + reader.GetString(2);
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