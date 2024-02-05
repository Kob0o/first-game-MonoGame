using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace first_game
{
    public class Map
    {
        private Tile[,] _tiles;
        private int _tileSize = 50;
        private Texture2D _pathTexture;
        private Texture2D _wallTexture;
        public int Rows => _tiles.GetLength(0);
        public int Columns => _tiles.GetLength(1);
        public int TileSize => _tileSize;
        public Point SpawnPosition { get; private set; }
        public Point EndPosition { get; private set; }
        public Point DeathPosition { get; private set; }
        public Map(int rows, int columns, ContentManager content)
        {
            _tiles = new Tile[rows, columns];
            _pathTexture = content.Load<Texture2D>("chemin");
            _wallTexture = content.Load<Texture2D>("mur");
            InitializeTiles(rows, columns);
        }

        private void InitializeTiles(int rows, int columns)
        {
            // Création de notre map
            int[,] layout = {
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0 },
                { 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 4, 0 },
                { 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0 },
                { 0, 0, 0, 1, 1, 4, 0, 1, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 1, 0, 0, 1, 1, 1, 0, 0 },
                { 0, 0, 0, 0, 1, 0, 0, 1, 0, 4, 0, 0 },
                { 0, 0, 4, 1, 1, 1, 1, 1, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 1, 0, 4, 0, 0, 0, 0, 0 },
                { 0, 4, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 1, 1, 1, 1, 1, 4, 0, 0, 0, 0, 0 },
                { 0, 1, 2, 1, 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            };

            // int[,] layout = {
            //     { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            //     { 0, 3, 1, 1, 1, 0, 1, 0, 0, 0, 0, 0 },
            //     { 0, 0, 0, 0, 1, 0, 1, 0, 0, 0, 0, 0 },
            //     { 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0 },
            //     { 0, 0, 0, 4, 0, 0, 1, 0, 0, 1, 0, 0 },
            //     { 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 0, 0 },
            //     { 0, 0, 0, 0, 0, 0, 1, 1, 0, 1, 0, 0 },
            //     { 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 0 },
            //     { 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 1, 0 },
            //     { 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0 },
            //     { 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 4 },
            //     { 0, 0, 0, 0, 0, 0, 0, 1, 2, 1, 0, 0 },
            //     { 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0 },
            //     { 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0 },
            // };

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    _tiles[i, j] = new Tile
                    {
                        Texture = DetermineTexture(layout[i, j]),
                        LayoutValue = layout[i, j]
                    };
                    UpdateSpecialPositions(layout[i, j], i, j);
                }
            }
        }
        // Si la case n'est pas un 0, alors on lui ajoute la texture du chemin
        private Texture2D DetermineTexture(int layoutValue)
        {
            return layoutValue == 0 ? _wallTexture : _pathTexture;
        }

        private void UpdateSpecialPositions(int layoutValue, int i, int j)
        {
            if (layoutValue == 2)
            {
                SpawnPosition = new Point(j * _tileSize, i * _tileSize);
            }
            else if (layoutValue == 3)
            {
                EndPosition = new Point(j * _tileSize, i * _tileSize);
            }
            else if (layoutValue == 4)
            {
                DeathPosition = new Point(j * _tileSize, i * _tileSize);
            }
        }

        public bool IsWall(int row, int column)
        {
            return row < 0 || row >= _tiles.GetLength(0) || column < 0 || column >= _tiles.GetLength(1) || _tiles[row, column].IsWall;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < _tiles.GetLength(0); i++)
            {
                for (int j = 0; j < _tiles.GetLength(1); j++)
                {
                    Rectangle tileRect = new Rectangle(j * _tileSize, i * _tileSize, _tileSize, _tileSize);
                    Color color;
                    if (_tiles[i, j].Texture == _wallTexture)
                    {
                        color = Color.Black;
                    }
                    else
                    {
                        color = Color.White;

                        // Si c'est la case de départ (2)
                        if (_tiles[i, j].LayoutValue == 2)
                        {
                            color = Color.Green;
                        }
                        // Si c'est la case de fin (3)
                        else if (_tiles[i, j].LayoutValue == 3)
                        {
                            color = Color.Red;
                        }
                        // Si c'est la case piège (4)
                        else if (_tiles[i, j].LayoutValue == 4)
                        {
                            color = Color.Brown;
                        }
                    }
                    spriteBatch.FillRectangle(tileRect, color);
                }
            }
        }
        // Méthode pour vérifier si nous sommes sur une case spéciale
        private bool IsSpecialTile(int row, int column, int layoutValue)
        {
            return _tiles[row, column].LayoutValue == layoutValue;
        }
        // Méthode pour vérifier si nous sommes sur la case de fin
        public bool IsEndPoint(int x, int y)
        {
            int tileX = x / _tileSize;
            int tileY = y / _tileSize;
            return IsSpecialTile(tileY, tileX, 3);
        }
        // Méthode pour vérifier si nous sommes sur une case piège
        public bool IsDeathPoint(int x, int y)
        {
            int tileX = x / _tileSize;
            int tileY = y / _tileSize;
            return IsSpecialTile(tileY, tileX, 4);
        }
    }
}
