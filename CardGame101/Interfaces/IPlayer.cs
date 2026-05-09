using System.Collections.Generic;
using CardGame101.Models;

namespace CardGame101.Interfaces
{
    public interface IPlayer
    {
        string Name { get; set; }
        bool IsBot { get; set; }
        List<Card> Hand { get; set; }
        int Score { get; set; }
        bool IsValidPlay(Card card, Card topCard, Suit currentSuit, bool needsToCoverNine);
    }
}