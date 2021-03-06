﻿// This file has been autogenerated from a class added in the UI designer.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppKit;
using Foundation;
using MyImdbLibrary;

namespace ImdbMacApp
{
    public partial class ViewRecommendations : NSViewController
	{
        public DatabaseManager DbManager { get; set; }
        public User MainUser { get; set; }

        public ApiModelOmdb CurrentOmdbMovie { get; set; }
        public List<ApiModelOmdb> TwentyOmdbMovies { get; set; }
        public List<ApiModelOmdb> FifteenOmdbMovies { get; set; }

        private Dictionary<string,string> YouTubeUrls { get; set; }

        private string UserTenMostRatedMoviesByImdbNames { get; set; }
        private List<ApiModelTaste> TwentyTasteRecommendedMovies { get; set; }

        private int NbOfResults { get; set; }
        private double imdbRating = 0;
        private int imdbVotes = 0;

        public ViewController MainView { get; set; }

        public ViewRecommendations (IntPtr handle) : base (handle) { }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            ClearForm();
        }

        public override async void ViewDidAppear()
        {
            TwentyOmdbMovies = new List<ApiModelOmdb>();
            FifteenOmdbMovies = new List<ApiModelOmdb>();
            TwentyTasteRecommendedMovies = new List<ApiModelTaste>();
            YouTubeUrls = new Dictionary<string, string>();

            await LaunchRecommendationProcess();
        }

        private async Task LaunchRecommendationProcess()
        {
            UserTenMostRatedMoviesByImdbNames = DbManager.GetUserTenMostRatedMoviesByImdbNames(MainUser.Id);

            await GetMovieRecommendations();

            await CompleteMovieRecommendationsFromOmdb();

            FillYouTubeUrlsDictionary();

            GetBestFifteenMoviesOutOfTwenty();

            LoadNextMovie();
        }

        private async Task GetMovieRecommendations()
        {
            TwentyTasteRecommendedMovies = await ApiProcessorTaste.LoadTasteRecommendations(UserTenMostRatedMoviesByImdbNames);
        }

        private async Task CompleteMovieRecommendationsFromOmdb()
        {
            foreach (ApiModelTaste m in TwentyTasteRecommendedMovies)
            {
                ApiModelOmdb OmdbMovie = await ApiProcessorOmdb.LoadOmdbObjectAsync(m.Title, "", MainUser.OmdbApiKey);

                if (OmdbMovie != null) TwentyOmdbMovies.Add(OmdbMovie);
            }
        }

        private void FillYouTubeUrlsDictionary()
        {
            foreach (ApiModelOmdb m in TwentyOmdbMovies)
            {
                ApiModelTaste t = TwentyTasteRecommendedMovies.Find(o => o.Title == m.Title);

                if (t != null)
                {
                    if (!string.IsNullOrEmpty(t.YouTubeUrl))
                    {
                        YouTubeUrls.Add(m.imdbID, t.YouTubeUrl);
                    }
                    else
                    {
                        YouTubeUrls.Add(m.imdbID, "");
                    }
                }
                else
                {
                    YouTubeUrls.Add(m.imdbID, "");
                }
            }
        }

        private void GetBestFifteenMoviesOutOfTwenty()
        {
            // Remove movies that already exist in the list
            List<Movie> UserExistingMovies = DbManager.GetUserMovies(MainUser.Id);

            foreach (Movie m in UserExistingMovies)
            {
                ApiModelOmdb duplicateMovie = TwentyOmdbMovies.FirstOrDefault(o => o.imdbID == m.imdbID);

                if (duplicateMovie != null) TwentyOmdbMovies.Remove(duplicateMovie);
            }

            // Sort on IMDB ratings
            TwentyOmdbMovies = TwentyOmdbMovies.OrderByDescending(o => o.imdbRating).ToList();

            FifteenOmdbMovies = TwentyOmdbMovies.Take(15).ToList();

            NbOfResults = FifteenOmdbMovies.Count;
        }

