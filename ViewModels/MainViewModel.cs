using System;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EVMS.Data;
using EVMS.Models;

namespace EVMS.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly VoterDatabase _db = new();

        // ── Form fields ──────────────────────────────────────────────────────
        [ObservableProperty] private string voterCardId = "";
        [ObservableProperty] private string nationalIdNumber = "";
        [ObservableProperty] private string firstName = "";
        [ObservableProperty] private string middleName = "";
        [ObservableProperty] private string surname = "";
        [ObservableProperty] private string pollingStation = "";
        [ObservableProperty] private string dateOfBirth = "";   // dd-mm-yyyy
        [ObservableProperty] private string gender = "Male";

        // ── UI state ─────────────────────────────────────────────────────────
        [ObservableProperty] private string statusMessage = "Welcome to IIEC Electronic Voting Management System";
        [ObservableProperty] private bool isError = false;
        [ObservableProperty] private string searchCardId = "";
        [ObservableProperty] private string voterDetails = "";
        [ObservableProperty] private bool hasVoterDetails = false;
        [ObservableProperty] private Voter? selectedVoter;

        public ObservableCollection<Voter> Voters { get; } = new();
        public string[] GenderOptions { get; } = { "Male", "Female" };

        public MainViewModel() => LoadVoters();

        // ─────────────────────────────────────────────────────────────────────
        // ADD VOTER
        // ─────────────────────────────────────────────────────────────────────
        [RelayCommand]
        private void AddVoter()
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(VoterCardId) ||
                string.IsNullOrWhiteSpace(NationalIdNumber) ||
                string.IsNullOrWhiteSpace(FirstName) ||
                string.IsNullOrWhiteSpace(Surname) ||
                string.IsNullOrWhiteSpace(PollingStation) ||
                string.IsNullOrWhiteSpace(DateOfBirth))
            {
                SetStatus("Please fill in all required fields.", error: true);
                return;
            }

            // Validate date format dd-mm-yyyy
            if (!Regex.IsMatch(DateOfBirth, @"^\d{2}-\d{2}-\d{4}$"))
            {
                SetStatus("Date of Birth must be in dd-mm-yyyy format (e.g. 15-03-1990).", error: true);
                return;
            }

            var voter = new Voter
            {
                VoterCardId      = VoterCardId.Trim(),
                NationalIdNumber = NationalIdNumber.Trim(),
                FirstName        = FirstName.Trim(),
                MiddleName       = MiddleName.Trim(),
                Surname          = Surname.Trim(),
                PollingStation   = PollingStation.Trim(),
                DateOfBirth      = DateOfBirth.Trim(),
                Gender           = Gender
            };

            try
            {
                if (_db.AddVoter(voter))
                {
                    SetStatus($"Voter '{voter.FullName}' (Card ID: {voter.VoterCardId}) registered successfully.", error: false);
                    ClearForm();
                    LoadVoters();
                }
                else
                {
                    SetStatus("Failed to add voter. Please try again.", error: true);
                }
            }
            catch (Exception ex)
            {
                SetStatus($"Error: {ex.Message}", error: true);
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // DELETE VOTER
        // ─────────────────────────────────────────────────────────────────────
        [RelayCommand]
        private void DeleteVoter()
        {
            if (SelectedVoter == null)
            {
                SetStatus("Please select a voter from the list to delete.", error: true);
                return;
            }

            if (_db.DeleteVoter(SelectedVoter.VoterCardId))
            {
                SetStatus($"Voter '{SelectedVoter.FullName}' has been removed from the register.", error: false);
                VoterDetails = "";
                HasVoterDetails = false;
                LoadVoters();
            }
            else
            {
                SetStatus("Could not delete the voter. Record may not exist.", error: true);
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // SEARCH / DISPLAY VOTER
        // ─────────────────────────────────────────────────────────────────────
        [RelayCommand]
        private void SearchVoter()
        {
            if (string.IsNullOrWhiteSpace(SearchCardId))
            {
                SetStatus("Enter a Voter Card ID to search.", error: true);
                return;
            }

            var voter = _db.GetVoterByCardId(SearchCardId.Trim());
            if (voter != null)
            {
                VoterDetails = voter.ToString();
                HasVoterDetails = true;
                SetStatus("Voter record found.", error: false);
            }
            else
            {
                VoterDetails = "";
                HasVoterDetails = false;
                SetStatus($"No voter found with Card ID: {SearchCardId}", error: true);
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // HELPERS
        // ─────────────────────────────────────────────────────────────────────
        private void LoadVoters()
        {
            Voters.Clear();
            foreach (var v in _db.GetAllVoters())
                Voters.Add(v);
        }

        private void ClearForm()
        {
            VoterCardId = NationalIdNumber = FirstName = MiddleName =
            Surname = PollingStation = DateOfBirth = "";
            Gender = "Male";
        }

        private void SetStatus(string message, bool error)
        {
            StatusMessage = message;
            IsError = error;
        }
    }
}