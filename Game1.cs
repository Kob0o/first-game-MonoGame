// Game1.cs
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using Newtonsoft.Json;

namespace first_game
{
    public class Game1 : Game
    {
        private Rectangle _player;
        private Texture2D _playerTexture;
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Map _gameMap;
        private GameState _currentState;
        private SpriteFont _gameFont;
        private TimeSpan _gameTime;
        private float _currentScore;
        private float _bestScore;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            // Mise en place de la résolution de la fenêtre par rapport à la taille de notre map
            _graphics.PreferredBackBufferWidth = 12 * 50;
            _graphics.PreferredBackBufferHeight = 14 * 50;
        }

        // Création des différents étatas de notre jeu
        public enum GameState
        {
            MainMenu,
            Playing,
            GameOver,
            WellPlayed,
            Pause,
        }

        protected override void Initialize()
        {
            _currentState = GameState.MainMenu;
            LoadBestScore();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _gameFont = Content.Load<SpriteFont>("GameFont");
            _playerTexture = new Texture2D(GraphicsDevice, 1, 1);
            _playerTexture.SetData(new[] { Color.Red });
            _gameMap = new Map(14, 12, Content);
            SpriteBatchExtensions.Initialize(GraphicsDevice);
            // Initialise la position du joueur au début du chemin
            SetPlayerStartPosition();
        }

        protected override void Update(GameTime gameTime)
        {
            switch (_currentState)
            {
                case GameState.MainMenu:
                    if (Keyboard.GetState().IsKeyDown(Keys.Enter)) // Commencer une nouvelle partie
                    {
                        _currentState = GameState.Playing;
                        SetPlayerStartPosition();
                    }
                    if (Keyboard.GetState().IsKeyDown(Keys.L)) // Charger la dernière partie
                    {
                        _currentState = GameState.Playing;
                        LoadGame();
                    }
                    if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                        Exit();
                    break;

                case GameState.Playing:
                    // Mettre en pause
                    if (Keyboard.GetState().IsKeyDown(Keys.P))
                    {
                        _currentState = GameState.Pause;
                    }
                    else
                    {
                        MovePlayer();
                        _gameTime += gameTime.ElapsedGameTime;
                        //Si le joueur atteint la case de fin changement de fenêtre et affichage des scores
                        if (_gameMap.IsEndPoint(_player.X, _player.Y))
                        {
                            _currentState = GameState.WellPlayed;
                            _currentScore = (float)_gameTime.TotalSeconds;
                            if (_currentScore < _bestScore || _bestScore == 0)
                            {
                                _bestScore = _currentScore;
                                SaveBestScore();
                            }
                        }
                        if (_gameMap.IsDeathPoint(_player.X, _player.Y))
                        {
                            _currentState = GameState.GameOver;
                        }
                    }
                    break;

                case GameState.Pause:
                    if (Keyboard.GetState().IsKeyDown(Keys.R)) // Reprendre
                    {
                        _currentState = GameState.Playing;
                        LoadGame(); // Chargement de l'état sauvegardé du jeu
                    }
                    if (Keyboard.GetState().IsKeyDown(Keys.S)) // Sauvegarder
                    {
                        SaveGame();
                    }
                    if (Keyboard.GetState().IsKeyDown(Keys.Escape)) // Quitter
                    {
                        Exit();
                    }
                    if (Keyboard.GetState().IsKeyDown(Keys.Enter)) // Recommencer
                    {
                        _currentState = GameState.Playing;
                        SetPlayerStartPosition();
                    }
                    if (Keyboard.GetState().IsKeyDown(Keys.M)) // Menu principal
                    {
                        _currentState = GameState.MainMenu;
                    }
                    break;

                case GameState.WellPlayed:
                    if (Keyboard.GetState().IsKeyDown(Keys.M))
                    {
                        _currentState = GameState.MainMenu; // Menu principal
                    }
                    break;

                case GameState.GameOver:
                    if (Keyboard.GetState().IsKeyDown(Keys.M))
                    {
                        _currentState = GameState.MainMenu; // Menu principal
                    }
                    break;

            }
            base.Update(gameTime);
        }
        private void SetPlayerStartPosition()
        {
            _gameTime = TimeSpan.Zero; // Réinitialiser le temps de jeu
            _player = new Rectangle(_gameMap.SpawnPosition.X, _gameMap.SpawnPosition.Y, 30, 30);
        }