        private void LoadNextMovie()
        {
            ClearForm();

            if ((FifteenOmdbMovies != null) && (FifteenOmdbMovies.Count > 0))
            {
                try
                {
                    try
                    {
                        MessageToUser.StringValue = string.Format("Total results {0}", NbOfResults);

                        if (FifteenOmdbMovies.Count >= 3) MessageToUser2.StringValue = string.Format("{0} results left.", FifteenOmdbMovies.Count - 1);
                        else MessageToUser2.StringValue = string.Format("{0} result left.", FifteenOmdbMovies.Count - 1);

                        CkeckOnlineMoviePage.Enabled = true;

                        CurrentOmdbMovie = FifteenOmdbMovies.First();
                    }
                    catch (Exception) { }
                    try
                    {
                        if (!string.IsNullOrEmpty(CurrentOmdbMovie.Poster) && (!CurrentOmdbMovie.Poster.Contains("N/A")))
                        {
                            MoviePoster.Enabled = true;
                            Uri uriSource = new Uri(CurrentOmdbMovie.Poster, UriKind.Absolute);
                            MoviePoster.Image = new NSImage(uriSource);
                        }
                        else MoviePoster.Image = NSImage.ImageNamed("unavailable");
                    }
                    catch (Exception)
                    {
                        MoviePoster.Image = NSImage.ImageNamed("unavailable");
                    }
                    try
                    {
                        if (!string.IsNullOrEmpty(CurrentOmdbMovie.Title))
                        {
                            MovieTitle.Enabled = true;
                            MovieTitle.StringValue = CurrentOmdbMovie.Title;
                        }
                    }
                    catch (Exception) { }
                    try
                    {
                        if (!string.IsNullOrEmpty(CurrentOmdbMovie.Year))
                        {
                            MovieYear.Enabled = true;
                            MovieYear.StringValue = CurrentOmdbMovie.Year;
                        }
                    }
                    catch (Exception) { }
                    try
                    {
                        MovieGenres.StringValue = CurrentOmdbMovie.Genre;
                    }
                    catch (Exception) { }
                    try
                    {
                        GetMovieImdbRatings();

                        MovieRatingBar.DoubleValue = imdbRating;
                        MovieRating.StringValue = string.Format("{0}", imdbRating);

                        if (imdbVotes == 1) MovieNbOfRatings.StringValue = "1 rating";
                        else MovieNbOfRatings.StringValue = string.Format("{0:n0} ratings", imdbVotes);
                    }
                    catch (Exception) { }
                    try
                    {
                        MovieCountries.StringValue = CurrentOmdbMovie.Country;
                    }
                    catch (Exception) { }
                    try
                    {
                        int match = CalculateMatchUserPreferencesToMovie(CurrentOmdbMovie);

                        if (match > 0) MatchLevel.StringValue = string.Format("{0} %", match);
                        else MatchLevel.StringValue = "N/A";
                    }
                    catch (Exception) { }

                    StoryButton.Enabled |= !string.IsNullOrEmpty(CurrentOmdbMovie.Plot);
                    YouTubeButton.Enabled |= !string.IsNullOrEmpty(YouTubeUrls[CurrentOmdbMovie.imdbID]);
                    SuccessLoadingMovie();
                }
                catch
                {
                    FailureLoadingMovie();
                }
            }
            else
            {
                MoviePoster.Image = NSImage.ImageNamed("no_results");
                MessageToUser.StringValue = "No results!";
                MessageToUser2.StringValue = "";

                FailureLoadingMovie();
            }
            DisablePreLoader();
        }

        private void GetMovieImdbRatings()
        {
            double.TryParse(CurrentOmdbMovie.imdbRating, out imdbRating);
            int.TryParse(CurrentOmdbMovie.imdbVotes.Replace(",", ""), out imdbVotes);
        }

        private int CalculateMatchUserPreferencesToMovie(ApiModelOmdb movie)
        {
            List<int> Genres = new List<int>();
            int match = 0;

            if (!string.IsNullOrEmpty(movie.Genre))
            {
                foreach (var t in DbManager.moviesTypes)
                {
                    if (movie.Genre.Contains(t.Name)) Genres.Add(t.Id);
                }

                if (Genres.Count > 0)
                {
                    List<double> UserAverageGenreRatings = new List<double>();

                    foreach (int genre_id in Genres)
                    {
                        double r = DbManager.GetUserAverageGenreRating(MainUser.Id, 1);
                        UserAverageGenreRatings.Add(r);
                    }

                    if (UserAverageGenreRatings.Count > 0)
                    {
                        double.TryParse(movie.imdbRating, out double imdbMovieRating);

                        double UserAverageGenresRating = UserAverageGenreRatings.Average();

                        if (imdbMovieRating <= UserAverageGenresRating) match = (int)(100 * imdbMovieRating / UserAverageGenresRating);
                        else match = (int)(100 * UserAverageGenresRating / imdbMovieRating);
                    }
                }
            }
            return match;
        }

        private void ClearForm()
        {
            EnablePreLoader();
            CkeckOnlineMoviePage.Enabled = false;

            YesButton.Hidden = true;
            NoButton.Hidden = true;
            CloseButton.Hidden = true;
            YesNoMessage.StringValue = "";

            MovieTitle.StringValue = "";
            MovieYear.StringValue = "";

            MoviePoster.Enabled = true;
            MoviePoster.Image = NSImage.ImageNamed("loading");

            StoryButton.Enabled = false;
            YouTubeButton.Enabled = false;

            MovieCountries.StringValue = "";
            MovieNbOfRatings.StringValue = "";
            MovieRatingBar.DoubleValue = 0;
        }

