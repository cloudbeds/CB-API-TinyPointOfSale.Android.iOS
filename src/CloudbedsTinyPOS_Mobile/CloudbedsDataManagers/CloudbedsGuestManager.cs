using System;
using System.Text;
using System.Collections.Generic;

/// <summary>
/// Manages retreiving lists of Guests from Cloudbeds
/// </summary>
class CloudbedsGuestManager
{
    private readonly ICloudbedsServerInfo _cbServerInfo;
    private readonly ICloudbedsAuthSessionId _authSession;
    private readonly TaskStatusLogs _statusLog;

    private class CachedData
    {
        public readonly ICollection<CloudbedsGuest> Guests;
        public readonly DateTime LastUpdated;

        public CachedData(ICollection<CloudbedsGuest> guests, DateTime updated)
        {
            this.Guests = guests;
            this.LastUpdated = updated;
        }
    }

    CachedData _cachedData;
    public ICollection<CloudbedsGuest>? Guests
    {
        get 
        { 
            if(_cachedData == null)
            {
                return null;
            }
            return _cachedData.Guests; 
        }
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="cbServerInfo"></param>
    /// <param name="authSession"></param>
    /// <param name="statusLog"></param>
    public CloudbedsGuestManager(
        ICloudbedsServerInfo cbServerInfo,
        ICloudbedsAuthSessionId authSession,
        TaskStatusLogs statusLog)
    {
        _cbServerInfo = cbServerInfo;
        _authSession = authSession;
        _statusLog = statusLog;      
    }

    /// <summary>
    /// Queries for guests, if needed
    /// </summary>
    /// <exception cref="Exception"></exception>
    public void EnsureCachedData()
    {
        //Success
        if(_cachedData != null)
        {
            return;
        }

        ForceRefreshOfCachedData();
    }

    /// <summary>
    /// Get the latest data in the cache
    /// </summary>
    /// <exception cref="Exception"></exception>
    public void ForceRefreshOfCachedData()
    {
        ICollection<CloudbedsGuest> queriedGuests = null;

        var queryTime = DateTime.Now;

        if (AppSettings.UseSimulatedGuestData)
        {
            //Create simulated data
            queriedGuests = Testing_CreateSimulatedGuestData();
        }
        else //Query the real data...
        {
            var cbQueryGuests = new CloudbedsRequestCurrentGuests(_cbServerInfo, _authSession, _statusLog);
            var querySuccess = cbQueryGuests.ExecuteRequest();
            if (!querySuccess)
            {
                throw new Exception("1021-825: CloudbedsGuestManager, query failure");
            }

            queriedGuests = cbQueryGuests.CommandResults_Guests;
            IwsDiagnostics.Assert(queriedGuests != null, "1021-826: Expected query results");
        }

        //Store the cached results
        _cachedData = new CachedData(queriedGuests, queryTime);

    }


    /// <summary>
    /// Create test data
    /// </summary>
    /// <returns></returns>
    private static ICollection<CloudbedsGuest> Testing_CreateSimulatedGuestData()
    {
        DateTime reservationDate = DateTime.Today.AddDays(-4);
        DateTime reservationDateEnd = DateTime.Today.AddDays(14);

        var testData = new List<CloudbedsGuest>();
        testData.Add(Testing_CreateSimulatedGuest("Dopy Sevendwarves", "dopy.sevendwarves@cloudbeds.com", "111.123.1111", "Rm 201"));
        testData.Add(Testing_CreateSimulatedGuest("Bashful Sevendwarves", "bashful.sevendwarves@cloudbeds.com", "111.123.1112", "Rm 202"));
        testData.Add(Testing_CreateSimulatedGuest("Sleepy Sevendwarves", "bashful.sevendwarves@cloudbeds.com", "111.123.1112", "Rm 203"));
        testData.Add(Testing_CreateSimulatedGuest("Sneezy Sevendwarves", "sneezy.sevendwarves@cloudbeds.com", "111.123.1113", "Rm 204"));
        testData.Add(Testing_CreateSimulatedGuest("Grumpy Sevendwarves", "grumpy.sevendwarves@cloudbeds.com", "111.123.1114", "Rm 205"));
        testData.Add(Testing_CreateSimulatedGuest("Doc Sevendwarves", "doc.sevendwarves@cloudbeds.com", "111.123.1115", "Rm 206"));
        testData.Add(Testing_CreateSimulatedGuest("Sneezy Sevendwarves", "sneezy.sevendwarves@cloudbeds.com", "111.123.1116", "Rm 212"));
        testData.Add(Testing_CreateSimulatedGuest("Happy Sevendwarves", "happy.sevendwarves@cloudbeds.com", "111.123.1117", "Rm 214"));
        testData.Add(Testing_CreateSimulatedGuest("Snow White", "snow.white@cloudbeds.com", "111.123.1130", "Rm 107"));
        testData.Add(Testing_CreateSimulatedGuest("Evil Queen", "evil.queen@cloudbeds.com", "111.123.1140", "Rm 321"));

        return testData;
    }


    /// <summary>
    /// Create a similated guest
    /// </summary>
    /// <param name="name"></param>
    /// <param name="email"></param>
    /// <param name="phone"></param>
    /// <param name="roomName"></param>
    /// <returns></returns>
    private static CloudbedsGuest Testing_CreateSimulatedGuest(string name, string email, string phone, string roomName)
    {
        var random = new Random();

        DateTime reservationDate = DateTime.Today.AddDays(-2 - random.Next(8));
        DateTime reservationDateEnd = DateTime.Today.AddDays(14 + random.Next(32));
        Guid fakeId1 = Guid.NewGuid();
        Guid fakeId2 = Guid.NewGuid();
        Guid fakeId3 = Guid.NewGuid();

        return new CloudbedsGuest(
                "fake_guestID_" + fakeId1.ToString(),
                name,
                email,
                phone,
                "fake_reservationID_" + fakeId2,
                reservationDate.ToShortDateString(),
                reservationDateEnd.ToShortDateString(),
                "fake_roomID_" + fakeId3,
                roomName);

    }

    /// <summary>
}
