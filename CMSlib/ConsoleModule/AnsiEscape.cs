namespace CMSlib.ConsoleModule
{
    public class AnsiEscape
    {
        public const string ControlSequenceIntroducer =                              "\u001B\u005B";       // <ESC>[
        
        public const char   NullTerminator            =  '\u0000';
        
        public const string SgrBlackForeGround        =  ControlSequenceIntroducer + "\u0033\u0030\u006D"; // <ESC>[30m
        public const string SgrRedForeGround          =  ControlSequenceIntroducer + "\u0033\u0031\u006D"; // <ESC>[31m
        public const string SgrGreenForeGround        =  ControlSequenceIntroducer + "\u0033\u0032\u006D"; // <ESC>[32m
        public const string SgrYellowForeGround       =  ControlSequenceIntroducer + "\u0033\u0033\u006D"; // <ESC>[33m
        public const string SgrBlueForeGround         =  ControlSequenceIntroducer + "\u0033\u0034\u006D"; // <ESC>[34m
        public const string SgrMagentaForeGround      =  ControlSequenceIntroducer + "\u0033\u0035\u006D"; // <ESC>[35m
        public const string SgrCyanForeGround         =  ControlSequenceIntroducer + "\u0033\u0036\u006D"; // <ESC>[36m
        public const string SgrWhiteForeGround        =  ControlSequenceIntroducer + "\u0033\u0037\u006D"; // <ESC>[37m
        
        public const string SgrBlackBackGround        =  ControlSequenceIntroducer + "\u0034\u0030\u006D"; // <ESC>[40m
        public const string SgrRedBackGround          =  ControlSequenceIntroducer + "\u0034\u0031\u006D"; // <ESC>[41m
        public const string SgrGreenBackGround        =  ControlSequenceIntroducer + "\u0034\u0032\u006D"; // <ESC>[42m
        public const string SgrYellowBackGround       =  ControlSequenceIntroducer + "\u0034\u0033\u006D"; // <ESC>[43m
        public const string SgrBlueBackGround         =  ControlSequenceIntroducer + "\u0034\u0034\u006D"; // <ESC>[44m
        public const string SgrMagentaBackGround      =  ControlSequenceIntroducer + "\u0034\u0035\u006D"; // <ESC>[45m
        public const string SgrCyanBackGround         =  ControlSequenceIntroducer + "\u0034\u0036\u006D"; // <ESC>[46m
        public const string SgrWhiteBackGround        =  ControlSequenceIntroducer + "\u0034\u0037\u006D"; // <ESC>[47m
        
        public const string SgrNegative               =  ControlSequenceIntroducer + "\u0037\u006D";       // <ESC>[7m
        public const string SgrUnderline              =  ControlSequenceIntroducer + "\u0030\u0034\u006D"; // <ESC>[04m
        public const string SgrNoUnderline            =  ControlSequenceIntroducer + "\u0032\u0034\u006D"; // <ESC>[24m
        public const string SgrBrightBold             =  ControlSequenceIntroducer + "\u0031\u006D";       // <ESC>[1m
        public const string SgrClear                  =  ControlSequenceIntroducer + "\u0030\u006D";       // <ESC>[0m
        public const string SgrForeGroundClear        =  ControlSequenceIntroducer + "\u0033\u0039\u006D"; // <ESC>[49m

        public const string AlternateScreenBuffer     =  ControlSequenceIntroducer + "?1049h";
        public const string MainScreenBuffer          =  ControlSequenceIntroducer + "?1049l";
        public const string SoftReset                 =  ControlSequenceIntroducer + "!p";
        
        public const string EnableCursorBlink         =  ControlSequenceIntroducer + "?12h";
        public const string DisableCursorBlink        =  ControlSequenceIntroducer + "?12l";
        public const string EnableCursorVisibility    =  ControlSequenceIntroducer + "?25h";
        public const string DisableCursorVisibility   =  ControlSequenceIntroducer + "?25l";

        public const string AsciiMode                 = "\u001B(B";
        public const string LineDrawingMode           = "\u001B(0\u0000";

        public const char   LowerLeftCorner           = 'm';
        public const char   UpperLeftCorner           = 'l';
        public const char   LowerRightCorner          = 'j';
        public const char   UpperRightCorner          = 'k';
        public const char   Cross                     = 'n';
        public const char   HorizWithUp               = 'v';
        public const char   HorizWithDown             = 'w';
        public const char   VerticalWithLeft          = 'u';
        public const char   VerticalWithRight         = 't';
        public const char   HorizontalLine            = 'q';
        public const char   VerticalLine              = 'x';
        
        
        
        
        public static string WindowTitle(string title) => $"\u001B\u005D\u0032\u003B{title[..System.Math.Min(title.Length, 256)]}\u0007";

        public static string Underline(string toUnderline) => SgrUnderline + toUnderline + SgrNoUnderline;

        public static string SetCursorPosition(int x, int y) => ControlSequenceIntroducer + (y + 1) + ';' + (x + 1) + 'H';
    }
}