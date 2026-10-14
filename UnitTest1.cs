using Microsoft.Data.SqlClient;
using Part2_Raceday.Backend;
using System.Text.RegularExpressions;

namespace Test_Raceday
{
    // ---------------------------------------------------------------
    // Helper: creates fresh, unique test data so tests can be re-run
    // ---------------------------------------------------------------
    internal static class TestData
    {
        private const string Conn = @"Data source=(localdb)\MSSQLLocalDB;database=Race_day;";

        private static int GetId(string sql, object value)
        {
            using (SqlConnection connect = new SqlConnection(Conn))
            {
                connect.Open();
                using (SqlCommand cmd = new SqlCommand(sql, connect))
                {
                    cmd.Parameters.AddWithValue("@v", value);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public static string UniqueEmail()
        {
            return "test" + Guid.NewGuid().ToString("N") + "@gmail.com";
        }

        public static (int Id, string Email) CreateUser(string role)
        {
            string email = UniqueEmail();
            new Auth().user_auth_reg("Test", "User", email, "Test@123", role);

            int id = GetId("SELECT UserId FROM Users WHERE Email = @v;", email);
            return (id, email);
        }

        public static int CreateEvent(int organiserId)
        {
            string name = "Event" + Guid.NewGuid().ToString("N");
            new Events().uploading_event(organiserId, name, "Test event", "2026-12-01", "Johannesburg");

            return GetId("SELECT EventId FROM Events WHERE Name = @v;", name);
        }

        public static int CreateCategory(int eventId)
        {
            string name = "Cat" + Guid.NewGuid().ToString("N");
            new Categories().uploading_category(eventId, name, "10.5", 100);

            return GetId("SELECT CategoryId FROM Categories WHERE Name = @v;", name);
        }

        public static int CreateRoute(int categoryId)
        {
            new Routes().uploading_route(categoryId, "Start", "End", 120, "https://maps.test/route");

            return GetId("SELECT TOP 1 RouteId FROM Routes WHERE CategoryId = @v ORDER BY RouteId DESC;", categoryId);
        }

        private static string? _status;

        // Finds a Status value the Entries table accepts (CHECK constraint, existing rows, then common guesses)
        public static string ValidStatus()
        {
            if (_status != null) return _status;

            List<string> candidates = new List<string>();

            using (SqlConnection connect = new SqlConnection(Conn))
            {
                connect.Open();

                using (SqlCommand cmd = new SqlCommand(
                    "SELECT definition FROM sys.check_constraints WHERE parent_object_id = OBJECT_ID('Entries');", connect))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        foreach (Match m in Regex.Matches(reader.GetString(0), "'([^']+)'"))
                            candidates.Add(m.Groups[1].Value);
                    }
                }

                using (SqlCommand cmd = new SqlCommand("SELECT DISTINCT Status FROM Entries;", connect))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (!reader.IsDBNull(0)) candidates.Add(reader.GetString(0));
                    }
                }
            }

            candidates.AddRange(new[] { "Registered", "Confirmed", "Pending", "Entered", "Active", "Paid" });

            _status = candidates[0];
            return _status;
        }

        public static int CreateEntry(int participantId, int categoryId)
        {
            new Entries().uploading_entry(participantId, categoryId.ToString(), "2026-11-01", ValidStatus());

            return GetId("SELECT TOP 1 EntryId FROM Entries WHERE ParticipantId = @v ORDER BY EntryId DESC;", participantId);
        }

