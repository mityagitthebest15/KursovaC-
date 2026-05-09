using System;
using System.Collections.Generic;
using System.Linq;
using CardGame101.Models;

namespace CardGame101.Engine
{
    
    public interface IPlayer
    {
        string Name { get; set; }
        List<Card> Hand { get; set; }
        int Score { get; set; }
        bool IsValidPlay(Card card, Card topCard, Suit currentSuit, bool needsToCoverNine);
    }

    public class Player : IPlayer
    {
        public string Name { get; set; }
        public List<Card> Hand { get; set; } = new List<Card>();
        public int Score { get; set; }
        public bool IsBot { get; set; }

        
        public bool IsValidPlay(Card card, Card topCard, Suit currentSuit, bool needsToCoverNine)
        {
            
            if (needsToCoverNine)
            {
                return card.Rank == Rank.Nine || card.Suit == currentSuit;
            }

            
            if (card.Rank == Rank.Queen) return true;

            
            if (card.Suit == Suit.Spades && card.Rank == Rank.King)
            {
                return topCard.Rank == Rank.King || currentSuit == Suit.Spades;
            }

            

            
            
            return card.Rank == topCard.Rank || card.Suit == currentSuit;
        }

        
        public Card PlayBotCard(Card topCard, Suit currentSuit, bool needsToCoverNine, out Suit? chosenSuit)
        {
            chosenSuit = null;
            
            
            var validCards = Hand.Where(c => IsValidPlay(c, topCard, currentSuit, needsToCoverNine)).ToList();

            if (!validCards.Any()) return null; 

            
            validCards = validCards.OrderByDescending(c => c.GetPoints()).ToList();
            var cardToPlay = validCards.First();

            
            if (cardToPlay.Rank == Rank.Queen)
            {
                
                var bestSuitGroup = Hand.Where(c => c != cardToPlay)
                                        .GroupBy(c => c.Suit)
                                        .OrderByDescending(g => g.Count())
                                        .FirstOrDefault();
                                        
                
                chosenSuit = bestSuitGroup != null ? bestSuitGroup.Key : Suit.Hearts;
            }

            return cardToPlay;
        }
    }
}