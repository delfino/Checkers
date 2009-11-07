using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace Checkers
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        private Texture2D BoardImage;
        private Texture2D CheckerImage1, CheckerImage2;
        private Texture2D KingImage1, KingImage2;
        private Texture2D Cursor;
        private int turn;
        private bool multiJump = false;
        private bool gameOver;

        ButtonState LastMouseState = ButtonState.Released, CurrentMouseState = ButtonState.Released;
        int mouseX, mouseY;

        int activePiece = 0;
        int origX = 0;
        int origY = 0;

        private int[,] Board = new int[8,8];

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            ResetGame(Board);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            graphics.PreferredBackBufferWidth = 512;
            graphics.PreferredBackBufferHeight = 512;
            graphics.IsFullScreen = false;
            graphics.ApplyChanges();

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            BoardImage = Content.Load<Texture2D>("Board");
            CheckerImage1 = Content.Load<Texture2D>("Red");
            CheckerImage2 = Content.Load<Texture2D>("Black");
            KingImage1 = Content.Load<Texture2D>("Red_King");
            KingImage2 = Content.Load<Texture2D>("Black_King");
            Cursor = Content.Load<Texture2D>("Mouse");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            //  if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            //   this.Exit();

            // TODO: Add your update logic here
            UpdateMouse();
            base.Update(gameTime);
        }



        protected void UpdateMouse()
        {
            MouseState current_mouse = Mouse.GetState();

            // The mouse x and y positions are returned relative to the
            // upper-left corner of the game window.
            mouseX = current_mouse.X;
            mouseY = current_mouse.Y;

            int x = mouseX / 64, y = mouseY / 64;

            bool mouseOutOfBounds = false;

            if ((x > 7 || x < 0 || y > 7 || y < 0) && activePiece != 0 && activePiece != 99)
            {
                mouseOutOfBounds = true;
                Board[origX, origY] = activePiece;
                activePiece = 0;
                x = 0; y = 0;
            }
            else if (x > 7 || x < 0 || y > 7 || y < 0)
            {
                x = 0; y = 0;
            }

            LastMouseState = CurrentMouseState;
            CurrentMouseState = current_mouse.LeftButton;

            //OnClick
            if (current_mouse.LeftButton.Equals(ButtonState.Pressed) && LastMouseState.Equals(ButtonState.Released) && !mouseOutOfBounds)
            {
                if (Board[x, y] == turn || Board[x,y] == 2*turn)
                {
                    activePiece = Board[x, y];
                    origX = x; origY = y;
                    Board[x, y] = 0;
                }
            }

            //OnRelease
            if (current_mouse.LeftButton.Equals(ButtonState.Released) && LastMouseState.Equals(ButtonState.Pressed) && !mouseOutOfBounds)
            {
                if (isLegalMove(x, y))
                {
                    Move(x, y);
                    turn = -turn;
                }
                else if (activePiece != 0 && activePiece != 99)
                    Board[origX, origY] = activePiece;
                activePiece = 0;
            }
        }

        protected void Move(int x, int y)
        {
            //normal move (non-king), going forward one space diagonally left or right
            //if (origY == y + turn && (origX + 1 == x || origX - 1 == x ))
            //{
                Board[x, y] = activePiece;
                if (Math.Abs(activePiece) == 1 && ((y == 0 && activePiece > 0) || (y == 7 && activePiece < 0)))
                {
                    Board[x, y] *= 2;
                }
            //}

            //normal king move

            //jumping a piece: non-king

            //king jump
        }

        protected bool isLegalMove(int x, int y)
        {
            int dx = x - origX;
            int dy = y - origY;

            //move must be diagonal
            if (dx == 0 || dy == 0) return false;
            if (Math.Abs(dx) != Math.Abs(dx)) return false;

            //cannot move more than 2 spaces diagonally
            if (Math.Abs(dx) > 2 || Math.Abs(dy) > 2) return false;

            //cannot land on occupied square
            if (Board[x, y] != 0) return false;
 
            //must be moving with an actualy piece
            if (Math.Abs(activePiece) != 1 && Math.Abs(activePiece) != 2) return false;

            //non-kings cannot move backwards
            if (Math.Abs(activePiece) != 2  && ( (turn < 0 && dy < 0) || (turn > 0 && dy > 0) )) return false;

            if (Math.Abs(dx) == 2 && (Board[(x + origX) / 2, (y + origY) / 2] == -turn || Board[(x + origX) / 2, (y + origY) / 2] == -turn * 2))
            {
                Board[(x + origX) / 2, (y + origY) / 2] = 0;
            }
            else if (Math.Abs(dx) == 2) return false;

            return true;
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            spriteBatch.Begin();
            spriteBatch.Draw(BoardImage, new Rectangle(0, 0, 512, 512), Color.White);
            for (int i = 0; i < 8; ++i)
            {
                for (int j = 0; j < 8; ++j)
                {
                    if (Board[j,i] == 1)
                        spriteBatch.Draw(CheckerImage1, new Rectangle(j * 64, i * 64, 64, 64), Color.White);
                    else if (Board[j,i] == 2)
                        spriteBatch.Draw(KingImage1, new Rectangle(j * 64, i * 64, 64, 64), Color.White);
                    else if (Board[j,i] == -1)
                        spriteBatch.Draw(CheckerImage2, new Rectangle(j * 64, i * 64, 64, 64), Color.White);
                    else if (Board[j, i] == -2)
                        spriteBatch.Draw(KingImage2, new Rectangle(j * 64, i * 64, 64, 64), Color.White);
                }
            }

            if (activePiece == 0 || activePiece == 99)
                spriteBatch.Draw(Cursor, new Rectangle(mouseX, mouseY, 30, 38), Color.White);
            else if (activePiece == 1)
                spriteBatch.Draw(CheckerImage1, new Rectangle(mouseX - 32, mouseY - 32, 64, 64), Color.White);
            else if (activePiece == -1)
                spriteBatch.Draw(CheckerImage2, new Rectangle(mouseX - 32, mouseY - 32, 64, 64), Color.White);
            else if (activePiece == 2)
                spriteBatch.Draw(KingImage1, new Rectangle(mouseX - 32, mouseY - 32, 64, 64), Color.White);
            else if (activePiece == -2)
                spriteBatch.Draw(KingImage2, new Rectangle(mouseX - 32, mouseY - 32, 64, 64), Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
        }



        protected void ResetGame(int[,] board)
        {
            turn = 1;
            gameOver = false;
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    board[j, i] = 0;
                    if (i % 2 == j % 2)
                        board[j, i] = 99;
                }
            }

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (i % 2 == 0)
                    {
                        board[j * 2 + 1, i] = -1;
                        board[j * 2, i + 5] = 1;
                    }
                    else
                    {
                        board[j * 2, i] = -1;
                        board[j * 2 + 1, i + 5] = 1;
                    }
                }
            }         

        }
    }
}
