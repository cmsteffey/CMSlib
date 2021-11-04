using System;

namespace CMSlib.ConsoleModule
{
    public class AnsiEscape
    {
        public const string ControlSequenceIntroducer = "\u001B\u005B"; // <ESC>[

        public const char NullTerminator = '\u0000';

        public const string SgrBlackForeGround = ControlSequenceIntroducer + "\u0033\u0030\u006D\u0000"; // <ESC>[30m
        public const string SgrRedForeGround = ControlSequenceIntroducer + "\u0033\u0031\u006D\u0000"; // <ESC>[31m
        public const string SgrGreenForeGround = ControlSequenceIntroducer + "\u0033\u0032\u006D\u0000"; // <ESC>[32m
        public const string SgrYellowForeGround = ControlSequenceIntroducer + "\u0033\u0033\u006D\u0000"; // <ESC>[33m
        public const string SgrBlueForeGround = ControlSequenceIntroducer + "\u0033\u0034\u006D\u0000"; // <ESC>[34m
        public const string SgrMagentaForeGround = ControlSequenceIntroducer + "\u0033\u0035\u006D\u0000"; // <ESC>[35m
        public const string SgrCyanForeGround = ControlSequenceIntroducer + "\u0033\u0036\u006D\u0000"; // <ESC>[36m
        public const string SgrWhiteForeGround = ControlSequenceIntroducer + "\u0033\u0037\u006D\u0000"; // <ESC>[37m

        public const string
            SgrBrightBlackForeGround = ControlSequenceIntroducer + "\u0039\u0030\u006D\u0000"; // <ESC>[90m

        public const string
            SgrBrightRedForeGround = ControlSequenceIntroducer + "\u0039\u0031\u006D\u0000"; // <ESC>[91m

        public const string
            SgrBrightGreenForeGround = ControlSequenceIntroducer + "\u0039\u0032\u006D\u0000"; // <ESC>[92m

        public const string
            SgrBrightYellowForeGround = ControlSequenceIntroducer + "\u0039\u0033\u006D\u0000"; // <ESC>[93m

        public const string
            SgrBrightBlueForeGround = ControlSequenceIntroducer + "\u0039\u0034\u006D\u0000"; // <ESC>[94m

        public const string
            SgrBrightMagentaForeGround = ControlSequenceIntroducer + "\u0039\u0035\u006D\u0000"; // <ESC>[95m

        public const string
            SgrBrightCyanForeGround = ControlSequenceIntroducer + "\u0039\u0036\u006D\u0000"; // <ESC>[96m

        public const string
            SgrBrightWhiteForeGround = ControlSequenceIntroducer + "\u0039\u0037\u006D\u0000"; // <ESC>[97m

        public const string SgrBlackBackGround = ControlSequenceIntroducer + "\u0034\u0030\u006D\u0000"; // <ESC>[40m
        public const string SgrRedBackGround = ControlSequenceIntroducer + "\u0034\u0031\u006D\u0000"; // <ESC>[41m
        public const string SgrGreenBackGround = ControlSequenceIntroducer + "\u0034\u0032\u006D\u0000"; // <ESC>[42m
        public const string SgrYellowBackGround = ControlSequenceIntroducer + "\u0034\u0033\u006D\u0000"; // <ESC>[43m
        public const string SgrBlueBackGround = ControlSequenceIntroducer + "\u0034\u0034\u006D\u0000"; // <ESC>[44m
        public const string SgrMagentaBackGround = ControlSequenceIntroducer + "\u0034\u0035\u006D\u0000"; // <ESC>[45m
        public const string SgrCyanBackGround = ControlSequenceIntroducer + "\u0034\u0036\u006D\u0000"; // <ESC>[46m
        public const string SgrWhiteBackGround = ControlSequenceIntroducer + "\u0034\u0037\u006D\u0000"; // <ESC>[47m

        public const string
            SgrBrightBlackBackGround = ControlSequenceIntroducer + "\u0031\u0030\u0030\u006D\u0000"; // <ESC>[100m

        public const string
            SgrBrightRedBackGround = ControlSequenceIntroducer + "\u0031\u0030\u0031\u006D\u0000"; // <ESC>[101m

        public const string
            SgrBrightGreenBackGround = ControlSequenceIntroducer + "\u0031\u0030\u0032\u006D\u0000"; // <ESC>[102m

        public const string
            SgrBrightYellowBackGround = ControlSequenceIntroducer + "\u0031\u0030\u0033\u006D\u0000"; // <ESC>[103m