        private void MovePlayer()
        {
            KeyboardState keyboardState = Keyboard.GetState();
            // Déplacement du joueur seulement si la case suivante n'est pas un mur
            if (keyboardState.IsKeyDown(Keys.Left) && !IsCollisionWithWall(_player.X - 3, _player.Y))
                _player.X -= 2;
            else if (keyboardState.IsKeyDown(Keys.Right) && !IsCollisionWithWall(_player.X + 3, _player.Y))
                _player.X += 2;
            else if (keyboardState.IsKeyDown(Keys.Up) && !IsCollisionWithWall(_player.X, _player.Y - 3))
                _player.Y -= 2;
            else if (keyboardState.IsKeyDown(Keys.Down) && !IsCollisionWithWall(_player.X, _player.Y + 3))
                _player.Y += 2;
        }

        private bool IsCollisionWithWall(int x, int y)
        {
            int playerTileX = x / _gameMap.TileSize;
            int playerTileY = y / _gameMap.TileSize;

            return _gameMap.IsWall(playerTileY, playerTileX);
        }

        private void SaveBestScore() // Sauvegarde du meilleur score
        {
            System.IO.File.WriteAllText("bestscore.txt", _bestScore.ToString());
        }

        private void LoadBestScore() // Chargement du meilleur score
        {
            if (System.IO.File.Exists("bestscore.txt"))
            {
                float.TryParse(System.IO.File.ReadAllText("bestscore.txt"), out _bestScore);
            }
        }
        private void SaveGame() // Sauvegarde de la partie
        {
            var gameState = new
            {
                PlayerPosition = new { X = _player.X, Y = _player.Y },
                GameTime = _gameTime.TotalSeconds
            };
            var gameStateString = JsonConvert.SerializeObject(gameState);
            System.IO.File.WriteAllText("gamestate.txt", gameStateString);
        }

