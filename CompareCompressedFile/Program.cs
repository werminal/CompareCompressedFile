using System.Formats.Tar;
using System.IO.Compression;

const string initialStagingFolder = "staging";
const string initialFileName = "file";
const string initialFileExtension = "bin";
const string initialFullFileName = $"{initialStagingFolder}\\{initialFileName}.{initialFileExtension}";

const string initialCompressedFullFileName = $"{initialFileName}.tar.gz";


InitialSetup(initialStagingFolder);


await CreateFile(initialFullFileName);

var originalSize = GetFileSize(initialFullFileName);
Console.WriteLine($"File Size: {originalSize} bytes");

await Compress(initialStagingFolder, initialCompressedFullFileName);

var compressedSize = GetFileSize(initialCompressedFullFileName);
Console.WriteLine($"File Size: {compressedSize} bytes");

await Extract(initialCompressedFullFileName, initialStagingFolder);

var verifyExtract = GetFileSize(initialFullFileName);
Console.WriteLine($"File Size: {verifyExtract} bytes");

var compareOriginalWithOutput =
    originalSize == verifyExtract ? "Files Match" : "No Match";

Console.WriteLine(compareOriginalWithOutput);

var compressionRatio = 100 * (1 - (double)compressedSize / originalSize);
Console.WriteLine($"Compression Ratio: {compressionRatio:F2}%");

return;

static void InitialSetup(string folder)
{
    if (Directory.Exists(folder))
        Directory.Delete(folder, true);

    Directory.CreateDirectory(folder);
}

static async Task CreateFile(string fileName)
{
    if (File.Exists(fileName))
        File.Delete(fileName);

    await File.WriteAllBytesAsync(fileName, new byte[1024 * 1024 * 500]);
}

static long GetFileSize(string fileName)
{
    var fileInfo = new FileInfo(fileName);
    return fileInfo.Length;
}

static async Task Compress(string sourceDirectory, string targetTarFile)
{
    if (File.Exists(targetTarFile))
        File.Delete(targetTarFile);

    var tempTar = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".tar");


    await TarFile.CreateFromDirectoryAsync(
        sourceDirectory,
        tempTar,
        false
    );

    await using var tarStream = File.OpenRead(tempTar);
    await using var gzStream = File.Create(targetTarFile);
    await using var gzip = new GZipStream(gzStream, CompressionLevel.Fastest);
    await tarStream.CopyToAsync(gzip);

    tarStream.Close();

    File.Delete(tempTar);
}

static async Task Extract(string sourceFile, string targetDirectory)
{
    var tempTar = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".tar");

    await using var gzStream = File.OpenRead(sourceFile);
    await using var gzip = new GZipStream(gzStream, CompressionMode.Decompress);
    await using var tarStream = File.Create(tempTar);

    await gzip.CopyToAsync(tarStream);

    tarStream.Close();

    await TarFile.ExtractToDirectoryAsync(
        tempTar,
        targetDirectory,
        true
    );
}
