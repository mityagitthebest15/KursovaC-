using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.IO;
using CardGame101.Models;

namespace CardGame101.Engine
{
    public class GameEngine
    {
        public List<Player> Players { get; set; } = new();
        public List<Card> Deck { get; set; } = new();
        public Card TopCard { get; set; }
        public Suit CurrentSuit { get; set; }
        public int CurrentPlayerIndex { get; set; }
        public bool NeedsToCoverNine { get; set; }
        public int CardsToDraw { get; set; }
        public bool SkipNext { get; set; }

        public bool IsRoundOver => Deck.Count == 0 || Players.Any(p => p.Hand.Count == 0);
        public bool IsGameOver => Players.Any(p => p.Score > 101);

        // Цей метод створює тебе і 3 ботів з правильними іменами
        public void InitializeGame()
        {
            Players.Clear();
            Players.Add(new Player { Name = "Ви", IsBot = false });
            for (int i = 1; i <= 3; i++)
                Players.Add(new Player { Name = $"Bot {i}", IsBot = true });
        }

        public void StartRound()
        {
            Deck = GenerateDeck();
            ShuffleDeck();
            CardsToDraw = 0;
            SkipNext = false;
            CurrentPlayerIndex = 0;

            foreach (var player in Players)
            {
                player.Hand.Clear();
                for (int i = 0; i < 4; i++) player.Hand.Add(DrawCard());
            }
            TopCard = DrawCard();
            CurrentSuit = TopCard.Suit;
            NeedsToCoverNine = TopCard.Rank == Rank.Nine;
        }

        private List<Card> GenerateDeck()
        {
            var deck = new List<Card>();
            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
            foreach (Rank rank in Enum.GetValues(typeof(Rank)))
                deck.Add(new Card { Suit = suit, Rank = rank });
            return deck;
        }

        private void ShuffleDeck()
        {
            var rng = new Random();
            Deck = Deck.OrderBy(c => rng.Next()).ToList();
        }

        public Card DrawCard()
        {
            if (!Deck.Any()) return null;
            var card = Deck.First();
            Deck.RemoveAt(0);
            return card;
        }

        
        public void ApplyCardEffect(Card playedCard, Suit chosenSuit)
        {
            TopCard = playedCard;
            CurrentSuit = playedCard.Rank == Rank.Queen ? chosenSuit : playedCard.Suit;
            NeedsToCoverNine = playedCard.Rank == Rank.Nine;

            if (playedCard.Rank == Rank.Ace) SkipNext = true;
            else if (playedCard.Rank == Rank.Seven) { CardsToDraw = 2; SkipNext = true; }
            else if (playedCard.Rank == Rank.Six) { CardsToDraw = 1; SkipNext = true; }
            else if (playedCard.Suit == Suit.Spades && playedCard.Rank == Rank.King) { CardsToDraw = 4; SkipNext = true; }
        }

        
        
        public bool CheckAndApplyPenalty()
        {
            bool hasPenalty = false;
            if (CardsToDraw > 0)
            {
                for (int i = 0; i < CardsToDraw; i++)
                {
                    var card = DrawCard();
                    if (card != null) Players[CurrentPlayerIndex].Hand.Add(card);
                }
                CardsToDraw = 0;
                hasPenalty = true;
            }
            if (SkipNext)
            {
                SkipNext = false;
                hasPenalty = true;
            }
            return hasPenalty;
        }

        public void NextPlayer() => CurrentPlayerIndex = (CurrentPlayerIndex + 1) % Players.Count;

        
        public void EndRound()
        {
            var winner = Players.FirstOrDefault(p => p.Hand.Count == 0);

            foreach (var player in Players)
                player.Score += player.Hand.Sum(c => c.GetPoints());

            if (winner != null && TopCard.Rank == Rank.Queen)
            {
                winner.Score -= TopCard.Suit == Suit.Spades ? 40 : 20;
            }

            foreach (var player in Players)
            {
                if (player.Score == 101) player.Score = 0;
            }
        }

        public void SaveGame(string path)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(this, options);
            File.WriteAllText(path, json);
        }

        public static GameEngine LoadGame(string path)
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<GameEngine>(json);
        }
    }
}