        private void LoadGame() // Chargement de la partie
        {
            if (System.IO.File.Exists("gamestate.txt"))
            {
                var gameStateString = System.IO.File.ReadAllText("gamestate.txt");
                var gameState = JsonConvert.DeserializeObject<dynamic>(gameStateString);
                _player = new Rectangle((int)gameState.PlayerPosition.X, (int)gameState.PlayerPosition.Y, _player.Width, _player.Height);
                _gameTime = TimeSpan.FromSeconds((double)gameState.GameTime);
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();

            switch (_currentState)
            {
                case GameState.MainMenu:
                    string menuTitle = "Menu Principal";
                    string newGameOption = "Lancer une nouvelle partie : Appuyez sur Entrer";
                    string loadGameOption = "Reprendre la dernière partie : Appuyez sur L";
                    string bestScoreMessage0 = $"Meilleur score : {_bestScore:F2} secondes";
                    string quitGameOption = "Quitter : Appuyez sur Echap";


                    Vector2 titlePosition = new Vector2((GraphicsDevice.Viewport.Width - _gameFont.MeasureString(menuTitle).X) / 2, 100);
                    Vector2 newGamePosition = new Vector2((GraphicsDevice.Viewport.Width - _gameFont.MeasureString(newGameOption).X) / 2, 150);
                    Vector2 loadGamePosition = new Vector2((GraphicsDevice.Viewport.Width - _gameFont.MeasureString(loadGameOption).X) / 2, 200);
                    Vector2 bestScorePosition0 = new Vector2((GraphicsDevice.Viewport.Width - _gameFont.MeasureString(bestScoreMessage0).X) / 2, 250);
                    Vector2 quitGamePosition = new Vector2((GraphicsDevice.Viewport.Width - _gameFont.MeasureString(quitGameOption).X) / 2, 300);

                    _spriteBatch.DrawString(_gameFont, menuTitle, titlePosition, Color.White);
                    _spriteBatch.DrawString(_gameFont, newGameOption, newGamePosition, Color.White);
                    _spriteBatch.DrawString(_gameFont, loadGameOption, loadGamePosition, Color.White);
                    _spriteBatch.DrawString(_gameFont, bestScoreMessage0, bestScorePosition0, Color.White);
                    _spriteBatch.DrawString(_gameFont, quitGameOption, quitGamePosition, Color.White);
                    break;

                case GameState.WellPlayed:
                    string wellplayedMessage = "Félicitations, vous avez gagné !";
                    string startNewGameMessage0 = "Appuyez sur M pour retourner au menu principal";
                    string scoreMessage = $"Votre score : {_currentScore:F2} secondes";
                    string bestScoreMessage = $"Meilleur score : {_bestScore:F2} secondes";

                    Vector2 wellPlayedPosition = new Vector2((GraphicsDevice.Viewport.Width - _gameFont.MeasureString(wellplayedMessage).X) / 2, 100);
                    Vector2 startNewGamePosition0 = new Vector2((GraphicsDevice.Viewport.Width - _gameFont.MeasureString(startNewGameMessage0).X) / 2, 150);
                    Vector2 scorePosition = new Vector2((GraphicsDevice.Viewport.Width - _gameFont.MeasureString(scoreMessage).X) / 2, 200);
                    Vector2 bestScorePosition = new Vector2((GraphicsDevice.Viewport.Width - _gameFont.MeasureString(bestScoreMessage).X) / 2, 250);

                    _spriteBatch.DrawString(_gameFont, wellplayedMessage, wellPlayedPosition, Color.White);
                    _spriteBatch.DrawString(_gameFont, startNewGameMessage0, startNewGamePosition0, Color.White);
                    _spriteBatch.DrawString(_gameFont, scoreMessage, scorePosition, Color.White);
                    _spriteBatch.DrawString(_gameFont, bestScoreMessage, bestScorePosition, Color.White);
                    break;

                case GameState.GameOver:
                    string gameOverMessage = "Perdu ! Vous avez touché une mauvaise case !";
                    string startNewGameMessage = "Appuyez sur M pour retourner au menu principal";

                    Vector2 gameOverPosition = new Vector2((GraphicsDevice.Viewport.Width - _gameFont.MeasureString(gameOverMessage).X) / 2, 100);
                    Vector2 startNewGamePosition = new Vector2((GraphicsDevice.Viewport.Width - _gameFont.MeasureString(startNewGameMessage).X) / 2, 150);

                    _spriteBatch.DrawString(_gameFont, gameOverMessage, gameOverPosition, Color.White);
                    _spriteBatch.DrawString(_gameFont, startNewGameMessage, startNewGamePosition, Color.White);
                    break;

                case GameState.Playing:
                    _gameMap.Draw(_spriteBatch);
                    _spriteBatch.Draw(_playerTexture, _player, Color.White);
                    break;

                case GameState.Pause:
                    string pauseMessage = "Pause";
                    string resumeMessage = "Appuyez sur R pour reprendre";
                    string saveMessage = "Appuyez sur S pour sauvegarder";
                    string quitMessage = "Appuyez sur Echap pour quitter";
                    string restartMessage = "Appuyez sur Entrer pour recommencer";
                    string menuMessage = "Appuyez sur M pour retourner au menu principal";

                    Vector2 pausePosition = new Vector2((GraphicsDevice.Viewport.Width - _gameFont.MeasureString(pauseMessage).X) / 2, 100);
                    Vector2 resumePosition = new Vector2((GraphicsDevice.Viewport.Width - _gameFont.MeasureString(resumeMessage).X) / 2, 150);
                    Vector2 savePosition = new Vector2((GraphicsDevice.Viewport.Width - _gameFont.MeasureString(saveMessage).X) / 2, 200);
                    Vector2 quitPosition = new Vector2((GraphicsDevice.Viewport.Width - _gameFont.MeasureString(quitMessage).X) / 2, 250);
                    Vector2 restartPosition = new Vector2((GraphicsDevice.Viewport.Width - _gameFont.MeasureString(restartMessage).X) / 2, 300);
                    Vector2 menuPosition = new Vector2((GraphicsDevice.Viewport.Width - _gameFont.MeasureString(menuMessage).X) / 2, 350);

                    _spriteBatch.DrawString(_gameFont, pauseMessage, pausePosition, Color.White);
                    _spriteBatch.DrawString(_gameFont, resumeMessage, resumePosition, Color.White);
                    _spriteBatch.DrawString(_gameFont, saveMessage, savePosition, Color.White);
                    _spriteBatch.DrawString(_gameFont, quitMessage, quitPosition, Color.White);
                    _spriteBatch.DrawString(_gameFont, restartMessage, restartPosition, Color.White);
                    _spriteBatch.DrawString(_gameFont, menuMessage, menuPosition, Color.White);
                    break;
            }
            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
