using Spectre.Console;

class DigitalClock
{
    public bool DisplayYear { get; set; }
    public bool DisplayMonth { get; set; }
    public bool DisplayDay { get; set; }
    public bool DisplayHour { get; set; }
    public bool DisplayMinute { get; set; }
    public bool DisplaySecond { get; set; }
    public bool DisplayMilliseconds { get; set; }
    public (int x, int y) ClockTopLeftPosition { get; set; }
    private Dictionary<short, bool[][]> NumsShape { get; set; }
    public DigitalClock((int x, int y) ClockTopLeftPosition, bool DisplayYear, bool DisplayMonth, bool DisplayDay, bool DisplayHour, bool DisplayMinute, bool DisplaySecond, bool DisplayMilliseconds)
    {
        this.DisplayYear = DisplayYear;
        this.DisplayMonth = DisplayMonth;
        this.DisplayDay = DisplayDay;
        this.DisplayHour = DisplayHour;
        this.DisplayMinute = DisplayMinute;
        this.DisplaySecond = DisplaySecond;
        this.DisplayMilliseconds = DisplayMilliseconds;
        this.ClockTopLeftPosition = ClockTopLeftPosition;
        NumsShape = new();
        ReadAndSetNumsShape();
    }
    private void ReadAndSetNumsShape()
    {
        var collection = File.ReadAllText("nums.txt").Where(x => x == '1' || x == '0');
        var splited = collection.Chunk(15).Select(x => x.ToList());
        for (int i = 0; i < splited.Count(); i++)
        {
            NumsShape.Add((short)i, splited.ElementAt(i).Select(y => y == '1').Chunk(3).ToArray());
        }
    }
    public bool[,] ConvertNumToDigital(int num)
    {
        var str = num.ToString();
        if (num < 10)
        {
            str = "0" + str;
        }
        var splited = str.Select(x => NumsShape[(short)int.Parse(x.ToString())]);
        var result = new bool[5, splited.Count() * 4 - 1];
        for (int i = 0; i < result.GetLength(0); i++)
        {
            for (int j = 0; j < result.GetLength(1); j++)
            {
                result[i, j] = false;
            }
        }
        for (int x = 0; x < splited.Count(); x++)
        {
            for (int y = 0; y < splited.ElementAt(x).Length; y++)
            {
                for (int z = 0; z < splited.ElementAt(x)[y].Length; z++)
                {
                    if (splited.ElementAt(x)[y][z])
                    {
                        result[y, x * 4 + z] = true;
                    }
                }
            }
        }
        return result;
    }
    private List<bool[,]> PixelatedDateTime(DateTime time)
    {
        var list = new List<bool[,]>();
        if (DisplayYear) list.Add(ConvertNumToDigital(time.Year));
        if (DisplayMonth) list.Add(ConvertNumToDigital(time.Month));
        if (DisplayDay) list.Add(ConvertNumToDigital(time.Day));
        if (DisplayHour) list.Add(ConvertNumToDigital(time.Hour));
        if (DisplayMinute) list.Add(ConvertNumToDigital(time.Minute));
        if (DisplaySecond) list.Add(ConvertNumToDigital(time.Second));
        if (DisplayMilliseconds) list.Add(ConvertNumToDigital(time.Millisecond));
        return list;
    }
    public void Display(DateTime time)
    {
        var list = PixelatedDateTime(time);
        Console.SetCursorPosition(ClockTopLeftPosition.x, ClockTopLeftPosition.y);
        for (int k = 0; k < list.Count; k++)
        {
            for (int i = 0; i < list[k].GetLength(0); i++)
            {
                for (int j = 0; j < list[k].GetLength(1); j++)
                {
                    if (list[k][i, j])
                    {
                        Console.BackgroundColor = ConsoleColor.White;
                    }
                    Console.Write("  ");
                    Console.ResetColor();
                }
                Console.CursorTop += 1;
                if (i != list[k].GetLength(0) - 1)
                {
                    Console.CursorLeft -= 2 * list[k].GetLength(1);
                }
            }
            if (k != list.Count - 1)
            {
                Console.BackgroundColor = ConsoleColor.White;
                Console.CursorLeft += 2;
                Console.CursorTop -= 2;
                Console.Write("  ");
                Console.CursorLeft -= 2;
                Console.CursorTop -= 2;
                Console.Write("  ");
                Console.CursorLeft += 2;
                Console.CursorTop -= 1;
                Console.ResetColor();
            }
        }
    }
    public async Task Play(int sleep)
    {
        while (true)
        {
            if (Console.KeyAvailable) break;
            Display(DateTime.Now);
            await Task.Delay(sleep);
        }
    }
}
class Clock
{
    public enum DisplayEnum
    {
        None = 0,
        ClockChar = 1,
        SecondChar = 2,
        MinuteChar = 3,
        HourChar = 4
    }
    private int clockRedius = 64;
    private int secondRedius = 60;
    private int minutesRedius = 54;
    private int hourRedius = 46;
    private char clockChar = '#';
    private char secondChar = '*';
    private char minuteChar = '*';
    private char hourChar = '*';
    public bool HaveDigitalClock { get; set; }
    public DigitalClock DigitalClock { get; set; }
    public DisplayEnum[,] ClockScreen { get; set; }
    public Clock(int clockRedius, int secondRedius, int minutesRedius, int hourRedius, char clockChar, char secondChar, char minuteChar, char hourChar, bool HaveDigitalClock, DigitalClock? digitalClock = null)
    {
        this.HaveDigitalClock = HaveDigitalClock;
        if (digitalClock is null)
        {
            DigitalClock = new((clockRedius * 5, clockRedius - 3), false, false, false, true, true, true, true);
        }
        else
        {
            DigitalClock = digitalClock;
        }
        this.clockRedius = clockRedius;
        this.secondRedius = secondRedius;
        this.minutesRedius = minutesRedius;
        this.hourRedius = hourRedius;
        this.clockChar = clockChar;
        this.secondChar = secondChar;
        this.minuteChar = minuteChar;
        this.hourChar = hourChar;
        ClockScreen = new DisplayEnum[clockRedius * 2 + 1, clockRedius * 2 + 1];
        for (int i = 0; i < ClockScreen.GetLength(0); i++)
        {
            for (int j = 0; j < ClockScreen.GetLength(1); j++)
            {
                ClockScreen[i, j] = DisplayEnum.None;
            }
        }
        SetClockChars();
    }

