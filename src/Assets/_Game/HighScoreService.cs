using System;
using System.Collections.Generic;
using System.Linq;
using Core.Mediators;
using UnityEngine;

namespace _Game
{

    [Serializable]
    public class HighScore
    {
        public string Name;

        public int Kills;

        public string Time;

        public string Date;
    }

    [Serializable]
    public class HighScores
    {
        public List<HighScore> Scores = new();
    }

    public interface IHighScoreService
    {
        IEnumerable<HighScore> GetHighScores();

        void AddHighScore(string name, int kills, TimeSpan time);
    }

    public class HighScoreService : IHighScoreService
    {
        private readonly HighScores _highScores = new();

        private readonly IStorageService _storageService;

        public HighScoreService(IStorageService storageService)
        {
            _storageService = storageService;

            string highScore = _storageService.Get("HighScore");
            if (string.IsNullOrWhiteSpace(highScore) == false)
            {
                _highScores = JsonUtility.FromJson<HighScores>(highScore);
            }
        }

        public IEnumerable<HighScore> GetHighScores()
        {
            return _highScores.Scores.ToList();
        }

        public void AddHighScore(string name, int kills, TimeSpan time)
        {
            string timeString = $"{(int)time.TotalMinutes:00}:{time.Seconds:00}";
            if (_highScores.Scores.Any(hs => hs.Name == name && hs.Kills == kills && hs.Time == timeString))
            {
                return; // Skip if already added
            }

            int position = _highScores.Scores.Count;
            for (int i = 0; i < _highScores.Scores.Count; i++)
            {
                if (_highScores.Scores[i].Kills < kills)
                {
                    position = i;
                    break;
                }
            }

            _highScores.Scores.Insert(position, new HighScore()
            {
                Name = name,
                Kills = kills,
                Time = timeString,
                Date = DateTime.Now.ToString()
            });

            string highScore = JsonUtility.ToJson(_highScores);
            _storageService.Set("HighScore", highScore);
        }
    }
}