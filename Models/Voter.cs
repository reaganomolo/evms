using System;

namespace EVMS.Models
{
    /// <summary>
    /// Represents a registered voter in the IIEC Electronic Voting Management System
    /// </summary>
    public class Voter
    {
        // primary key from the database
        public int Id { get; set; }

        public string VoterCardId { get; set; } = "";
        public string NationalIdNumber { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string MiddleName { get; set; } = "";
        public string Surname { get; set; } = "";
        public string PollingStation { get; set; } = "";
        public string DateOfBirth { get; set; } = "";
        public string Gender { get; set; } = "";

        public string FullName => $"{FirstName} {MiddleName} {Surname}".Trim();

        public override string ToString()
        {
            return $"Card ID: {VoterCardId}\n" +
                   $"National ID: {NationalIdNumber}\n" +
                   $"Name: {FullName}\n" +
                   $"Polling Station: {PollingStation}\n" +
                   $"Date of Birth: {DateOfBirth}\n" +
                   $"Gender: {Gender}";
        }
    }
}