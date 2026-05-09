using System.Drawing;

namespace CardGame101.Models
{
    public class Card
    {
        public Suit Suit { get; set; }
        public Rank Rank { get; set; }

        public int GetPoints()
        {
            return Rank switch
            {
                Rank.Ace => 11,
                Rank.Ten => 10,
                Rank.Eight => 8,
                Rank.Seven => 7,
                Rank.Six => 6,
                Rank.King => 4,
                Rank.Queen => 3,
                Rank.Jack => 2,
                Rank.Nine => 0,
                _ => 0
            };
        }

        // Метод для красивого відображення масті
        public string GetSuitSymbol()
        {
            return Suit switch
            {
                Suit.Hearts => "♥️",
                Suit.Diamonds => "♦️",
                Suit.Clubs => "♣️",
                Suit.Spades => "♠️",
                _ => ""
            };
        }

        // Метод для кольору тексту карти (Червоні або Чорні)
        public Color GetColor()
        {
            return (Suit == Suit.Hearts || Suit == Suit.Diamonds) ? Color.Red : Color.Black;
        }
    }
}