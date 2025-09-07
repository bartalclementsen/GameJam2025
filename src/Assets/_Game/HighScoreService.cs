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

        public int Percent;

        public int Score;

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

        void AddHighScore(string name, int percent, int score);
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

        public void AddHighScore(string name, int percent, int score)
        {
            if (_highScores.Scores.Any(hs => hs.Name == name && hs.Percent == score && hs.Score == score))
            {
                return; // Skip if already added
            }

            int position = _highScores.Scores.Count;
            for (int i = 0; i < _highScores.Scores.Count; i++)
            {
                if (_highScores.Scores[i].Percent < percent)
                {
                    position = i;
                    break;
                }
            }

            _highScores.Scores.Insert(position, new HighScore()
            {
                Name = name,
                Percent = percent,
                Score = score,
                Date = DateTime.Now.ToString()
            });

            string highScore = JsonUtility.ToJson(_highScores);
            _storageService.Set("HighScore", highScore);
        }
    }
}