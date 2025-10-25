using System;
using System.Collections.ObjectModel;
using C_C.Model;
using C_C.Repositories;
using C_C.Services;

namespace C_C.ViewModel
{
    public class MatchesViewModel : ViewModelBase
    {
        private readonly IMatchRepository _matchRepository;
        private readonly ObservableCollection<MatchModel> _matches = new ObservableCollection<MatchModel>();

        public MatchesViewModel()
        {
            _matchRepository = new MatchRepository();
            CargarMatches();
        }

        public ObservableCollection<MatchModel> Matches => _matches;

        private void CargarMatches()
        {
            _matches.Clear();
            var matches = _matchRepository.GetMatchesForUser(UserSession.Instance.CurrentUserId);
            foreach (var match in matches)
            {
                _matches.Add(match);
            }
        }
    }
}