        private void SuccessLoadingMovie()
        {
            YesNoMessage.StringValue = "Would you like to add this movie to your list ?";
            SetButtonsStatus(ConstAndParams.ButtonsAndComboBoxState_Enabled);
            YesButton.Hidden = false;
            NoButton.Hidden = false;
        }

        private void FailureLoadingMovie()
        {
            YesButton.Hidden = true;
            NoButton.Hidden = true;
            CloseButton.Hidden = false;
            CkeckOnlineMoviePage.Enabled = false;
            YesNoMessage.Hidden = true;
        }

        private void SetButtonsStatus(bool status)
        {
            YesButton.Enabled = status;
            NoButton.Enabled = status;
        }

        partial void YesButton_Clicked(NSObject sender)
        {
            SetButtonsStatus(ConstAndParams.ButtonsAndComboBoxState_Disabled);

            int.TryParse(CurrentOmdbMovie.Year, out int year);

            Movie NewMovie = new Movie(CurrentOmdbMovie.Title, year, CurrentOmdbMovie.Poster, CurrentOmdbMovie.imdbID, CurrentOmdbMovie.Plot);

            List<int> Countries_Ids = new List<int>();
            if (!string.IsNullOrEmpty(CurrentOmdbMovie.Country))
            {
                Countries_Ids = JsonTextProcessor.ExtractCountriesIds(CurrentOmdbMovie.Country, DbManager.countriesNamesAndCodes);
            }

            List<int> Genres = new List<int> { };
            if (!string.IsNullOrEmpty(CurrentOmdbMovie.Genre))
            {
                foreach (var t in DbManager.moviesTypes)
                {
                    if (CurrentOmdbMovie.Genre.Contains(t.Name)) Genres.Add(t.Id);
                }
            }

            if (DbManager.IsThereAnExistingMovie(NewMovie.imdbID))
            {
                if (DbManager.IsThereAnExistingMovieForUser(NewMovie.imdbID, MainUser.Id))
                {
                    DismissViewController(this);
                    MainView.AlertMessage("This movie already exists in your list of movies.", "Already exists!");
                    NewMovie = DbManager.GetMovieByImdbId(NewMovie.imdbID);
                    MainView.ReloadDisplay(NewMovie);
                }
                else
                {
                    DismissViewController(this);
                    MainView.AddMovieToDbAndReload(false, NewMovie, Countries_Ids, Genres, ConstAndParams.ImdbOnlineUserId, imdbRating, imdbVotes);
                }
            }
            else
            {
                DismissViewController(this);
                MainView.AddMovieToDbAndReload(true, NewMovie, Countries_Ids, Genres, ConstAndParams.ImdbOnlineUserId, imdbRating, imdbVotes);
            }
        }

        partial void CloseButton_Clicked(NSObject sender)
        {
            DismissViewController(this);
        }

        partial void NoButton_Clicked(NSObject sender)
        {
            if ((FifteenOmdbMovies != null) && (FifteenOmdbMovies.Count > 1))
            {
                FifteenOmdbMovies.Remove(CurrentOmdbMovie);
                LoadNextMovie();
            }
            else DismissViewController(this);
        }

        private void EnablePreLoader()
        {
            LoadingGif.Image = NSImage.ImageNamed("loading2");
            LoadingGif.Enabled = true;
        }

        private void DisablePreLoader()
        {
            LoadingGif.Image = null;
            LoadingGif.Enabled = false;
        }

        [Export("prepareForSegue:sender:")]
        public override void PrepareForSegue(NSStoryboardSegue segue, NSObject sender)
        {
            base.PrepareForSegue(segue, sender);

            switch (segue.Identifier)
            {
                case "LaunchViewImdbMoviePage":
                    {
                        ViewImdbMoviePage target = segue.DestinationController as ViewImdbMoviePage;
                        target.MovieImdbId = CurrentOmdbMovie.imdbID;
                    }
                    break;
                case "LaunchMovieStory":
                    {
                        ViewMovieStory target = segue.DestinationController as ViewMovieStory;
                        target.MovieStory = CurrentOmdbMovie.Plot;
                    }
                    break;
                case "LaunchViewYouTube":
                    {
                        ViewYouTube target = segue.DestinationController as ViewYouTube;
                        target.MovieUrl = YouTubeUrls[CurrentOmdbMovie.imdbID];
                    }
                    break;
            }
        }
    }
}
