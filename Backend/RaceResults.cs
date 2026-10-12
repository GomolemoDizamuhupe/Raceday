using Microsoft.AspNetCore.Mvc.Diagnostics;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Data;

namespace Part2_Raceday.Backend
{
    public class RaceResults
    {
        //Event object
        Events events = new Events();

        private const string connection_string = @"Data source=(localdb)\MSSQLLocalDB;database=Race_day;";

        public string uploading_entry(int EntryId, string FinishTime, int Position, int OrganiserId)
        {
            string message = string.Empty;


            //Converting the string "FinishTime" to DateTime using the format "HH:mm:ss"
            Convert.ToDateTime(FinishTime).ToString("HH:mm:ss");

            string role = events.role_valiation(OrganiserId);

            if (role == "organiser")
            {
                try
                {
                    using (SqlConnection connect = new SqlConnection(connection_string))
                    {
                        connect.Open();

                        string query = "INSERT INTO Results (EntryId, FinishTime, Position) " +
                                        "VALUES (@EntryId, @FinishTime, @Position);";

                        using (SqlCommand run_query = new SqlCommand(query, connect))
                        {
                            run_query.Parameters.AddWithValue("@OrganiserId", OrganiserId);
                            run_query.Parameters.AddWithValue("@EntryId", EntryId);
                            run_query.Parameters.AddWithValue("@FinishTime", FinishTime);
                            run_query.Parameters.AddWithValue("@Position", Position);

                            run_query.ExecuteNonQuery();
                            message = "Organiser Has captured the Results of a participant Successfully.";
                        }
                    }
                }
                catch (Exception error)
                {
                    message = error.Message;
                }
            }
            else
            {
                message = "User Has not Entered The Event Unsuccessfully because of they don't exist.";
            }
            return message;
        }




    }
}
