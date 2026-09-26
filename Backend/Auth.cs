using Microsoft.Data.SqlClient;
using System.Data;

namespace Part2_Raceday.Backend
{
    public class Auth
    {

        public string user_auth_reg(string name, string surname, string email, string password, string role)
        {
            //Variable declearation
            string message = string.Empty;

            //Path to the MSSQL Database
            string connection_string = @"Data source=(localdb)\Race_Day_DB;database=RACEDAY_DB;";

            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    //Opens connection
                    connect.Open();

                    bool email_exists = false;

                    List<string> existing_emails = email_checker();

                    //chacks if the email exists inthe database and returns true or false
                    foreach (string existing_email in existing_emails)
                    {
                        if (email.ToLower().Trim() == existing_email.ToLower().Trim())
                        {
                            email_exists = true;
                            break;
                        }
                    }

                    if (email_exists)
                    {
                        message = "Email already registered";
                    }
                    else
                    {
                        //hashes the password
                        string password_hash = BCrypt.Net.BCrypt.HashPassword(password);

                        bool role_correct = false;
                        bool email_correct = false;

                        //Checks the roles
                        if (role.ToLower().Contains("organiser") || role.ToLower().Contains("participant"))
                        {
                            role_correct = true;
                        }

                        //checks if the email ends with "@gmail.com"
                        if (email.ToLower().Trim().EndsWith("@gmail.com"))
                        {
                            email_correct = true;
                        }

                        //If both conditions is true it will run the query
                        if (role_correct && email_correct)
                        {
                            //SQL query
                            string query = @"INSERT INTO Users (Name, Surname, Email, PasswordHash, Role)
                                             VALUES (@name, @surname, @email, @password, @role);";

                            using (SqlCommand run_query = new SqlCommand(query, connect))
                            {
                                run_query.Parameters.AddWithValue("@name", name);
                                run_query.Parameters.AddWithValue("@surname", surname);
                                run_query.Parameters.AddWithValue("@email", email.Trim());
                                run_query.Parameters.AddWithValue("@password", password_hash);
                                run_query.Parameters.AddWithValue("@role", role);
                                run_query.ExecuteNonQuery();
                            }

                            message = "user registered successfully...";
                        }
                        else
                        {
                            message = "Role is invalid or Email is invalid.";
                        }
                    }
                    //Close connection
                    connect.Close();
                }
            }
            catch (Exception error)
            {
                message = error.Message;
            }

            return message;
        }



        //It runs a "SELECT *" for the emails
        public List<string> email_checker()
        {
            List<string> emails = new List<string>();

            //Path to the MSSQL Database
            string connection_string = @"Data source=(localdb)\Race_Day_DB;database=RACEDAY_DB;";

            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    //Opens connection
                    connect.Open();

                    //SQL query
                    string query = @"SELECT Email FROM Users;";

                    using (SqlCommand run_query = new SqlCommand(query, connect))
                    using (SqlDataReader reader = run_query.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (!reader.IsDBNull(0))
                                emails.Add(reader.GetString(0));
                        }
                    }
                }
            }
            catch (Exception error)
            {
                //Returns an error
                emails.Add("Error: " + error.Message);
            }

            //returns all the emails
            return emails;
        }

    }
}