        public const string
            SgrBrightBlueBackGround = ControlSequenceIntroducer + "\u0031\u0030\u0034\u006D\u0000"; // <ESC>[104m

        public const string
            SgrBrightMagentaBackGround = ControlSequenceIntroducer + "\u0031\u0030\u0035\u006D\u0000"; // <ESC>[105m

        public const string
            SgrBrightCyanBackGround = ControlSequenceIntroducer + "\u0031\u0030\u0036\u006D\u0000"; // <ESC>[106m

        public const string
            SgrBrightWhiteBackGround = ControlSequenceIntroducer + "\u0031\u0030\u0037\u006D\u0000"; // <ESC>[107m


        public const string SgrNegative = ControlSequenceIntroducer + "\u0037\u006D\u0000"; // <ESC>[7m
        public const string SgrUnderline = ControlSequenceIntroducer + "\u0030\u0034\u006D\u0000"; // <ESC>[04m
        public const string SgrNoUnderline = ControlSequenceIntroducer + "\u0032\u0034\u006D\u0000"; // <ESC>[24m
        public const string SgrBrightBold = ControlSequenceIntroducer + "\u0031\u006D\u0000"; // <ESC>[1m
        public const string SgrClear = ControlSequenceIntroducer + "\u0030\u006D\u0000"; // <ESC>[0m
        public const string SgrStrikeThrough = ControlSequenceIntroducer + "\u0039\u006D\u0000"; // <ESC>[9m
        public const string SgrNoStrikeThrough = ControlSequenceIntroducer + "\u0032\u0039\u006D\u0000"; // <ESC>[29m
        public const string SgrForeGroundClear = ControlSequenceIntroducer + "\u0033\u0039\u006D\u0000"; // <ESC>[39m
        public const string SgrBackGroundClear = ControlSequenceIntroducer + "\u0034\u0039\u006D\u0000"; // <ESC>[49m
        public const string SgrOverline = ControlSequenceIntroducer + "\u0035\u0033\u006D\u0000"; // <ESC>[53m
        public const string SgrNoOverline = ControlSequenceIntroducer + "\u0035\u0035\u006D\u0000"; // <ESC>[55m
        public const string SgrNoBrightBold = ControlSequenceIntroducer + "\u0032\u006D\u0000";
        public const string SgrBlinking = ControlSequenceIntroducer + "\u0030\u0035\u006D\u0000"; // <ESC>[05m
        public const string SgrNoBlinking = ControlSequenceIntroducer + "\u0032\u0035\u006D\u0000"; // <ESC>[25m

        public const string AlternateScreenBuffer = ControlSequenceIntroducer + "?1049h\u0000";
        public const string MainScreenBuffer = ControlSequenceIntroducer + "?1049l\u0000";
        public const string SoftReset = ControlSequenceIntroducer + "!p\u0000";

        public const string EnableCursorBlink = ControlSequenceIntroducer + "?12h\u0000";
        public const string DisableCursorBlink = ControlSequenceIntroducer + "?12l\u0000";
        public const string EnableCursorVisibility = ControlSequenceIntroducer + "?25h\u0000";
        public const string DisableCursorVisibility = ControlSequenceIntroducer + "?25l\u0000";

        public const string AsciiMode = "\u001B(B\u0000";
        public const string LineDrawingMode = "\u001B(0\u0000";

        public const char LowerLeftCorner = 'm';
        public const char UpperLeftCorner = 'l';
        public const char LowerRightCorner = 'j';
        public const char UpperRightCorner = 'k';
        public const char Cross = 'n';
        public const char HorizWithUp = 'v';
        public const char HorizWithDown = 'w';
        public const char VerticalWithLeft = 'u';
        public const char VerticalWithRight = 't';
        public const char HorizontalLine = 'q';
        public const char VerticalLine = 'x';


        public static string WindowTitle(string title) =>
            $"\u001B\u005D\u0032\u003B{title[..Math.Min(title.Length, 256)]}\u0007\u0000";

        public static string Underline(string toUnderline) => SgrUnderline + toUnderline + SgrNoUnderline;

        public static string SgrForeGroundColor(int color) => ControlSequenceIntroducer + "38;5;" + color + 'm';
        public static string SgrBackGroundColor(int color) => ControlSequenceIntroducer + "48;5;" + color + 'm';

        public static string SetCursorPosition(int x, int y) =>
            ControlSequenceIntroducer + (y + 1) + ';' + (x + 1) + 'H';
    }
}