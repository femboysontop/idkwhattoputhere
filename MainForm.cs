using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Globalization;
using System.Threading;
using System.IO;
using System.Text.Json;

namespace idkwhattoputhere
{
    public class GameState
    {
        public int ClickCount { get; set; } = 0;
        public int MonsterCount { get; set; } = 0;
        public double PointsPerClick { get; set; } = 1;
        public string CurrentLanguage { get; set; } = "en";
    }

    public enum AppView { Game, Shop, Settings }

    public partial class MainForm : Form
    {
        private Image defaultImage;
        private Image pressedImage;
        private PictureBox pictureBox1;
        private Label counterLabel;
        private Label monsterCountLabel;
        private Button mainUpgradeButton;
        private Button shopButton;
        private Button settingsButton;

        private Panel gamePanel;
        private Panel shopPanel;
        private Panel settingsPanel;
        private TableLayoutPanel shopLayout;
        private TableLayoutPanel settingsLayout;

        private Label shopTitleLabel;
        private Label settingsTitleLabel;
        private Label languageLabel;
        private Button shopBackButton;
        private Button settingsBackButton;

        private Button skirtButton;
        private Button thighHighsButton;
        private Button monsterDrinkButton;


        private GameState gameState;
        private System.Windows.Forms.Timer autoSaveTimer;
        private const string saveFilePath = "gamestate.json";

        private const int skirtCost = 500;
        private const int thighHighsCost = 1000;
        private const int monsterDrinkCost = 1500;
        private const double skirtMultiplier = 1.25;
        private const double thighHighsMultiplier = 1.5;
        private const double monsterDrinkMultiplier = 2.0;

        private Dictionary<string, Dictionary<string, string>> languages;

        public MainForm()
        {
            InitializeComponent();
            InitializeLocalization();
            LoadGameState();

            Thread.CurrentThread.CurrentUICulture = new CultureInfo(gameState.CurrentLanguage);

            try
            {
                defaultImage = Image.FromFile("Assets/default.webp");
                pressedImage = Image.FromFile("Assets/pressed.webp");
            }
            catch (System.IO.FileNotFoundException)
            {
                MessageBox.Show("Image files not found! Please ensure 'default.webp' and 'pressed.webp' are in the 'Assets' folder.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                defaultImage = new Bitmap(100, 100);
                using (Graphics g = Graphics.FromImage(defaultImage)) g.Clear(Color.Gray);
                pressedImage = new Bitmap(100, 100);
                using (Graphics g = Graphics.FromImage(pressedImage)) g.Clear(Color.DimGray);
            }

            this.BackColor = Color.FromArgb(28, 28, 28);
            this.ForeColor = Color.White;
            this.Font = new Font("Segoe UI", 16, FontStyle.Regular);
            this.Size = new Size(400, 600);
            this.MinimumSize = new Size(350, 500);
            this.DoubleBuffered = true;

            InitializeMasterLayout();

            SwitchView(AppView.Game);

            UpdateLanguage();

            autoSaveTimer = new System.Windows.Forms.Timer();
            autoSaveTimer.Interval = 30000;
            autoSaveTimer.Tick += (sender, e) => SaveGameState();
            autoSaveTimer.Start();

            this.FormClosing += (sender, e) => SaveGameState();
        }

        private void InitializeMasterLayout()
        {
            this.Controls.Clear();

            var masterLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Color.FromArgb(28, 28, 28)
            };
            masterLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            masterLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            InitializeGamePanel();
            InitializeShopPanel();
            InitializeSettingsPanel();

            masterLayout.Controls.Add(gamePanel, 0, 1);
            masterLayout.Controls.Add(shopPanel, 0, 1);
            masterLayout.Controls.Add(settingsPanel, 0, 1);

            var navPanel = InitializeNavigationButtons();
            masterLayout.Controls.Add(navPanel, 0, 0);

            this.Controls.Add(masterLayout);
        }

        private void LoadGameState()
        {
            if (File.Exists(saveFilePath))
            {
                try
                {
                    string jsonString = File.ReadAllText(saveFilePath);
                    gameState = JsonSerializer.Deserialize<GameState>(jsonString) ?? new GameState();
                }
                catch
                {
                    gameState = new GameState();
                }
            }
            else
            {
                gameState = new GameState();
            }
        }

