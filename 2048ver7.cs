using Cairo;
using Gdk;
using Gtk;
using System;
using static System.Console;
using static System.Math;

using Color = Gdk.Color;
using Rectangle = Cairo.Rectangle;

using System.IO;
using System.Collections.Generic;

public class Tile
{
    public Tile(int value, int row, int col)
    {
        Value = value;
        Row = row;
        Col = col;
        Animating = false;
        ResetShift();
    }

    public int Value { get; set; }

    public int Row { get; set; }

    public int Col { get; set; }

    public double ShiftX { get; set; }

    public double ShiftY { get; set; }

    public bool Animating { get; set; }

    public void SetPosition(int row, int col)
    {
        Row = row;
        Col = col;
    }

    public void ResetShift()
    {
        ShiftX = 0;
        ShiftY = 0;
    }

    public int Merging()
    {
        return (Value += Value);
    }

    public bool HasMoved(int row, int col)
    {
        if (!IsEmpty() && ((Row != row) || (Col != col)))
        {
            Animating = true;
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool IsEmpty()
    {
        return (Value == 0);
    }

}
public class Board
{
    public Board(int size)
    {
        Size = size;
        Score = 0;
        EmptyTiles = Size * Size;
        InitTiles = 2;
        GameOver = false;
        GenNewTile = false;
        Tiles = new List<List<Tile>>();

        Start();
    }

    private readonly string FILEPATH = @"highest-score.txt";

    public int Size { get; set; }                       
    public int Score { get; set; }                      
    public int HighestScore { get; set; }               
    public int EmptyTiles { get; set; }                 
    public int InitTiles { get; set; }                  
    public bool GameOver { get; set; }                  
    public string WonOrLost { get; set; }               
    public bool GenNewTile { get; set; }                
    public List<List<Tile>> Tiles { get; set; }         

    private void Initialize()
    {
        //WriteLine("Hello World!");
        for (int row = 0; row < Size; row++)
        {
            Tiles.Add(new List<Tile>());
            for (int col = 0; col < Size; col++)
            {
                Tiles[row].Add(new Tile(0, row, col));
            }
        }

        if (!File.Exists(FILEPATH))
        {
            HighestScore = 0;
            File.WriteAllText(FILEPATH, "0");
        }
        else
        {
            HighestScore = Convert.ToInt32(File.ReadAllText(FILEPATH));
        }

    }

    private void Start()
    {
        Initialize();
        GenInitTiles();
    }

    public Tile GetTileAt(int row, int col)
    {
        return Tiles[row][col];
    }

    public void SetTileAt(int row, int col, Tile x)
    {
        Tiles[row][col] = x;
    }

    public void updateScore(int addition)
    {
        Score += addition;
        if (Score > HighestScore)
        {
            HighestScore = Score;
            File.WriteAllText(FILEPATH, Convert.ToString(HighestScore));
        }
    }

    private List<Tile> MergeRight(List<Tile> nums)
    {
        for (int l = 0; l < nums.Count - 1; l++)
        {
            if (nums[l].Value == nums[l + 1].Value)
            {
                int value;
                if ((value = nums[l].Merging()) ==2048)
                {
                    GameOver = true;
                }
                updateScore(value);
                nums.RemoveAt(l + 1);
                GenNewTile = true; // state change
                EmptyTiles++;
            }
        }
        return nums;
    }

    private List<Tile> MergeLeft(List<Tile> nums)
    {
        for (int l = nums.Count - 1; l > 0; l--)
        {
            if (nums[l].Value == nums[l - 1].Value)
            {
                int value;
                if ((value = nums[l].Merging()) == 2048)
                {
                    GameOver = true;
                }
                updateScore(value);
                nums.RemoveAt(l - 1);
                GenNewTile = true; //  state change
                EmptyTiles++;
            }
        }
        return nums;
    }

    private List<Tile> AddEmptyTilesFirst(List<Tile> merged)
    {
        for (int k = merged.Count; k < Size; k++)
        {
            merged.Insert(0, new Tile(0, 0, 0));
        }
        return merged;
    }

    private List<Tile> AddEmptyTilesLast(List<Tile> merged)
    { 
        for (int k = merged.Count; k < Size; k++)
        {
            merged.Insert(k, new Tile(0, 0, 0));
        }
        return merged;
    }

    private List<Tile> RemoveEmptyTilesRows(int row)
    {

        List<Tile> moved = new List<Tile>();

        for (int col = 0; col < Size; col++)
        {
            if (!GetTileAt(row, col).IsEmpty())
            { 
                moved.Add(GetTileAt(row, col));
            }
        }

        return moved;
    }

    private List<Tile> RemoveEmptyTilesCols(int row)
    {

        List<Tile> moved = new List<Tile>();

        for (int col = 0; col < Size; col++)
        {
            if (!GetTileAt(col, row).IsEmpty())
            { 
                moved.Add(GetTileAt(col, row));
            }
        }

        return moved;
    }

    private List<Tile> setRow(List<Tile> moved, int row)
    {
        for (int col = 0; col < Tiles.Count; col++)
        {
            if (moved[col].HasMoved(row, col))
            {
                GenNewTile = true;
                Grid.TilesAnimationsDone--;
            }
            SetTileAt(row, col, moved[col]);
        }

        return moved;
    }

    private List<Tile> SetCol(List<Tile> moved, int row)
    {
        for (int col = 0; col < Tiles.Count; col++)
        {
            if (moved[col].HasMoved(col, row))
            {
                GenNewTile = true;
                Grid.TilesAnimationsDone--;
            }
            SetTileAt(col, row, moved[col]);
        }

        return moved;
    }

    public void MoveUp()
    {

        List<Tile> moved;

        for (int row = 0; row < Size; row++)
        {

            moved = RemoveEmptyTilesCols(row);
            moved = MergeRight(moved);
            moved = AddEmptyTilesLast(moved);
            moved = SetCol(moved, row);

        }

    }

    public void MoveDown()
    {

        List<Tile> moved;

        for (int row = 0; row < Size; row++)
        {

            moved = RemoveEmptyTilesCols(row);
            moved = MergeLeft(moved);
            moved = AddEmptyTilesFirst(moved);
            moved = SetCol(moved, row);

        }

    }

    public void MoveLeft()
    {

        List<Tile> moved;

        for (int row = 0; row < Size; row++)
        {

            moved = RemoveEmptyTilesRows(row);
            moved = MergeLeft(moved);
            moved = AddEmptyTilesLast(moved);
            moved = setRow(moved, row);

        }

    }

    public void MoveRight()
    {

        List<Tile> moved;

        for (int row = 0; row < Size; row++)
        {

            moved = RemoveEmptyTilesRows(row);
            moved = MergeRight(moved);
            moved = AddEmptyTilesFirst(moved);
            moved = setRow(moved, row);

        }

    }

    public void IsGameOver()
    {
        if (GameOver)
        {
            WonOrLost = "WON";
        }
        else
        {
            if (IsFull())
            {
                if (!IsMovePossible())
                {
                    WonOrLost = "LOST";
                }

            }
            else
            {
                AddNew();
            }
        }
    }

    private bool IsFull()
    {
        return EmptyTiles == 0;
    }

    private bool IsMovePossible()
    {
        for (int row = 0; row < Size; row++)
        {
            for (int col = 0; col < Size - 1; col++)
            {
                if (GetTileAt(row, col).Value == GetTileAt(row, col + 1).Value)
                {
                    return true;
                }
            }
        }

        for (int row = 0; row < Size - 1; row++)
        {
            for (int col = 0; col < Size; col++)
            {
                if (GetTileAt(col, row).Value == GetTileAt(col, row + 1).Value)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void GenInitTiles()
    {
        for (int i = 0; i < InitTiles; i++)
        {
            GenNewTile = true;
            AddNew();
        }
    }

    private void AddNew()
    {
        if (GenNewTile)
        {
            int row;
            int col;
            Random rand = new Random();
            int value = rand.Next(10) < 9 ? 2 : 4;
            do
            {
                row = rand.Next(0, 4);
                col = rand.Next(0, 4);
            } while (GetTileAt(row, col).Value != 0);
            SetTileAt(row, col, new Tile(value, row, col));
            EmptyTiles--;
            GenNewTile = false;
        }
    }

    public void SetDefaultPositions()
    {
        for (int i = 0; i < Tiles.Count; i++)
        {
            for (int j = 0; j < Tiles[i].Count; j++)
            {
                if (GetTileAt(i, j).Value != 0)
                {
                    GetTileAt(i, j).SetPosition(i, j);
                }
            }
        }
    }


}

public class Controls
{
    public void keyPressed(Gdk.Key keyCode)
    {
        //WriteLine("Hello button!");
        if (Grid.TilesAnimationsDone == 16)
        {
            switch (keyCode)
            {
                case Gdk.Key.Up:
                    Console.WriteLine("UP");
                    Game.BOARD.MoveUp();
                    break;
                case Gdk.Key.Down:
                    Console.WriteLine("DOWN");
                    Game.BOARD.MoveDown();
                    break;
                case Gdk.Key.Left:
                    Console.WriteLine("LEFT");
                    Game.BOARD.MoveLeft();
                    break;
                case Gdk.Key.Right:
                    Console.WriteLine("RIGHT");
                    Game.BOARD.MoveRight();
                    break;
                default:
                    break;
            }
        }

        Game.BOARD.IsGameOver();
        Game.WINDOW.StartAnimation();
    }
}



public class Grid : DrawingArea
{
	public Grid() {
		this.ExposeEvent += OnExposeEvent;

		}

    private static int RADIUS = 7;
    private static int WIN_MARGIN = 20;
    private static int TILE_SIZE = 65;
    private static int TILE_MARGIN = 15;

    private static int DURATION = 150;

    private static int _tilesAnimationsDone = 16;
    public static int TilesAnimationsDone { get { return _tilesAnimationsDone; } set { _tilesAnimationsDone = value; } }
    protected void OnExposeEvent(object sender, ExposeEventArgs a)
    {
			Context c = CairoHelper.Create(GdkWindow);

            drawTitle(c);
            drawScoreBoard(c);
            drawBoard(c);

			c.GetTarget().Dispose();
			c.Dispose();
        
    }
    private static void drawTitle(Context c)
    {
        c.SelectFontFace("Arial", FontSlant.Normal, FontWeight.Bold);
        c.SetFontSize(38);
        c.MoveTo(WIN_MARGIN, 50);
        c.ShowText("2048");
        c.Fill();
    }

    private static void drawScoreBoard(Context c)
    {
        int width = 160;
        int height = 40;
        int xOffset = Game.WINDOW.WIDTH - WIN_MARGIN - width;
        int yOffset = 20;
        string strScore = "SCORE";
        string strHighestScore = "HIGHEST";
        string score = Convert.ToString(Game.BOARD.Score);
        string highestScore = Convert.ToString(Game.BOARD.HighestScore);
        int xScore = Game.WINDOW.WIDTH - WIN_MARGIN - width + (width / 4);
        int xHighestScore = Game.WINDOW.WIDTH - WIN_MARGIN - (width / 4);
        //c.SetSourceRGBA(119, 110, 101, 1);

        DrawRoundedRectangle(c, xOffset, yOffset, width, height, RADIUS);
        c.Fill();

        c.SetSourceRGBA(1,1,1,1);
        c.SelectFontFace("Arial", FontSlant.Normal, FontWeight.Normal);
        c.SetFontSize(8);
        c.MoveTo(xScore - ((int)c.TextExtents(strScore).Width / 2), yOffset + 15);
        c.ShowText(strScore);
        c.MoveTo(xHighestScore - ((int)c.TextExtents(strHighestScore).Width / 2), yOffset + 15);
        c.ShowText(strHighestScore);
        c.SetFontSize(14);
        c.MoveTo(xScore - ((int)c.TextExtents(score).Width / 2), yOffset + 30);
        c.ShowText(score);
        c.MoveTo(xHighestScore - ((int)c.TextExtents(highestScore).Width / 2), yOffset + 30);
        c.ShowText(highestScore);
        //c.Fill();
    }

    private static void drawTilesBackground(Context c)
    {

        int posX;
        int posY;
        //c.SetSourceRGBA(119, 110, 101, 1);
        for (int row = 0; row < 4; row++)
        {
            for (int col = 0; col < 4; col++)
            {
                posX = col * (TILE_MARGIN + TILE_SIZE) + TILE_MARGIN;
                posY = row * (TILE_MARGIN + TILE_SIZE) + TILE_MARGIN;
                DrawRoundedRectangle(c, posX, posY, TILE_SIZE, TILE_SIZE, RADIUS);
            }
        }

        c.Fill();
    }

    private static void drawBoard(Context c)
    {
        c.Translate(WIN_MARGIN, 80);

        DrawRoundedRectangle(c, 0, 0, Game.WINDOW.WIDTH - (WIN_MARGIN * 2), 320 + TILE_MARGIN, RADIUS);
        c.SetSourceRGBA(1, 1, 1, 0.75);
        c.Fill();

        drawTilesBackground(c);

        for (int row = 0; row < 4; row++)
        {
            for (int col = 0; col < 4; col++)
            {
                drawTile(c, Game.BOARD.GetTileAt(row, col), col, row);
            }
        }

        if (Window.timer && TilesAnimationsDone < 16)
        {
            // continue timer
        }
        else
        {
            Window.timer = false;
            Game.BOARD.SetDefaultPositions();
            TilesAnimationsDone = 16;
        }
    }

    private static void drawTile(Context c, Tile tile, int x, int y)
    {
        int value = tile.Value;
        int prevX = tile.Col;
        int prevY = tile.Row;
        int fromX = tile.Col * (TILE_MARGIN + TILE_SIZE) + TILE_MARGIN;
        int fromY = tile.Row * (TILE_MARGIN + TILE_SIZE) + TILE_MARGIN;
        int toX = x * (TILE_MARGIN + TILE_SIZE) + TILE_MARGIN;
        int toY = y * (TILE_MARGIN + TILE_SIZE) + TILE_MARGIN;
        int posX = 0;
        int posY = 0;

        int distanceX = Math.Abs(fromX - toX);
        int distanceY = Math.Abs(fromY - toY);

        tile.ShiftX += (((double)Window.INTERVAL * distanceX) / DURATION);
        tile.ShiftY += (((double)Window.INTERVAL * distanceY) / DURATION);
        
        if (tile.Animating)
        {
            bool done = false;
            if (prevX != x)
            { // horizontal moving
                if (prevX < x)
                {
                    if ((posX = (int)(fromX + tile.ShiftX)) >= toX)
                    {
                        done = true;
                    }
                }
                else if (prevX > x)
                {
                    if ((posX = (int)(fromX - tile.ShiftX)) <= toX)
                    {
                        done = true;
                    }
                }
                posY = toY;
            }
            else if (prevY != y)
            { // vertical moving
                if (prevY < y)
                {
                    if ((posY = (int)(fromY + tile.ShiftY)) >= toY)
                    {
                        done = true;
                    }
                }
                else if (prevY > y)
                {
                    if ((posY = (int)(fromY - tile.ShiftY)) <= toY)
                    {
                        done = true;
                    }
                }
                posX = toX;
            }
            else
            {
                Console.WriteLine("MOVE ERROR!");
            }

            if (done)
            {
                tile.Animating = false;
                TilesAnimationsDone++;
                tile.ResetShift();
                posX = toX;
                posY = toY;
            }
        }
        else
        { 
            posX = toX;
            posY = toY;
        }
        if (value != 0)
        {

            c.SetSourceRGBA(0, 0, 1, 1);
            DrawRoundedRectangle(c, posX, posY, TILE_SIZE, TILE_SIZE, RADIUS);
            c.Fill();

            int size = value < 100 ? 36 : value < 1000 ? 32 : 24;
            c.SetFontSize(size);
            c.SetSourceRGBA(1, 1, 1, 1);
            c.SelectFontFace("Arial", FontSlant.Normal, FontWeight.Bold);

            string s = value.ToString();
            TextExtents te = c.TextExtents(s);
            double w = te.Width;
            double h = te.Height;

            c.MoveTo(posX + ((TILE_SIZE - w) / 2 - te.XBearing),
                posY + TILE_SIZE - ((TILE_SIZE - h) / 2));

            c.ShowText(s);
        }

        if (Game.BOARD.WonOrLost != null && Game.BOARD.WonOrLost.Length != 0)
        {
            string WonOrLost = "You " + Game.BOARD.WonOrLost;
            c.SetSourceRGBA(0, 0, 0, 1);

            c.Rectangle(0, 0, Game.WINDOW.WIDTH, Game.WINDOW.HEIGHT);
            //c.SetSourceRGBA(1, 1, 1, 0.5);

            c.Fill();
            c.SetSourceRGBA(1, 1, 1, 1);
            //c.Fill();

            c.SelectFontFace("Arial", FontSlant.Normal, FontWeight.Normal);
            c.SetFontSize(30);
            c.MoveTo(68, 150);
            c.ShowText(WonOrLost);
            c.Fill();
        }

        c.Fill();

    }

    static void DrawRoundedRectangle(Context c, double x, double y, int width, int height, int radius)
    {
        c.Save();
        
        if ((radius > height / 2) || (radius > width / 2))
            radius = Math.Min(height / 2, width / 2);

        c.MoveTo(x, y + radius);
        c.Arc(x + radius, y + radius, radius, Math.PI, -Math.PI / 2);
        c.LineTo(x + width - radius, y);
        c.Arc(x + width - radius, y + radius, radius, -Math.PI / 2, 0);
        c.LineTo(x + width, y + height - radius);
        c.Arc(x + width - radius, y + height - radius, radius, 0, Math.PI / 2);
        c.LineTo(x + radius, y + height);
        c.Arc(x + radius, y + height - radius, radius, Math.PI / 2, Math.PI);
        c.ClosePath();
        c.Restore();
    }

}

public partial class Window : Gtk.Window
{
	

    public readonly int WIDTH = 375;
    public readonly int HEIGHT = 450;

    public static bool timer = false;
    public static readonly uint INTERVAL = 16;

    private Grid grid;

    public Window() : base(Gtk.WindowType.Toplevel)
    {

        this.Name = "Window";
        this.Title = "2048";
        this.Resize(WIDTH, HEIGHT);
        this.SetPosition(WindowPosition.Center);
        grid = new Grid(); 
        grid.ExposeEvent += OnExposeEvent;
        Add(grid); 
		//Add(new Grid());
        //this.ExposeEvent += OnExposeEvent;
        this.DeleteEvent += OnDeleteEvent;
        this.KeyPressEvent += OnKeyPressEvent;

        //drawingarea = new DrawingArea();
        //drawingarea.ExposeEvent += OnExposeEvent;
        //Add(drawingarea);
	
        this.ShowAll();

    }

    public void StartAnimation()
    {
        timer = true;
        GLib.Timeout.Add(Window.INTERVAL, new GLib.TimeoutHandler(Animation));
    }

    public bool Animation()
    {
        if (!timer)
            return false;

        grid.QueueDraw();
        return true;
    }

    protected void OnDeleteEvent(object sender, DeleteEventArgs a)
    {
        Application.Quit();
    }

    protected void OnExposeEvent(object sender, ExposeEventArgs a)
    {
        DrawingArea area = (DrawingArea)sender;
        //Grid.draw();
    }

    [GLib.ConnectBefore()]
    protected virtual void OnKeyPressEvent(object o, Gtk.KeyPressEventArgs a)
    {
        Game.CONTROLS.keyPressed(a.Event.Key);
    }

}


public class Game
{
    public readonly static Window WINDOW = new Window();
    public readonly static Controls CONTROLS = new Controls();
    public readonly static Board BOARD = new Board(4);
}

class MainClass
{
    public static void Main(string[] args)
    {
        Application.Init();
        new Game();
        Application.Run();
        //WriteLine("Hello World!");
    }
}



