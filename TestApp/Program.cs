using System;
using System.Runtime.InteropServices;

partial class TestClass
{
    [LibraryImport("ECLibrary.dll")]
    private static partial ushort ReadWord(byte _register);

    [LibraryImport("ECLibrary.dll")]
    private static partial byte WriteWord(byte _register, ushort _value);

    [LibraryImport("ECLibrary.dll")]
    private static partial void Setup();

    [LibraryImport("ECLibrary.dll")]
    private static partial void Shutdown();

    static void Main(string[] args)
    {
        Console.WriteLine("Loading DLL");
        NativeLibrary.Load(Path.Combine(Directory.GetCurrentDirectory(), "Plugins\\Acer-PO3-640\\ECLibrary.dll"));
        Setup();

        if (args.Length >= 3)
        {
            if (args[0] == "set")
            {
                int value = Convert.ToInt32(args[2]);
                switch (args[1])
                {
                    case "cpu":
                        WriteWord((byte)0xF0, (ushort)value);
                        Console.WriteLine($"Set CPU fan to {value}");
                        break;
                    case "front":
                        WriteWord((byte)0xF2, (ushort)value);
                        Console.WriteLine($"Set Front fan to {value}");
                        break;
                    case "back":
                        WriteWord((byte)0xF6, (ushort)value);
                        Console.WriteLine($"Set Back fan to {value}");
                        break;
                    default:
                        Console.WriteLine("Invalid Fan");
                        break;
                }
            }
            else Console.WriteLine("Invalid Command");
        }
        else
        {
            Console.WriteLine("Test Suite");

            Console.WriteLine("Trying read method");
            ushort result = ReadWord((byte)0x14);
            Console.WriteLine(result);

            Console.WriteLine("Trying write method");
            byte result2 = WriteWord((byte) 0xF2, (ushort) 2100);
            Console.WriteLine(result2);

        }

        Shutdown();
    }
}