using System.Runtime.InteropServices;

partial class TestClass
{
    [LibraryImport("ECLibrary.dll")]
    private static partial ushort ReadWord(byte _register);

    [LibraryImport("ECLibrary.dll")]
    private static partial byte WriteWord(byte _register, ushort _value);

    [LibraryImport("ECLibrary.dll")]
    private static partial void Setup();

    static void Main(string[] args)
    {
        Console.WriteLine("Loading DLL");
        NativeLibrary.Load(Path.Combine(Directory.GetCurrentDirectory(), "Plugins\\Acer-PO3-640\\ECLibrary.dll"));
        
        Console.WriteLine("Trying init method");
        Setup();

        Console.WriteLine("Trying read method");
        ushort result = ReadWord((byte) 0x14);
        Console.WriteLine(result);
    }
}