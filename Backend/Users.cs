using Microsoft.Data.SqlClient;
using Part2_Raceday.Backend;
using Part2_Raceday.Models;

namespace Part2_Raceday.Backend
{
    public class Users
    {

        private const string connection_string = @"Data source=(localdb)\MSSQLLocalDB;database=Race_day;";
        /*********************************************/


        public UserDto? get_user_dto(int user_id)
        {
            using (SqlConnection connect = new SqlConnection(connection_string))
            {
                connect.Open();

                using (SqlCommand cmd = new SqlCommand("SELECT UserId, Name, Surname, Email, Role FROM Users WHERE UserId = @UserId;", connect))
                {
                    cmd.Parameters.AddWithValue("@UserId", user_id);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read()) return null;

                        return new UserDto
                        {
                            UserId = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Surname = reader.GetString(2),
                            Email = reader.GetString(3),
                            Role = reader.GetString(4).ToLower()
                        };
                    }
                }
            }
        }

        // 1 = updated, 0 = user not found, -1 = email belongs to someone else
        public int update_profile(int user_id, UpdateProfileRequest req)
        {
            using (SqlConnection connect = new SqlConnection(connection_string))
            {
                connect.Open();

                using (SqlCommand taken = new SqlCommand("SELECT COUNT(*) FROM Users WHERE LOWER(Email) = @Email AND UserId <> @UserId;", connect))
                {
                    taken.Parameters.AddWithValue("@Email", req.Email.Trim().ToLower());
                    taken.Parameters.AddWithValue("@UserId", user_id);
                    if ((int)taken.ExecuteScalar() > 0) return -1;
                }

                string query = "UPDATE Users SET Name = @Name, Surname = @Surname, Email = @Email WHERE UserId = @UserId;";

                using (SqlCommand cmd = new SqlCommand(query, connect))
                {
                    cmd.Parameters.AddWithValue("@Name", req.Name.Trim());
                    cmd.Parameters.AddWithValue("@Surname", req.Surname.Trim());
                    cmd.Parameters.AddWithValue("@Email", req.Email.Trim());
                    cmd.Parameters.AddWithValue("@UserId", user_id);
                    return cmd.ExecuteNonQuery() > 0 ? 1 : 0;
                }
            }
        }

        /*********************************************/
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