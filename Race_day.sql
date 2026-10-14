Create Database Race_day;

Use [Race_day];

CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    Name VARCHAR(100) NOT NULL,
    Surname VARCHAR(100) NOT NULL,
    Email VARCHAR(150) NOT NULL,
    PasswordHash VARCHAR(255) NOT NULL,
    Role VARCHAR(20) NOT NULL CHECK (Role IN ('organiser', 'participant'))
);

CREATE TABLE Events (
    EventId INT IDENTITY(1,1) PRIMARY KEY,
    OrganiserId INT NOT NULL,
    Name VARCHAR(150) NOT NULL,
    Description VARCHAR(255) NULL,
    EventDate DATE NOT NULL,
    Location VARCHAR(150) NOT NULL,
    FOREIGN KEY (OrganiserId) REFERENCES dbo.Users(UserId)
);

CREATE TABLE Categories (
    CategoryId INT IDENTITY(1,1) PRIMARY KEY,
    EventId INT NOT NULL,
    Name VARCHAR(50) NOT NULL,
    DistanceKm DECIMAL(5,2) NOT NULL,
    MaxParticipants INT NOT NULL DEFAULT 500,
    FOREIGN KEY (EventId) REFERENCES dbo.Events(EventId)
);

CREATE TABLE Routes (
    RouteId INT IDENTITY(1,1) PRIMARY KEY,
    CategoryId INT NOT NULL,
    StartPoint VARCHAR(150) NOT NULL,
    EndPoint VARCHAR(150) NOT NULL,
    ElevationGainM INT NULL,
    MapUrl VARCHAR(255) NULL,
    FOREIGN KEY (CategoryId) REFERENCES dbo.Categories(CategoryId)
);


CREATE TABLE Entries (
    EntryId INT IDENTITY(1,1) PRIMARY KEY,
    ParticipantId INT NOT NULL,
    CategoryId INT NOT NULL,
    EntryDate DATE NOT NULL,
    Status VARCHAR(20) NOT NULL DEFAULT 'Confirmed'
    CONSTRAINT CK_Entries_Status CHECK (Status IN ('Pending', 'Confirmed', 'Cancelled')),
    FOREIGN KEY (ParticipantId) REFERENCES Users(UserId),
    FOREIGN KEY (CategoryId) REFERENCES Categories(CategoryId)
);

CREATE TABLE Results (
    ResultId INT IDENTITY(1,1) PRIMARY KEY,
    EntryId INT NOT NULL UNIQUE,
    FinishTime TIME NOT NULL,
    Position INT NULL,
    FOREIGN KEY (EntryId) REFERENCES Entries(EntryId)
);


INSERT INTO Users (Name, Surname, Email, PasswordHash, Role) VALUES
('Thandiwe', 'Nkosi', 'thandiwe.nkosi@raceday.co.za', 'HASH_PLACEHOLDER_1', 'Organiser'),
('Johan', 'van der Merwe','johan.vdm@raceday.co.za', 'HASH_PLACEHOLDER_2', 'Organiser'),
('Lerato', 'Mokoena', 'lerato.mokoena@gmail.com', 'HASH_PLACEHOLDER_3', 'Participant'),
('David', 'Pillay', 'david.pillay@gmail.com', 'HASH_PLACEHOLDER_4', 'Participant');


-- 3 Events (minimum required), owned by the two Organisers
INSERT INTO Events (OrganiserId, Name, Description, EventDate, Location) VALUES
(1, 'Bay to Bay 10K', 'Scenic coastal road run along the Durban beachfront.', '2026-09-06', 'Durban, KwaZulu-Natal'),
(1, 'Joburg City Cycle Classic', 'Charity cycling event through the Johannesburg CBD and surrounds.', '2026-10-11', 'Johannesburg, Gauteng'),
(2, 'Winelands Half Marathon','Half marathon through the vineyards of the Cape Winelands.', '2026-08-23', 'Stellenbosch, Western Cape');


-- Categories for each Event (at least one per event)
INSERT INTO Categories (EventId, Name, DistanceKm, MaxParticipants) VALUES
(1, '5km Fun Run', 5.00,  1000),
(1, '10km Race', 10.00, 1500),
(2, '40km Road Ride',40.00, 800),
(2, '80km Road Ride',80.00, 400),
(3, '10km Race', 10.00, 1200),
(3, '21.1km Half Marathon', 21.10, 2000);

-- Routes (one per Category)
INSERT INTO Routes (CategoryId, StartPoint, EndPoint, ElevationGainM, MapUrl) VALUES
(1, 'North Beach, Durban', 'uShaka Marine World', 15, 'https://maps.example.com/route1'),
(2, 'North Beach, Durban', 'Blue Lagoon', 30, 'https://maps.example.com/route2'),
(3, 'Sandton City', 'Melrose Arch', 120, 'https://maps.example.com/route3'),
(4, 'Sandton City', 'Lonehill', 260, 'https://maps.example.com/route4'),
(5, 'Stellenbosch Town Square', 'Jonkershoek Valley', 90, 'https://maps.example.com/route5'),
(6, 'Stellenbosch Town Square', 'Delheim Wine Estate', 210, 'https://maps.example.com/route6');

-- Sample Entries (Participants entering Categories)
INSERT INTO Entries (ParticipantId, CategoryId, EntryDate, Status) VALUES
(3, 2, '2026-08-20', 'Confirmed'),  -- Lerato -> Bay to Bay 10km
(3, 5, '2026-07-13', 'Confirmed'),  -- Lerato -> Winelands 10km
(4, 6, '2026-08-04', 'Confirmed'),  -- David  -> Winelands Half Marathon
(4, 3, '2026-06-17', 'Pending');    -- David  -> Joburg 40km Ride

-- Sample Results (captured by the relevant event's Organiser)
INSERT INTO Results (EntryId, FinishTime, Position) VALUES
(1, '00:52:14', 34),
(2, '00:49:02', 12),
(3, '01:58:47', 87);