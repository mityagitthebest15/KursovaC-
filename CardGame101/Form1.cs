using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CardGame101.Engine;
using CardGame101.Models;

namespace CardGame101
{
    public partial class Form1 : Form
    {
        private GameEngine engine;
        private PictureBox topCardPic;
        private PictureBox deckPic;
        private Panel handPanel;
        
        private Label deckLabel;
        private Label botLeftLabel, botTopLabel, botRightLabel;
        private PictureBox botLeftPic, botTopPic, botRightPic;
        
        private ListBox gameLog;
        private Label statusLabel;
        
        private bool isUserTurn = false;
        private bool hasDrawnCardThisTurn = false;

        public Form1()
        {
            InitializeComponent();
            this.Text = "Карткова гра 101";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(34, 139, 34); 

            engine = new GameEngine();
            engine.InitializeGame();
            engine.StartRound();

            SetupUI();
            
            
            typeof(Panel).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(handPanel, true, null);

            Log("Гра почалася! Удачі!");
            PrepareUserTurn();
        }

        private void SetupUI()
        {
            
            Label logTitle = new Label { Text = "ЛОГ ПОДІЙ:", Location = new Point(20, 20), ForeColor = Color.Yellow, Font = new Font("Arial", 12, FontStyle.Bold), AutoSize = true, BackColor = Color.Transparent };
            this.Controls.Add(logTitle);
            
            gameLog = new ListBox { 
                Location = new Point(20, 45), 
                Size = new Size(340, 280), 
                Font = new Font("Segoe UI", 10, FontStyle.Regular), 
                BackColor = Color.FromArgb(20, 60, 20), 
                ForeColor = Color.LightGreen,           
                BorderStyle = BorderStyle.None 
            };
            this.Controls.Add(gameLog);

            
            statusLabel = new Label { 
                Location = new Point(880, 20), 
                Size = new Size(280, 160), 
                Font = new Font("Arial", 12, FontStyle.Bold), 
                ForeColor = Color.Yellow, 
                BackColor = Color.FromArgb(20, 60, 20), 
                Padding = new Padding(15) 
            };
            this.Controls.Add(statusLabel);

            // БОТИ (ПОЗИЦІЇ ТА КАРТИНКИ)
            botLeftLabel = new Label { Location = new Point(50, 330), ForeColor = Color.White, AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            botLeftPic = new PictureBox { Location = new Point(50, 350), Size = new Size(80, 120), Image = GenerateCardBack(), SizeMode = PictureBoxSizeMode.StretchImage };
            
            botTopLabel = new Label { Location = new Point(550, 80), ForeColor = Color.White, AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            
            
            Image topBackImg = GenerateCardBack();
            topBackImg.RotateFlip(RotateFlipType.Rotate270FlipNone); 
            botTopPic = new PictureBox { 
                Location = new Point(550, 100), 
                Size = new Size(120, 80), 
                Image = topBackImg, 
                SizeMode = PictureBoxSizeMode.StretchImage 
            };
            // ------------------------------------------------
            
            botRightLabel = new Label { Location = new Point(1050, 330), ForeColor = Color.White, AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            botRightPic = new PictureBox { Location = new Point(1050, 350), Size = new Size(80, 120), Image = GenerateCardBack(), SizeMode = PictureBoxSizeMode.StretchImage };
            
            this.Controls.Add(botLeftLabel); this.Controls.Add(botLeftPic);
            this.Controls.Add(botTopLabel); this.Controls.Add(botTopPic);
            this.Controls.Add(botRightLabel); this.Controls.Add(botRightPic);

            // ЦЕНТР (КОЛОДА + КІН)
            deckLabel = new Label { Location = new Point(440, 320), ForeColor = Color.Yellow, AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            this.Controls.Add(deckLabel);
            deckPic = new PictureBox { Location = new Point(440, 340), Size = new Size(100, 150), Image = GenerateCardBack(), SizeMode = PictureBoxSizeMode.StretchImage, Cursor = Cursors.Hand };
            deckPic.Click += (s, e) => { if (isUserTurn && !hasDrawnCardThisTurn) DrawCard_Click(); };
            
            topCardPic = new PictureBox { Location = new Point(560, 340), Size = new Size(100, 150), SizeMode = PictureBoxSizeMode.StretchImage };
            this.Controls.Add(deckPic); this.Controls.Add(topCardPic);

            // ПАНЕЛЬ ГРАВЦЯ
            handPanel = new Panel { Location = new Point(150, 580), Size = new Size(900, 180), AutoScroll = true };
            this.Controls.Add(handPanel);

            
            Button btnSave = new Button { 
                Text = "ЗБЕРЕГТИ", Location = new Point(1055, 680), Size = new Size(130, 40),
                BackColor = Color.FromArgb(20, 60, 20), ForeColor = Color.Yellow, FlatStyle = FlatStyle.Flat, Font = new Font("Arial", 8, FontStyle.Bold)
            };
            btnSave.FlatAppearance.BorderSize = 1; btnSave.FlatAppearance.BorderColor = Color.Yellow;
            btnSave.Click += (s, e) => { engine.SaveGame("save.json"); Log("✅ Гру успішно збережено!"); };

            Button btnLoad = new Button { 
                Text = "ЗАВАНТАЖИТИ", Location = new Point(1055, 730), Size = new Size(130, 40),
                BackColor = Color.FromArgb(20, 60, 20), ForeColor = Color.Yellow, FlatStyle = FlatStyle.Flat, Font = new Font("Arial", 8, FontStyle.Bold)
            };
            btnLoad.FlatAppearance.BorderSize = 1; btnLoad.FlatAppearance.BorderColor = Color.Yellow;
            btnLoad.Click += (s, e) => { 
                try { engine = GameEngine.LoadGame("save.json"); UpdateUI(); PrepareUserTurn(); Log("📂 Гру відновлено з файлу."); }
                catch { MessageBox.Show("Файл збереження не знайдено!"); }
            };
            this.Controls.Add(btnSave); this.Controls.Add(btnLoad);
        }

        // --- ГРАФІЧНІ МЕТОДИ ---
        private Image GenerateCardImage(string rank, string suitSymbol, Color color)
        {
            Bitmap bmp = new Bitmap(100, 150);
            using (Graphics g = Graphics.FromImage(bmp)) {
                g.SmoothingMode = SmoothingMode.AntiAlias; g.Clear(Color.White);
                g.DrawRectangle(new Pen(Color.Gray, 1), 0, 0, 99, 149);
                Font fS = new Font("Arial", 12, FontStyle.Bold); Font fB = new Font("Arial", 36, FontStyle.Bold); Brush b = new SolidBrush(color);
                g.DrawString(rank, fS, b, 5, 5); g.DrawString(suitSymbol, fS, b, 5, 22);
                SizeF sz = g.MeasureString(suitSymbol, fB);
                g.DrawString(suitSymbol, fB, b, (100 - sz.Width) / 2, (150 - sz.Height) / 2);
            }
            return bmp;
        }

        private Image GenerateCardBack()
        {
            Bitmap bmp = new Bitmap(100, 150);
            using (Graphics g = Graphics.FromImage(bmp)) {
                g.Clear(Color.White); g.FillRectangle(Brushes.MidnightBlue, 5, 5, 90, 140);
                Pen p = new Pen(Color.CornflowerBlue, 1);
                for (int i = 0; i < 150; i += 10) { g.DrawLine(p, 0, i, 100, i + 10); g.DrawLine(p, 100, i, 0, i + 10); }
            }
            return bmp;
        }

        private string GetRankStr(Rank r) => r switch { Rank.Ace => "A", Rank.King => "K", Rank.Queen => "Q", Rank.Jack => "J", Rank.Ten => "10", Rank.Nine => "9", Rank.Eight => "8", Rank.Seven => "7", Rank.Six => "6", _ => "" };
        private string GetSuitUa(Suit s) => s switch { Suit.Hearts => "Чирва", Suit.Diamonds => "Бубна", Suit.Clubs => "Хреста", Suit.Spades => "Піка", _ => "" };

        private void Log(string m) { gameLog.Items.Add(m); gameLog.TopIndex = gameLog.Items.Count - 1; }

        private void UpdateUI()
        {
            botLeftLabel.Text = $"{engine.Players[1].Name}: {engine.Players[1].Hand.Count}";
            botTopLabel.Text = $"{engine.Players[2].Name}: {engine.Players[2].Hand.Count}";
            botRightLabel.Text = $"{engine.Players[3].Name}: {engine.Players[3].Hand.Count}";
            deckLabel.Text = $"В колоді: {engine.Deck.Count}";
            statusLabel.Text = $"СТАТУС ГРИ:\n\nМАСТЬ: {GetSuitUa(engine.CurrentSuit)}\n\nХІД: {engine.Players[engine.CurrentPlayerIndex].Name}";
            topCardPic.Image = GenerateCardImage(GetRankStr(engine.TopCard.Rank), engine.TopCard.GetSuitSymbol(), engine.CurrentSuit == Suit.Hearts || engine.CurrentSuit == Suit.Diamonds ? Color.Red : Color.Black);
            handPanel.Controls.Clear(); int x = 0;
            foreach (var card in engine.Players[0].Hand.ToList()) {
                PictureBox p = new PictureBox { Size = new Size(90, 135), Location = new Point(x, 10), Image = GenerateCardImage(GetRankStr(card.Rank), card.GetSuitSymbol(), card.GetColor()), SizeMode = PictureBoxSizeMode.StretchImage, Cursor = Cursors.Hand, Tag = card };
                p.Click += (s, e) => PlayCard_Click((Card)((PictureBox)s).Tag);
                handPanel.Controls.Add(p); x += 95;
            }
        }

        // --- ЛОГІКА ХОДІВ ---
        private async void PrepareUserTurn()
        {
            if (CheckRoundEnd()) return;
            isUserTurn = false; hasDrawnCardThisTurn = false; UpdateUI();

            if (engine.CheckAndApplyPenalty()) {
                UpdateUI(); Log("! Ви пропускаєте хід через штраф !");
                await Task.Delay(1500); if (CheckRoundEnd()) return;
                engine.NextPlayer(); RunBotTurns(); return;
            }

            var user = engine.Players[0];
            if (!user.Hand.Any(c => user.IsValidPlay(c, engine.TopCard, engine.CurrentSuit, engine.NeedsToCoverNine))) {
                if (engine.NeedsToCoverNine) {
                    Log("Треба перекрити 9! Добір...");
                    while (!user.Hand.Any(c => user.IsValidPlay(c, engine.TopCard, engine.CurrentSuit, engine.NeedsToCoverNine))) {
                        var d = engine.DrawCard(); if (d == null) { CheckRoundEnd(); return; }
                        user.Hand.Add(d); UpdateUI(); Log($"Витягнуто: {GetRankStr(d.Rank)} {d.GetSuitSymbol()}"); await Task.Delay(800);
                    }
                } else Log("Немає ходу. Натисніть на колоду.");
            }
            isUserTurn = true; UpdateUI();
        }

        private async void DrawCard_Click()
        {
            if (!isUserTurn || hasDrawnCardThisTurn) return;
            hasDrawnCardThisTurn = true;
            var d = engine.DrawCard();
            if (d != null) {
                engine.Players[0].Hand.Add(d); UpdateUI(); Log($"Ви потягнули: {GetRankStr(d.Rank)} {d.GetSuitSymbol()}");
                if (!engine.Players[0].IsValidPlay(d, engine.TopCard, engine.CurrentSuit, engine.NeedsToCoverNine)) {
                    Log("Не підходить. Пропуск ходу."); isUserTurn = false; await Task.Delay(1500);
                    if (CheckRoundEnd()) return; engine.NextPlayer(); RunBotTurns();
                } else Log("Карта підходить! Зіграйте нею.");
            } else CheckRoundEnd();
        }

        private void PlayCard_Click(Card card)
        {
            if (!isUserTurn) return;
            if (engine.Players[0].IsValidPlay(card, engine.TopCard, engine.CurrentSuit, engine.NeedsToCoverNine)) {
                isUserTurn = false; Suit s = card.Suit;
                if (card.Rank == Rank.Queen) s = PromptForSuit();
                engine.Players[0].Hand.Remove(card); engine.ApplyCardEffect(card, s);
                Log($"Ви зіграли {GetRankStr(card.Rank)} {card.GetSuitSymbol()}" + (card.Rank == Rank.Queen ? $" ({GetSuitUa(s)})" : ""));
                if (!CheckRoundEnd()) { engine.NextPlayer(); RunBotTurns(); }
            } else MessageBox.Show("Невірний хід!", "Помилка");
        }

        private Suit PromptForSuit()
        {
            Suit sel = Suit.Hearts;
            Form f = new Form { Width = 280, Height = 160, Text = "Оберіть масть", StartPosition = FormStartPosition.CenterParent, ControlBox = false, BackColor = Color.WhiteSmoke };
            ComboBox cb = new ComboBox { Left = 30, Top = 20, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 12, FontStyle.Regular) };
            cb.Items.AddRange(new string[] { "♥️ Чирва", "♦️ Бубна", "♣️ Хреста", "♠️ Піка" }); cb.SelectedIndex = 0;
            Button b = new Button { Text = "ОК", Left = 30, Top = 65, Width = 200, Height = 35, Font = new Font("Segoe UI", 10, FontStyle.Bold), BackColor = Color.LightGray, Cursor = Cursors.Hand };
            b.Click += (s, e) => { sel = (Suit)cb.SelectedIndex; f.Close(); };
            f.Controls.Add(cb); f.Controls.Add(b); f.ShowDialog(); return sel;
        }

        private async void RunBotTurns()
        {
            try {
                if (CheckRoundEnd()) return;
                while (engine.CurrentPlayerIndex != 0 && !engine.IsRoundOver) {
                    var bot = engine.Players[engine.CurrentPlayerIndex]; UpdateUI(); await Task.Delay(1500);
                    if (engine.CheckAndApplyPenalty()) { UpdateUI(); Log($"{bot.Name} пропустив через штраф."); await Task.Delay(1000); if (CheckRoundEnd()) return; engine.NextPlayer(); continue; }
                    Card p = bot.PlayBotCard(engine.TopCard, engine.CurrentSuit, engine.NeedsToCoverNine, out Suit? bs);
                    if (p != null) {
                        bot.Hand.Remove(p); engine.ApplyCardEffect(p, bs ?? Suit.Hearts);
                        Log($"{bot.Name} зіграв {GetRankStr(p.Rank)} {p.GetSuitSymbol()}" + (p.Rank == Rank.Queen ? $" ({GetSuitUa(bs ?? Suit.Hearts)})" : "")); 
                    } else {
                        if (engine.NeedsToCoverNine) {
                            while (true) {
                                var d = engine.DrawCard(); if (d == null) break; bot.Hand.Add(d); UpdateUI();
                                if (bot.IsValidPlay(d, engine.TopCard, engine.CurrentSuit, engine.NeedsToCoverNine)) {
                                    bot.Hand.Remove(d); Suit s = d.Rank == Rank.Queen ? Suit.Hearts : d.Suit; engine.ApplyCardEffect(d, s); Log($"{bot.Name} перекрив 9: {GetRankStr(d.Rank)}"); break;
                                }
                                await Task.Delay(500);
                            }
                        } else {
                            var d = engine.DrawCard();
                            if (d != null) {
                                bot.Hand.Add(d); UpdateUI();
                                if (bot.IsValidPlay(d, engine.TopCard, engine.CurrentSuit, engine.NeedsToCoverNine)) {
                                    bot.Hand.Remove(d); Suit s = d.Rank == Rank.Queen ? Suit.Hearts : d.Suit; engine.ApplyCardEffect(d, s); Log($"{bot.Name} потягнув і зіграв {GetRankStr(d.Rank)}");
                                } else Log($"{bot.Name} потягнув карту і скіпнув.");
                            }
                        }
                    }
                    if (CheckRoundEnd()) return; engine.NextPlayer();
                }
                if (!engine.IsRoundOver) PrepareUserTurn();
            }
            catch { engine.NextPlayer(); if (engine.CurrentPlayerIndex == 0) PrepareUserTurn(); else RunBotTurns(); }
        }

        // --- ВІКНО ЗАВЕРШЕННЯ ---
        private bool CheckRoundEnd()
        {
            if (engine.IsRoundOver) {
                engine.EndRound(); UpdateUI();
                if (engine.IsGameOver) {
                    var w = engine.Players.OrderBy(p => p.Score).First();
                    ShowRoundOverDialog($"ГРУ ЗАВЕРШЕНО!\nПереможець: {w.Name}", true);
                    engine.InitializeGame(); engine.StartRound(); PrepareUserTurn();
                } else {
                    ShowRoundOverDialog("РАУНД ЗАВЕРШЕНО!", false);
                    engine.StartRound(); if (engine.CurrentPlayerIndex != 0) RunBotTurns(); else PrepareUserTurn();
                }
                return true;
            }
            return false;
        }

        private void ShowRoundOverDialog(string title, bool isGameOver)
        {
            Form overlay = new Form { FormBorderStyle = FormBorderStyle.None, StartPosition = FormStartPosition.CenterParent, Size = new Size(500, 400), BackColor = Color.FromArgb(20, 20, 20), Opacity = 1.0, ShowInTaskbar = false };
            Label lblTitle = new Label { Text = title, Font = new Font("Arial", 22, FontStyle.Bold), ForeColor = Color.Gold, AutoSize = false, Width = overlay.Width, Height = 80, TextAlign = ContentAlignment.MiddleCenter, Location = new Point(0, 20) };
            overlay.Controls.Add(lblTitle);
            int y = 110;
            foreach (var p in engine.Players) {
                string name = p.Name == "Ви" ? "Ти" : p.Name;
                Label lblScore = new Label { Text = $"{name} : {p.Score} очок", Font = new Font("Arial", 14, FontStyle.Bold), ForeColor = Color.LightSkyBlue, AutoSize = false, Width = overlay.Width, Height = 30, TextAlign = ContentAlignment.MiddleCenter, Location = new Point(0, y) };
                overlay.Controls.Add(lblScore); y += 35;
            }
            Button btnC = new Button { Text = isGameOver ? "НОВА ГРА" : "ПРОДОВЖИТИ", Font = new Font("Arial", 11, FontStyle.Bold), BackColor = Color.FromArgb(34, 139, 34), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Size = new Size(160, 45), Location = new Point(80, y + 30), Cursor = Cursors.Hand };
            btnC.FlatAppearance.BorderSize = 0; btnC.Click += (s, e) => { overlay.DialogResult = DialogResult.OK; overlay.Close(); };
            Button btnE = new Button { Text = "ВИЙТИ", Font = new Font("Arial", 11, FontStyle.Bold), BackColor = Color.Crimson, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Size = new Size(160, 45), Location = new Point(260, y + 30), Cursor = Cursors.Hand };
            btnE.FlatAppearance.BorderSize = 0; btnE.Click += (s, e) => { overlay.DialogResult = DialogResult.Cancel; overlay.Close(); };
            overlay.Controls.Add(btnC); overlay.Controls.Add(btnE);
            if (overlay.ShowDialog(this) == DialogResult.Cancel) { Application.Exit(); Environment.Exit(0); }
        }
    }
}