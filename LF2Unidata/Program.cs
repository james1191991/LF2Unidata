using System.Text;

const int BUFFER_SIZE = 1024;
const string CAESAR_CIPHER = "odBearBecauseHeIsVeryGoodSiuHungIsAGo";
const string CUSTOM_HEADER = // Need to be exactly 123 bytes.
    "= This file were encoded by LF2Unidata " +
    "which you can take the source code from https://github.com/james1191991/LF2Unidata =";
const string USAGE_MESSAGE = @"LF2Unidata
  usage: {0} command <file>
    command:
      e, encode    Encode a given plain text file to .dat LF2 data file.
      d, decode    Decode a given LF2 data file to .txt plain text file.";

if (args.Length <= 0)
{
    HelpMe();
}

var inputAction = args[0];
var inputFilePath = args[1];

var commandMappings = new Dictionary<string, bool> {
    { "e", true },
    { "encode", true },
    { "d", false },
    { "decode", false },
};

if (!commandMappings.ContainsKey(inputAction))
{
    OutputError("Invalid command. Oh no.");
}

var pathSeparatorChar = Path.DirectorySeparatorChar;
var filePath = Path.GetDirectoryName(inputFilePath);
var fileName = Path.GetFileName(inputFilePath);

if (string.IsNullOrWhiteSpace(fileName))
{
    OutputError("Invalid filename. Oh my god.");
}

if (string.IsNullOrWhiteSpace(filePath))
{
    inputFilePath = $"{Environment.CurrentDirectory}{pathSeparatorChar}{fileName}";
    filePath = Path.GetDirectoryName(inputFilePath);
}

if (!File.Exists(inputFilePath))
{
    OutputError("Missing file. Holy shit.");
}

var isEncode = commandMappings.GetValueOrDefault(inputAction);
var resultData = await Process(inputFilePath, isEncode);
await SaveFile(resultData, isEncode);

async Task<char[]> Process(string fullFilePath, bool isEncode)
{
    Console.WriteLine(isEncode ? "Encoding..." : "Decoding...");

    using var file = File.OpenRead(fullFilePath);

    int byteCount = 0, offest = 0, headerSize = CUSTOM_HEADER.Length;
    char orignalChar, key;
    char[] outputChars;

    if (isEncode)
    {
        outputChars = new char[file.Length + headerSize];
        for (var i = 0; i < headerSize; i++)
        {
            outputChars[i] = CUSTOM_HEADER[i];
        }
    }
    else
    {
        outputChars = new char[file.Length - headerSize];
        file.Seek(headerSize, SeekOrigin.Begin);
    }

    var buffer = new byte[BUFFER_SIZE];
    while ((byteCount = await file.ReadAsync(buffer)) > 0)
    {
        for (var i = 0; i < byteCount; i++)
        {
            orignalChar = (char)buffer[i];
            key = CAESAR_CIPHER[(offest + i) % CAESAR_CIPHER.Length];
            if (isEncode)
            {
                outputChars[headerSize + offest + i] = (char)(orignalChar + key);
            }
            else
            {
                outputChars[offest + i] = (char)(orignalChar - key);
            }
        }
        offest += byteCount;
    }
    return outputChars;
}

async Task SaveFile(char[] charData, bool isEncode)
{
    var fileNameWithoutExt =
        $"{Path.GetFileNameWithoutExtension(inputFilePath)}.{(isEncode ? "dat" : "txt")}";
    var fullSavePath = $"{filePath}{pathSeparatorChar}{fileNameWithoutExt}";
    await File.WriteAllTextAsync(fullSavePath, new string(charData), Encoding.Latin1);

    Console.WriteLine(
        $"The {(isEncode ? "encoded" : "decoded")} data was saved at:\n" +
        $"  {fullSavePath}");
}

void HelpMe()
{
    Console.WriteLine(string.Format(USAGE_MESSAGE, AppDomain.CurrentDomain.FriendlyName));
    Environment.Exit(0);
}

void OutputError(string errorMessage)
{
    Console.WriteLine(errorMessage);
    Environment.Exit(1);
}