        public static int CreateResult(int entryId, int organiserId)
        {
            new RaceResults().uploading_results(entryId, "01:30:00", 1, organiserId);

            return GetId("SELECT TOP 1 ResultId FROM Results WHERE EntryId = @v ORDER BY ResultId DESC;", entryId);
        }
    }

    // ---------------------------------------------------------------
    // Auth
    // ---------------------------------------------------------------
    public class AuthTests
    {
        private readonly Auth auth = new Auth();

        [Fact]
        public void Registration_NewUser_Succeeds()
        {
            string result = auth.user_auth_reg("Gomolemo", "Tom", TestData.UniqueEmail(), "gomo@123", "organiser");

            Assert.Equal("user registered successfully...", result);
        }

        [Fact]
        public void Registration_DuplicateEmail_Fails()
        {
            string email = TestData.UniqueEmail();
            auth.user_auth_reg("A", "B", email, "pass123", "participant");

            string result = auth.user_auth_reg("A", "B", email, "pass123", "participant");

            Assert.Equal("Email already registered", result);
        }

        [Fact]
        public void Registration_InvalidRole_Fails()
        {
            string result = auth.user_auth_reg("A", "B", TestData.UniqueEmail(), "pass123", "admin");

            Assert.Equal("Role is invalid or Email is invalid.", result);
        }

        [Fact]
        public void Registration_NonGmailEmail_Fails()
        {
            string result = auth.user_auth_reg("A", "B", "someone@yahoo.com", "pass123", "participant");

            Assert.Equal("Role is invalid or Email is invalid.", result);
        }

        [Fact]
        public void Login_CorrectDetails_Succeeds()
        {
            string email = TestData.UniqueEmail();
            auth.user_auth_reg("A", "B", email, "pass123", "participant");

            string result = auth.user_auth_login(email, "pass123");

            Assert.Equal("Welcome user", result);
        }

        [Fact]
        public void Login_WrongPassword_Fails()
        {
            string email = TestData.UniqueEmail();
            auth.user_auth_reg("A", "B", email, "pass123", "participant");

            string result = auth.user_auth_login(email, "wrongpass");

            Assert.Equal("Invalid email or password.", result);
        }

        [Fact]
        public void Login_UnknownEmail_Fails()
        {
            string result = auth.user_auth_login(TestData.UniqueEmail(), "pass123");

            Assert.Equal("Invalid email or password.", result);
        }

        [Fact]
        public void EmailChecker_ContainsRegisteredEmail()
        {
            string email = TestData.UniqueEmail();
            auth.user_auth_reg("A", "B", email, "pass123", "participant");

            List<string> emails = auth.email_checker();

            Assert.Contains(email, emails);
        }
    }

    // ---------------------------------------------------------------
    // Users
    // ---------------------------------------------------------------
    public class UsersTests
    {
        private readonly Users users = new Users();

        [Fact]
        public void GetOwnProfile_ReturnsDetails()
        {
            var user = TestData.CreateUser("participant");

            string result = users.get_own_profile(user.Id);

            Assert.Contains(user.Email, result);
            Assert.Contains("participant", result);
        }

        [Fact]
        public void GetOwnProfile_UnknownUser_ReturnsNotFound()
        {
            string result = users.get_own_profile(int.MaxValue);

            Assert.Equal("User not found.", result);
        }

        [Fact]
        public void UpdateOwnProfile_ValidEmail_Succeeds()
        {
            var user = TestData.CreateUser("participant");

            string result = users.update_own_profile(user.Id, "New", "Name", TestData.UniqueEmail());

            Assert.Equal("Profile updated successfully.", result);
        }

        [Fact]
        public void UpdateOwnProfile_InvalidEmail_Fails()
        {
            var user = TestData.CreateUser("participant");

            string result = users.update_own_profile(user.Id, "New", "Name", "bad@yahoo.com");

            Assert.Equal("Email is invalid.", result);
        }

        [Fact]
        public void UpdateOwnProfile_UnknownUser_ReturnsNotFound()
        {
            string result = users.update_own_profile(int.MaxValue, "New", "Name", TestData.UniqueEmail());

            Assert.Equal("User not found.", result);
        }

        [Fact]
        public void GetUserById_ReturnsNameSurnameRole()
        {
            var user = TestData.CreateUser("organiser");

            string result = users.get_user_by_id(user.Id);

            Assert.Equal("Name: Test, Surname: User, Role: organiser", result);
        }

        [Fact]
        public void GetUserById_UnknownUser_ReturnsNotFound()
        {
            string result = users.get_user_by_id(int.MaxValue);

            Assert.Equal("User not found.", result);
        }
    }

    // ---------------------------------------------------------------
    // Events
    // ---------------------------------------------------------------
    public class EventsTests
    {
        private readonly Events events = new Events();

        [Fact]
        public void RoleValidation_Organiser_ReturnsOrganiser()
        {
            var organiser = TestData.CreateUser("organiser");

            Assert.Equal("organiser", events.role_valiation(organiser.Id));
        }

        [Fact]
        public void RoleValidation_Participant_ReturnsParticipant()
        {
            var participant = TestData.CreateUser("participant");

            Assert.Equal("participant", events.role_valiation(participant.Id));
        }

        [Fact]
        public void RoleValidation_UnknownUser_ReturnsEmpty()
        {
            Assert.Equal(string.Empty, events.role_valiation(int.MaxValue));
        }

        [Fact]
        public void UploadingEvent_Organiser_Succeeds()
        {
            var organiser = TestData.CreateUser("organiser");

            string result = events.uploading_event(organiser.Id, "Test Race", "Fun run", "2026-12-01", "Sandton");

            Assert.Equal("Event Created Successfully.", result);
        }

        [Fact]
        public void UploadingEvent_Participant_Fails()
        {
            var participant = TestData.CreateUser("participant");

            string result = events.uploading_event(participant.Id, "Test Race", "Fun run", "2026-12-01", "Sandton");

            Assert.Equal("Event can not be added because of your role. You are not a organiser.", result);
        }

        [Fact]
        public void EditingEvent_Organiser_Succeeds()
        {
            var organiser = TestData.CreateUser("organiser");
            int eventId = TestData.CreateEvent(organiser.Id);

            string result = events.editing_event(eventId, organiser.Id, "Edited", "Edited desc", "2026-12-15", "Pretoria");

            Assert.Equal("Event Updated Successfully.", result);
        }

        [Fact]
        public void EditingEvent_Participant_Fails()
        {
            var participant = TestData.CreateUser("participant");

            string result = events.editing_event(1, participant.Id, "Edited", "Edited desc", "2026-12-15", "Pretoria");

            Assert.Equal("Event can not be edited because of your role. You are not a organiser.", result);
        }

        [Fact]
        public void DeletingEvent_Organiser_Succeeds()
        {
            var organiser = TestData.CreateUser("organiser");
            int eventId = TestData.CreateEvent(organiser.Id);

            string result = events.deleting_event(eventId, organiser.Id);

            Assert.Equal("Event Deleted Successfully.", result);
            Assert.Equal("Event not found.", events.viewing_event_using_eventid(eventId));
        }

        [Fact]
        public void ViewingEvent_ReturnsEvents()
        {
            var organiser = TestData.CreateUser("organiser");
            TestData.CreateEvent(organiser.Id);

            List<string> result = events.viewing_event();

            Assert.NotEmpty(result);
            Assert.Contains("Test event", string.Join(" ", result));
        }

        [Fact]
        public void ViewingEventById_ReturnsEvent()
        {
            var organiser = TestData.CreateUser("organiser");
            int eventId = TestData.CreateEvent(organiser.Id);

            string result = events.viewing_event_using_eventid(eventId);

            Assert.Contains("Location: Johannesburg", result);
        }

        [Fact]
        public void ViewingEventById_UnknownId_ReturnsNotFound()
        {
            Assert.Equal("Event not found.", events.viewing_event_using_eventid(int.MaxValue));
        }
    }

    // ---------------------------------------------------------------
    // Categories
    // ---------------------------------------------------------------
    public class CategoriesTests
    {
        private readonly Categories categories = new Categories();

        [Fact]
        public void UploadingCategory_Valid_Succeeds()
        {
            var organiser = TestData.CreateUser("organiser");
            int eventId = TestData.CreateEvent(organiser.Id);

            string result = categories.uploading_category(eventId, "10km", "10.5", 200);

            Assert.Equal("Category Created Successfully.", result);
        }

        [Fact]
        public void UploadingCategory_InvalidDistance_Fails()
        {
            string result = categories.uploading_category(1, "10km", "abc", 200);

            Assert.Equal("Invalid distance value.", result);
        }

        [Fact]
        public void GetEventCategory_ReturnsCategories()
        {
            var organiser = TestData.CreateUser("organiser");
            int eventId = TestData.CreateEvent(organiser.Id);
            TestData.CreateCategory(eventId);

            List<string> result = categories.get_event_category(eventId);

            Assert.StartsWith("Event category: ", result[0]);
        }

        [Fact]
        public void GetEventCategory_NoCategories_ReturnsNotFound()
        {
            List<string> result = categories.get_event_category(int.MaxValue);

            Assert.Equal("Event category not found.", result[0]);
        }

        [Fact]
        public void EditingCategory_Owner_Succeeds()
        {
            var organiser = TestData.CreateUser("organiser");
            int eventId = TestData.CreateEvent(organiser.Id);
            int categoryId = TestData.CreateCategory(eventId);

            string result = categories.editing_category(categoryId, organiser.Id, "21km", "21.1", 300);

            Assert.Equal("Category Updated Successfully.", result);
        }

        [Fact]
        public void EditingCategory_OtherOrganiser_Fails()
        {
            var owner = TestData.CreateUser("organiser");
            var other = TestData.CreateUser("organiser");
            int categoryId = TestData.CreateCategory(TestData.CreateEvent(owner.Id));

            string result = categories.editing_category(categoryId, other.Id, "21km", "21.1", 300);

            Assert.Equal("No matching category found for this organiser.", result);
        }

        [Fact]
        public void EditingCategory_InvalidDistance_Fails()
        {
            string result = categories.editing_category(1, 1, "21km", "abc", 300);

            Assert.Equal("Invalid distance value.", result);
        }

        [Fact]
        public void DeletingCategory_Owner_Succeeds()
        {
            var organiser = TestData.CreateUser("organiser");
            int categoryId = TestData.CreateCategory(TestData.CreateEvent(organiser.Id));

            string result = categories.deleting_category(categoryId, organiser.Id);

            Assert.Equal("Category Deleted Successfully.", result);
        }

        [Fact]
        public void DeletingCategory_Participant_Fails()
        {
            var participant = TestData.CreateUser("participant");

            string result = categories.deleting_category(1, participant.Id);

            Assert.Equal("Category can not be deleted because of your role. You are not a organiser.", result);
        }

        [Fact]
        public void GetCategoryRoute_ReturnsDetails()
        {
            var organiser = TestData.CreateUser("organiser");
            int categoryId = TestData.CreateCategory(TestData.CreateEvent(organiser.Id));
            TestData.CreateRoute(categoryId);

            string result = categories.get_category_route(categoryId);

            Assert.Contains("10.50km", result.Replace("10.5000", "10.50"));
            Assert.Contains("Start -> End", result);
        }

        [Fact]
        public void GetCategoryRoute_UnknownCategory_ReturnsNotFound()
        {
            Assert.Equal("No category found with that id.", categories.get_category_route(int.MaxValue));
        }
    }

    // ---------------------------------------------------------------
    // Routes
    // ---------------------------------------------------------------
    public class RoutesTests
    {
        private readonly Routes routes = new Routes();

        [Fact]
        public void UploadingRoute_Valid_Succeeds()
        {
            var organiser = TestData.CreateUser("organiser");
            int categoryId = TestData.CreateCategory(TestData.CreateEvent(organiser.Id));

            string result = routes.uploading_route(categoryId, "Start", "End", 100, "https://maps.test/route");

            Assert.Equal("Route Created Successfully.", result);
        }

        [Fact]
        public void GetRouteForCategory_ReturnsRoute()
        {
            var organiser = TestData.CreateUser("organiser");
            int categoryId = TestData.CreateCategory(TestData.CreateEvent(organiser.Id));
            TestData.CreateRoute(categoryId);

            string result = routes.get_route_for_category(categoryId);

            Assert.Contains("Start Point: Start", result);
            Assert.Contains("Elevation: 120m", result);
        }

        [Fact]
        public void GetRouteForCategory_NoRoute_ReturnsNotFound()
        {
            Assert.Equal("No category found with that ID.", routes.get_route_for_category(int.MaxValue));
        }

        [Fact]
        public void EditingRoute_Owner_Succeeds()
        {
            var organiser = TestData.CreateUser("organiser");
            int categoryId = TestData.CreateCategory(TestData.CreateEvent(organiser.Id));
            int routeId = TestData.CreateRoute(categoryId);

            string result = routes.editing_route(organiser.Id, routeId, categoryId, "New Start", "New End", 200, "https://maps.test/new");

            Assert.Equal("Route Updated Successfully.", result);
        }

        [Fact]
        public void EditingRoute_Participant_Fails()
        {
            var participant = TestData.CreateUser("participant");

            string result = routes.editing_route(participant.Id, 1, 1, "S", "E", 100, "url");

            Assert.Equal("Route can not be edited because of your role. You are not a organiser.", result);
        }
    }

    // ---------------------------------------------------------------
    // Entries
    // ---------------------------------------------------------------
    public class EntriesTests
    {
        private readonly Entries entries = new Entries();

        [Fact]
        public void UploadingEntry_Participant_Succeeds()
        {
            var organiser = TestData.CreateUser("organiser");
            var participant = TestData.CreateUser("participant");
            int categoryId = TestData.CreateCategory(TestData.CreateEvent(organiser.Id));

            string result = entries.uploading_entry(participant.Id, categoryId.ToString(), "2026-11-01", TestData.ValidStatus());

            Assert.Equal("Participant Entered Into The Event Successfully.", result);
        }

        [Fact]
        public void UploadingEntry_Organiser_Fails()
        {
            var organiser = TestData.CreateUser("organiser");

            string result = entries.uploading_entry(organiser.Id, "1", "2026-11-01", "Registered");

            Assert.Equal("User is not Entered Into The Event Unsuccessfully because of they don't exist.", result);
        }

        [Fact]
        public void GetParticipantEntries_ReturnsEntries()
        {
            var organiser = TestData.CreateUser("organiser");
            var participant = TestData.CreateUser("participant");
            int categoryId = TestData.CreateCategory(TestData.CreateEvent(organiser.Id));
            TestData.CreateEntry(participant.Id, categoryId);

            List<string> result = entries.get_participant_event_entries(participant.Id);

            Assert.Contains("EntryDate: 2026-11-01", result[0]);
        }

        [Fact]
        public void GetParticipantEntries_NoEntries_ReturnsMessage()
        {
            var participant = TestData.CreateUser("participant");

            List<string> result = entries.get_participant_event_entries(participant.Id);

            Assert.Equal("There is Events Entries no found.", result[0]);
        }

        [Fact]
        public void GetParticipantEntries_Organiser_ReturnsUserNotFound()
        {
            var organiser = TestData.CreateUser("organiser");

            List<string> result = entries.get_participant_event_entries(organiser.Id);

            Assert.Equal("User not found", result[0]);
        }

        [Fact]
        public void DisplayingOrganiserEntries_ReturnsEntries()
        {
            var organiser = TestData.CreateUser("organiser");
            var participant = TestData.CreateUser("participant");
            int eventId = TestData.CreateEvent(organiser.Id);
            int categoryId = TestData.CreateCategory(eventId);
            TestData.CreateEntry(participant.Id, categoryId);

            List<string> result = entries.displaying_organiser_event_entries(organiser.Id, eventId);

            Assert.Contains("ParticipantId: " + participant.Id, result[0]);
        }

        [Fact]
        public void DisplayingOrganiserEntries_Participant_Fails()
        {
            var participant = TestData.CreateUser("participant");

            List<string> result = entries.displaying_organiser_event_entries(participant.Id, 1);

            Assert.Equal("You are not a organiser.", result[0]);
        }

        [Fact]
        public void DeletingEntry_Participant_Succeeds()
        {
            var organiser = TestData.CreateUser("organiser");
            var participant = TestData.CreateUser("participant");
            int categoryId = TestData.CreateCategory(TestData.CreateEvent(organiser.Id));
            int entryId = TestData.CreateEntry(participant.Id, categoryId);

            string result = entries.deleting_entry(entryId, participant.Id);

            Assert.Equal("Participant Withdraws From A Event Successfully.", result);
        }

        [Fact]
        public void DeletingEntry_Organiser_Fails()
        {
            var organiser = TestData.CreateUser("organiser");

            string result = entries.deleting_entry(1, organiser.Id);

            Assert.Equal("User can not Withdraw From the Event because of they don't exist in the event.", result);
        }
    }

    // ---------------------------------------------------------------
    // RaceResults
    // ---------------------------------------------------------------
    public class RaceResultsTests
    {
        private readonly RaceResults results = new RaceResults();

        // Creates organiser, participant, event, category and entry
        private (int OrganiserId, int ParticipantId, int EventId, int EntryId) Setup()
        {
            var organiser = TestData.CreateUser("organiser");
            var participant = TestData.CreateUser("participant");
            int eventId = TestData.CreateEvent(organiser.Id);
            int categoryId = TestData.CreateCategory(eventId);
            int entryId = TestData.CreateEntry(participant.Id, categoryId);

            return (organiser.Id, participant.Id, eventId, entryId);
        }

        [Fact]
        public void UploadingResults_Organiser_Succeeds()
        {
            var data = Setup();

            string result = results.uploading_results(data.EntryId, "01:30:00", 1, data.OrganiserId);

            Assert.Equal("Organiser Has captured the Results of a participant Successfully.", result);
        }

        [Fact]
        public void UploadingResults_InvalidTime_Fails()
        {
            var data = Setup();

            string result = results.uploading_results(data.EntryId, "abc", 1, data.OrganiserId);

            Assert.Equal("Invalid finish time format. Use HH:mm:ss.", result);
        }

        [Fact]
        public void UploadingResults_Participant_Fails()
        {
            var data = Setup();

            string result = results.uploading_results(data.EntryId, "01:30:00", 1, data.ParticipantId);

            Assert.Equal("User Has not Entered The Event Unsuccessfully because of they don't exist.", result);
        }

        [Fact]
        public void UpdatingResults_Organiser_Succeeds()
        {
            var data = Setup();
            int resultId = TestData.CreateResult(data.EntryId, data.OrganiserId);

            string result = results.updating_results(resultId, "01:45:00", 2, data.OrganiserId);

            Assert.Equal("Organiser Has updated the Results of a participant Successfully.", result);
        }

        [Fact]
        public void UpdatingResults_InvalidTime_Fails()
        {
            var data = Setup();

            string result = results.updating_results(1, "abc", 2, data.OrganiserId);

            Assert.Equal("Invalid finish time format. Use HH:mm:ss.", result);
        }

        [Fact]
        public void GetParticipantResults_ReturnsResults()
        {
            var data = Setup();
            TestData.CreateResult(data.EntryId, data.OrganiserId);

            List<string> result = results.get_participant_event_results(data.ParticipantId);

            Assert.Contains("Finish Time: 01:30:00", result[0]);
            Assert.Contains("Position: 1", result[0]);
        }

        [Fact]
        public void GetParticipantResults_NoResults_ReturnsMessage()
        {
            var participant = TestData.CreateUser("participant");

            List<string> result = results.get_participant_event_results(participant.Id);

            Assert.Equal("There are no results for this user found.", result[0]);
        }

        [Fact]
        public void GetEventResults_Organiser_ReturnsResults()
        {
            var data = Setup();
            TestData.CreateResult(data.EntryId, data.OrganiserId);

            List<string> result = results.get_event_results(data.EventId, data.OrganiserId);

            Assert.Contains("Finish Time: 01:30:00", result[0]);
        }

        [Fact]
        public void GetEventResults_Participant_ReturnsUserNotFound()
        {
            var data = Setup();

            List<string> result = results.get_event_results(data.EventId, data.ParticipantId);

            Assert.Equal("User not found", result[0]);
        }
    }
}