        private void SaveGameState()
        {
            try
            {
                string jsonString = JsonSerializer.Serialize(gameState);
                File.WriteAllText(saveFilePath, jsonString);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving game: {ex.Message}", "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeLocalization()
        {
            languages = new Dictionary<string, Dictionary<string, string>>
            {
                ["en"] = new Dictionary<string, string> { ["title"] = "Femboy Clicker", ["points"] = "Points:", ["monsters"] = "Monsters:", ["upgradeMonster"] = "Buy Monster", ["shop"] = "Shop", ["settings"] = "Settings", ["language"] = "Language:", ["skirt"] = "Skirt", ["thighHighs"] = "Thigh-Highs", ["monsterDrink"] = "Monster Energy", ["cost"] = "Cost:", ["notEnoughPoints"] = "You don't have enough points!", ["back"] = "Back", ["boughtItem"] = "You bought an item! Your point multiplier is now" },
                ["hu"] = new Dictionary<string, string> { ["title"] = "Femboy Klikkelő", ["points"] = "Pontok:", ["monsters"] = "Szörnyek:", ["upgradeMonster"] = "Vásárolj szörnyet", ["shop"] = "Bolt", ["settings"] = "Beállítások", ["language"] = "Nyelv:", ["skirt"] = "Szoknya", ["thighHighs"] = "Combzokni", ["monsterDrink"] = "Monster Energy", ["cost"] = "Ár:", ["notEnoughPoints"] = "Nincs elég pontod!", ["back"] = "Vissza", ["boughtItem"] = "Vettél egy tárgyat! A pontszorzód most" },
                ["es"] = new Dictionary<string, string> { ["title"] = "Clicker Femboy", ["points"] = "Puntos:", ["monsters"] = "Monstruos:", ["upgradeMonster"] = "Comprar Monstruo", ["shop"] = "Tienda", ["settings"] = "Configuración", ["language"] = "Idioma:", ["skirt"] = "Falda", ["thighHighs"] = "Medias altas", ["monsterDrink"] = "Monster Energy", ["cost"] = "Costo:", ["notEnoughPoints"] = "¡No tienes suficientes puntos!", ["back"] = "Atrás", ["boughtItem"] = "¡Compraste un artículo! Tu multiplicador de puntos es ahora" },
                ["fr"] = new Dictionary<string, string> { ["title"] = "Clicker Femboy", ["points"] = "Points:", ["monsters"] = "Monstres:", ["upgradeMonster"] = "Acheter un monstre", ["shop"] = "Boutique", ["settings"] = "Paramètres", ["language"] = "Langue:", ["skirt"] = "Jupe", ["thighHighs"] = "Chaussettes hautes", ["monsterDrink"] = "Monster Energy", ["cost"] = "Coût:", ["notEnoughPoints"] = "Vous n'avez pas assez de points!", ["back"] = "Retour", ["boughtItem"] = "Vous avez acheté un article! Votre multiplicateur de points est maintenant" },
                ["de"] = new Dictionary<string, string> { ["title"] = "Femboy Klicker", ["points"] = "Punkte:", ["monsters"] = "Monster:", ["upgradeMonster"] = "Monster kaufen", ["shop"] = "Laden", ["settings"] = "Einstellungen", ["language"] = "Sprache:", ["skirt"] = "Rock", ["thighHighs"] = "Overknee-Strümpfe", ["monsterDrink"] = "Monster Energy", ["cost"] = "Kosten:", ["notEnoughPoints"] = "Du hast nicht genug Punkte!", ["back"] = "Zurück", ["boughtItem"] = "Du hast einen Gegenstand gekauft! Dein Punktemultiplikator ist jetzt" }
            };
        }

        private void InitializeGamePanel()
        {
            gamePanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(28, 28, 28) };
            var mainLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 80F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var topLabelsPanel = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
            topLabelsPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 60F));
            topLabelsPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));
            counterLabel = new Label { Dock = DockStyle.Fill, TextAlign = ContentAlignment.BottomCenter, Font = new Font("Segoe UI", 24, FontStyle.Bold), ForeColor = Color.Gold, BackColor = Color.Transparent };
            monsterCountLabel = new Label { Dock = DockStyle.Fill, TextAlign = ContentAlignment.TopCenter, Font = new Font("Segoe UI", 12, FontStyle.Italic), ForeColor = Color.White, BackColor = Color.Transparent, Padding = new Padding(0, 5, 0, 0) };
            topLabelsPanel.Controls.Add(counterLabel, 0, 0);
            topLabelsPanel.Controls.Add(monsterCountLabel, 0, 1);
            mainLayout.Controls.Add(topLabelsPanel, 0, 0);

            pictureBox1 = new PictureBox { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom, Image = defaultImage, Cursor = Cursors.Hand };
            pictureBox1.MouseDown += PictureBox1_MouseDown;
            pictureBox1.MouseUp += PictureBox1_MouseUp;
            mainLayout.Controls.Add(pictureBox1, 0, 1);

            mainUpgradeButton = CreateStyledButton(string.Empty);
            mainUpgradeButton.Dock = DockStyle.Fill;
            mainUpgradeButton.Click += UpgradeMonster_Click;
            mainLayout.Controls.Add(mainUpgradeButton, 0, 2);

            gamePanel.Controls.Add(mainLayout);
        }

        private void InitializeShopPanel()
        {
            shopPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(28, 28, 28), Visible = false };
            var mainLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            shopTitleLabel = CreateCenteredTitleLabel(string.Empty);
            mainLayout.Controls.Add(shopTitleLabel, 0, 0);

            shopLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1, Padding = new Padding(20) };
            shopLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 33));
            shopLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 33));
            shopLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 34));

            skirtButton = CreateShopItemButton("skirtButton", (sender, e) => BuyShopItem(skirtCost, skirtMultiplier));
            thighHighsButton = CreateShopItemButton("thighHighsButton", (sender, e) => BuyShopItem(thighHighsCost, thighHighsMultiplier));
            monsterDrinkButton = CreateShopItemButton("monsterDrinkButton", (sender, e) => BuyShopItem(monsterDrinkCost, monsterDrinkMultiplier));

            shopLayout.Controls.Add(skirtButton, 0, 0);
            shopLayout.Controls.Add(thighHighsButton, 0, 1);
            shopLayout.Controls.Add(monsterDrinkButton, 0, 2);
            mainLayout.Controls.Add(shopLayout, 0, 1);

            shopBackButton = CreateStyledButton(string.Empty);
            shopBackButton.Dock = DockStyle.Fill;
            shopBackButton.Click += (sender, e) => SwitchView(AppView.Game);
            mainLayout.Controls.Add(shopBackButton, 0, 2);

            shopPanel.Controls.Add(mainLayout);
        }

        private void InitializeSettingsPanel()
        {
            settingsPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(28, 28, 28), Visible = false };
            var mainLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4 };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            settingsTitleLabel = CreateCenteredTitleLabel(string.Empty);
            mainLayout.Controls.Add(settingsTitleLabel, 0, 0);

            languageLabel = new Label { Dock = DockStyle.Top, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 18, FontStyle.Bold), Padding = new Padding(10), Height = 60 };
            mainLayout.Controls.Add(languageLabel, 0, 1);

            settingsLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 5, ColumnCount = 1, Padding = new Padding(20) };
            for (int i = 0; i < 5; i++) { settingsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 20)); }
            settingsLayout.Controls.Add(CreateLanguageButton("en", "English"), 0, 0);
            settingsLayout.Controls.Add(CreateLanguageButton("hu", "Magyar"), 0, 1);
            settingsLayout.Controls.Add(CreateLanguageButton("es", "Español"), 0, 2);
            settingsLayout.Controls.Add(CreateLanguageButton("fr", "Français"), 0, 3);
            settingsLayout.Controls.Add(CreateLanguageButton("de", "Deutsch"), 0, 4);
            mainLayout.Controls.Add(settingsLayout, 0, 2);

            settingsBackButton = CreateStyledButton(string.Empty);
            settingsBackButton.Dock = DockStyle.Fill;
            settingsBackButton.Click += (sender, e) => SwitchView(AppView.Game);
            mainLayout.Controls.Add(settingsBackButton, 0, 3);

            settingsPanel.Controls.Add(mainLayout);
        }

        private FlowLayoutPanel InitializeNavigationButtons()
        {
            var navPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(45, 45, 48),
                Padding = new Padding(0, 5, 0, 5),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = false,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };

            shopButton = CreateStyledButton(string.Empty);
            shopButton.Click += (sender, e) => SwitchView(AppView.Shop);
            shopButton.AutoSize = true;
            shopButton.Anchor = AnchorStyles.None;
            navPanel.Controls.Add(shopButton);

            settingsButton = CreateStyledButton(string.Empty);
            settingsButton.Click += (sender, e) => SwitchView(AppView.Settings);
            settingsButton.AutoSize = true;
            settingsButton.Anchor = AnchorStyles.None;
            navPanel.Controls.Add(settingsButton);

            var leftSpacer = new Panel { Width = 1, Dock = DockStyle.Left };
            var rightSpacer = new Panel { Width = 1, Dock = DockStyle.Right };
            navPanel.Controls.Add(leftSpacer);
            navPanel.Controls.Add(rightSpacer);

            navPanel.Layout += (s, e) =>
            {
                int totalButtonWidth = 0;
                foreach (Control c in navPanel.Controls)
                    if (c is Button) totalButtonWidth += c.Width + c.Margin.Horizontal;
                navPanel.Padding = new Padding((navPanel.Width - totalButtonWidth) / 2, 5, 0, 5);
            };

            return navPanel;
        }

        private void SwitchView(AppView view)
        {
            gamePanel.Visible = view == AppView.Game;
            shopPanel.Visible = view == AppView.Shop;
            settingsPanel.Visible = view == AppView.Settings;
        }

        private Button CreateStyledButton(string text)
        {
            var button = new Button { Text = text, Height = 50, Font = new Font("Segoe UI", 14, FontStyle.Bold), BackColor = Color.FromArgb(63, 63, 70), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Margin = new Padding(5), Cursor = Cursors.Hand };
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.BorderColor = Color.FromArgb(80, 80, 80);
            button.MouseEnter += (sender, e) => button.BackColor = Color.FromArgb(80, 80, 90);
            button.MouseLeave += (sender, e) => button.BackColor = Color.FromArgb(63, 63, 70);
            return button;
        }

        private Label CreateCenteredTitleLabel(string text)
        {
            return new Label { Text = text, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 24, FontStyle.Bold), ForeColor = Color.White, BackColor = Color.FromArgb(45, 45, 48), Height = 60 };
        }

        private Button CreateShopItemButton(string buttonName, EventHandler clickHandler)
        {
            var button = CreateStyledButton(string.Empty);
            button.Name = buttonName;
            button.Dock = DockStyle.Fill;
            button.Click += clickHandler;
            return button;
        }

        private Button CreateLanguageButton(string langCode, string langName)
        {
            var button = CreateStyledButton(langName);
            button.Dock = DockStyle.Fill;
            button.Click += (sender, e) =>
            {
                gameState.CurrentLanguage = langCode;
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(langCode);
                UpdateLanguage();
            };
            return button;
        }

        private void UpgradeMonster_Click(object sender, EventArgs e)
        {
            if (gameState.ClickCount >= 300)
            {
                gameState.ClickCount -= 300;
                gameState.MonsterCount++;
                gameState.PointsPerClick *= 1.2;
                UpdateGameDisplay();
                MessageBox.Show($"{languages[gameState.CurrentLanguage]["boughtItem"]} {gameState.PointsPerClick:0.00}!");
            }
            else
            {
                MessageBox.Show(languages[gameState.CurrentLanguage]["notEnoughPoints"]);
            }
        }

        private void BuyShopItem(int cost, double multiplier)
        {
            if (gameState.ClickCount >= cost)
            {
                gameState.ClickCount -= cost;
                gameState.PointsPerClick *= multiplier;
                UpdateGameDisplay();
                MessageBox.Show($"{languages[gameState.CurrentLanguage]["boughtItem"]} {gameState.PointsPerClick:0.00}!");
            }
            else
            {
                MessageBox.Show(languages[gameState.CurrentLanguage]["notEnoughPoints"]);
            }
        }

        private void PictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            pictureBox1.Image = pressedImage;
            int gain = (int)Math.Ceiling(gameState.PointsPerClick);
            gameState.ClickCount += gain;
            UpdateGameDisplay();
        }

        private void PictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            pictureBox1.Image = defaultImage;
        }

        private void UpdateLanguage()
        {
            var culture = gameState.CurrentLanguage;
            var translations = languages[culture];

            this.Text = translations["title"];
            shopButton.Text = translations["shop"];
            settingsButton.Text = translations["settings"];

            UpdateGameDisplay();

            shopTitleLabel.Text = translations["shop"];
            shopBackButton.Text = translations["back"];
            skirtButton.Text = $"{translations["skirt"]} ({translations["cost"]} {skirtCost})";
            thighHighsButton.Text = $"{translations["thighHighs"]} ({translations["cost"]} {thighHighsCost})";
            monsterDrinkButton.Text = $"{translations["monsterDrink"]} ({translations["cost"]} {monsterDrinkCost})";

            settingsTitleLabel.Text = translations["settings"];
            languageLabel.Text = translations["language"];
            settingsBackButton.Text = translations["back"];
        }

        private void UpdateGameDisplay()
        {
            var translations = languages[gameState.CurrentLanguage];
            counterLabel.Text = $"{translations["points"]} {gameState.ClickCount}";
            monsterCountLabel.Text = $"{translations["monsters"]} {gameState.MonsterCount}";
            mainUpgradeButton.Text = $"{translations["upgradeMonster"]} (300)";
        }
    }
}