    private void SetClockChars()
    {
        double accuracy = 0.005;
        for (double degree = 0; degree <= Math.PI; degree += accuracy)
        {
            var res = Math.SinCos(degree);
            ClockScreen[(int)Math.Round(clockRedius - clockRedius * res.Sin), (int)Math.Round(clockRedius - clockRedius * res.Cos)] = DisplayEnum.ClockChar;
        }
        for (int i = 0; i <= clockRedius; i++)
        {
            for (int j = 0; j < ClockScreen.GetLength(1); j++)
            {
                if (ClockScreen[i, j] == DisplayEnum.ClockChar)
                {
                    ClockScreen[clockRedius * 2 - i, j] = DisplayEnum.ClockChar;
                }
            }
        }
    }
    public void UpdateClock()
    {
        for (int i = 0; i < ClockScreen.GetLength(0); i++)
        {
            for (int j = 0; j < ClockScreen.GetLength(1); j++)
            {
                if ((int)ClockScreen[i, j] > 1)
                {
                    ClockScreen[i, j] = DisplayEnum.None;
                }
            }
        }
        SetHourChars();
        SetMinuteChars();
        SetSecondChars();
    }
    private void SetHourChars()
    {
        var hour = DateTime.Now.Hour + ((double)DateTime.Now.Minute / 60);
        hour = (hour >= 12) ? hour - 12 : hour;
        var degree = (hour / 6d * Math.PI * -1) + Math.PI / 2;
        var sin = Math.Sin(degree);
        var cos = Math.Cos(degree);
        double accuracy = 0.2;
        for (double i = 0; i <= hourRedius; i += accuracy)
        {
            ClockScreen[(int)Math.Round(clockRedius - i * sin), (int)Math.Round(clockRedius + i * cos)] = DisplayEnum.HourChar;
        }
    }
    private void SetMinuteChars()
    {
        var minute = DateTime.Now.Minute + ((double)DateTime.Now.Second / 60);
        var degree = (minute / 30d * Math.PI * -1) + Math.PI / 2;
        var sin = Math.Sin(degree);
        var cos = Math.Cos(degree);
        double accuracy = 0.2;
        for (double i = 0; i <= minutesRedius; i += accuracy)
        {
            ClockScreen[(int)Math.Round(clockRedius - i * sin), (int)Math.Round(clockRedius + i * cos)] = DisplayEnum.MinuteChar;
        }
    }
    private void SetSecondChars()
    {
        var second = DateTime.Now.Second;
        var degree = (second / 30d * Math.PI * -1) + Math.PI / 2;
        var sin = Math.Sin(degree);
        var cos = Math.Cos(degree);
        double accuracy = 0.2;
        for (double i = 0; i <= secondRedius; i += accuracy)
        {
            ClockScreen[(int)Math.Round(clockRedius - i * sin), (int)Math.Round(clockRedius + i * cos)] = DisplayEnum.SecondChar;
        }
    }
    public void Display()
    {
        Console.SetCursorPosition(0, 0);
        for (int i = 0; i < ClockScreen.GetLength(0); i++)
        {
            for (int j = 0; j < ClockScreen.GetLength(1); j++)
            {

                switch (ClockScreen[i, j])
                {
                    case DisplayEnum.ClockChar:
                        Console.Write(clockChar + " "); break;
                    case DisplayEnum.HourChar:
                        Console.Write(hourChar + " "); break;
                    case DisplayEnum.MinuteChar:
                        Console.Write(minuteChar + " "); break;
                    case DisplayEnum.SecondChar:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write(secondChar + " ");
                        Console.ResetColor(); break;
                    case DisplayEnum.None:
                        Console.Write("  "); break;
                }
            }
            Console.WriteLine();
        }
    }
    public async Task Play()
    {
        while (true)
        {
            if (Console.KeyAvailable) break;
            UpdateClock();
            Display();
            int sec = DateTime.Now.Second;
            if (HaveDigitalClock)
            {
                if (DigitalClock.DisplayMilliseconds)
                {
                    while (DateTime.Now.Second == sec)
                    {
                        DigitalClock.Display(DateTime.Now);
                    }
                }
                else
                {
                    DigitalClock.Display(DateTime.Now);
                    while (DateTime.Now.Second == sec) ;
                }
            }
        }
    }
}
class Program
{
    public static void Main(string[] args)
    {
        Console.CursorVisible = false;
        Console.ReadKey();
        var digital = new DigitalClock((155, 33), true, true, true, true, true, true, true);
        var clock = new Clock(35, 32, 28, 21 , '#' , '0' , '0' , '0' , true , digital);
        var play = clock.Play();
        play.Wait();
    }
}