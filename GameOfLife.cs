using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameOfLife;

public class GameOfLife : Game
{
	private const int RECT_SIZE = 14;

	private bool isPlaying;
	private HashSet<Vector2> aliveCells;
	private int updateTimeMs = 250;
	private KeyboardState prevKeebState;
	private MouseState prevMouseState;
	private readonly GraphicsDeviceManager graphics;
	private SpriteBatch spriteBatch;
	private SpriteFont font;
	private Texture2D pixel;
	private TimeSpan lastUpdateTime;

	public GameOfLife()
	{
		Content.RootDirectory = "Content";
		graphics = new GraphicsDeviceManager(this);
		IsMouseVisible = true;
		Window.AllowUserResizing = true;
	}

	protected override void Initialize()
	{
		base.Initialize();
	}

	protected override void LoadContent()
	{
		aliveCells = [];
		font = Content.Load<SpriteFont>("Fonts/Consolas");
		isPlaying = false;
		pixel = new Texture2D(GraphicsDevice, 1, 1);
		pixel.SetData([Color.White]);
		prevKeebState = Keyboard.GetState();
		prevMouseState = Mouse.GetState();
		spriteBatch = new SpriteBatch(GraphicsDevice);
	}

	protected override void Update(GameTime gameTime)
	{
		base.Update(gameTime);

		if (!IsActive) return;

		var mouseState = Mouse.GetState();
		if (!prevMouseState.Equals(mouseState))
		{
			var mouseX = mouseState.Position.X / RECT_SIZE;
			var mouseY = mouseState.Position.Y / RECT_SIZE;

			if (mouseState.LeftButton == ButtonState.Pressed)
			{
				aliveCells.Add(new Vector2(mouseX, mouseY));
			}
			if (mouseState.RightButton == ButtonState.Pressed)
			{
				aliveCells.Remove(new Vector2(mouseX, mouseY));
			}
		}

		var keebState = Keyboard.GetState();
		if (!prevKeebState.Equals(keebState))
		{
			if (keebState.IsKeyDown(Keys.Space)) isPlaying = !isPlaying;
			if (keebState.IsKeyDown(Keys.F)) updateTimeMs = Math.Max(updateTimeMs - 50, 0);
			if (keebState.IsKeyDown(Keys.S)) updateTimeMs = Math.Min(updateTimeMs + 50, 1000);
			if (keebState.IsKeyDown(Keys.Delete)) aliveCells.Clear();
			if (keebState.IsKeyDown(Keys.Escape)) Exit();
		}

		if (isPlaying)
		{
			if (gameTime.TotalGameTime.Subtract(lastUpdateTime).TotalMilliseconds >= updateTimeMs)
			{
				RunSimulation();
				lastUpdateTime = gameTime.TotalGameTime;
			}
		}

		prevMouseState = mouseState;
		prevKeebState = keebState;
	}

	private void RunSimulation()
	{
		var nextState = new HashSet<Vector2>();
		var potentialCells = new Dictionary<Vector2, int>();

		foreach (var cell in aliveCells)
		{
			for (var row = -1; row <= 1; row++)
			{
				for (var col = -1; col <= 1; col++)
				{
					if (row == 0 && col == 0) continue;
					var neighbor = new Vector2(cell.X + col, cell.Y + row);

					if (!potentialCells.ContainsKey(neighbor)) potentialCells[neighbor] = 0;
					potentialCells[neighbor]++;
				}

			}
		}

		foreach (var potentialCell in potentialCells)
		{
			var cell = potentialCell.Key;
			var count = potentialCell.Value;

			if (aliveCells.Contains(cell) && (count == 2 || count == 3)) nextState.Add(cell);
			else if (count == 3) nextState.Add(cell);
		}

		aliveCells = [.. nextState.Where(IsOnScreen)];
	}

	private bool IsOnScreen(Vector2 cell)
	{
		var left = cell.X * RECT_SIZE;
		var right = left + RECT_SIZE;

		var top = cell.Y * RECT_SIZE;
		var bottom = top + RECT_SIZE;

		var viewport = GraphicsDevice.Viewport;

		return left > 0 && right < viewport.Width && top > 0 && bottom < viewport.Height;
	}

	protected override void Draw(GameTime gameTime)
	{
		GraphicsDevice.Clear(Color.DarkSlateGray);

		spriteBatch.Begin();

		foreach (var cell in aliveCells)
		{
			DrawRectangle((int)cell.X, (int)cell.Y);
		}

		spriteBatch.DrawString(font, isPlaying ? "Playing" : "Paused", new Vector2(10, 10), Color.GreenYellow);
		spriteBatch.DrawString(font, $"Speed: {updateTimeMs}", new Vector2(10, 22), Color.GreenYellow);

		spriteBatch.End();

		base.Draw(gameTime);
	}

	private void DrawRectangle(int x, int y)
	{
		var rectX = x * RECT_SIZE;
		var rectY = y * RECT_SIZE;
		spriteBatch.Draw(pixel, new Rectangle(rectX, rectY, RECT_SIZE, RECT_SIZE), Color.White);
	}
}